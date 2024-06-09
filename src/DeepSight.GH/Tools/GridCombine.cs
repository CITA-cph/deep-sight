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

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridCombine : GH_Component
    {
        public Cmpt_GridCombine()
          : base("GridCombine", "GCom",
              "Combine two grids together (min, max, sum, mult, diff).",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid 1", "G1", "First grid to combine.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Grid 2", "G2", "Second grid to combine.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Mode", "M", "Mode to combine grids. 0=max, 1=min, 2=sum, 3=diff, 4=if zero, 5=mul.", GH_ParamAccess.item, 1);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Output of combine.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            List<object> m_grids = new List<object>();
            Grid grid0 = null;
            List<Grid> grid1 = new List<Grid>();

            if (DA.GetData("Grid 1", ref m_grid))
            {
                if (m_grid is Grid)
                    grid0 = m_grid as Grid;
                else if (m_grid is GH_Grid)
                    grid0 = (m_grid as GH_Grid).Value as Grid;
                else
                    return;
            }
            if (DA.GetDataList("Grid 2", m_grids))
            {
                foreach(var obj in m_grids)
                {
                    if (obj is Grid)
                        grid1.Add(obj as Grid);
                    else if (m_grid is GH_Grid)
                        grid1.Add((obj as GH_Grid).Value as Grid);
                    else
                        continue;
                }
            }

            if (grid0 == null || grid1.Count < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unsupported grid types.");
                return;
            }

            int mode = 0;
            DA.GetData("Mode", ref mode);

            Grid ngrid = grid0;
            Grid temp = null;

            for (int i = 0; i < grid1.Count; i++)
            {
                temp = Tools.Combine(ngrid, grid1[i], (CombineType)mode);
                ngrid = temp;
            }

            /*
            var ngrid0 = grid0.Duplicate();
            var ngrid1 = grid1.Duplicate();

            switch(mode)
            {
                case 1:
                    Combine.Maximum(ngrid0, ngrid1);
                    break;
                case 2:
                    Combine.Sum(ngrid0, ngrid1);
                    break;
                case 3:
                    Combine.Multiply(ngrid0, ngrid1);
                    break;
                case 4:
                    Combine.Diff(ngrid0, ngrid1);
                    break;
                case 5:
                    Combine.IfZero(ngrid0, ngrid1);
                    break;
                default:
                    Combine.Minimum(ngrid0, ngrid1);
                    break;
            }
            */
            ngrid.Prune();
            

            DA.SetData("Grid", new GH_Grid(ngrid));

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