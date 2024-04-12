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
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DeepSight.GH.Components
{

    public class Cmpt_PointCloudCreate : GH_Component
    {
        public Cmpt_PointCloudCreate()
          : base("PointCloudCreate", "PtClNew",
              "Create point cloud with optional colors.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddGenericParameter("PointCloud", "PC", "Base point cloud to add to (optional)", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "P", "Points to add to cloud.", GH_ParamAccess.list);
            pManager.AddColourParameter("Colors", "C", "Colors for points (optional).", GH_ParamAccess.list);
            pManager[1].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PointCloud", "PC", "PointCloud", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<Point3d> pts = new List<Point3d>();
            DA.GetDataList(0, pts);

            List<System.Drawing.Color> col = new List<System.Drawing.Color>();
            DA.GetDataList(1, col);
            if(col.Count != pts.Count)
            {
                if(col.Count <= 0 || col[0] == null)
                {
                    Color monochrome = Color.FromArgb(1);
                    for (int i = 0; i < pts.Count; i++)
                    {
                        col.Add(monochrome);
                    }
                } else
                {
                    while(col.Count < pts.Count)
                    {
                        col.Add(col.Last());
                    }
                }
            }

            PointCloud pc = new PointCloud();
            pc.AddRange(pts,col);

            DA.SetData(0, pc);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridNew_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5484E40A-D1CF-4283-8BAA-C3A5068938CB"); }
        }
    }
}