using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Writes a HyperVoxel to a single .vdb file, one named grid per layer.
    /// </summary>
    /// <remarks>
    /// EXPORT DESIGN. A HyperVoxel is a set of named grids, and a .vdb file is a
    /// set of named grids, so export is just GridIO.Write over the layers - no
    /// new format. The resulting .vdb opens in Houdini, Blender, Maya and every
    /// other OpenVDB tool, and reloads through GridLoad + HyperVoxelMerge for a
    /// clean round-trip.
    ///
    /// THE ONE THING THAT MATTERS: each grid's OpenVDB name is set to the layer
    /// name before writing. Without this, the layers reload as "grid_0, grid_1"
    /// and lose their identity. The HyperVoxel already keeps layer names unique,
    /// which also matters because readers key on names and duplicates collide.
    /// </remarks>
    public class Cmpt_HyperVoxelSave : GH_Component
    {
        public Cmpt_HyperVoxelSave()
          : base("HyperVoxelSave", "HVSave",
              "Save a HyperVoxel to a single .vdb file, one named grid per layer. " +
              "Reloads with GridLoad + HyperVoxelMerge, and opens in any OpenVDB tool.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HyperVoxelSave; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2625e051-a2c8-44c6-ae5e-0091d093f5a1"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The stack to save.", GH_ParamAccess.item);
            pManager.AddTextParameter("Filepath", "FP", "Where to write the .vdb file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Save", "S",
                "Set to true to write the file. Guards against overwriting on every solve.",
                GH_ParamAccess.item, false);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "The file that was written.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "What the component did.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var info = new List<string>();

            object obj = null;
            if (!DA.GetData(0, ref obj)) return;

            string path = null;
            if (!DA.GetData(1, ref path)) return;

            bool save = false;
            DA.GetData(2, ref save);

            HyperVoxel hyper = GH_HyperVoxel.ParseStructure(obj);
            if (hyper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a HyperVoxel.");
                return;
            }

            if (hyper.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "HyperVoxel has no layers; nothing to save.");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No filepath supplied.");
                return;
            }

            // Write only on the Save toggle. A destructive file write on every
            // canvas change would be a nasty surprise.
            if (!save)
            {
                info.Add("Save is false. Set it to true to write the file.");
                DA.SetDataList(1, info);
                return;
            }

            if (!path.EndsWith(".vdb", StringComparison.OrdinalIgnoreCase))
                path = path + ".vdb";

            // Stamp each grid with its layer name so the file round-trips.
            // Names are already unique within the HyperVoxel.
            var grids = new List<GridApi>();
            foreach (var layer in hyper.Layers)
            {
                if (layer.Grid == null || !layer.Grid.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                        string.Format("Layer '{0}' is null or disposed; aborting save.", layer.Name));
                    return;
                }

                try { layer.Grid.Name = layer.Name; }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        string.Format("Could not set the name of layer '{0}': {1}", layer.Name, e.Message));
                }

                grids.Add(layer.Grid);
            }

            try
            {
                GridIO.Write(path, grids.ToArray());
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Write failed: " + e.Message);
                return;
            }

            info.Add(string.Format("Wrote {0} layers to {1}.", grids.Count, path));
            info.Add("Layers: " + string.Join(", ", hyper.LayerNames));

            DA.SetData(0, path);
            DA.SetDataList(1, info);
        }
    }
}
