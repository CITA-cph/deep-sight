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
using System.Linq;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.Rhino;
using Grasshopper.Kernel.Types;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_FlipBasePlane : GH_Component
    {
        public Cmpt_FlipBasePlane()
          : base("FlipBasePlane", "FlipBP",
              "Flip the baseplane of a geometry.",
              DeepSight.GH.Api.ComponentCategory, "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geo", "G", "Geometry.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Planes", "P", "Baseplane of geometry.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Indices", "I", "Optional. Given a list, only flip these indices.", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Baseplanes", "P", "Best baseplane for Brep.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GeometryBase> inputGeometries = new List<GeometryBase>();
            List<Plane> inputPlanes = new List<Plane>();
            List<int> indices = new List<int>();


            if (!DA.GetDataList("Geo", inputGeometries)) return;
            if (!DA.GetDataList("Planes", inputPlanes)) return;

            DA.GetDataList("Indices", indices);

            List<Plane> outputPlanes = new List<Plane>(inputPlanes);

            int N = Math.Min(inputGeometries.Count, inputPlanes.Count);

            if (indices.Count < 1) indices = Enumerable.Range(0, N).ToList();

            foreach (int i in indices)
            {
                if (i < 0 || i >= N) continue;
                outputPlanes[i] = RawLamb.Geometry.FlipBasePlane(inputGeometries[i], inputPlanes[i]);
            }

            DA.SetDataList("Baseplanes", outputPlanes);
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
            get { return new Guid("14cfa1c6-6876-4055-b075-de3711c9cf87"); }
        }
    }
}