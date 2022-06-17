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
using System.Diagnostics;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using DeepSight;
using RawLamb;
using System.Collections.Generic;

namespace RawLamb.GH.Components
{

    public class Cmpt_GridCreate : GH_Component
    {
        public Cmpt_GridCreate()
          : base("GridCreate", "GNew",
              "Create empty grid.",
              "RawLamb", "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Name of grid.", GH_ParamAccess.item, "Grid");
            pManager[0].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "New grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            string name = "Grid";
            DA.GetData(0, ref name);

            Grid grid = new Grid();
            grid.Name = name;


            DA.SetData(0, new GH_Grid(grid));

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
            get { return new Guid("03e5d07a-4776-4fff-bd66-8e6152e6f9b1"); }
        }
    }
}