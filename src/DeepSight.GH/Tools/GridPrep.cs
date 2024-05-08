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
using Grasshopper.Kernel;
using Rhino.Geometry;
using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridPrep : GH_Component
    {
        public Cmpt_GridPrep()
          : base("GridPrep", "GPrep",
              "Get target parameters for a grid.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "geometry", GH_ParamAccess.item);
            pManager.AddNumberParameter("Count", "C", "max voxel count", GH_ParamAccess.item, 1000000);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Size", "S", "voxel size", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geo = null;
            if(!DA.GetData("Geometry", ref geo)) return;

            double count = 1000000;
            DA.GetData("Count", ref count);

            BoundingBox b = geo.GetBoundingBox(false);

            double size = Math.Pow(b.Volume / count, 1/3);
            DA.SetData("Size", size);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridResample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("05BB9EC2-CA59-45B7-B6A5-5328EA147900"); }
        }
    }
}