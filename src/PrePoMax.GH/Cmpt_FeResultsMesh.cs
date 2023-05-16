
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
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using CaeResults;
using Grasshopper;

namespace PrePoMax.GH
{
    public class Cmpt_MeshResults : GH_Component
    {
        public Cmpt_MeshResults()
            : base("FrdMesh", "FrdMesh",
                "Get mesh from results (.frd).",
                "PrePoMax", "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FeResults", "FR", "FeResults from simulation.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Step ID", "S", "Step ID to get mesh from.", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("Deformation factor", "DF", "Factor to control strength of deformation.", GH_ParamAccess.item, 1.0);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Undeformed mesh", "UM", "Undeformed mesh from simulation.", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Deformed mesh", "DM", "Deformed mesh from simulation", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Node indices", "N", "Indices of nodes that map onto the meshes.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            object fres_obj = null;
            DA.GetData("FeResults", ref fres_obj);
            var fres = GH_FeResults.Parse(fres_obj);

            Message = String.Format("fres: {0}", fres);

            if (fres == null) return;

            double factor = 1.0;
            DA.GetData("Deformation factor", ref factor);

            var unitSystem = fres.UnitSystem;
            var inUnitSystem = Rhino.UnitSystem.Meters;

            

            if (unitSystem.UnitSystemType == CaeGlobals.UnitSystemType.M_KG_S_C)
                inUnitSystem = Rhino.UnitSystem.Meters;
            else if (unitSystem.UnitSystemType == CaeGlobals.UnitSystemType.MM_TON_S_C)
                inUnitSystem = Rhino.UnitSystem.Millimeters;
            double scaling = RhinoMath.UnitScale(inUnitSystem, RhinoDoc.ActiveDoc.ModelUnitSystem);
            //scaling = 1.0;

            Message = String.Format("Scaling: {0}", scaling);

            int step_id = 1;
            DA.GetData("Step ID", ref step_id);

            var increments = fres.GetIncrementIds(step_id);
            var last_increment = increments.Last();

            var undeformed_meshes = fres.ToMesh(out int[][] nodeIds, scaling);
            var deformed_meshes = new Mesh[undeformed_meshes.Length];

            DataTree<Mesh> umesh_tree = new DataTree<Mesh>();
            DataTree<Mesh> dmesh_tree = new DataTree<Mesh>();
            DataTree<int> nodeId_tree = new DataTree<int>();

            for (int i = 0; i < undeformed_meshes.Length; i++)
            {
                deformed_meshes[i] = fres.DeformMesh(undeformed_meshes[i], nodeIds[i], step_id, scaling * factor);

                var path = new GH_Path(i);
                umesh_tree.Add(undeformed_meshes[i], path);
                dmesh_tree.Add(deformed_meshes[i], path);
                nodeId_tree.AddRange(nodeIds[i], path);
            }

            DA.SetDataTree(0, umesh_tree);
            DA.SetDataTree(1, dmesh_tree);
            DA.SetDataTree(2, nodeId_tree);

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
            get { return new Guid("44609ed3-e18a-4666-b8ea-2b6dfb0be096"); }
        }
    }
}
