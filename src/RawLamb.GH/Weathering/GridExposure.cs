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
using System.Collections.Generic;
using System.Diagnostics;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;
using Grasshopper.Kernel.Types;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridExposure : GH_Component
    {
        public Cmpt_GridExposure()
          : base("GridExposure", "GExp",
              "Get a grid's exposure values.",
              DeepSight.GH.Api.ComponentCategory, "Biopol")
        {
        }



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to inspect.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points representing the active coordinates of the grid.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Values", "V", "Exposure values of the grid at the active coordinates.", GH_ParamAccess.list);
            pManager.AddTransformParameter("Transform", "T", "The transformation matrix of the grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid = null;

            DA.GetData(0, ref m_grid);
            
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;
           
            var xform = temp_grid.Transform.ToRhinoTransform();
            var active_values = temp_grid.GetActiveVoxels();

            var N = active_values.Length / 3;

            int i, j, k;
            
            var values = new GH_Number[N];
            var tpoints = new GH_Point[N];
            for (int x = 0; x < N; x++)
            {
                i = active_values[x * 3];
                j = active_values[x * 3 + 1];
                k = active_values[x * 3 + 2];

                values[x] = new GH_Number(Weathering.Exposure(temp_grid.GetNeighbours(new int[] { i, j, k })));
                tpoints[x] = new GH_Point(new Point3d(i, j, k));
            }

            DA.SetData("Transform", xform);
            DA.SetDataList("Points", tpoints);
            DA.SetDataList("Values", values);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("412cea81-b041-4b39-925d-1db240ec85ee"); }
        }
    }
}