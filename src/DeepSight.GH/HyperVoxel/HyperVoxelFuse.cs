using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;
using Grasshopper.Kernel;

using DeepSight.RhinoCommon;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Fuses several grids that live in DIFFERENT index spaces onto one common
    /// grid, by resampling every source into shared world space at a chosen
    /// voxel size (the "tolerance").
    /// </summary>
    /// <remarks>
    /// WHAT THIS IS FOR. The rest of the HyperVoxel components assume all layers
    /// share one transform. That breaks down when the sources come from
    /// different places: an outside building scan, an inside scan, and a CAD
    /// model each have their own origin and resolution. Stacking them as layers
    /// would be meaningless because voxel (i,j,k) is a different physical point
    /// in each. This component instead rebuilds them all on ONE grid so they
    /// finally line up in space.
    ///
    /// THE TWO HARD LIMITATIONS - READ THESE.
    ///
    /// 1. Sources must already be aligned in WORLD SPACE. This resamples using
    ///    each grid's existing transform; it does NOT register or align them.
    ///    If your scans have not been aligned to a common coordinate system
    ///    first (in Rhino, CloudCompare, etc.), the fused result will be a mess.
    ///    No software can auto-align them without knowing the correspondence, so
    ///    that step genuinely has to happen before this component.
    ///
    /// 2. The "tolerance" IS the target voxel size. Two source points closer
    ///    than one target voxel fall into the same cell and merge; farther apart
    ///    they land in separate cells and stay distinct. There is no separate
    ///    fuzzy-merge radius - quantisation to the common grid is the merge.
    ///
    /// HOW IT WORKS. For each source: read active voxels, convert to world points
    /// via the source transform, quantise to the target voxel size, and write
    /// into the output grid. Where several sources hit the same target cell, the
    /// Overlap mode decides the value.
    ///
    /// Float sources produce a float output; vector (colour) sources produce a
    /// vector output. Mixing the two is refused - there is no meaningful fusion
    /// of a scalar and a colour into one cell.
    /// </remarks>
    public class Cmpt_HyperVoxelFuse : GH_Component
    {
        public Cmpt_HyperVoxelFuse()
          : base("HyperVoxelFuse", "HVFuse",
              "Fuse grids from different coordinate systems onto one common grid at a chosen " +
              "voxel size. Sources must already be aligned in world space.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Hypervoxel_fuse; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f72fd0da-ee07-4d68-bae0-f1d28eed1fc7"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grids", "G", "Grids to fuse (all float, or all vector).", GH_ParamAccess.list);
            pManager.AddNumberParameter("VoxelSize", "V",
                "Target voxel size in world units. This is the fusion tolerance: points closer " +
                "than this merge into one cell.",
                GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Overlap", "O",
                "How to resolve cells hit by more than one source. 0=last wins, 1=first wins, " +
                "2=average, 3=max (float only).",
                GH_ParamAccess.item, 2);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "The fused grid on the common transform.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "C", "Number of occupied cells in the result.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            var raw = new List<object>();
            if (!DA.GetDataList(0, raw) || raw.Count == 0) return;

            double voxelSize = 1.0;
            DA.GetData(1, ref voxelSize);
            if (voxelSize <= 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "VoxelSize must be positive.");
                return;
            }

            int overlap = 2;
            DA.GetData(2, ref overlap);

            // Classify all inputs. Must be uniformly float or uniformly vector.
            var fgrids = new List<FGrid>();
            var vgrids = new List<VGrid>();

            foreach (var obj in raw)
            {
                GridApi g = GH_Grid.ParseStructure(obj);
                if (g == null || !g.IsValid) continue;
                if (g is FGrid) fgrids.Add(g as FGrid);
                else if (g is VGrid) vgrids.Add(g as VGrid);
            }

            if (fgrids.Count > 0 && vgrids.Count > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Cannot fuse float and vector grids together. Fuse each type separately.");
                return;
            }
            if (fgrids.Count == 0 && vgrids.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No valid float or vector grids supplied.");
                return;
            }

            info.Add("Sources must already be aligned in world space; this does not register them.");

            if (fgrids.Count > 0)
                FuseFloat(DA, fgrids, voxelSize, overlap, info);
            else
                FuseVector(DA, vgrids, voxelSize, overlap, info);
        }

        // Quantise a world point to an integer cell on the target grid.
        private static long CellKey(double wx, double wy, double wz, double v, out int ix, out int iy, out int iz)
        {
            ix = (int)Math.Floor(wx / v);
            iy = (int)Math.Floor(wy / v);
            iz = (int)Math.Floor(wz / v);
            const int OFF = 1 << 20;
            return ((long)(ix + OFF) << 42) | ((long)(iy + OFF) << 21) | (long)(iz + OFF);
        }

        private void FuseFloat(IGH_DataAccess DA, List<FGrid> grids, double v, int overlap, List<string> info)
        {
            // Accumulate per target cell so overlaps can be resolved.
            var cellVal = new Dictionary<long, double>();
            var cellCount = new Dictionary<long, int>();
            var cellIjk = new Dictionary<long, int[]>();

            foreach (var grid in grids)
            {
                int[] active;
                float[] vals;
                Transform xf;
                try
                {
                    active = grid.GetActiveVoxels();
                    vals = grid.GetValuesIndex(active);
                    xf = grid.Transform.ToRhinoTransform();
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Skipped a grid: " + e.Message);
                    continue;
                }

                int n = active.Length / 3;
                for (int i = 0; i < n; i++)
                {
                    var p = new Point3d(active[i * 3], active[i * 3 + 1], active[i * 3 + 2]);
                    p.Transform(xf);

                    int ix, iy, iz;
                    long key = CellKey(p.X, p.Y, p.Z, v, out ix, out iy, out iz);
                    double val = vals[i];

                    Accumulate(cellVal, cellCount, cellIjk, key, val, overlap, ix, iy, iz);
                }
            }

            var outGrid = new FGrid("fused", 0.0f);
            SetUniformTransform(outGrid, v);

            WriteFloatCells(outGrid, cellVal, cellCount, cellIjk, overlap);

            info.Add(string.Format("Fused {0} float grids into {1:N0} cells at voxel size {2}.",
                grids.Count, cellVal.Count, v));
            DA.SetData(0, new GH_Grid(outGrid));
            DA.SetData(1, cellVal.Count);
            DA.SetDataList(2, info);
        }

        private void FuseVector(IGH_DataAccess DA, List<VGrid> grids, double v, int overlap, List<string> info)
        {
            var cellR = new Dictionary<long, double>();
            var cellG = new Dictionary<long, double>();
            var cellB = new Dictionary<long, double>();
            var cellCount = new Dictionary<long, int>();
            var cellIjk = new Dictionary<long, int[]>();

            foreach (var grid in grids)
            {
                int[] active;
                Vec3<float>[] vals;
                Transform xf;
                try
                {
                    active = grid.GetActiveVoxels();
                    vals = grid.GetValuesIndex(active);
                    xf = grid.Transform.ToRhinoTransform();
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Skipped a grid: " + e.Message);
                    continue;
                }

                int n = active.Length / 3;
                for (int i = 0; i < n; i++)
                {
                    var p = new Point3d(active[i * 3], active[i * 3 + 1], active[i * 3 + 2]);
                    p.Transform(xf);

                    int ix, iy, iz;
                    long key = CellKey(p.X, p.Y, p.Z, v, out ix, out iy, out iz);

                    if (!cellCount.ContainsKey(key))
                    {
                        cellR[key] = 0; cellG[key] = 0; cellB[key] = 0; cellCount[key] = 0;
                        cellIjk[key] = new int[] { ix, iy, iz };
                    }

                    // 'max' is not defined for colours, so vector fusion supports
                    // last(0)/first(1)/average(2) only; anything else averages.
                    if (overlap == 1 && cellCount[key] > 0)
                    {
                        cellCount[key]++;           // first wins: ignore later
                        continue;
                    }
                    if (overlap == 0)
                    {
                        cellR[key] = vals[i].X; cellG[key] = vals[i].Y; cellB[key] = vals[i].Z;
                        cellCount[key]++;
                        continue;
                    }
                    // average
                    cellR[key] += vals[i].X; cellG[key] += vals[i].Y; cellB[key] += vals[i].Z;
                    cellCount[key]++;
                }
            }

            var outGrid = new VGrid("fused", new float[] { 0, 0, 0 });
            SetUniformTransform(outGrid, v);

            foreach (var kv in cellCount)
            {
                long key = kv.Key;
                int[] ijk = cellIjk[key];
                int c = kv.Value;

                float r, g, b;
                if (overlap == 2 && c > 0)
                {
                    r = (float)(cellR[key] / c); g = (float)(cellG[key] / c); b = (float)(cellB[key] / c);
                }
                else
                {
                    r = (float)cellR[key]; g = (float)cellG[key]; b = (float)cellB[key];
                }

                try { outGrid.SetValue(ijk, new float[] { r, g, b }); }
                catch { /* skip a cell that fails to write rather than abort */ }
            }

            info.Add(string.Format("Fused {0} vector grids into {1:N0} cells at voxel size {2}.",
                grids.Count, cellCount.Count, v));
            DA.SetData(0, new GH_Grid(outGrid));
            DA.SetData(1, cellCount.Count);
            DA.SetDataList(2, info);
        }

        private static void Accumulate(Dictionary<long, double> val, Dictionary<long, int> count,
            Dictionary<long, int[]> ijk, long key, double v, int overlap, int ix, int iy, int iz)
        {
            if (!count.ContainsKey(key))
            {
                count[key] = 0;
                val[key] = (overlap == 3) ? double.MinValue : 0.0;
                ijk[key] = new int[] { ix, iy, iz };
            }

            switch (overlap)
            {
                case 0: val[key] = v; break;                       // last wins
                case 1: if (count[key] == 0) val[key] = v; break;  // first wins
                case 3: if (v > val[key]) val[key] = v; break;     // max
                default: val[key] += v; break;                     // average (sum now, divide later)
            }
            count[key]++;
        }

        private static void WriteFloatCells(FGrid grid, Dictionary<long, double> val,
            Dictionary<long, int> count, Dictionary<long, int[]> ijk, int overlap)
        {
            foreach (var kv in val)
            {
                long key = kv.Key;
                double value = kv.Value;
                if (overlap == 2 && count[key] > 0) value = value / count[key];   // average

                try { grid.SetValue(ijk[key], (float)value); }
                catch { /* skip */ }
            }
        }

        // Build an axis-aligned uniform-scale transform (voxel size v, origin 0)
        // as a 16-float row-major matrix.
        private static void SetUniformTransform(GridApi grid, double v)
        {
            float s = (float)v;
            float[] m = new float[16]
            {
                s, 0, 0, 0,
                0, s, 0, 0,
                0, 0, s, 0,
                0, 0, 0, 1
            };
            try { grid.Transform = m; } catch { /* leave default */ }
        }
    }
}
