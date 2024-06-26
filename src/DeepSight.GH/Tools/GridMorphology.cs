﻿/*
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
using Eto.Forms;
using Grasshopper.Kernel;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;


namespace DeepSight.GH.Components
{

    public class Cmpt_GridMorphology : GH_Component
    {
        public enum MorphOpType
        {
            DILATE = 0,
            ERODE = 1,
            OPEN = 2,
            CLOSE = 3,
            POPEN = 4,
            PCLOSE = 5,
            AUTOMED = 6
        }

        public Cmpt_GridMorphology()
          : base("GridMorph", "GMorph",
              "Perform morphological operations on a float grid.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridFilter_01;
        public override Guid ComponentGuid => new Guid("C0B6B053-1347-42DF-BF02-01AD7E0CEA77");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "I", "Number of iterations.", GH_ParamAccess.item, 1);
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

            int iterations = 1;
            DA.GetData("Iterations", ref iterations);

            int mode = 0;
            if (!DA.GetData("Mode", ref mode)) return;

            object m_grid = null;
            FGrid temp_fgrid = null;
            VGrid temp_vgrid = null;

            if (m_grid is FGrid)
                temp_fgrid = m_grid as FGrid;
            else if (m_grid is VGrid)
                temp_vgrid = m_grid as VGrid;
            else if (m_grid is GH_Grid)
                if ((m_grid as GH_Grid).Value is FGrid)
                    temp_fgrid = (m_grid as GH_Grid).Value as FGrid;
                else if ((m_grid as GH_Grid).Value is VGrid)
                    temp_vgrid = (m_grid as GH_Grid).Value as VGrid;
                else
                    return;
            else
                return;

            if (temp_fgrid != null)
            {
                FGrid new_grid = temp_fgrid.DuplicateGrid();
                switch (mode)
                {
                    case (int)MorphOpType.DILATE:
                        Tools.Dilate(new_grid, iterations);
                        break;
                    case (int)MorphOpType.ERODE:
                        Tools.Erode(new_grid, iterations);
                        break;
                    case (int)MorphOpType.OPEN:
                        for (int i = 0; i < iterations; i++)
                        {
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                        }
                        break;
                    case (int)MorphOpType.CLOSE:
                        for (int i = 0; i < iterations; i++)
                        {
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                        }
                        break;
                    case (int)MorphOpType.POPEN:
                        for (int i = 0; i < iterations; i++)
                        {
                            FGrid I = new_grid.DuplicateGrid();
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            new_grid = Tools.Combine(I, new_grid, CombineType.MIN);
                        }
                        break;
                    case (int)MorphOpType.PCLOSE:
                        for (int i = 0; i < iterations; i++)
                        {
                            FGrid I = new_grid.DuplicateGrid();
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            new_grid = Tools.Combine(I, new_grid, CombineType.MAX);
                        }
                        break;
                    case (int)MorphOpType.AUTOMED:
                        for (int i = 0; i < iterations; i++)
                        {
                            FGrid I = new_grid.DuplicateGrid();

                            FGrid new_grid_2 = new_grid.DuplicateGrid();

                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                            new_grid = Tools.Combine(I, new_grid, CombineType.MIN);

                            Tools.Dilate(new_grid_2, 1);
                            Tools.Erode(new_grid_2, 1);
                            Tools.Erode(new_grid_2, 1);
                            Tools.Dilate(new_grid_2, 1);
                            Tools.Dilate(new_grid_2, 1);
                            Tools.Erode(new_grid_2, 1);
                            new_grid_2 = Tools.Combine(I, new_grid_2, CombineType.MAX);

                            new_grid = Tools.Combine(new_grid, new_grid_2, CombineType.MIN);
                        }
                        break;
                    default: return;
                }
                debug.Add(string.Format("{0} : Completed morphology operation.", stopwatch.ElapsedMilliseconds));

                DA.SetDataList("debug", debug);
                DA.SetData("Grid", new GH_Grid(new_grid));
            }
            else if (temp_vgrid != null)
            {
                VGrid new_grid = temp_vgrid.DuplicateGrid();
                switch (mode)
                {
                    case (int)MorphOpType.DILATE:
                        Tools.Dilate(new_grid, iterations);
                        break;
                    case (int)MorphOpType.ERODE:
                        Tools.Erode(new_grid, iterations);
                        break;
                    case (int)MorphOpType.OPEN:
                        for (int i = 0; i < iterations; i++)
                        {
                            Tools.Erode(new_grid, 1);
                            Tools.Dilate(new_grid, 1);
                        }
                        break;
                    case (int)MorphOpType.CLOSE:
                        for (int i = 0; i < iterations; i++)
                        {
                            Tools.Dilate(new_grid, 1);
                            Tools.Erode(new_grid, 1);
                        }
                        break;
                    case (int)MorphOpType.POPEN:
                    case (int)MorphOpType.PCLOSE:
                    case (int)MorphOpType.AUTOMED:
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "This operation is not currently supported on Vector Grids");
                        break;
                    default: return;
                }
                debug.Add(string.Format("{0} : Completed morphology operation.", stopwatch.ElapsedMilliseconds));

                DA.SetDataList("debug", debug);
                DA.SetData("Grid", new GH_Grid(new_grid));
            }
        }
    }
}