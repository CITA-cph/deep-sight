
/*
 * RawLamb
 * Copyright 2023 Tom Svilans
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
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using CaeResults;

namespace PrePoMax.GH
{
    public class Cmpt_ResultsFrd : GH_Component
    {
        public Cmpt_ResultsFrd()
            : base("LoadResults", "LoadFrd",
                "Load CCX results (.frd).",
                "PrePoMax", "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "FP", "File path for .frd results file.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FeResults", "FR", "Results of CCX job.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string file_path = "";
            DA.GetData("File path", ref file_path);
            if (string.IsNullOrEmpty(file_path)) return;

            if (!System.IO.File.Exists(file_path)) return;

            var results = FrdFileReader.Read(file_path);
            if (results == null) return;

            //results.Mesh.ScaleParts(results.Mesh.Parts.Keys.ToArray(), new double[] { 0, 0, 0 }, new double[] { 1000, 1000, 1000 }, false, null);


            DA.SetData("FeResults", new GH_FeResults(results));

            Message = results.ToString();


        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            //  return Properties.Resources.GridSample_01;
            }
        }

        public override Guid ComponentGuid
        {
            /*
                0fe48fc4-68ff-484a-949c-37bae7f882c1
                45094c4f-ef59-4a0a-b30a-6a3fcc21a024
             */
            get { return new Guid("54552501-22ed-466f-b9ff-3efd9fa7d3dc"); }
        }
    }
}
