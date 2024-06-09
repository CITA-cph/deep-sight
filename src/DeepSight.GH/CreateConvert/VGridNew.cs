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
using System.Diagnostics;

using Grasshopper.Kernel;

using Grid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridCreate : GH_Component
    {
        public Cmpt_VGridCreate()
          : base("VGridCreate", "VGNew",
              "Create empty vector grid.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of vector grid.", GH_ParamAccess.item, "default");
            pManager[0].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VGrid", "VG", "New vector grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            string name = "default";
            DA.GetData(0, ref name);

            Grid grid = new Grid();
            grid.Name = name;


            DA.SetData(0, new GH_Grid(grid));

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.VGridNew_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("1babdbfb-6330-473f-b818-83184eacbc6e"); }
        }
    }
}