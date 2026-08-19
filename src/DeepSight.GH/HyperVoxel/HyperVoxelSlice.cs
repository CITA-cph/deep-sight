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
    using Mesh = Rhino.Geometry.Mesh;

    /// <summary>
    /// Slices one layer of a HyperVoxel. Outputs three things:
    ///   - the flat cross-section sheet at the cut (as before),
    ///   - a solid box mesh of all that layer's voxels on ONE side of the cut,
    ///   - the bounding-box-sized outline of the cut plane.
    /// The side shown is reversible with a toggle.
    /// </summary>
    /// <remarks>
    /// The flat sheet is unchanged from the original slice. The "clipped solid"
    /// is new: it is the layer's voxels drawn as coloured boxes, keeping only
    /// those on the chosen side of the plane - i.e. a clipping-plane view of the
    /// cut-open volume. Run three of these at different layers/positions to
    /// reveal three layers' interiors at three depths.
    ///
    /// The clipped solid does NOT cull hidden faces (unlike GridBoxes): the cut
    /// exposes interior faces that must be drawn, and vector layers need every
    /// face present to colour it. MaxVoxels still guards runaway meshes.
    /// </remarks>
    public class Cmpt_HyperVoxelSlice : GH_Component
    {
        // The plane is drawn directly in the viewport as a semi-transparent
        // surface rather than emitted as a coloured mesh. A single flat surface
        // renders transparency reliably in Rhino (unlike per-voxel box alpha),
        // so this actually shows through instead of reading as solid black.
        private Mesh m_planeMesh = null;
        private double m_planeTransparency = 0.8;

        public Cmpt_HyperVoxelSlice()
          : base("HyperVoxelSlice", "HVSlc",
              "Slice one layer of a HyperVoxel: flat cross-section plus a clipped solid of the " +
              "voxels on one side of the cut. Reversible.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Hypervoxel_slice; } }
        public override Guid ComponentGuid { get { return new Guid("3f21b810-8f4f-4114-acdb-7d7ae29609b2"); } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The stack to slice.", GH_ParamAccess.item);
            pManager.AddTextParameter("Layer", "L", "Name of the layer to slice (empty = use Index).", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddIntegerParameter("Index", "I", "Index of the layer, if no name given.", GH_ParamAccess.item, 0);
            pManager[2].Optional = true;
            pManager.AddIntegerParameter("Axis", "A", "Slice plane: 0 = XY, 1 = YZ, 2 = XZ.", GH_ParamAccess.item, 0);
            pManager[3].Optional = true;
            pManager.AddNumberParameter("Position", "P", "Cut position, 0..1 across the bounding box.", GH_ParamAccess.item, 0.5);
            pManager[4].Optional = true;
            pManager.AddBooleanParameter("FlipSide", "F",
                "Which side of the cut to show as the clipped solid. Toggle to reverse.",
                GH_ParamAccess.item, false);
            pManager[5].Optional = true;
            pManager.AddNumberParameter("BoxScale", "Sc", "Box size for the clipped solid (fraction of a voxel).",
                GH_ParamAccess.item, 1.0);
            pManager[6].Optional = true;
            pManager.AddIntegerParameter("MaxVoxels", "M", "Safety limit for the clipped solid.", GH_ParamAccess.item, 400000);
            pManager[7].Optional = true;
            pManager.AddNumberParameter("PlaneTransparency", "Pt",
                "Transparency of the drawn cut plane, 0 = opaque, 1 = invisible.",
                GH_ParamAccess.item, 0.8);
            pManager[8].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Named for what each thing IS, not what it looks like. The
            // cross-section is a flat MESH (it reads as a plane on screen, which
            // is why calling it "Sheet" next to a "Plane" curve was confusing),
            // and the cut plane comes out as both an outline curve and a surface.
            pManager.AddMeshParameter("CrossSection", "X",
                "Flat mesh of the layer's values sampled at the cut. Coloured by the layer data.",
                GH_ParamAccess.item);
            pManager.AddMeshParameter("ClippedSolid", "C",
                "Box mesh of the layer's voxels on the chosen side of the cut.",
                GH_ParamAccess.item);
            pManager.AddSurfaceParameter("CutPlane", "P",
                "The cut plane as a surface. Also drawn semi-transparent in the viewport.",
                GH_ParamAccess.item);
            pManager.AddCurveParameter("CutOutline", "O",
                "Outline curve of the cut plane, sized to the layer's bounding box.",
                GH_ParamAccess.item);
            pManager.AddNumberParameter("Values", "V",
                "Cross-section values per vertex (float layers only).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            HyperVoxel hyper = GH_HyperVoxel.ParseStructure(obj);
            if (hyper == null) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a HyperVoxel."); return; }
            if (hyper.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "HyperVoxel has no layers."); return; }

            string layerName = null;
            int index = 0, axis = 0, maxVoxels = 400000;
            double position = 0.5, boxScale = 1.0;
            bool flip = false;
            bool hasName = DA.GetData(1, ref layerName) && !string.IsNullOrEmpty(layerName);
            DA.GetData(2, ref index);
            DA.GetData(3, ref axis);
            DA.GetData(4, ref position);
            DA.GetData(5, ref flip);
            DA.GetData(6, ref boxScale);
            DA.GetData(7, ref maxVoxels);
            double planeTransparency = 0.8;
            DA.GetData(8, ref planeTransparency);
            m_planeTransparency = Math.Max(0.0, Math.Min(1.0, planeTransparency));
            position = Math.Max(0.0, Math.Min(1.0, position));
            if (boxScale <= 0.0 || boxScale > 1.0) boxScale = 1.0;
            if (maxVoxels < 1) maxVoxels = 1;

            HyperVoxelLayer layer = hasName ? hyper.FindLayer(layerName)
                                            : (index >= 0 && index < hyper.Count ? hyper[index] : null);
            if (layer == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, hasName
                    ? string.Format("No layer named '{0}'. Available: {1}.", layerName, string.Join(", ", hyper.LayerNames))
                    : string.Format("Index {0} out of range (0..{1}).", index, hyper.Count - 1));
                return;
            }

            GridApi grid = layer.Grid;
            if (grid == null || !grid.IsValid) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Layer grid is invalid."); return; }

            int[] min, max;
            grid.BoundingBox(out min, out max);
            var xform = grid.Transform.ToRhinoTransform();

            int[] axes;
            switch (axis)
            {
                case 1: axes = new int[] { 1, 2, 0 }; break; // YZ
                case 2: axes = new int[] { 0, 2, 1 }; break; // XZ
                default: axes = new int[] { 0, 1, 2 }; break; // XY
            }

            int lo = min[axes[2]], hi = max[axes[2]];
            int cut = lo + (int)Math.Round(position * (hi - lo));

            var fgrid = grid as FGrid;
            var vgrid = grid as VGrid;

            // ---- 1. FLAT SHEET (unchanged behaviour) -------------------------
            var sheet = new Mesh();
            var ijk = new int[3];
            var outputValues = new List<double>();
            int stride = max[axes[1]] - min[axes[1]] + 1;
            int xrange = max[axes[0]] - min[axes[0]];
            int yrange = max[axes[1]] - min[axes[1]];

            for (int x = min[axes[0]]; x <= max[axes[0]]; ++x)
                for (int y = min[axes[1]]; y <= max[axes[1]]; ++y)
                {
                    ijk[axes[0]] = x; ijk[axes[1]] = y; ijk[axes[2]] = cut;
                    sheet.Vertices.Add((float)ijk[0], (float)ijk[1], (float)ijk[2]);

                    if (vgrid != null)
                    {
                        Vec3<float> c = vgrid[ijk[0], ijk[1], ijk[2]];
                        sheet.VertexColors.Add(C(c.X), C(c.Y), C(c.Z));
                    }
                    else if (fgrid != null)
                    {
                        double val = fgrid[ijk[0], ijk[1], ijk[2]];
                        outputValues.Add(val);
                        int g = C(val);
                        sheet.VertexColors.Add(g, g, g);
                    }
                }

            int row = 0, rowp = stride;
            for (int x = 0; x < xrange; ++x)
            {
                for (int y = 0; y < yrange; ++y)
                    sheet.Faces.AddFace(row + y, row + y + 1, rowp + y + 1, rowp + y);
                row += stride; rowp += stride;
            }
            sheet.Transform(xform);

            // ---- 2. CLIPPED SOLID (new) --------------------------------------
            // All active voxels of this layer whose normal-axis coordinate is on
            // the chosen side of the cut, drawn as coloured boxes.
            var solid = new Mesh();
            int normalAxis = axes[2];
            int drawn = 0;

            int[] active = null;
            Vec3<float>[] vcols = null;
            float[] fvals = null;
            try
            {
                if (vgrid != null) { active = vgrid.GetActiveVoxels(); vcols = vgrid.GetValuesIndex(active); }
                else if (fgrid != null) { active = fgrid.GetActiveVoxels(); fvals = fgrid.GetValuesIndex(active); }
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not read voxels for the solid: " + e.Message);
            }

            if (active != null)
            {
                int n = active.Length / 3;
                double h = 0.5 * boxScale;

                for (int i = 0; i < n; i++)
                {
                    int cx = active[i * 3], cy = active[i * 3 + 1], cz = active[i * 3 + 2];
                    int coordOnNormal = (normalAxis == 0) ? cx : (normalAxis == 1 ? cy : cz);

                    // Keep one side of the cut; FlipSide swaps which.
                    bool keep = flip ? (coordOnNormal > cut) : (coordOnNormal <= cut);
                    if (!keep) continue;

                    System.Drawing.Color col = (vgrid != null)
                        ? System.Drawing.Color.FromArgb(C(vcols[i].X), C(vcols[i].Y), C(vcols[i].Z))
                        : Grey(fvals[i]);

                    AddBox(solid, cx, cy, cz, h, col);
                    drawn++;

                    if (drawn >= maxVoxels)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                            "Clipped solid hit the MaxVoxels limit of {0:N0}. Raise it or thin the data.", maxVoxels));
                        break;
                    }
                }

                if (solid.Vertices.Count > 0)
                {
                    solid.Transform(xform);
                    solid.Normals.ComputeNormals();
                    solid.Compact();
                }
            }

            // ---- 3. PLANE OUTLINE --------------------------------------------
            var corners = new[]
            {
                MakeIjk(axes, min[axes[0]], min[axes[1]], cut),
                MakeIjk(axes, max[axes[0]], min[axes[1]], cut),
                MakeIjk(axes, max[axes[0]], max[axes[1]], cut),
                MakeIjk(axes, min[axes[0]], max[axes[1]], cut),
            };
            var pts = corners.Select(c => { var p = new Point3d(c[0], c[1], c[2]); p.Transform(xform); return p; }).ToArray();
            var poly = new Polyline(new[] { pts[0], pts[1], pts[2], pts[3], pts[0] });

            // Build a quad surface for the plane, both for the viewport draw and
            // the surface output.
            var planeMesh = new Mesh();
            planeMesh.Vertices.Add(pts[0]);
            planeMesh.Vertices.Add(pts[1]);
            planeMesh.Vertices.Add(pts[2]);
            planeMesh.Vertices.Add(pts[3]);
            planeMesh.Faces.AddFace(0, 1, 2, 3);
            planeMesh.Normals.ComputeNormals();
            m_planeMesh = planeMesh;

            Surface planeSrf = null;
            try { planeSrf = NurbsSurface.CreateFromCorners(pts[0], pts[1], pts[2], pts[3]); }
            catch { /* degenerate plane; leave null */ }

            DA.SetData(0, sheet);                  // CrossSection (mesh)
            DA.SetData(1, solid);                  // ClippedSolid (mesh)
            DA.SetData(2, planeSrf);               // CutPlane (surface)
            DA.SetData(3, poly.ToNurbsCurve());    // CutOutline (curve)
            DA.SetDataList(4, outputValues);
        }

        // Draw the cut plane as a semi-transparent grey surface. This is the
        // reliable route for transparency in Rhino: a flat shaded mesh with a
        // DisplayMaterial whose transparency is set. The value output still
        // carries the surface for anyone who wants to use it downstream.
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (m_planeMesh == null) return;
            var mat = new Rhino.Display.DisplayMaterial(System.Drawing.Color.Gray, m_planeTransparency);
            args.Display.DrawMeshShaded(m_planeMesh, mat);
        }

        public override BoundingBox ClippingBox
        {
            get { return m_planeMesh != null ? m_planeMesh.GetBoundingBox(false) : BoundingBox.Empty; }
        }

        private static void AddBox(Mesh mesh, int x, int y, int z, double h, System.Drawing.Color col)
        {
            for (int d = 0; d < 6; d++)
            {
                int b = mesh.Vertices.Count;
                AddFace(mesh, x, y, z, h, d);
                for (int v = 0; v < 4; v++) mesh.VertexColors.Add(col);
            }
        }

        private static void AddFace(Mesh mesh, int x, int y, int z, double h, int d)
        {
            double cx = x, cy = y, cz = z;
            int b = mesh.Vertices.Count;
            switch (d)
            {
                case 0:
                    mesh.Vertices.Add(cx + h, cy - h, cz - h); mesh.Vertices.Add(cx + h, cy + h, cz - h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h); mesh.Vertices.Add(cx + h, cy - h, cz + h); break;
                case 1:
                    mesh.Vertices.Add(cx - h, cy - h, cz - h); mesh.Vertices.Add(cx - h, cy - h, cz + h);
                    mesh.Vertices.Add(cx - h, cy + h, cz + h); mesh.Vertices.Add(cx - h, cy + h, cz - h); break;
                case 2:
                    mesh.Vertices.Add(cx - h, cy + h, cz - h); mesh.Vertices.Add(cx - h, cy + h, cz + h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h); mesh.Vertices.Add(cx + h, cy + h, cz - h); break;
                case 3:
                    mesh.Vertices.Add(cx - h, cy - h, cz - h); mesh.Vertices.Add(cx + h, cy - h, cz - h);
                    mesh.Vertices.Add(cx + h, cy - h, cz + h); mesh.Vertices.Add(cx - h, cy - h, cz + h); break;
                case 4:
                    mesh.Vertices.Add(cx - h, cy - h, cz + h); mesh.Vertices.Add(cx + h, cy - h, cz + h);
                    mesh.Vertices.Add(cx + h, cy + h, cz + h); mesh.Vertices.Add(cx - h, cy + h, cz + h); break;
                default:
                    mesh.Vertices.Add(cx - h, cy - h, cz - h); mesh.Vertices.Add(cx - h, cy + h, cz - h);
                    mesh.Vertices.Add(cx + h, cy + h, cz - h); mesh.Vertices.Add(cx + h, cy - h, cz - h); break;
            }
            mesh.Faces.AddFace(b, b + 1, b + 2, b + 3);
        }

        private static int[] MakeIjk(int[] axes, int a0, int a1, int a2)
        {
            var ijk = new int[3];
            ijk[axes[0]] = a0; ijk[axes[1]] = a1; ijk[axes[2]] = a2;
            return ijk;
        }

        private static int C(double v01) { int v = (int)Math.Round(v01 * 255); return v < 0 ? 0 : (v > 255 ? 255 : v); }
        private static System.Drawing.Color Grey(double v01) { int g = C(v01); return System.Drawing.Color.FromArgb(g, g, g); }
    }
}
