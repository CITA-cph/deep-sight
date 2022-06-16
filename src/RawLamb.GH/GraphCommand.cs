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
using System.Drawing;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;

using Rhino.Geometry;
using GH_IO.Serialization;

namespace RawLamb.GH.Components
{

    public class Cmpt_GraphCommand : GH_Component
    {
        public Cmpt_GraphCommand()
          : base("GraphCommand", "GCom",
              "Run a Neo4j command.",
              "RawLamb", "Graph")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Graph", "G", "Graph connection.", GH_ParamAccess.item);
            pManager.AddTextParameter("Command", "C", "Neo4j command", GH_ParamAccess.item, "");

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "R", "Command result", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string command = "";
            object input = null;
            N4jGraph graph = null;

            DA.GetData("Graph", ref input);

            if (input is GH_N4jGraph)
                graph = (input as GH_N4jGraph).Value;
            else if (input is N4jGraph)
                graph = (input as N4jGraph);

            DA.GetData("Command", ref command);

            if (graph != null)
            {
                var res = graph.DoTransaction(command);
                DA.SetDataList("Result", res);
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("aaa70a33-7606-4cee-afba-ecabbd5fa18b"); }
        }
    }
}