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
using System.Diagnostics;

using Grasshopper.Kernel;

using VGrid = DeepSight.Vec3fGrid;
using FGrid = DeepSight.FloatGrid;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using Eto.Forms;

namespace DeepSight.GH.Components
{
    
public class Cmpt_ScalarMath : GH_Component
    {
        public Cmpt_ScalarMath()
          : base("ScalarMath", "ScalarMath",
              "Perform various operations with scalars",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Float grid to operate upon.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number", "N", "Scalar number.", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Mode", "M", "Mode to operate. 0 = sum, 1 = diff, 2 = mul, 3 = div, 4 = pow, 5 = min, 6 = max, 7 = <, 8 = >, 9 = ==", GH_ParamAccess.item, 0);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Resulting Grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int mode = 0;
            if (!DA.GetData("Mode", ref mode)) return;

            double n = 0;
            if (!DA.GetData("Number", ref n)) return;

            if (n == 0 && mode == 3)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Division by zero.");
                return;
            }

            object m_grid = null;
            FGrid temp_grid = null;

            DA.GetData("Grid", ref m_grid);
            if (m_grid is FGrid)
                temp_grid = m_grid as FGrid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as FGrid;
            else
                return;

            if (m_grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Improper or missing grid.");
                return;
            }

            FGrid new_grid = Tools.Combine(temp_grid, (float)n, (ScalarCombineType)mode);

            DA.SetDataList(0, debug);
            DA.SetData("Grid", new GH_Grid(new_grid));

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridGoo_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("26455EE1-8C81-4EDE-90DA-8D5061047BA3"); }
        }
    }
}