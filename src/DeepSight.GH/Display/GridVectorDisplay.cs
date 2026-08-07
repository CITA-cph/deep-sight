using System;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;
using Grasshopper.Kernel;

using DeepSight.RhinoCommon;

using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Displays each active voxel of a vector grid as a line/arrow showing the
    /// vector's direction and magnitude.
    /// </summary>
    /// <remarks>
    /// MEANING DEPENDS ON WHAT THE VECTOR IS. This is genuinely useful when the
    /// vector is a physical direction - a gradient, a flow or displacement field,
    /// a surface normal. It is NOT meaningful for a colour grid: there the vector
    /// is RGB, so the "direction" points in colour space, not in the model. If
    /// you feed it a colour grid you will get arrows, but they mean nothing
    /// physical - use GridBoxes for colour instead. The component cannot tell the
    /// two apart, so it just draws what it is given.
    ///
    /// Outputs lines (as curves) plus their end points, so you can pipe them into
    /// your own arrow/preview logic. Length is the vector magnitude times a
    /// scale, in world units.
    /// </remarks>
    public class Cmpt_GridVectorDisplay : GH_Component
    {
        public Cmpt_GridVectorDisplay()
          : base("GridVectorDisplay", "GVec",
              "Show a vector grid's voxels as direction lines. Meaningful for direction/flow " +
              "fields; not for colour grids (use GridBoxes for those).",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }
        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.GridDisplayVector; } }
        public override Guid ComponentGuid { get { return new Guid("42982404-41d8-4723-9619-b5165b083d49"); } }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid to display.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Length multiplier for the vectors, in world units.",
                GH_ParamAccess.item, 1.0);
            pManager[1].Optional = true;
            pManager.AddIntegerParameter("Step", "St", "Show every Nth voxel per axis, to thin a dense field.",
                GH_ParamAccess.item, 1);
            pManager[2].Optional = true;
            pManager.AddBooleanParameter("Normalize", "N",
                "If true, draw all vectors the same length (direction only). If false, length shows magnitude.",
                GH_ParamAccess.item, false);
            pManager[3].Optional = true;
            pManager.AddIntegerParameter("MaxVectors", "M", "Safety limit on how many lines to draw.",
                GH_ParamAccess.item, 100000);
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "L", "One line per voxel, from voxel centre along its vector.", GH_ParamAccess.list);
            pManager.AddPointParameter("Anchors", "A", "Voxel centre points (line starts).", GH_ParamAccess.list);
            pManager.AddVectorParameter("Vectors", "V", "The raw vectors, for your own arrow logic.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            VGrid grid = GH_Grid.ParseStructure(obj) as VGrid;
            if (grid == null) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input must be a vector grid."); return; }
            if (!grid.IsValid) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid is invalid or disposed."); return; }

            double scale = 1.0; int step = 1; bool normalize = false; int maxVec = 100000;
            DA.GetData(1, ref scale);
            DA.GetData(2, ref step);
            DA.GetData(3, ref normalize);
            DA.GetData(4, ref maxVec);
            if (step < 1) step = 1;
            if (maxVec < 1) maxVec = 1;

            int[] active;
            Vec3<float>[] values;
            try
            {
                active = grid.GetActiveVoxels();
                values = grid.GetValuesIndex(active);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read the grid: " + e.Message);
                return;
            }

            var xform = grid.Transform.ToRhinoTransform();
            int n = active.Length / 3;

            var lines = new List<Line>();
            var anchors = new List<Point3d>();
            var vectors = new List<Vector3d>();

            for (int i = 0; i < n; i++)
            {
                int x = active[i * 3], y = active[i * 3 + 1], z = active[i * 3 + 2];
                if (step > 1 && (Mod(x, step) != 0 || Mod(y, step) != 0 || Mod(z, step) != 0)) continue;

                var centre = new Point3d(x, y, z);
                centre.Transform(xform);

                var v = new Vector3d(values[i].X, values[i].Y, values[i].Z);
                if (normalize && v.Length > 1e-9) v.Unitize();
                v *= scale;

                lines.Add(new Line(centre, centre + v));
                anchors.Add(centre);
                vectors.Add(v);

                if (lines.Count >= maxVec)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format(
                        "Hit the MaxVectors limit of {0:N0}. Raise Step or MaxVectors to see more.", maxVec));
                    break;
                }
            }

            DA.SetDataList(0, lines);
            DA.SetDataList(1, anchors);
            DA.SetDataList(2, vectors);
        }

        private static int Mod(int a, int m) { return ((a % m) + m) % m; }
    }
}
