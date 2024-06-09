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
using DeepSight.RhinoCommon;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridInspect : GH_Component
    {
        public Cmpt_GridInspect()
          : base("GridInspect", "GInsp",
              "See a grid's properties.",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridDisplay_01;
        public override Guid ComponentGuid => new Guid("b191c02a-5156-49bb-8797-79068fdafd79");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to inspect.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Get the name of the grid.", GH_ParamAccess.item);
            pManager.AddTransformParameter("Transform", "T", "The transformation matrix of the grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            object m_grid = null;
            GridApi temp_grid = null;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value;
            else
                return;

            var xform = temp_grid.Transform.ToRhinoTransform();

            DA.SetData("Transform", xform);
            DA.SetData("Name", temp_grid.Name);
        }
    }
}