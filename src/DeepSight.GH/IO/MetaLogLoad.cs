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
using System.Collections.Generic;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSightCommon;

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

            m_path = m_path.Trim();

            if (System.IO.File.Exists(m_path))
            {

                var infolog = DeepSightCommon.InfoLog.Read(m_path);

                var log = new RawLamb.Log();
                string name = System.IO.Path.GetFileNameWithoutExtension(m_path);
                log.Name = name;

                log.Pith = new Polyline();

                for (int i = 0; i < infolog.Pith.Length; ++i)
                {
                    log.Pith.Add(new Point3d(infolog.Pith[i].Item1, infolog.Pith[i].Item2, i * 10));
                }

                log.Knots = new List<RawLamb.Knot>();
                for (int i = 0; i < infolog.Knots.Length; ++i)
                {
                    DeepSightCommon.Knot iknot = infolog.Knots[i];
                    var rknot = new RawLamb.Knot();
                    rknot.Length = iknot.Length;
                    rknot.Volume = iknot.Volume;
                    rknot.Radius = iknot.Radius;
                    rknot.Index = iknot.Index;
                    rknot.DeadKnotRadius = iknot.DeadKnotBorder;
                    rknot.Axis = new Line(
                        new Point3d(
                            iknot.Start.Item1,
                            iknot.Start.Item2,
                            iknot.Start.Item3
                            ),
                         new Point3d(
                            iknot.End.Item1,
                            iknot.End.Item2,
                            iknot.End.Item3
                            )
                         );

                    log.Knots.Add(rknot);
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