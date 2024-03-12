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

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridMask: GH_Component
    {
        public Cmpt_GridMask()
          : base("GridMask", "GMask",
              "Mask a grid with the active values of another.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mask", "M", "Match active states to this grid.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Resulting grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid, mask_grid = null;

            DA.GetData("Grid", ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            DA.GetData("Mask", ref m_grid);
            if (m_grid is Grid)
                mask_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                mask_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            var new_grid = new Grid("new_grid", 0f);
            new_grid.Transform = temp_grid.Transform;

            int[] active = mask_grid.GetActiveVoxels();
            float[] values = temp_grid.GetValuesIndex(active);
            new_grid.SetValues(active, values);

            DA.SetData(0, new GH_Grid(new_grid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridFilter_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2406FA31-D1C6-4F73-8669-62525F13E43C"); }
        }
    }
}