using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;
using Grasshopper.Kernel;

using DeepSight.RhinoCommon;

using FGrid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Gradually transforms one voxel grid into another, step by step.
    /// </summary>
    /// <remarks>
    /// THE UPDATE RULE. Same solver family as GridFill / GridErosion, but instead
    /// of a fixed erosion term the field is pulled toward a target:
    ///
    ///     da/dt = f * LAP(a)  +  k * (target - a)
    ///
    ///   f * LAP(a)          diffusion: keeps the evolving shape smooth and
    ///                       blobby rather than pixelated, and lets material
    ///                       bridge across gaps as it travels
    ///   k * (target - a)    attraction: every voxel is drawn toward whatever
    ///                       the target grid holds there
    ///
    /// WHY NOT JUST CROSSFADE. The obvious implementation is
    /// result = A*(1-t) + B*t, which is one line. But it looks wrong: A fades
    /// out in place while B fades in on top, so mid-transition you see two
    /// translucent ghosts rather than one shape becoming another. The attraction
    /// term plus diffusion gives an actual travelling front - material grows and
    /// recedes, which is the behaviour you get from GridFill and wanted to keep.
    ///
    /// CONVERGENCE. The attraction term is exponential: each step closes a
    /// fraction (dt*k) of the remaining gap, so it approaches the target quickly
    /// at first and then asymptotically. Progress output reports how far along it
    /// is. Raise Attraction for a faster, harder transition; lower it (and raise
    /// Diffusion) for a slower, more molten one.
    ///
    /// BOTH GRIDS MUST SHARE AN INDEX SPACE. The simulation runs over the union
    /// of the two bounding boxes, and voxel (i,j,k) is assumed to mean the same
    /// physical point in both. If the transforms differ the morph is meaningless;
    /// the component checks and warns. Resample one onto the other first.
    /// </remarks>
    public class Cmpt_GridMorphTo : GH_Component
    {
        public Cmpt_GridMorphTo()
          : base("HypervoxelMetamorph", "GMeta",
              "Gradually transform one voxel grid into another. Diffusion keeps the transition " +
              "smooth; attraction pulls the field toward the target shape.",
              DeepSight.GH.Api.ComponentCategory, "Hypervoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.HypervoxelMetamorph; } }
        public override Guid ComponentGuid { get { return new Guid("bd1ff383-07f5-46d6-8d05-42e3815e6972"); } }

        // ---- state (instance, not static: two components must not share buffers) ----
        private float[] m_a;        // evolving field
        private float[] m_target;   // destination field
        private float[] m_tmp;
        private int[] m_label;      // connected-component ids, reused each step
        private int m_nx, m_ny, m_nz;
        private int[] m_origin;
        private float[] m_transform;
        private string m_cfg = "";
        private int m_frame = 0;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("From", "A", "Grid to start from.", GH_ParamAccess.item);
            pManager.AddGenericParameter("To", "B", "Grid to transform into.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Reset back to the From grid.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "Run", "Advance the transformation.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Steps", "S", "Steps to advance per solve.", GH_ParamAccess.item, 1);

            pManager.AddNumberParameter("dt", "dt", "Time step.", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Diffusion", "f",
                "Smoothing. Higher values make the transition more molten and blobby.",
                GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Attraction", "k",
                "How strongly the field is pulled toward the target. Higher = faster, harder change.",
                GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Spread", "Sp",
                "How much growth must travel through space rather than appearing everywhere at once. " +
                "1 = material can only grow next to existing material (a travelling front). " +
                "0 = every voxel moves toward the target independently, which makes the whole " +
                "target shape pop into view at the same moment.",
                GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("KeepConnected", "KC",
                "Delete any visible voxels not connected to the main body. Stops specks appearing " +
                "ahead of the growing front and stops thin necks pinching off into stranded " +
                "fragments as the shape shrinks.",
                GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Threshold", "T",
                "Density below which a voxel is dropped from the output. Mid-transition densities " +
                "are soft, so keep this low (0.05-0.2).",
                GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("MaxCells", "M",
                "Safety limit on the simulation volume (nx*ny*nz).", GH_ParamAccess.item, 20000000);

            for (int i = 2; i < 12; i++) pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "The current state of the transformation.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Frame", "F", "Steps run so far.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Progress", "P",
                "How far along, 0 = at the From grid, 1 = converged on the To grid.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            object objA = null, objB = null;
            if (!DA.GetData(0, ref objA)) return;
            if (!DA.GetData(1, ref objB)) return;

            bool reset = false, run = false;
            int steps = 1, maxCells = 20000000;
            double dt = 0.1, f = 0.3, k = 0.5, threshold = 0.1, spread = 1.0;
            bool keepConnected = true;

            DA.GetData(2, ref reset);
            DA.GetData(3, ref run);
            DA.GetData(4, ref steps);
            DA.GetData(5, ref dt);
            DA.GetData(6, ref f);
            DA.GetData(7, ref k);
            DA.GetData(8, ref spread);
            DA.GetData(9, ref keepConnected);
            DA.GetData(10, ref threshold);
            DA.GetData(11, ref maxCells);
            if (spread < 0.0) spread = 0.0;
            if (spread > 1.0) spread = 1.0;

            if (steps < 0) steps = 0;

            FGrid gridA = GH_Grid.ParseStructure(objA) as FGrid;
            FGrid gridB = GH_Grid.ParseStructure(objB) as FGrid;

            if (gridA == null || gridB == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Both inputs must be float grids.");
                return;
            }
            if (!gridA.IsValid || !gridB.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "One of the grids is invalid or disposed.");
                return;
            }

            // Stability of the explicit scheme: the diffusion limit is the tighter
            // of the two in practice, but the attraction term also blows up if
            // dt*k > 1 (it overshoots the target and oscillates).
            if (dt * f > 1.0 / 6.0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                    "dt * Diffusion = {0:F3} exceeds 0.167 and will diverge. Reduce dt or Diffusion.", dt * f));
            if (dt * k > 1.0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                    "dt * Attraction = {0:F3} is above 1, so each step overshoots the target and " +
                    "the result will oscillate. Reduce dt or Attraction.", dt * k));

            string cfg = MakeConfig(gridA, gridB);
            if (reset || m_a == null || cfg != m_cfg)
            {
                if (!Init(gridA, gridB, maxCells, info)) return;
                m_cfg = cfg;
                m_frame = 0;
            }

            if (run && !reset && steps > 0)
            {
                int removedTotal = 0;
                for (int s = 0; s < steps; s++)
                {
                    DoStep(dt, f, k, spread);
                    if (keepConnected) removedTotal += EnforceConnectivity(threshold);
                }
                m_frame += steps;

                if (removedTotal > 0)
                    info.Add(string.Format("Removed {0:N0} disconnected voxels.", removedTotal));
            }

            var outGrid = BuildGrid(threshold, info);
            double progress = Progress();

            info.Add(string.Format("Volume {0} x {1} x {2}, {3} steps, {4:P0} converged.",
                m_nx, m_ny, m_nz, m_frame, progress));

            DA.SetData(0, new GH_Grid(outGrid));
            DA.SetData(1, m_frame);
            DA.SetData(2, progress);
            DA.SetDataList(3, info);
        }

        private string MakeConfig(FGrid a, FGrid b)
        {
            int[] amn, amx, bmn, bmx;
            a.BoundingBox(out amn, out amx);
            b.BoundingBox(out bmn, out bmx);
            return string.Join(",", amn) + "|" + string.Join(",", amx) + "||"
                 + string.Join(",", bmn) + "|" + string.Join(",", bmx);
        }

        private bool Init(FGrid a, FGrid b, int maxCells, List<string> info)
        {
            int[] amn, amx, bmn, bmx;
            a.BoundingBox(out amn, out amx);
            b.BoundingBox(out bmn, out bmx);

            // Union of both boxes: the shape has to be able to grow into
            // wherever the target lives, and recede from where the source was.
            int[] mn = new int[3], mx = new int[3];
            for (int i = 0; i < 3; i++)
            {
                mn[i] = Math.Min(amn[i], bmn[i]);
                mx[i] = Math.Max(amx[i], bmx[i]);
            }

            m_nx = mx[0] - mn[0] + 1;
            m_ny = mx[1] - mn[1] + 1;
            m_nz = mx[2] - mn[2] + 1;
            m_origin = new int[] { mn[0], mn[1], mn[2] };

            if (m_nx <= 0 || m_ny <= 0 || m_nz <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Empty bounding box.");
                return false;
            }

            long cells = (long)m_nx * m_ny * m_nz;
            if (cells > maxCells)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                    "Combined volume is {0:N0} cells ({1}x{2}x{3}), over MaxCells ({4:N0}). " +
                    "Resample the grids coarser, or move them closer together - the volume spans " +
                    "the union of both bounding boxes.",
                    cells, m_nx, m_ny, m_nz, maxCells));
                return false;
            }

            int n = (int)cells;
            m_a = new float[n];
            m_target = new float[n];
            m_tmp = new float[n];

            try { m_transform = a.Transform; } catch { m_transform = null; }

            // Transform check: without this the morph silently interpolates
            // between points that are nowhere near each other in space.
            try
            {
                float[] ta = a.Transform, tb = b.Transform;
                bool match = ta != null && tb != null && ta.Length == 16 && tb.Length == 16;
                if (match)
                    for (int i = 0; i < 16; i++)
                        if (Math.Abs(ta[i] - tb[i]) > 1e-5f) { match = false; break; }

                if (!match)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "The two grids have different transforms, so voxel coordinates do not refer " +
                        "to the same physical points. Resample one onto the other, or the morph will " +
                        "interpolate between unrelated locations.");
            }
            catch { }

            if (!Scatter(a, m_a, "From")) return false;
            if (!Scatter(b, m_target, "To")) return false;

            info.Add(string.Format("Initialised a {0}x{1}x{2} volume from both grids.", m_nx, m_ny, m_nz));
            return true;
        }

        private bool Scatter(FGrid grid, float[] dest, string label)
        {
            int[] active;
            float[] vals;
            try
            {
                active = grid.GetActiveVoxels();
                vals = grid.GetValuesIndex(active);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    string.Format("Could not read the {0} grid: {1}", label, ex.Message));
                return false;
            }

            int count = active.Length / 3;
            for (int i = 0; i < count; i++)
            {
                int ix = active[i * 3] - m_origin[0];
                int iy = active[i * 3 + 1] - m_origin[1];
                int iz = active[i * 3 + 2] - m_origin[2];
                if (ix < 0 || iy < 0 || iz < 0 || ix >= m_nx || iy >= m_ny || iz >= m_nz) continue;
                dest[Idx(ix, iy, iz)] = vals[i];
            }
            return true;
        }

        private int Idx(int x, int y, int z) { return (z * m_ny + y) * m_nx + x; }

        private void DoStep(double dt, double f, double k, double spread)
        {
            for (int z = 0; z < m_nz; z++)
            {
                int zm = Math.Max(0, z - 1), zp = Math.Min(m_nz - 1, z + 1);
                for (int y = 0; y < m_ny; y++)
                {
                    int ym = Math.Max(0, y - 1), yp = Math.Min(m_ny - 1, y + 1);
                    for (int x = 0; x < m_nx; x++)
                    {
                        int xm = Math.Max(0, x - 1), xp = Math.Min(m_nx - 1, x + 1);

                        int id = Idx(x, y, z);
                        float a = m_a[id];

                        double nsum = m_a[Idx(xm, y, z)] + m_a[Idx(xp, y, z)]
                                    + m_a[Idx(x, ym, z)] + m_a[Idx(x, yp, z)]
                                    + m_a[Idx(x, y, zm)] + m_a[Idx(x, y, zp)];

                        double lap = nsum - 6.0 * a;

                        double pull = m_target[id] - a;

                        // WHY THE GATE. Ungated, the attraction term moves every
                        // voxel toward the target at the same exponential rate, so
                        // every target voxel crosses the display threshold on the
                        // same step and the whole shape pops into view at once -
                        // a crossfade with extra steps.
                        //
                        // Gating by local density forces the change to travel:
                        // a voxel may only GROW where there is already material
                        // beside it, and only SHRINK where it is exposed. Material
                        // therefore creeps outward from the source shape and
                        // recedes from exposed surfaces, which is a front rather
                        // than a fade. Spread blends between the two behaviours.
                        double nbr = nsum / 6.0;                       // 0..1 local density
                        double gate = (pull > 0.0) ? nbr : (1.0 - nbr);
                        double eff = (1.0 - spread) + spread * gate;

                        double next = a + dt * (f * lap + k * pull * eff);

                        if (next < 0.0) next = 0.0;
                        if (next > 1.0) next = 1.0;

                        m_tmp[id] = (float)next;
                    }
                }
            }

            float[] swap = m_a; m_a = m_tmp; m_tmp = swap;
        }

        /// <summary>
        /// Delete visible voxels that are not part of the largest connected body.
        /// Returns how many were removed.
        /// </summary>
        /// <remarks>
        /// Two things produce islands, and neither is fixable by tuning.
        ///
        /// GROWING: diffusion spreads a faint, uneven halo ahead of the front.
        /// Wherever that halo happens to tip over the display threshold you get a
        /// lone speck, disconnected from the body, that later gets absorbed when
        /// the real front arrives.
        ///
        /// SHRINKING: the shrink gate erodes inward from exposed surfaces, so a
        /// thin neck is eaten from both sides at once and eventually pinches off,
        /// stranding whatever was beyond it.
        ///
        /// A flood fill over the cells at or above the threshold finds the
        /// connected bodies; everything outside the largest one is zeroed.
        ///
        /// Note it deliberately only zeroes cells that are AT OR ABOVE the
        /// threshold. The sub-threshold halo is left intact, because that faint
        /// tissue is what carries the growth front forward - clearing it too
        /// would stop the shape advancing at all.
        /// </remarks>
        private int EnforceConnectivity(double threshold)
        {
            int n = m_a.Length;
            if (n == 0) return 0;

            if (m_label == null || m_label.Length != n) m_label = new int[n];
            Array.Clear(m_label, 0, n);

            int plane = m_nx * m_ny;
            var stack = new Stack<int>();

            int id = 0, bestId = 0, bestSize = 0;

            for (int start = 0; start < n; start++)
            {
                if (m_a[start] < threshold || m_label[start] != 0) continue;

                id++;
                int size = 0;
                stack.Push(start);
                m_label[start] = id;

                while (stack.Count > 0)
                {
                    int c = stack.Pop();
                    size++;

                    int x = c % m_nx;
                    int rem = c / m_nx;
                    int y = rem % m_ny;
                    int z = rem / m_ny;

                    // 6-connected neighbours
                    if (x > 0)          TryPush(stack, c - 1, threshold, id);
                    if (x < m_nx - 1)   TryPush(stack, c + 1, threshold, id);
                    if (y > 0)          TryPush(stack, c - m_nx, threshold, id);
                    if (y < m_ny - 1)   TryPush(stack, c + m_nx, threshold, id);
                    if (z > 0)          TryPush(stack, c - plane, threshold, id);
                    if (z < m_nz - 1)   TryPush(stack, c + plane, threshold, id);
                }

                if (size > bestSize) { bestSize = size; bestId = id; }
            }

            if (bestId == 0) return 0;

            int removed = 0;
            for (int i = 0; i < n; i++)
            {
                if (m_a[i] >= threshold && m_label[i] != bestId)
                {
                    m_a[i] = 0.0f;
                    removed++;
                }
            }
            return removed;
        }

        private void TryPush(Stack<int> stack, int idx, double threshold, int id)
        {
            if (m_label[idx] != 0) return;
            if (m_a[idx] < threshold) return;
            m_label[idx] = id;
            stack.Push(idx);
        }

        /// <summary>
        /// 1 minus the mean absolute distance to the target, so 1 = converged.
        /// </summary>
        private double Progress()
        {
            if (m_a == null || m_target == null || m_a.Length == 0) return 0.0;

            double sum = 0.0;
            for (int i = 0; i < m_a.Length; i++) sum += Math.Abs(m_target[i] - m_a[i]);
            double mean = sum / m_a.Length;

            double p = 1.0 - mean;
            return p < 0.0 ? 0.0 : (p > 1.0 ? 1.0 : p);
        }

        private FGrid BuildGrid(double threshold, List<string> info)
        {
            var outGrid = new FGrid("morphed", 0.0f);
            if (m_transform != null)
            {
                try { outGrid.Transform = m_transform; } catch { }
            }

            var coords = new List<int>();
            var vals = new List<float>();

            for (int z = 0; z < m_nz; z++)
                for (int y = 0; y < m_ny; y++)
                    for (int x = 0; x < m_nx; x++)
                    {
                        float v = m_a[Idx(x, y, z)];
                        if (v <= threshold) continue;
                        coords.Add(x + m_origin[0]);
                        coords.Add(y + m_origin[1]);
                        coords.Add(z + m_origin[2]);
                        vals.Add(v);
                    }

            if (vals.Count > 0)
            {
                try { outGrid.SetValues(coords.ToArray(), vals.ToArray()); }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not write the result: " + ex.Message);
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Nothing above the threshold. Lower Threshold, or check both grids have material.");
            }

            info.Add(string.Format("{0:N0} voxels above the threshold.", vals.Count));
            return outGrid;
        }
    }
}
