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
using Grasshopper.Kernel;
using Rhino.Geometry;
using DeepSight.RhinoCommon;
using System.Collections.Generic;

using RawLamb;

namespace DeepSight.GH.Components
{
    public class Cmpt_DeLog : GH_Component
    {
        public Cmpt_DeLog()
          : base("DeLog", "DeLog",
              "Extract features from a Log.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Log", "L", "MetaLog object.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Pith", "P", "The pith of the log as a polyline.", GH_ParamAccess.item);
            pManager.AddLineParameter("Knot axes", "KA", "The axes of individual knots as lines.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Knot radii", "KR", "The radii of individual knots.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = null;
            DA.GetData("Log", ref obj);

            Log m_log = GH_Log.ParseLog(obj);
            if (m_log == null) return;

            DA.SetData("Pith", m_log.Pith);
            DA.SetDataList("Knot axes", m_log.Knots.Select(x => x.Axis));
            DA.SetDataList("Knot radii", m_log.Knots.Select(x => x.Radius));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridMesh_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("39554eac-9da8-411f-a9ea-7d0ec320155d"); }
        }
    }
}