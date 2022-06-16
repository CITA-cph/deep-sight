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
using System.Diagnostics;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using DeepSight;
using RawLamb;
using System.Collections.Generic;

namespace RawLamb.GH.Components
{

    public class Cmpt_GridSample : GH_Component
    {
        public Cmpt_GridSample()
          : base("GridSample", "GSmp",
              "Query a point in the grid.",
              "RawLamb", "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddPointParameter("Sample points", "SP", "Points to query in the grid.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Mode", "M", "Sampling method to use.", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Values", "V", "Grid value at each sampling point.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            object m_grid = null;
            DA.GetData(0, ref m_grid);

            Grid temp_grid;
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value;
            else
                return;

            debug.Add(string.Format("{0} : Wrangled Grid from inputs.", stopwatch.ElapsedMilliseconds));

            //var points = new List<Point3d>();
            var points = new List<GH_Point>();
            DA.GetDataList(1, points);

            int mode = 0;
            DA.GetData(2, ref mode);
            if (mode < 0) mode = 0;
            else if (mode > 2) mode = 2;

            debug.Add(string.Format("{0} : Got other inputs.", stopwatch.ElapsedMilliseconds));

            var floats = new float[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                floats[i * 3] = (float)points[i].Value.X;
                floats[i * 3 + 1] = (float)points[i].Value.Y;
                floats[i * 3 + 2] = (float)points[i].Value.Z;

            }
            debug.Add(string.Format("{0} : Flattened list of samples.", stopwatch.ElapsedMilliseconds));

            var values = temp_grid.Evaluate(floats, mode);

            debug.Add(string.Format("{0} : Finished sampling grid.", stopwatch.ElapsedMilliseconds));

            
            DA.SetDataList(1, values.Select(x => new GH_Number(x)));

            debug.Add(string.Format("{0} : Set output values.", stopwatch.ElapsedMilliseconds));
            DA.SetDataList(0, debug);

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
            get { return new Guid("f6b26d6b-0962-4e4d-a632-33d227af57df"); }
        }
    }
}