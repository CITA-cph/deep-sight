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

using Grid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridMorphology : GH_Component
    {
        public Cmpt_VGridMorphology()
          : base("VGridMorph", "VGMorph",
              "Apply morphology operations to a vector grid.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }
        //public int[] GetNeighborIndices(int[] coords)
        //{
        //    int[] nbrs = new int[27 * 3];
        //    int x=coords[0], y=coords[1], z=coords[2];
        //    int counter = 0;
        //    for(int i=-1; i<2; i++)
        //    {
        //        for(int j=-1; j<2; j++)
        //        {
        //            for (int k=-1; k<2; k++)
        //            {
        //                nbrs[counter] = x + i;
        //                nbrs[counter+1] = y + j;
        //                nbrs[counter+2] = z + k;
        //                counter += 3;
        //            }
        //        }
        //    }
        //    return nbrs;
        //}

        //public Vec3<float>[] GetNeighborValues(Vec3fGrid grid, int[] coords)
        //{
        //    int[] nbrs_indices = GetNeighborIndices(coords);
        //    Vec3<float>[] nbrs_values = grid.GetValuesIndex(nbrs_indices);
        //    return nbrs_values;
        //}

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "I", "Number of filter iterations.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Mode", "M", "Type of morphology operation to perform. 0=dilate, 1=erode, 2=open, 3=close, 4=proper open, 5=proper close, 6=automedian", GH_ParamAccess.item, 0);
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
            int iterations = 1;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            DA.GetData(1, ref iterations);
            debug.Add(string.Format("{0} : Got input data.", stopwatch.ElapsedMilliseconds));

            int mode = 0;
            if (!DA.GetData(2, ref mode)) return;

            Grid new_grid = temp_grid.DuplicateGrid();
            switch (mode)
            {
                case (int)(MorphOpType.DILATE):
                    Tools.Dilate(new_grid, iterations);
                    break;
                case (int)(MorphOpType.ERODE):
                    Tools.Erode(new_grid, iterations);
                    break;
                case (int)(MorphOpType.OPEN):
                    for (int i = 0; i < iterations; i++)
                    {
                        Tools.Erode(new_grid, 1);
                        Tools.Dilate(new_grid, 1);
                    }
                    break;
                case (int)(MorphOpType.CLOSE):
                    for (int i = 0; i < iterations; i++)
                    {
                        Tools.Dilate(new_grid, 1);
                        Tools.Erode(new_grid, 1);
                    }
                    break;
                //case (int)(MorphOpType.POPEN):
                //    for (int i = 0; i < iterations; i++)
                //    {
                //        Grid I = new_grid.DuplicateGrid();
                //        Tools.Erode(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        new_grid = Tools.Combine(I, new_grid, CombineType.MIN);
                //    }
                //    break;
                //case (int)(MorphOpType.PCLOSE):
                //    for (int i = 0; i < iterations; i++)
                //    {
                //        Grid I = new_grid.DuplicateGrid();
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        new_grid = Tools.Combine(I, new_grid, CombineType.MAX);
                //    }
                //    break;
                //case (int)(MorphOpType.AUTOMED):
                //    for (int i = 0; i < iterations; i++)
                //    {
                //        Grid I = new_grid.DuplicateGrid();

                //        Grid new_grid_2 = new_grid.DuplicateGrid();

                //        Tools.Erode(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Erode(new_grid, 1);
                //        Tools.Dilate(new_grid, 1);
                //        new_grid = Tools.Combine(I, new_grid, CombineType.MIN);

                //        Tools.Dilate(new_grid_2, 1);
                //        Tools.Erode(new_grid_2, 1);
                //        Tools.Erode(new_grid_2, 1);
                //        Tools.Dilate(new_grid_2, 1);
                //        Tools.Dilate(new_grid_2, 1);
                //        Tools.Erode(new_grid_2, 1);
                //        new_grid_2 = Tools.Combine(I, new_grid_2, CombineType.MAX);

                //        new_grid = Tools.Combine(new_grid, new_grid_2, CombineType.MIN);
                //    }
                //    break;
                default: return;
            }
            debug.Add(string.Format("{0} : Completed morphology operation.", stopwatch.ElapsedMilliseconds));

            DA.SetDataList(0, debug);
            DA.SetData(1, new GH_Grid(new_grid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.VGridResample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("E06ACEA1-ED84-48C8-BF7C-B99B2C4D71D3"); }
        }
    }
}