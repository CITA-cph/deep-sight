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

using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DeepSight.GH.Components
{

    public class Cmpt_GetPointCloud: GH_Component
    {
        public Cmpt_GetPointCloud()
          : base("GetPointCloud", "PtClGet",
              "Get point cloud(s) from document",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Get", "G", "Get from document", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PointClouds", "PC", "PointCloud", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Boolean get = true;
            DA.GetData("Get", ref get);

            List<Rhino.Geometry.PointCloud> clouds = new List<Rhino.Geometry.PointCloud>();
            if (get)
            {
                
                foreach(RhinoObject obj in RhinoDoc.ActiveDoc.Objects)
                {
                    if(obj is Rhino.DocObjects.PointCloudObject)
                    {
                        clouds.Add(obj.Geometry as Rhino.Geometry.PointCloud);
                    }
                }
            }

            DA.SetDataList("PointClouds", clouds);

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
            get { return new Guid("59A8A05A-2DC9-44D5-BD9A-A774FF1CF1F9"); }
        }
    }
}