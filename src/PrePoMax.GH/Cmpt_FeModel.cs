
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
using CaeModel;

namespace PrePoMax.GH
{
    public class Cmpt_FeModel : GH_Component
    {
        public Cmpt_FeModel()
            : base("LoadModel", "Load",
                "Load FeModel (.inp).",
                "PrePoMax", "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "FP", "File path for .inp model file.", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "Optional model name.", GH_ParamAccess.item, "FeModel");

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FeModel", "FM", "Imported FeModel.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string file_path = "";
            DA.GetData("File path", ref file_path);
            if (string.IsNullOrEmpty(file_path)) return;

            string name = "FeModel";
            DA.GetData("Name", ref name);

            var model = new FeModel(name);
            model.ImportModelFromInpFile(file_path, Rhino.RhinoApp.WriteLine);



            DA.SetData("FeModel", new GH_FeModel(model));

            Message = model.ToString();

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
            get { return new Guid("45094c4f-ef59-4a0a-b30a-6a3fcc21a024"); }
        }
    }
}
