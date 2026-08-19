using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Pulls one layer (or all layers) out of a HyperVoxel as ordinary grids.
    /// </summary>
    /// <remarks>
    /// This component does not display anything itself. It hands back a GridApi,
    /// which the existing components already visualise: GridBoxes for coloured
    /// boxes, GridSlice for a plane, GridMesh for a surface. Keeping extraction
    /// separate from display means one extract feeds any viewer.
    ///
    /// Layer selection is by name if a name is given, otherwise by index. Name
    /// lookup is case-insensitive and takes priority because it survives
    /// reordering the stack; index is the fallback and always works.
    /// </remarks>
    public class Cmpt_HyperVoxelExtract : GH_Component
    {
        public Cmpt_HyperVoxelExtract()
          : base("HyperVoxelExtract", "HVGet",
              "Extract a layer from a HyperVoxel as a grid, by name or index. " +
              "Leave both empty to output every layer as a list.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ExtractHypervoxel_01; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("8ca1b5d0-d87d-4602-b597-d5527427837a"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The stack to extract from.", GH_ParamAccess.item);

            pManager.AddTextParameter("Name", "N",
                "Name of the layer to extract. Takes priority over Index if given.",
                GH_ParamAccess.item);
            pManager[1].Optional = true;

            pManager.AddIntegerParameter("Index", "I",
                "Index of the layer to extract (0-based). Used when Name is empty. " +
                "Leave both Name and Index empty to output all layers.",
                GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G",
                "The extracted grid, or all grids if no selector was given.", GH_ParamAccess.list);
            pManager.AddTextParameter("Name", "N", "Name(s) of the extracted layer(s).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

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

            // Did the user actually supply a selector? GetData leaves these at
            // their defaults if nothing is wired, so track it explicitly rather
            // than trusting the value (index 0 is a legitimate selection).
            string name = null;
            int index = 0;
            bool hasName = DA.GetData(1, ref name) && !string.IsNullOrEmpty(name);
            bool hasIndex = DA.GetData(2, ref index);

            // --- No selector: output every layer ------------------------------
            if (!hasName && !hasIndex)
            {
                DA.SetDataList(0, hyper.Layers.Select(l => new GH_Grid(l.Grid)));
                DA.SetDataList(1, hyper.LayerNames);
                return;
            }

            // --- By name (priority) -------------------------------------------
            if (hasName)
            {
                HyperVoxelLayer layer = hyper.FindLayer(name);
                if (layer == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                        "No layer named '{0}'. Available layers: {1}.",
                        name, string.Join(", ", hyper.LayerNames)));
                    return;
                }

                DA.SetDataList(0, new[] { new GH_Grid(layer.Grid) });
                DA.SetDataList(1, new[] { layer.Name });
                return;
            }

            // --- By index -----------------------------------------------------
            if (index < 0 || index >= hyper.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(
                    "Index {0} is out of range. This HyperVoxel has {1} layers (valid 0..{2}).",
                    index, hyper.Count, hyper.Count - 1));
                return;
            }

            HyperVoxelLayer byIndex = hyper[index];
            DA.SetDataList(0, new[] { new GH_Grid(byIndex.Grid) });
            DA.SetDataList(1, new[] { byIndex.Name });
        }
    }
}
