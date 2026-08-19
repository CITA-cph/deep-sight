using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Loads a .vdb file directly into a HyperVoxel, one layer per grid.
    /// </summary>
    /// <remarks>
    /// This exists to fix a broken workflow. A .vdb written by HyperVoxelSave is
    /// already a stack of named grids, but the only way to reload it used to be
    /// GridLoad -> HyperVoxelMerge -> HyperVoxelExtract, which is nonsensical for
    /// something that is already a merged stack. This reads the file and returns
    /// a HyperVoxel in one step, mirroring HyperVoxelSave.
    ///
    /// It is deliberately lenient: it does NOT run the transform-alignment check
    /// that HyperVoxelMerge does, because a file written by HyperVoxelSave is
    /// already known-good, and a file from another tool (Houdini, Blender) is
    /// the user's own structure to trust or not. If you want it validated, pass
    /// the result through HyperVoxelMerge, which re-checks.
    /// </remarks>
    public class Cmpt_HyperVoxelLoad : GH_Component
    {
        public Cmpt_HyperVoxelLoad()
          : base("HyperVoxelLoad", "HVLoad",
              "Load a .vdb file directly into a HyperVoxel, one layer per grid. " +
              "The counterpart to HyperVoxelSave.",
              DeepSight.GH.Api.ComponentCategory, "HyperVoxel")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.primary; } }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.HyperVoxelLoad; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a1f3322d-d5c3-4db2-9227-2f081bad958f"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "FP", "Path of the .vdb file to load.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Optional name for the resulting HyperVoxel.",
                GH_ParamAccess.item, "hypervoxel");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("HyperVoxel", "HV", "The loaded stack.", GH_ParamAccess.item);
            pManager.AddTextParameter("Layers", "L", "Names of the layers, in order.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Count", "C", "Number of layers.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = null;
            if (!DA.GetData(0, ref path)) return;

            string hyperName = "hypervoxel";
            DA.GetData(1, ref hyperName);

            if (string.IsNullOrEmpty(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No filepath supplied.");
                return;
            }

            if (!System.IO.File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File not found: " + path);
                return;
            }

            if (!path.EndsWith(".vdb", StringComparison.OrdinalIgnoreCase))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File is not a .vdb.");
                return;
            }

            GridApi[] grids;
            try
            {
                // GridIO.Read now raises a managed exception instead of crashing
                // Rhino when the file is missing or corrupt (one of the v0.8
                // fixes), so this try/catch actually catches something useful.
                grids = GridIO.Read(path);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read the file: " + e.Message);
                return;
            }

            if (grids == null || grids.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The file contained no readable grids.");
                return;
            }

            var hyper = new HyperVoxel(hyperName);

            foreach (var grid in grids)
            {
                if (grid == null || !grid.IsValid) continue;

                // Each grid's own OpenVDB name becomes the layer name, which is
                // exactly what HyperVoxelSave wrote. Add() falls back to a
                // generated name and de-duplicates if a file has blank or
                // repeated grid names (e.g. one not written by this plugin).
                try
                {
                    hyper.Add(grid);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                        "Skipped a grid that could not be added: " + e.Message);
                }
            }

            if (hyper.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No valid grids could be loaded as layers.");
                return;
            }

            DA.SetData(0, new GH_HyperVoxel(hyper));
            DA.SetDataList(1, hyper.LayerNames);
            DA.SetData(2, hyper.Count);
        }
    }
}
