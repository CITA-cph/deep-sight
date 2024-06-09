/*
 * RawLamb
 * Copyright 2022 Tom Svilans
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using System.Linq;
using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridLoad : GH_Component
    {
        public Cmpt_GridLoad()
          : base("GridLoad", "GLoad",
              "Load a grid.", 
              DeepSight.GH.Api.ComponentCategory, "IO")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridLoad_01;
        public override Guid ComponentGuid => new Guid("d18197ba-60f2-4a46-8479-ce6f5380918a");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "FP", "File path of grid.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string m_path = String.Empty;

            DA.GetData("Filepath", ref m_path);
            if (System.IO.File.Exists(m_path) && m_path.EndsWith(".vdb"))
            {
                var grids = GridIO.Read(m_path);
                DA.SetDataList("Grid", grids.Select(x => new GH_Grid(x)));
            }
        }
    }
}