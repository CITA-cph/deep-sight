using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Merges several voxel grids into a single named stack (a "HyperVoxel").
    /// </summary>
    public class Cmpt_HyperVoxelMerge : GH_Component
    {
        public Cmpt_HyperVoxelMerge()
          : base("HyperVoxelMerge", "HVMerge",
              "Merge multiple voxel grids into a single HyperVoxel stack, so they can be " +
              "addressed as one object and individual layers extracted later.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        // Reuses an existing icon so this compiles without adding a new resource.
        // Swap for a dedicated PNG once you have one.
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.MergeHypervoxel_01; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e0e2bc8c-f034-49c5-a65a-00caedcddcca"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grids", "G", "Voxel grids to merge into a stack.", GH_ParamAccess.list);
            pManager.AddTextParameter("Names", "N",
                "Optional layer names, one per grid. If omitted, each grid's own name is used.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Name", "HN", "Name for the resulting HyperVoxel.",
                GH_ParamAccess.item, "hypervoxel");
            pManager.AddBooleanParameter("Strict", "S",
                "If true, refuse to merge grids whose transforms do not match. If false, merge anyway and warn.",
                GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The merged stack.", GH_ParamAccess.item);
            pManager.AddTextParameter("Layers", "L", "Names of the layers, in order.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Count", "C", "Number of layers.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var raw = new List<object>();
            if (!DA.GetDataList(0, raw)) return;

            if (raw.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No grids supplied.");
                return;
            }

            var names = new List<string>();
            DA.GetDataList(1, names);

            string hyperName = "hypervoxel";
            DA.GetData(2, ref hyperName);

            bool strict = false;
            DA.GetData(3, ref strict);

            // Names, if given, must line up one-to-one with the grids. Silently
            // pairing a short list would mislabel layers, and mislabelled layers
            // are worse than unnamed ones because later lookups by name would
            // return the wrong volume.
            if (names.Count > 0 && names.Count != raw.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    string.Format("Supplied {0} names for {1} grids. Provide one name per grid, or none.",
                        names.Count, raw.Count));
                return;
            }

            var hyper = new HyperVoxel(hyperName);

            for (int i = 0; i < raw.Count; i++)
            {
                GridApi grid = GH_Grid.ParseStructure(raw[i]);

                if (grid == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Input {0} is not a grid.", i));
                    return;
                }

                if (!grid.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Grid at index {0} is invalid or has been disposed.", i));
                    return;
                }

                string layerName = (names.Count > 0) ? names[i] : null;

                try
                {
                    hyper.Add(grid, layerName);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Could not add grid {0}: {1}", i, e.Message));
                    return;
                }
            }

            // Transform alignment is the thing that makes or breaks this feature,
            // so it is checked every solve rather than assumed. See the remarks
            // on HyperVoxel.Validate for why a mismatch is dangerous rather than
            // merely untidy.
            var problems = hyper.Validate();

            foreach (var p in problems)
            {
                AddRuntimeMessage(
                    strict ? GH_RuntimeMessageLevel.Error : GH_RuntimeMessageLevel.Warning, p);
            }

            if (strict && problems.Count > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Strict mode is on, so the merge was refused. Resample the grids onto a " +
                    "common transform, or set Strict to false to merge anyway.");
                return;
            }

            DA.SetData(0, new GH_HyperVoxel(hyper));
            DA.SetDataList(1, hyper.LayerNames);
            DA.SetData(2, hyper.Count);
        }
    }
}
