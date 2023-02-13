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
using Grasshopper.Kernel;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridResample : GH_Component
    {
        public Cmpt_GridResample()
          : base("GridResample", "GRes",
              "Resample a grid to a new cell size.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "S", "New cell size.", GH_ParamAccess.item, 5);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Resampled grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid = null;
            double m_size = 0.15;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            if (temp_grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unsupported grid type ({m_grid})");
                return;
            }

            DA.GetData(1, ref m_size);

            var ngrid = temp_grid.DuplicateGrid();
            var new_grid = Tools.Resample(ngrid, m_size);
            new_grid.Name = temp_grid.Name;

            DA.SetData(0, new GH_Grid(new_grid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridResample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0bf9ba62-b6af-4667-aed1-cfd8179eb911"); }
        }
    }
}