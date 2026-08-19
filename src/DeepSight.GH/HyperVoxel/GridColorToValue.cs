using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using VGrid = DeepSight.Vec3fGrid;
using FGrid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Reads each voxel's colour (a vector grid) as a scalar value, by finding
    /// where that colour falls along a supplied gradient and mapping it into a
    /// numeric domain.
    /// </summary>
    /// <remarks>
    /// This is the inverse of colouring voxels by value. It only makes sense
    /// when the colours were produced by a gradient in the first place - a
    /// heatmap, a false-colour scalar field, etc. For arbitrary colours (e.g. a
    /// raw 3D scan of a real object), the "nearest position on the gradient" is
    /// an approximation with no real meaning, and the component says so.
    ///
    /// It outputs a NEW float grid with the same active voxels and transform, so
    /// the result drops straight into GridBoxes, GridSlice, GridMesh, etc.
    ///
    /// The gradient is sampled into a lookup table once, then each voxel colour
    /// is matched to the nearest table entry. This keeps a million-voxel grid to
    /// a million cheap comparisons rather than a full gradient evaluation each.
    /// </remarks>
    public class Cmpt_GridColorToValue : GH_Component
    {
        public Cmpt_GridColorToValue()
          : base("HyperVoxelGridColorToValue", "GC2V",
              "Convert a colour (vector) grid to a scalar float grid, by mapping each voxel's " +
              "colour onto a supplied gradient and domain.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ColortoValue; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("106cb6a4-ff9a-479d-bd78-48641572dcc8"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector (colour) grid to convert.", GH_ParamAccess.item);
            pManager.AddColourParameter("Gradient", "Gr",
                "Colours defining the gradient, in order from domain start to end. " +
                "Connect a Gradient component's colour output, or a list of swatches.",
                GH_ParamAccess.list);
            pManager.AddIntervalParameter("Domain", "D",
                "Numeric domain the gradient maps to (start = first colour, end = last colour).",
                GH_ParamAccess.item, new Rhino.Geometry.Interval(0.0, 1.0));
            pManager.AddIntegerParameter("Samples", "S",
                "Resolution of the internal gradient lookup table. Higher = more precise mapping.",
                GH_ParamAccess.item, 256);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Scalar float grid of mapped values.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            var colors = new List<System.Drawing.Color>();
            if (!DA.GetDataList(1, colors) || colors.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Provide at least two gradient colours (start and end).");
                return;
            }

            var domain = new Rhino.Geometry.Interval(0.0, 1.0);
            DA.GetData(2, ref domain);

            int samples = 256;
            DA.GetData(3, ref samples);
            if (samples < 2) samples = 2;

            VGrid vgrid = GH_Grid.ParseStructure(obj) as VGrid;
            if (vgrid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Input must be a vector (colour) grid.");
                return;
            }
            if (!vgrid.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid is invalid or disposed.");
                return;
            }

            // --- Build the gradient lookup table ------------------------------
            // Sample the piecewise-linear gradient between the supplied colours
            // into `samples` RGB points, each with its parameter t in [0,1].
            var lutR = new double[samples];
            var lutG = new double[samples];
            var lutB = new double[samples];

            for (int s = 0; s < samples; s++)
            {
                double t = (double)s / (samples - 1);
                System.Drawing.Color c = SampleGradient(colors, t);
                lutR[s] = c.R / 255.0;
                lutG[s] = c.G / 255.0;
                lutB[s] = c.B / 255.0;
            }

            // --- Read the colour grid -----------------------------------------
            int[] active;
            Vec3<float>[] values;
            try
            {
                active = vgrid.GetActiveVoxels();
                values = vgrid.GetValuesIndex(active);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read the grid: " + e.Message);
                return;
            }

            int n = active.Length / 3;
            if (n == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Grid has no active voxels.");
                return;
            }

            // --- Map each voxel colour to nearest gradient position -----------
            var outGrid = new FGrid("scalar", 0.0f);
            try { outGrid.Transform = vgrid.Transform; }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not copy transform: " + e.Message);
            }

            var outVals = new float[n];
            double maxDistSum = 0.0;

            for (int i = 0; i < n; i++)
            {
                double r = values[i].X;
                double g = values[i].Y;
                double b = values[i].Z;

                int best = 0;
                double bestDist = double.MaxValue;

                for (int s = 0; s < samples; s++)
                {
                    double dr = r - lutR[s];
                    double dg = g - lutG[s];
                    double db = b - lutB[s];
                    double dist = dr * dr + dg * dg + db * db;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        best = s;
                    }
                }

                maxDistSum += Math.Sqrt(bestDist);

                double t = (double)best / (samples - 1);
                outVals[i] = (float)domain.ParameterAt(t);
            }

            try
            {
                outGrid.SetValues(active, outVals);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not write output grid: " + e.Message);
                return;
            }

            // If colours sit far off the gradient on average, the mapping is a
            // guess - warn so the user knows the numbers may be meaningless.
            double avgDist = maxDistSum / n;
            info.Add(string.Format("Mapped {0:N0} voxels. Average colour distance from gradient: {1:F3}.", n, avgDist));
            if (avgDist > 0.15)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    "Colours lie well off the supplied gradient on average, so these values are " +
                    "approximate. This is expected for arbitrary (non-gradient-encoded) colours.");
            }

            DA.SetData(0, new GH_Grid(outGrid));
            DA.SetDataList(1, info);
        }

        /// <summary>
        /// Piecewise-linear interpolation across the supplied colours at t in [0,1].
        /// </summary>
        private static System.Drawing.Color SampleGradient(List<System.Drawing.Color> colors, double t)
        {
            if (t <= 0.0) return colors[0];
            if (t >= 1.0) return colors[colors.Count - 1];

            double scaled = t * (colors.Count - 1);
            int i0 = (int)Math.Floor(scaled);
            int i1 = Math.Min(i0 + 1, colors.Count - 1);
            double f = scaled - i0;

            var a = colors[i0];
            var b = colors[i1];

            int r = (int)Math.Round(a.R + (b.R - a.R) * f);
            int g = (int)Math.Round(a.G + (b.G - a.G) * f);
            int bl = (int)Math.Round(a.B + (b.B - a.B) * f);

            return System.Drawing.Color.FromArgb(
                Clamp(r), Clamp(g), Clamp(bl));
        }

        private static int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }
    }
}
