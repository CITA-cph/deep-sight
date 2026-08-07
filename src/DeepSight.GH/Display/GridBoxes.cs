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
    using Mesh = Rhino.Geometry.Mesh;   // must be inside the namespace to win
                                        // name resolution over DeepSight.Mesh

    /// <summary>
    /// Previews a grid's active voxels as boxes. Float grids are drawn plain
    /// (optionally thresholded by value); vector grids are drawn coloured, with
    /// each voxel's vector read as an RGB colour - which is exactly what
    /// GridFromPointCloud stores for a coloured point cloud.
    /// </summary>
    /// <remarks>
    /// PERFORMANCE: builds only the visible surface for float grids - a face is
    /// emitted only where an active voxel borders an inactive one. On a solid
    /// 100^3 region that is ~120k triangles instead of ~12M.
    ///
    /// Vector (coloured) grids skip surface culling. Per-face vertex colouring
    /// needs the faces present to colour them, and a coloured point cloud is
    /// usually sparse, so there is little interior to cull anyway. The MaxVoxels
    /// guard still applies.
    /// </remarks>
    public class Cmpt_GridBoxes : GH_Component
    {
        public Cmpt_GridBoxes()
          : base("GridBoxes", "GBox",
              "Preview active voxels as boxes. Float grids draw plain; vector grids draw " +
              "coloured, reading each voxel's vector as RGB (e.g. a coloured point cloud).",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.GridBox; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("331b5289-8c3c-45ab-ad5c-05d5c2881f67"); }
        }

        private const int COORD_OFFSET = 1 << 20;
        private const int COORD_LIMIT = 1 << 21;

        private static long PackCoord(int x, int y, int z)
        {
            long px = (long)(x + COORD_OFFSET);
            long py = (long)(y + COORD_OFFSET);
            long pz = (long)(z + COORD_OFFSET);
            return (px << 42) | (py << 21) | pz;
        }

        private static bool CoordInRange(int x, int y, int z)
        {
            return (x + COORD_OFFSET) >= 0 && (x + COORD_OFFSET) < COORD_LIMIT
                && (y + COORD_OFFSET) >= 0 && (y + COORD_OFFSET) < COORD_LIMIT
                && (z + COORD_OFFSET) >= 0 && (z + COORD_OFFSET) < COORD_LIMIT;
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Float or vector grid to preview.", GH_ParamAccess.item);

            pManager.AddNumberParameter("Threshold", "T",
                "Float grids only: show voxels whose value is >= this.",
                GH_ParamAccess.item, 0.0);
            pManager[1].Optional = true;

            pManager.AddIntegerParameter("Step", "S",
                "Show every Nth voxel along each axis. Raise to preview a huge grid cheaply.",
                GH_ParamAccess.item, 1);
            pManager[2].Optional = true;

            pManager.AddNumberParameter("Scale", "Sc",
                "Box size as a fraction of the voxel (1.0 = touching, 0.8 = gaps). Below 1 disables culling.",
                GH_ParamAccess.item, 1.0);
            pManager[3].Optional = true;

            pManager.AddIntegerParameter("MaxVoxels", "M",
                "Safety limit. If more voxels pass the filters, the component stops instead of hanging Rhino.",
                GH_ParamAccess.item, 400000);
            pManager[4].Optional = true;

            pManager.AddBooleanParameter("Alpha", "A",
                "Vector grids only: use luminance as transparency (white = opaque, black = clear).",
                GH_ParamAccess.item, false);
            pManager[5].Optional = true;

            pManager.AddBooleanParameter("InvertAlpha", "iA",
                "Reverse the transparency (black = opaque, white = clear).",
                GH_ParamAccess.item, false);
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Box mesh of the active voxels (coloured for vector grids).", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "C", "Number of voxels drawn.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            GridApi baseGrid = GH_Grid.ParseStructure(obj);
            if (baseGrid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a grid.");
                return;
            }
            if (!baseGrid.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid is invalid or has been disposed.");
                return;
            }

            double threshold = 0.0;
            int step = 1;
            double scale = 1.0;
            int maxVoxels = 400000;

            DA.GetData(1, ref threshold);
            DA.GetData(2, ref step);
            DA.GetData(3, ref scale);
            DA.GetData(4, ref maxVoxels);
            bool useAlpha = false;
            bool invertAlpha = false;
            DA.GetData(5, ref useAlpha);
            DA.GetData(6, ref invertAlpha);

            if (step < 1) step = 1;
            if (scale <= 0.0 || scale > 1.0) scale = 1.0;
            if (maxVoxels < 1) maxVoxels = 1;

            var vgrid = baseGrid as VGrid;
            if (vgrid != null)
            {
                SolveVector(DA, vgrid, step, scale, maxVoxels, useAlpha, invertAlpha, info);
                return;
            }

            var fgrid = baseGrid as FGrid;
            if (fgrid != null)
            {
                SolveFloat(DA, fgrid, threshold, step, scale, maxVoxels, info);
                return;
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                "Only float and vector grids are supported by this component.");
        }

        private void SolveFloat(IGH_DataAccess DA, FGrid grid, double threshold,
            int step, double scale, int maxVoxels, List<string> info)
        {
            int[] active;
            try { active = grid.GetActiveVoxels(); }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read active voxels: " + e.Message);
                return;
            }

            int totalActive = active.Length / 3;
            if (totalActive == 0) { Finish(DA, null, 0, info, "Grid has no active voxels."); return; }
            info.Add(string.Format("Float grid: {0:N0} active voxels.", totalActive));

            float[] values = null;
            bool needValues = threshold > 0.0;
            if (needValues)
            {
                try { values = grid.GetValuesIndex(active); }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Ignoring threshold: " + e.Message);
                    needValues = false;
                }
            }

            var kept = new List<int>();
            for (int i = 0; i < totalActive; i++)
            {
                int x = active[i * 3], y = active[i * 3 + 1], z = active[i * 3 + 2];
                if (step > 1 && (Mod(x, step) != 0 || Mod(y, step) != 0 || Mod(z, step) != 0)) continue;
                if (needValues && values != null && values[i] < threshold) continue;
                if (!CoordInRange(x, y, z)) continue;
                kept.Add(x); kept.Add(y); kept.Add(z);
            }

            int keptCount = kept.Count / 3;
            if (keptCount == 0) { Finish(DA, null, 0, info, "No voxels passed the filters."); return; }
            if (OverLimit(keptCount, maxVoxels, info, DA)) return;
            info.Add(string.Format("{0:N0} voxels after filtering.", keptCount));

            bool cull = (scale >= 1.0) && (step == 1);
            if (!cull) info.Add("Culling off (scale below 1 or step above 1).");

            HashSet<long> occupied = null;
            if (cull)
            {
                occupied = new HashSet<long>();
                for (int i = 0; i < keptCount; i++)
                    occupied.Add(PackCoord(kept[i * 3], kept[i * 3 + 1], kept[i * 3 + 2]));
            }

            var mesh = new Mesh();
            double h = 0.5 * scale;
            int faceCount = 0;

            for (int i = 0; i < keptCount; i++)
            {
                int x = kept[i * 3], y = kept[i * 3 + 1], z = kept[i * 3 + 2];
                for (int d = 0; d < 6; d++)
                {
                    if (cull && FaceHidden(occupied, x, y, z, d)) continue;
                    AddFace(mesh, x, y, z, h, d);
                    faceCount++;
                }
            }

            info.Add(string.Format("{0:N0} faces ({1}).", faceCount, cull ? "surface only" : "all"));
            FinishMesh(DA, mesh, grid.Transform, keptCount, info);
        }

        private void SolveVector(IGH_DataAccess DA, VGrid grid,
                    int step, double scale, int maxVoxels, bool useAlpha, bool invertAlpha, List<string> info)
        {
            int[] active;
            Vec3<float>[] values;
            try
            {
                active = grid.GetActiveVoxels();
                values = grid.GetValuesIndex(active);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read the vector grid: " + e.Message);
                return;
            }

            int totalActive = active.Length / 3;
            if (totalActive == 0) { Finish(DA, null, 0, info, "Grid has no active voxels."); return; }
            info.Add(string.Format("Vector grid: {0:N0} active voxels, coloured by vector as RGB.", totalActive));

            var keptXyz = new List<int>();
            var keptCol = new List<System.Drawing.Color>();

            for (int i = 0; i < totalActive; i++)
            {
                int x = active[i * 3], y = active[i * 3 + 1], z = active[i * 3 + 2];
                if (step > 1 && (Mod(x, step) != 0 || Mod(y, step) != 0 || Mod(z, step) != 0)) continue;
                if (!CoordInRange(x, y, z)) continue;

                keptXyz.Add(x); keptXyz.Add(y); keptXyz.Add(z);
                keptCol.Add(VecToColor(values[i], useAlpha, invertAlpha));
            }

            int keptCount = keptCol.Count;
            if (keptCount == 0) { Finish(DA, null, 0, info, "No voxels passed the step filter."); return; }
            if (OverLimit(keptCount, maxVoxels, info, DA)) return;

            var mesh = new Mesh();
            double h = 0.5 * scale;

            for (int i = 0; i < keptCount; i++)
            {
                int x = keptXyz[i * 3], y = keptXyz[i * 3 + 1], z = keptXyz[i * 3 + 2];
                System.Drawing.Color c = keptCol[i];
                for (int d = 0; d < 6; d++)
                {
                    AddFace(mesh, x, y, z, h, d);
                    for (int v = 0; v < 4; v++)
                        mesh.VertexColors.Add(c);
                }
            }

            info.Add(string.Format("{0:N0} coloured boxes.", keptCount));
            FinishMesh(DA, mesh, grid.Transform, keptCount, info);
        }

        private static int Mod(int a, int m)
        {
            return ((a % m) + m) % m;
        }

        private static System.Drawing.Color VecToColor(Vec3<float> v, bool useAlpha, bool invertAlpha)
        {
            int r = Clamp255((int)Math.Round(v.X * 255.0));
            int g = Clamp255((int)Math.Round(v.Y * 255.0));
            int b = Clamp255((int)Math.Round(v.Z * 255.0));

            if (!useAlpha)
                return System.Drawing.Color.FromArgb(r, g, b);

            // Perceptual luminance -> alpha. White (bright) = opaque, black = clear.
            double lum = (0.299 * r + 0.587 * g + 0.114 * b) / 255.0;
            if (invertAlpha) lum = 1.0 - lum;
            int a = Clamp255((int)Math.Round(lum * 255.0));

            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        private static int Clamp255(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        private bool OverLimit(int keptCount, int maxVoxels, List<string> info, IGH_DataAccess DA)
        {
            if (keptCount <= maxVoxels) return false;
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                "{0:N0} voxels passed the filters, over the MaxVoxels limit of {1:N0}. " +
                "Raise Step or MaxVoxels. Stopping rather than freezing Rhino.", keptCount, maxVoxels));
            DA.SetData(1, keptCount);
            DA.SetDataList(2, info);
            return true;
        }

        private static bool FaceHidden(HashSet<long> occupied, int x, int y, int z, int d)
        {
            int nx = x, ny = y, nz = z;
            switch (d)
            {
                case 0: nx++; break;
                case 1: nx--; break;
                case 2: ny++; break;
                case 3: ny--; break;
                case 4: nz++; break;
                default: nz--; break;
            }
            return CoordInRange(nx, ny, nz) && occupied.Contains(PackCoord(nx, ny, nz));
        }

        private void Finish(IGH_DataAccess DA, Mesh mesh, int count, List<string> info, string note)
        {
            if (note != null) info.Add(note);
            if (mesh != null) DA.SetData(0, mesh);
            DA.SetData(1, count);
            DA.SetDataList(2, info);
        }

        private void FinishMesh(IGH_DataAccess DA, Mesh mesh, float[] transform, int count, List<string> info)
        {
            try
            {
                Transform xform = transform.ToRhinoTransform();
                mesh.Transform(xform);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Could not apply grid transform, mesh is in index space: " + e.Message);
            }

            mesh.Normals.ComputeNormals();
            mesh.Compact();

            DA.SetData(0, mesh);
            DA.SetData(1, count);
            DA.SetDataList(2, info);
        }

        private static void AddFace(Mesh mesh, int x, int y, int z, double h, int d)
        {
            double cx = x, cy = y, cz = z;
            int b = mesh.Vertices.Count;

            switch (d)
            {
                case 0: // +X
                    mesh.Vertices.Add(cx + h, cy - h, cz - h);
                    mesh.Vertices.Add(cx + h, cy + h, cz - h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h);
                    mesh.Vertices.Add(cx + h, cy - h, cz + h);
                    break;
                case 1: // -X
                    mesh.Vertices.Add(cx - h, cy - h, cz - h);
                    mesh.Vertices.Add(cx - h, cy - h, cz + h);
                    mesh.Vertices.Add(cx - h, cy + h, cz + h);
                    mesh.Vertices.Add(cx - h, cy + h, cz - h);
                    break;
                case 2: // +Y
                    mesh.Vertices.Add(cx - h, cy + h, cz - h);
                    mesh.Vertices.Add(cx - h, cy + h, cz + h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h);
                    mesh.Vertices.Add(cx + h, cy + h, cz - h);
                    break;
                case 3: // -Y
                    mesh.Vertices.Add(cx - h, cy - h, cz - h);
                    mesh.Vertices.Add(cx + h, cy - h, cz - h);
                    mesh.Vertices.Add(cx + h, cy - h, cz + h);
                    mesh.Vertices.Add(cx - h, cy - h, cz + h);
                    break;
                case 4: // +Z
                    mesh.Vertices.Add(cx - h, cy - h, cz + h);
                    mesh.Vertices.Add(cx + h, cy - h, cz + h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h);
                    mesh.Vertices.Add(cx - h, cy + h, cz + h);
                    break;
                default: // -Z
                    mesh.Vertices.Add(cx - h, cy - h, cz - h);
                    mesh.Vertices.Add(cx - h, cy + h, cz - h);
                    mesh.Vertices.Add(cx + h, cy + h, cz - h);
                    mesh.Vertices.Add(cx + h, cy - h, cz - h);
                    break;
            }

            mesh.Faces.AddFace(b, b + 1, b + 2, b + 3);
        }
    }
}
