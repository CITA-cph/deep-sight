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

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;
using Grasshopper.Kernel.Types;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridCombine : GH_Component
    {
        public Cmpt_GridCombine()
          : base("GridCombine", "GCom",
              "Combine two grids together (min, max, sum, mult, diff).",
              DeepSight.GH.Api.ComponentCategory, "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid 1", "G1", "First grid to combine.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Grid 2", "G2", "Second grid to inspect.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Mode", "M", "Mode to combine grids. 0 = min, 1 = max, 2 = sum, 3 = mult, 4 = diff, 5 = if zero.", GH_ParamAccess.item, 1);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Output of combine.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid grid0 = null;
            Grid grid1 = null;

            if (DA.GetData("Grid 1", ref m_grid))
            {
                if (m_grid is Grid)
                    grid0 = m_grid as Grid;
                else if (m_grid is GH_Grid)
                    grid0 = (m_grid as GH_Grid).Value;
                else
                    return;
            }
            if (DA.GetData("Grid 2", ref m_grid))
            {
                if (m_grid is Grid)
                    grid1 = m_grid as Grid;
                else if (m_grid is GH_Grid)
                    grid1 = (m_grid as GH_Grid).Value;
                else
                    return;
            }

            int mode = 0;
            DA.GetData("Mode", ref mode);

            if (grid0 == null || grid1 == null) return;

            var ngrid0 = grid0.Duplicate();
            var ngrid1 = grid1.Duplicate();

            switch(mode)
            {
                case 1:
                    Grid.Maximum(ngrid0, ngrid1);
                    break;
                case 2:
                    Grid.Sum(ngrid0, ngrid1);
                    break;
                case 3:
                    Grid.Multiply(ngrid0, ngrid1);
                    break;
                case 4:
                    Grid.Diff(ngrid0, ngrid1);
                    break;
                case 5:
                    Grid.IfZero(ngrid0, ngrid1);
                    break;
                default:
                    Grid.Minimum(ngrid0, ngrid1);
                    break;
            }

            ngrid0.Prune();
            

            DA.SetData("Grid", new GH_Grid(ngrid0));

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
            get { return new Guid("1461a574-faea-4210-8fbe-4609184ae059"); }
        }
    }
}