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
using System.Threading.Tasks;
using System.Diagnostics;
using Rhino.Geometry;
using Grasshopper.Kernel;

using VGrid = DeepSight.Vec3fGrid;
using FGrid = DeepSight.FloatGrid;
using Eto.Forms;
using DeepSight.RhinoCommon;
using System.Linq;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;

namespace DeepSight.GH.Components
{

public class Cmpt_ColorFromPtCloud : GH_Component
    {
        public Cmpt_ColorFromPtCloud()
          : base("PtCloudColor", "PtClColor",
              "Color voxels by nearest point in a cloud.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "VGrid to be assigned colors.", GH_ParamAccess.item);
            pManager.AddGenericParameter("PointCloud", "P", "Point cloud with colors.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Resulting Grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            PointCloud pc = null;
            DA.GetData("PointCloud", ref pc);
            if (pc == null) return;
            if (!pc.ContainsColors)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Point cloud does not contain colors.");
                return;
            }

            object m_grid = null;
            DA.GetData("Grid", ref m_grid);

            VGrid temp_grid;
            if (m_grid is VGrid)
                temp_grid = m_grid as VGrid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as VGrid;
            else
                return;

            int[] active = temp_grid.GetActiveVoxels();
            System.Drawing.Color[] colors = pc.GetColors();
            Vec3<float>[] values = temp_grid.GetValuesIndex(active);
            debug.Add(string.Format("{0} : Finished getting colors.", stopwatch.ElapsedMilliseconds));

            //Parallel.For(0, active.Length - 3 - 1, i =>
            //{
            //    int index = pc.ClosestPoint(new Point3d(active[i], active[i + 1], active[i + 2]));
            //    System.Drawing.Color c = colors[index];
            //    values[i / 3] = new Vec3<float>(c.R, c.G, c.B);
            //    i += 3;
            //});

            for(int i=0; i<active.Length; i+=3)
            {
                int index = pc.ClosestPoint(new Point3d(active[i], active[i + 1], active[i + 2]));
                System.Drawing.Color c = colors[index];
                values[i / 3] = new Vec3<float>(c.R, c.G, c.B);
            }

            debug.Add(string.Format("{0} : Computed values.", stopwatch.ElapsedMilliseconds));


            VGrid new_grid = temp_grid.DuplicateGrid();
            new_grid.SetValues(active, values);
            debug.Add(string.Format("{0} : Set values.", stopwatch.ElapsedMilliseconds));


            DA.SetDataList(0, debug);
            DA.SetData("Grid", new GH_Grid(new_grid));

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridGoo_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("56806878-345A-47AF-8DB8-2A242992ED3A"); }
        }
    }
}