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
    public class Cmpt_AppendLog : GH_Component
    {
        public Cmpt_AppendLog()
          : base("AppendLog", "AddLog",
              "Append string to a log file.",
              DeepSight.GH.Api.ComponentCategory, "Utility")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to append to log file.", GH_ParamAccess.list);
            pManager.AddTextParameter("Directory", "D", "Directory of log file.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Optional. Name of log file.", GH_ParamAccess.item, "commits");
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> lines = new List<string>();
            string directory = "C:/tmp";
            string name = "commits";

            if (!DA.GetDataList("Text", lines)) return;
            DA.GetData("Directory", ref directory);
            DA.GetData("Name", ref name);

            System.IO.File.AppendAllLines(System.IO.Path.Combine(directory, name + ".log"), lines);
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
            get { return new Guid("343831b0-6c8a-4a1c-bd14-cb719ac07c7b"); }
        }
    }
}