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
using DeepSight.Rhino;
using Grasshopper.Kernel.Types;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridActive : GH_Component
    {
        public Cmpt_GridActive()
          : base("GridActive", "GAct",
              "Get a grid's active values.",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to inspect.", GH_ParamAccess.item);
            //pManager.AddBoxParameter("Bounds", "B", "Optional bounding box to constrain values to.", GH_ParamAccess.item);
            //pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "P", "Points representing the active coordinates of the grid.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Values", "V", "Values of the grid at the active coordinates.", GH_ParamAccess.list);
            pManager.AddTransformParameter("Transform", "T", "The transformation matrix of the grid.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "Debugging info.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var timer = new Stopwatch();
            timer.Start();
            var debug = new List<string>();


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

            debug.Add(string.Format("Wrangled grid: {0}s", timer.ElapsedMilliseconds / 1000.0));

            //Box bb = Box.Empty;
            //DA.GetData("Bounds", ref bb);

            //var points = new List<GH_Point>();
            //var values = new List<float>();

            var active_values = temp_grid.GetActiveVoxels();

            debug.Add(string.Format("Got active voxels: {0}s", timer.ElapsedMilliseconds / 1000.0));


            var N = active_values.Length / 3;

            int i, j, k;
            /*
            if (bb.IsValid && false)
                for (int x = 0; x < N; x++)
                {
                    i = active_values[x * 3];
                    j = active_values[x * 3 + 1];
                    k = active_values[x * 3 + 2];

                    var pt = new Point3d(i, j, k);

                    pt.Transform(xform);
                    if (bb.Contains(pt))
                    {
                        points.Add(new GH_Point(new Point3d(i, j, k)));
                        values.Add(temp_grid[i, j, k]);
                    }
                }
            else
            {*/

            
                var values = temp_grid.GetValuesIndex(active_values).Select(x => new GH_Number(x));
                var tpoints = new GH_Point[N];
                for (int x = 0; x < N; x++)
                {
                    i = active_values[x * 3];
                    j = active_values[x * 3 + 1];
                    k = active_values[x * 3 + 2];

                    tpoints[x] = new GH_Point(new Point3d(i, j, k));
                }
            //points = tpoints.ToList();
            //}

            debug.Add(string.Format("Got active values: {0}s", timer.ElapsedMilliseconds / 1000.0));

            DA.SetData("Transform", xform);

            DA.SetDataList("Points", tpoints);
            debug.Add(string.Format("Set output points: {0}s", timer.ElapsedMilliseconds / 1000.0));

            DA.SetDataList("Values", values);
            debug.Add(string.Format("Set output values: {0}s", timer.ElapsedMilliseconds / 1000.0));

            DA.SetDataList("debug", debug);

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
            get { return new Guid("377de62d-73fe-48ff-be51-4ca4aa52b065"); }
        }
    }
}