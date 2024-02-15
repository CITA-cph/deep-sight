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
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Xml.Schema;
using Grasshopper.Kernel;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridDilate : GH_Component
    {
        public Cmpt_GridDilate()
          : base("GridDilate", "GDil",
              "Dilate a float grid.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Width of filtering kernel.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Iterations", "I", "Number of filter iterations.", GH_ParamAccess.item, 1);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Dilated grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            object m_grid = null;
            Grid temp_grid = null;
            int width = 1, iterations = 1, type = 0;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            DA.GetData(1, ref width);
            DA.GetData(2, ref iterations);
            debug.Add(string.Format("{0} : Got input data.", stopwatch.ElapsedMilliseconds));
            
            Grid new_grid = temp_grid.DuplicateGrid();
            Tools.Dilate(new_grid, iterations);
            debug.Add(string.Format("{0} : Dilated grid.", stopwatch.ElapsedMilliseconds));


            DA.SetDataList(0, debug);
            DA.SetData(1, new GH_Grid(new_grid));
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
            get { return new Guid("C0B6B053-1347-42DF-BF02-01AD7E0CEA77"); }
        }
    }
}