
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

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using CaeResults;

namespace PrePoMax.GH
{
    public class Cmpt_ResultsValues : GH_Component
    {
        public Cmpt_ResultsValues()
            : base("FrdValues", "FrdValues",
                "Get result values from CCX results (.frd).",
                "PrePoMax", "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FeResults", "FR", "FeResults from simulation.", GH_ParamAccess.item);
            pManager.AddTextParameter("Field name", "FN", "Name of field to retrieve", GH_ParamAccess.item, "U");
            pManager.AddTextParameter("Component name", "CN", "Name of field component to retrieve.", GH_ParamAccess.item, "X");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result field name", "RFN", "Name of results.", GH_ParamAccess.item);
            pManager.AddTextParameter("Result component name", "RCN", "Name of results.", GH_ParamAccess.item);
            pManager.AddPointParameter("Result nodes", "RN", "Nodes of results.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Result values", "RV", "Values of results.", GH_ParamAccess.list);
            pManager.AddGenericParameter("debug", "d", "debugging info", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            object fres_obj = null;
            DA.GetData("FeResults", ref fres_obj);

            var results = GH_FeResults.Parse(fres_obj);
            Message = String.Format("results: {0}", results);
            if (results == null) return;

            string field_name = "U";
            string component_name = "X";
            int step_id = 1;

            DA.GetData("Field name", ref field_name);
            DA.GetData("Component name", ref component_name);

            var debug = new List<object>();
            var field_and_component_names = results.GetAllFiledNameComponentNames();
            foreach (string fn in field_and_component_names.Keys)
            {
                debug.Add(fn);
                foreach (string cn in field_and_component_names[fn])
                {
                    debug.Add(string.Format("    {0}", cn));
                }
            }

            if (!results.GetAllFieldNames().Contains(field_name))
                field_name = results.GetAllFieldNames().FirstOrDefault();

            if (!field_and_component_names[field_name].Contains(component_name))
                component_name = field_and_component_names[field_name].FirstOrDefault();

            /*
            var component_names = results.GetAllFiledNameComponentNames();
            if (component_names.Keys.Count < 1) return;

            var field_name = component_names.Keys.First();
            var component_name = component_names[field_name][0];
            */

            var data = results.GetFieldData(field_name, component_name, step_id, 1);
            float[] values;
            double[][] nodeCoor;

            results.GetNodesAndValues(data, results.Mesh.Nodes.Keys.ToArray(), out nodeCoor, out values);

            var unitSystem = results.UnitSystem;
            var inUnitSystem = Rhino.UnitSystem.Meters;

            if (unitSystem.UnitSystemType == CaeGlobals.UnitSystemType.M_KG_S_C)
                inUnitSystem = Rhino.UnitSystem.Meters;
            else if (unitSystem.UnitSystemType == CaeGlobals.UnitSystemType.MM_TON_S_C)
                inUnitSystem = Rhino.UnitSystem.Millimeters;
            double scaling = RhinoMath.UnitScale(inUnitSystem, RhinoDoc.ActiveDoc.ModelUnitSystem);

            var points = new GH_Point[nodeCoor.Length];
            for (int i = 0; i < nodeCoor.Length; i++)
            {
                points[i] = new GH_Point(new Point3d(nodeCoor[i][0], nodeCoor[i][1], nodeCoor[i][2]) * scaling);
            }

            DA.SetData("Result field name", field_name);
            DA.SetData("Result component name", component_name);
            DA.SetDataList("Result nodes", points);
            DA.SetDataList("Result values", values);
            DA.SetDataList("debug", debug);

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
            get { return new Guid("6d74adf6-eef9-447c-8fdf-ca5abae1a2b4"); }
        }
    }
}
