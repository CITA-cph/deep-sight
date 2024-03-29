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
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using DeepSight.RhinoCommon;
using Grid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridSet: GH_Component
    {
        public Cmpt_VGridSet()
          : base("VGridSet", "VGSet",
              "Set vector grid values.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddPointParameter("Sample points", "SP", "Coordinates to set in the grid.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Sample values", "SV", "Values to set at the coordinates in the grid.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Grid space", "GS", "Coordinates in grid-space (true) or world-space (false).", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Output grid.", GH_ParamAccess.list);
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
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            if (temp_grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unsupported grid type ({m_grid})");
                return;
            }

            debug.Add(string.Format("{0} : Wrangled Grid from inputs.", stopwatch.ElapsedMilliseconds));

            //var points = new List<Point3d>();
            var points = new List<GH_Point>();
            DA.GetDataList(1, points);

            var values = new List<GH_Vector>();
            DA.GetDataList(2, values);

            debug.Add(string.Format("{0} : Got other inputs.", stopwatch.ElapsedMilliseconds));

            bool gridspace = false;
            DA.GetData(3, ref gridspace);

            if (!gridspace)
            {
                Transform imat;
                Transform mat = temp_grid.Transform.ToRhinoTransform();
                mat.TryGetInverse(out imat);

                for (int i = 0; i < points.Count; ++i)
                {
                    points[i] = (GH_Point)points[i].Transform(imat);
                }
            }

            var fpoints = new int[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                fpoints[i * 3] = (int)Math.Floor(points[i].Value.X + 0.5);
                fpoints[i * 3 + 1] = (int)Math.Floor(points[i].Value.Y + 0.5);
                fpoints[i * 3 + 2] = (int)Math.Floor(points[i].Value.Z + 0.5);
            }

            debug.Add(string.Format("{0} : Flattened list of samples.", stopwatch.ElapsedMilliseconds));

            temp_grid.SetValues(fpoints, values.Select(x => new Vec3<float>((float)(x.Value.X),(float)(x.Value.Y),(float)(x.Value.Z))).ToArray());

            debug.Add(string.Format("{0} : Finished sampling grid.", stopwatch.ElapsedMilliseconds));


            DA.SetData(1, new GH_Grid(temp_grid));

            debug.Add(string.Format("{0} : Set output values.", stopwatch.ElapsedMilliseconds));
            DA.SetDataList(0, debug);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSet_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5BA24E24-DDF0-46AA-850C-4908F459D5D2"); }
        }
    }
}