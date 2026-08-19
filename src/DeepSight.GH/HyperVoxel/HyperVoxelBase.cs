using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Builds a "base occupancy" grid from a HyperVoxel: a float grid holding no
    /// data, just value 1 at every voxel that is active in ANY layer.
    /// </summary>
    /// <remarks>
    /// This is the raw shape of the stack, independent of colour or scalar data.
    /// It answers "where is there anything at all", which is useful for previewing
    /// structure, masking, or as a reference layer.
    ///
    /// IMPORTANT - this is only meaningful for CO-REGISTERED layers (same voxel
    /// size and origin). If the layers have different transforms, voxel (i,j,k)
    /// is a different physical point in each layer, so unioning their index
    /// coordinates is meaningless. The component checks and warns. To combine
    /// layers that do NOT share a transform, use HyperVoxelFuse first.
    ///
    /// Output is added as a new layer named "base" if AddToStack is true, and is
    /// always emitted on its own output so you can preview it directly.
    /// </remarks>
    public class Cmpt_HyperVoxelBase : GH_Component
    {
        public Cmpt_HyperVoxelBase()
          : base("HyperVoxelBase", "HVBase",
              "Build a base occupancy grid (value 1 wherever any layer is active), " +
              "carrying no data. Optionally add it back into the stack as a 'base' layer.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HypervoxelBase; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b7e4c1a0-3f52-4e88-9d21-6a4f0e8c5b90"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The stack to build a base from.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("AddToStack", "A",
                "If true, also return the HyperVoxel with the base added as a 'base' layer.",
                GH_ParamAccess.item, false);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Base", "B", "The base occupancy grid (float, value 1).", GH_ParamAccess.item);
            pManager.AddGenericParameter("HyperVoxel", "HV",
                "The stack with the base layer added (only if AddToStack is true).", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            bool addToStack = false;
            DA.GetData(1, ref addToStack);

            HyperVoxel hyper = GH_HyperVoxel.ParseStructure(obj);
            if (hyper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a HyperVoxel.");
                return;
            }
            if (hyper.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "HyperVoxel has no layers.");
                return;
            }

            // Only meaningful if layers are co-registered. Reuse the stack's own
            // validation, which reports transform mismatches.
            var problems = hyper.Validate();
            bool mismatch = problems.Any(p => p.Contains("different transform"));
            if (mismatch)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    "Layers do not all share a transform, so a union of their voxel coordinates is " +
                    "not physically meaningful. Use HyperVoxelFuse to bring them onto a common grid first.");
            }

            // Build the base on the reference (layer 0) transform.
            var baseGrid = new FGrid("base", 0.0f);
            try
            {
                baseGrid.Transform = hyper[0].Grid.Transform;
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not copy the reference transform: " + e.Message);
            }

            // Union of active voxels across all layers. A HashSet de-duplicates
            // where layers overlap, so shared voxels are written once.
            var seen = new HashSet<long>();
            var xs = new List<int>();
            var ys = new List<int>();
            var zs = new List<int>();

            foreach (var layer in hyper.Layers)
            {
                if (layer.Grid == null || !layer.Grid.IsValid) continue;

                int[] active;
                try
                {
                    // GetActiveVoxels lives on the concrete grid types
                    // (GridBase<T>), not on the plain GridApi base, so resolve
                    // the real type first.
                    var fg = layer.Grid as FGrid;
                    var vg = layer.Grid as VGrid;
                    if (fg != null) active = fg.GetActiveVoxels();
                    else if (vg != null) active = vg.GetActiveVoxels();
                    else { continue; }   // unsupported grid type; skip
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        string.Format("Skipped layer '{0}': {1}", layer.Name, e.Message));
                    continue;
                }

                int n = active.Length / 3;
                for (int i = 0; i < n; i++)
                {
                    int x = active[i * 3], y = active[i * 3 + 1], z = active[i * 3 + 2];
                    long key = Pack(x, y, z);
                    if (seen.Add(key))
                    {
                        xs.Add(x); ys.Add(y); zs.Add(z);
                    }
                }
            }

            if (xs.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No active voxels found in any layer.");
                return;
            }

            // Write value 1 at every unioned voxel.
            var coords = new int[xs.Count * 3];
            var vals = new float[xs.Count];
            for (int i = 0; i < xs.Count; i++)
            {
                coords[i * 3] = xs[i];
                coords[i * 3 + 1] = ys[i];
                coords[i * 3 + 2] = zs[i];
                vals[i] = 1.0f;
            }

            try
            {
                baseGrid.SetValues(coords, vals);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not write the base grid: " + e.Message);
                return;
            }

            DA.SetData(0, new GH_Grid(baseGrid));

            if (addToStack)
            {
                // Add to a COPY of the stack's layer list rather than mutating
                // the input, so upstream HyperVoxel references are untouched.
                var copy = new HyperVoxel(hyper.Name);
                foreach (var l in hyper.Layers) copy.Add(l.Grid, l.Name);
                copy.Add(baseGrid, "base");
                DA.SetData(1, new GH_HyperVoxel(copy));
            }
        }

        private const int OFF = 1 << 20;
        private static long Pack(int x, int y, int z)
        {
            return ((long)(x + OFF) << 42) | ((long)(y + OFF) << 21) | (long)(z + OFF);
        }
    }
}
