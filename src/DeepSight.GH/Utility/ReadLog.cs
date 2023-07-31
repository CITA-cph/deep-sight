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
    public class Cmpt_ReadLog : GH_Component
    {
        public Cmpt_ReadLog()
          : base("ReadLog", "ReadLog",
              "Read contents of log file.",
              DeepSight.GH.Api.ComponentCategory, "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Directory", "D", "Directory of log file.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Optional. Name of log file.", GH_ParamAccess.item, "commits");
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Log", "L", "Log file as list of strings.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directory = "C:/tmp";
            string name = "commits";

            DA.GetData("Directory", ref directory);
            DA.GetData("Name", ref name);


            var lines = System.IO.File.ReadAllLines(System.IO.Path.Combine(directory, name + ".log"));

            DA.SetDataList("Log", lines);
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
            get { return new Guid("0c7d8d73-8150-4e34-91b5-a887e209a3fd"); }
        }
    }
}