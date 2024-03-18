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

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;
using Eto.Forms;

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
            DA.GetData("Grid", ref m_grid);
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
                var xform = temp_fgrid.Transform.ToRhinoTransform();
                var active = temp_fgrid.GetActiveVoxels();
                var N = active.Length / 3;
                int i, j, k;
                var values = temp_fgrid.GetValuesIndex(active).Select(x => new GH_Number(x));
                var tpoints = new GH_Point[N];
                for (int x = 0; x < N; x++)
                {
                    i = active[x * 3];
                    j = active[x * 3 + 1];
                    k = active[x * 3 + 2];

                    tpoints[x] = new GH_Point(new Point3d(i, j, k));
                }
                DA.SetData("Transform", xform);
                DA.SetDataList("Points", tpoints);
                DA.SetDataList("Values", values);
                DA.SetDataList("debug", debug);
            }
            else if (temp_vgrid != null)
            {
                var xform = temp_vgrid.Transform.ToRhinoTransform();
                var active = temp_vgrid.GetActiveVoxels();
                var N = active.Length / 3;
                int i, j, k;
                var values = temp_vgrid.GetValuesIndex(active).Select(x => new GH_Vector(new Vector3d(x.X,x.Y,x.Z)));
                var tpoints = new GH_Point[N];
                for (int x = 0; x < N; x++)
                {
                    i = active[x * 3];
                    j = active[x * 3 + 1];
                    k = active[x * 3 + 2];
                    tpoints[x] = new GH_Point(new Point3d(i, j, k));
                }
                DA.SetData("Transform", xform);
                DA.SetDataList("Points", tpoints);
                DA.SetDataList("Values", values);
                DA.SetDataList("debug", debug);
            }
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