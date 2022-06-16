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

using DeepSight;

namespace RawLamb.GH.Components
{

    public class Cmpt_GridFilter : GH_Component
    {
        public Cmpt_GridFilter()
          : base("GridFilter", "GFil",
              "Filter a grid.",
              "RawLamb", "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Width of filtering kernel.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Iterations", "I", "Number of filter iterations.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Mode", "M", "Filter mode.", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Filtered grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid = null;
            int width = 1, iterations = 1, type = 0;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value;
            else
                return;

            DA.GetData(1, ref width);
            DA.GetData(2, ref iterations);
            DA.GetData(3, ref type);

            var new_grid = temp_grid.Duplicate();
            new_grid.Filter(width, iterations, type);

            DA.SetData(0, new GH_Grid(new_grid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("905431dd-9437-484a-acdf-e862ddc17685"); }
        }
    }
}