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

namespace DeepSight.GH.Components
{
    public class Cmpt_MetaLogLoad : GH_Component
    {
        public Cmpt_MetaLogLoad()
          : base("MetaLogLoad", "MLoad",
              "Load a metalog dataset.", 
              DeepSight.GH.Api.ComponentCategory, "IO")
        {
        }



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Filepath", "FP", "File path of the MetaLog.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Log", "L", "MetaLog.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string m_path = String.Empty;

            DA.GetData("Filepath", ref m_path);
            if (System.IO.File.Exists(m_path))
            {
                var log = new RawLamb.Log();
                try
                {
                    log.ReadInfoLog(m_path);
                    string name = System.IO.Path.GetFileNameWithoutExtension(m_path);
                    log.Name = name;
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                    return;
                }
                DA.SetData("Log", new GH_Log(log));
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File does not exist...");
            }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridLoad_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("4b452b45-9ee0-488c-8508-b1662885d370"); }
        }
    }
}