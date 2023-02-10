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
using System.Collections.Generic;

using Grasshopper.Kernel;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridSave : GH_Component
    {
        public Cmpt_GridSave()
          : base("GridSave", "GSave",
              "Save a grid to disk.",
              DeepSight.GH.Api.ComponentCategory, "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to save to disk.", GH_ParamAccess.list);
            pManager.AddTextParameter("Filepath", "FP", "File path of grid.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string m_path = String.Empty;
            var debug = new List<string>();

            DA.GetData("Filepath", ref m_path);

            object m_grid = null;
            GridApi temp_grid = null;

            var inputs = new List<object>();
            var input_grids = new List<GridApi>();

            DA.GetDataList(0, inputs);

            foreach(object input in inputs)
            {
                if (input is GridApi)
                    input_grids.Add(input as GridApi);
                else if (m_grid is GH_Grid)
                    input_grids.Add((input as GH_Grid).Value);
                else
                    continue;
            }

            if (input_grids.Count < 1) return;

            if (!string.IsNullOrEmpty(m_path) && m_path.EndsWith(".vdb"))
            {
                GridIO.Write(m_path, input_grids.ToArray());
            }

            DA.SetDataList("debug", debug);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSave_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("90678bae-c97f-41af-a82e-3014a272946b"); }
        }
    }
}