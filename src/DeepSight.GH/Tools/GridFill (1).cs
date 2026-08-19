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
    /// Iterative diffusion fill of a voxel density field.
    /// </summary>
    /// <remarks>
    /// SAME SOLVER AS GridErosion, RUN THE OTHER WAY. In a sparse 3D density
    /// field, an empty cell next to material has a strongly positive Laplacian,
    /// so the diffusion term pushes density into it: material spreads outward
    /// one cell per step. In GridErosion that behaviour is a failure mode and is
    /// clamped off by ErodeOnly. Here it IS the feature - it closes gaps,
    /// thickens thin shells, and bridges small holes in scan data.
    ///
    /// KNOWN BEHAVIOUR: run for enough steps this will not stop at the geometry.
    /// Diffusion has no notion of "inside", so it keeps spreading until it fills
    /// the whole bounding box. Use few steps, watch the result, and stop when it
    /// looks right. If you want a fill that stops at enclosed voids only, that
    /// needs a flood-fill algorithm rather than diffusion - a different component.
    ///
    /// Everything below this line is the erosion solver, unchanged. The physics
    /// notes still apply; only the sign of the useful term differs.
    /// </remarks>
    /// <remarks>
    /// THE PHYSICS. This is a 3D generalisation of the heightfield erosion PDE:
    ///
    ///     da/dt = f * LAP(a)  -  g * |GRAD(a)|^e * (1 + h*(b-1))
    ///
    ///   f * LAP(a)          diffusion: smooths the surface (creep / slumping)
    ///   g * |GRAD(a)|^e     slope/surface-driven erosion: material is removed
    ///                       where the field changes fastest, i.e. at surfaces
    ///   (1 + h*(b-1))       modulation by an erodibility field b (fractal
    ///                       noise), so some material is softer than the rest
    ///
    /// WHY 3D AND NOT A HEIGHTFIELD. The original version of this algorithm
    /// stored one height per (x,y) column. That cannot represent overhangs,
    /// vertical walls, or interior voids - sampling a scanned building into a
    /// heightfield collapses every wall to a single z per column and loses the
    /// interior entirely. Applied to voxel density instead, the same equation
    /// erodes the real 3D surface: interior voxels have near-zero gradient so
    /// they only diffuse, while surface voxels have a large gradient and are
    /// eaten away. Overhangs and walls survive.
    ///
    /// HOW IT RUNS. Voxels are read out of OpenVDB once into a dense array over
    /// the grid's bounding box, all steps run in managed memory, then the result
    /// is written back to a NEW grid. Crossing the native boundary per voxel per
    /// step would be hopelessly slow; a dense array is also far more cache
    /// friendly than hashing coordinates, which matters when every cell touches
    /// its 26 neighbours every step.
    ///
    /// STATE. Simulation state lives in instance fields, not statics, so two
    /// copies of this component on one canvas do not fight over the same buffers.
    /// Drive it with a Grasshopper timer plus Run, exactly like the original.
    /// </remarks>
    public class Cmpt_GridFill : GH_Component
    {
        public Cmpt_GridFill()
          : base("GridFill", "GFill",
              "Iterative 3D diffusion fill of a voxel density field. Material spreads outward " +
              "into empty space, closing gaps and thickening thin geometry.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.GridSample_01; } }
        public override Guid ComponentGuid { get { return new Guid("ea007046-d3c5-45f0-b38a-ca495ee6af09"); } }

        // ---- simulation state (instance, not static) ----
        private float[] m_a;        // density field, evolves
        private float[] m_b;        // erodibility field, static once built
        private float[] m_tmp;      // double buffer
        private float[] m_exp;      // directional exposure (1 = open, 0 = fully sheltered)
        private int m_nx, m_ny, m_nz;
        private int[] m_origin;     // index-space origin of the dense array
        private float[] m_transform;
        private string m_cfg = "";
        private int m_frame = 0;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Float grid to erode.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Reset the simulation to the input grid.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Run", "Run", "Advance the simulation.", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Steps", "S", "Steps to advance per solve.", GH_ParamAccess.item, 1);

            pManager.AddNumberParameter("dt", "dt", "Time step. Too large and the solution blows up; see the stability warning.", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Diffusion", "f", "Diffusion strength (surface smoothing).", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Erosion", "g",
                "Material loss at surfaces. Left at 0 the component purely fills; raise it to " +
                "fill and erode at the same time.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Exponent", "e", "Gradient exponent. 1 = linear, 2 = strongly favours sharp features.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Variation", "h", "How much the erodibility noise modulates erosion. 0 = uniform material.", GH_ParamAccess.item, 1.0);

            pManager.AddVectorParameter("Direction", "D",
                "Direction the weather travels (e.g. 0,0,-1 for rain falling). Material in the way " +
                "shelters whatever is behind it. Zero vector = uniform erosion from all sides.",
                GH_ParamAccess.item, Vector3d.Zero);
            pManager.AddIntegerParameter("Seed", "Sd", "Seed for the erodibility noise.", GH_ParamAccess.item, 42);
            pManager.AddNumberParameter("Threshold", "T",
                "Density below which a voxel is dropped from the output. Filling produces soft " +
                "partial densities at the spreading front, so a LOW value (0.05-0.2) keeps the " +
                "newly filled material; 0.5 would discard most of it.",
                GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("MaxCells", "M",
                "Safety limit on the dense simulation volume (nx*ny*nz).",
                GH_ParamAccess.item, 20000000);
            pManager.AddBooleanParameter("ErodeOnly", "EO",
                "Leave this OFF for filling. Turning it on forbids any density increase, which " +
                "converts this component back into pure erosion (use GridErosion for that).",
                GH_ParamAccess.item, false);

            for (int i = 1; i < 14; i++) pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "The eroded grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Frame", "F", "Steps simulated so far.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            bool reset = false, run = false;
            int steps = 1, seed = 42, maxCells = 20000000;
            double dt = 0.1, f = 0.5, g = 0.0, e = 1.0, h = 1.0, threshold = 0.1;
            Vector3d dir = Vector3d.Zero;
            bool erodeOnly = false;

            DA.GetData(1, ref reset);
            DA.GetData(2, ref run);
            DA.GetData(3, ref steps);
            DA.GetData(4, ref dt);
            DA.GetData(5, ref f);
            DA.GetData(6, ref g);
            DA.GetData(7, ref e);
            DA.GetData(8, ref h);
            DA.GetData(9, ref dir);
            DA.GetData(10, ref seed);
            DA.GetData(11, ref threshold);
            DA.GetData(12, ref maxCells);
            DA.GetData(13, ref erodeOnly);

            if (steps < 0) steps = 0;

            FGrid grid = GH_Grid.ParseStructure(obj) as FGrid;
            if (grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be a float grid.");
                return;
            }
            if (!grid.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid is invalid or disposed.");
                return;
            }

            // Explicit Euler on a 3D diffusion term is only stable for
            // dt*f <= dx^2/(2*ndim) = 1/6 with unit spacing. Past that the field
            // oscillates and then goes to NaN, which looks like the component is
            // broken rather than the timestep being wrong - so say so.
            if (dt * f > 1.0 / 6.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                    "dt * Diffusion = {0:F3} exceeds the stability limit of 0.167. The simulation " +
                    "will oscillate and diverge. Reduce dt or Diffusion.", dt * f));
            }

            // Re-initialise on reset, on a parameter change that invalidates the
            // field, or on the first run.
            string cfg = MakeConfig(grid, seed);
            if (reset || m_a == null || cfg != m_cfg)
            {
                if (!InitField(grid, seed, maxCells, info)) return;
                m_cfg = cfg;
                m_frame = 0;
            }

            if (run && !reset && steps > 0)
            {
                Vector3d ndir = dir;
                bool directional = ndir.Length > 1e-9;
                if (directional) ndir.Unitize();

                for (int s = 0; s < steps; s++)
                {
                    // Recomputed every step: as material erodes away, what it
                    // was sheltering becomes exposed. Computing this once up
                    // front would freeze the shadows in their initial state.
                    if (directional) ComputeExposure(ndir);
                    DoStep(dt, f, g, e, h, directional, erodeOnly);
                }

                m_frame += steps;
            }

            // Write the surviving voxels into a new grid, leaving the input alone.
            var outGrid = BuildGrid(threshold, info);

            info.Add(string.Format("Volume {0} x {1} x {2}, {3} steps simulated.", m_nx, m_ny, m_nz, m_frame));

            DA.SetData(0, new GH_Grid(outGrid));
            DA.SetData(1, m_frame);
            DA.SetDataList(2, info);
        }

        private string MakeConfig(FGrid grid, int seed)
        {
            int[] mn, mx;
            grid.BoundingBox(out mn, out mx);
            return string.Join(",", mn) + "|" + string.Join(",", mx) + "|" + seed;
        }

        private bool InitField(FGrid grid, int seed, int maxCells, List<string> info)
        {
            int[] mn, mx;
            grid.BoundingBox(out mn, out mx);

            m_nx = mx[0] - mn[0] + 1;
            m_ny = mx[1] - mn[1] + 1;
            m_nz = mx[2] - mn[2] + 1;
            m_origin = new int[] { mn[0], mn[1], mn[2] };

            long cells = (long)m_nx * m_ny * m_nz;
            if (m_nx <= 0 || m_ny <= 0 || m_nz <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid has an empty bounding box.");
                return false;
            }
            if (cells > maxCells)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                    "Simulation volume is {0:N0} cells ({1}x{2}x{3}), over the MaxCells limit of {4:N0}. " +
                    "Resample the grid coarser, or raise MaxCells if you have the memory " +
                    "(needs about {5:N0} MB).",
                    cells, m_nx, m_ny, m_nz, maxCells, (cells * 12) / (1024 * 1024)));
                return false;
            }

            int n = (int)cells;
            m_a = new float[n];
            m_b = new float[n];
            m_tmp = new float[n];
            m_exp = new float[n];

            try { m_transform = grid.Transform; }
            catch { m_transform = null; }

            // Scatter the sparse active voxels into the dense array.
            int[] active;
            float[] vals;
            try
            {
                active = grid.GetActiveVoxels();
                vals = grid.GetValuesIndex(active);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read the grid: " + ex.Message);
                return false;
            }

            int count = active.Length / 3;
            for (int i = 0; i < count; i++)
            {
                int ix = active[i * 3] - m_origin[0];
                int iy = active[i * 3 + 1] - m_origin[1];
                int iz = active[i * 3 + 2] - m_origin[2];
                if (ix < 0 || iy < 0 || iz < 0 || ix >= m_nx || iy >= m_ny || iz >= m_nz) continue;
                m_a[Idx(ix, iy, iz)] = vals[i];
            }

            // 3D fractal erodibility field, ported from the 2D version.
            for (int z = 0; z < m_nz; z++)
                for (int y = 0; y < m_ny; y++)
                    for (int x = 0; x < m_nx; x++)
                    {
                        double u = (double)x / Math.Max(1, m_nx - 1);
                        double v = (double)y / Math.Max(1, m_ny - 1);
                        double w = (double)z / Math.Max(1, m_nz - 1);
                        m_b[Idx(x, y, z)] = (float)Fbm(u, v, w, 8.0, 6, seed);
                    }

            // A signed distance field has negative values inside the surface and
            // a narrow active band around it, which this density-based model will
            // mangle. Worth catching, because GridFromMesh produces exactly that.
            float vmin = float.MaxValue, vmax = float.MinValue;
            for (int i = 0; i < vals.Length; i++)
            {
                if (vals[i] < vmin) vmin = vals[i];
                if (vals[i] > vmax) vmax = vals[i];
            }
            if (vals.Length > 0 && (vmin < -1e-6f || vmax > 1.0f + 1e-6f))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                    "Values run from {0:F3} to {1:F3}, which looks like a level set rather than a " +
                    "density field. This component expects occupancy in 0..1. Convert with " +
                    "SdfToFog first, or results will be meaningless.", vmin, vmax));
            }

            info.Add(string.Format("Initialised {0:N0} active voxels into a {1}x{2}x{3} volume.",
                count, m_nx, m_ny, m_nz));
            info.Add(string.Format("Input values range {0:F3} to {1:F3}.", vmin, vmax));
            return true;
        }

        private int Idx(int x, int y, int z)
        {
            return (z * m_ny + y) * m_nx + x;
        }

        /// <summary>
        /// Propagate directional exposure through the volume in a single sweep.
        /// </summary>
        /// <remarks>
        /// exposure[cell] = exposure[upstream] * (1 - density[upstream])
        ///
        /// Starting at 1.0 (open sky) on the boundary, each cell inherits its
        /// upstream neighbour's exposure attenuated by how solid that neighbour
        /// is. Solid material drives exposure to nearly zero for everything
        /// behind it, which is the shadow. Partially dense material partially
        /// shades, so shelter is soft rather than a binary in/out test.
        ///
        /// One pass, O(n). Ray marching from every surface voxel toward the sky
        /// would be O(n * ray length) and far too slow to run inside the step
        /// loop, which is where it has to be if shadows are to update as the
        /// geometry erodes.
        ///
        /// Cells are visited in slices along the direction's dominant axis,
        /// ordered so that upstream cells are always computed before the cells
        /// that read them.
        /// </remarks>
        private void ComputeExposure(Vector3d d)
        {
            // Dominant axis, so one slice step advances exactly one cell there.
            double ax = Math.Abs(d.X), ay = Math.Abs(d.Y), az = Math.Abs(d.Z);
            int axis = (ax >= ay && ax >= az) ? 0 : ((ay >= az) ? 1 : 2);
            double dom = (axis == 0) ? d.X : ((axis == 1) ? d.Y : d.Z);
            if (Math.Abs(dom) < 1e-9) { for (int i = 0; i < m_exp.Length; i++) m_exp[i] = 1.0f; return; }

            // Step vector whose dominant component is exactly +/-1.
            double sx = d.X / Math.Abs(dom), sy = d.Y / Math.Abs(dom), sz = d.Z / Math.Abs(dom);

            int n = (axis == 0) ? m_nx : ((axis == 1) ? m_ny : m_nz);

            // Weather travels along +dom, so upstream is -dom: iterate from the
            // low end when dom > 0, from the high end when dom < 0.
            bool forward = dom > 0;
            for (int k = 0; k < n; k++)
            {
                int slice = forward ? k : (n - 1 - k);

                for (int b = 0; b < ((axis == 0) ? m_ny : m_nx); b++)
                {
                    for (int c = 0; c < ((axis == 2) ? m_ny : m_nz); c++)
                    {
                        int x, y, z;
                        if (axis == 0) { x = slice; y = b; z = c; }
                        else if (axis == 1) { x = b; y = slice; z = c; }
                        else { x = b; y = c; z = slice; }

                        if (x < 0 || y < 0 || z < 0 || x >= m_nx || y >= m_ny || z >= m_nz) continue;

                        int id = Idx(x, y, z);

                        int ux = (int)Math.Round(x - sx);
                        int uy = (int)Math.Round(y - sy);
                        int uz = (int)Math.Round(z - sz);

                        if (ux < 0 || uy < 0 || uz < 0 || ux >= m_nx || uy >= m_ny || uz >= m_nz)
                        {
                            m_exp[id] = 1.0f;      // open to the weather
                        }
                        else
                        {
                            int uid = Idx(ux, uy, uz);
                            float att = 1.0f - m_a[uid];
                            if (att < 0.0f) att = 0.0f;
                            m_exp[id] = m_exp[uid] * att;
                        }
                    }
                }
            }
        }

        private void DoStep(double dt, double f, double g, double e, double h, bool directional, bool erodeOnly)
        {
            // 7-point Laplacian and central-difference gradient. Boundaries are
            // clamped (Neumann), matching the original's Math.Max/Min indexing.
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

                        float ax0 = m_a[Idx(xm, y, z)], ax1 = m_a[Idx(xp, y, z)];
                        float ay0 = m_a[Idx(x, ym, z)], ay1 = m_a[Idx(x, yp, z)];
                        float az0 = m_a[Idx(x, y, zm)], az1 = m_a[Idx(x, y, zp)];

                        double gx = (ax1 - ax0) * 0.5;
                        double gy = (ay1 - ay0) * 0.5;
                        double gz = (az1 - az0) * 0.5;
                        double gmag = Math.Sqrt(gx * gx + gy * gy + gz * gz);

                        double lap = (ax0 + ax1 + ay0 + ay1 + az0 + az1) - 6.0 * a;

                        double bb = m_b[id];

                        // Directional weathering by SHELTER, not by surface
                        // orientation. A dot product of the normal with the
                        // weather direction is the obvious approach and it is
                        // wrong for the common case: on a vertical panel with
                        // rain falling straight down, every face normal is
                        // perpendicular to the fall direction, so a dot product
                        // erodes nothing at all. What actually matters is whether
                        // material upstream is in the way. See ComputeExposure.
                        double expose = directional ? m_exp[id] : 1.0;

                        double delta = f * lap
                                     - g * Math.Pow(gmag, e) * (1.0 + h * (bb - 1.0)) * expose;

                        double next = a + dt * delta;

                        // THE IMPORTANT LINE. In a heightfield every cell holds
                        // a value, so diffusion smooths the surface. In a sparse
                        // 3D density field the empty cells next to material have
                        // a strongly POSITIVE Laplacian, so diffusion pushes
                        // density into them: the shape blurs outward one cell per
                        // step until it fills its own bounding box and reads as a
                        // solid block. Erosion only ever removes material, so
                        // forbidding any increase kills that failure mode
                        // outright while leaving the wearing-down behaviour
                        // (which is all negative) untouched.
                        if (erodeOnly && next > a) next = a;

                        if (next < 0.0) next = 0.0;

                        m_tmp[id] = (float)next;
                    }
                }
            }

            float[] swap = m_a; m_a = m_tmp; m_tmp = swap;
        }

        private FGrid BuildGrid(double threshold, List<string> info)
        {
            var outGrid = new FGrid("filled", 0.0f);
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
                        if (v <= threshold) continue;   // eroded away
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
                    "Nothing above the threshold. Lower Threshold, or check the input has material.");
            }

            info.Add(string.Format("{0:N0} voxels remain above the threshold.", vals.Count));
            return outGrid;
        }

        // ---- 3D fractal value noise (ported from the 2D original) ----

        private static double Fbm(double x, double y, double z, double baseFreq, int octaves, int seed)
        {
            double sum = 0, amp = 0.5, freq = baseFreq, norm = 0;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * ValueNoise(x * freq, y * freq, z * freq, seed + o * 131);
                norm += amp;
                amp *= 0.5; freq *= 2.0;
            }
            return norm > 0 ? sum / norm : 0.0;
        }

        private static double ValueNoise(double x, double y, double z, int seed)
        {
            int xi = (int)Math.Floor(x), yi = (int)Math.Floor(y), zi = (int)Math.Floor(z);
            double xf = x - xi, yf = y - yi, zf = z - zi;
            double u = Smooth(xf), v = Smooth(yf), w = Smooth(zf);

            double n000 = Hash(xi, yi, zi, seed), n100 = Hash(xi + 1, yi, zi, seed);
            double n010 = Hash(xi, yi + 1, zi, seed), n110 = Hash(xi + 1, yi + 1, zi, seed);
            double n001 = Hash(xi, yi, zi + 1, seed), n101 = Hash(xi + 1, yi, zi + 1, seed);
            double n011 = Hash(xi, yi + 1, zi + 1, seed), n111 = Hash(xi + 1, yi + 1, zi + 1, seed);

            double x00 = Lerp(n000, n100, u), x10 = Lerp(n010, n110, u);
            double x01 = Lerp(n001, n101, u), x11 = Lerp(n011, n111, u);

            return Lerp(Lerp(x00, x10, v), Lerp(x01, x11, v), w);
        }

        private static double Smooth(double t) { return t * t * (3.0 - 2.0 * t); }
        private static double Lerp(double a, double b, double t) { return a + (b - a) * t; }

        private static double Hash(int x, int y, int z, int seed)
        {
            unchecked
            {
                int hh = x * 374761393 + y * 668265263 + z * 1440662683 + seed * 982451653;
                hh = (hh ^ (hh >> 13)) * 1274126177;
                hh = hh ^ (hh >> 16);
                return (hh & 0x7fffffff) / (double)0x7fffffff;
            }
        }
    }
}
