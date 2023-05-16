
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
    public class Cmpt_QueryResults : GH_Component
    {
        public Cmpt_QueryResults()
            : base("FrdQuery", "FrdQuery",
                "Query results (.frd).",
                "PrePoMax", "Tools")
        {
        }

        FeResults m_results = null;
        Octree.PointOctree<int> m_octree = null;
        double m_scale = 1000.0;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FeResults", "FR", "FeResults from simulation.", GH_ParamAccess.item);
            pManager.AddPointParameter("Query points", "P", "Points to query the results.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Query range", "R", "Range to query the results.", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Step ID", "S", "Step ID to get mesh from.", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("Field name", "FN", "Field to query in results.", GH_ParamAccess.item, "STRESS");
            pManager.AddTextParameter("Component name", "CN", "Component of field to query in results.", GH_ParamAccess.item, "SGN-MAX-ABS-PRI");

            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Values", "V", "Value of field at query points.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double max_distance = 1;
            DA.GetData("Query range", ref max_distance);

            object fres_obj = null;
            DA.GetData("FeResults", ref fres_obj);
            var fres = GH_FeResults.Parse(fres_obj);
            if (fres == null) return;

            bool rebuild = false;
            if (m_results == null || m_results != fres)
            {
                rebuild = true;
                m_results = fres;
            }
            if (m_octree == null || rebuild)
            {
                var bb = m_results.Mesh.BoundingBox;
                var size = bb.GetDiagonal() * m_scale;
                Octree.Point min = new Octree.Point(bb.MinX * m_scale, bb.MinY * m_scale, bb.MinZ * m_scale);
                Octree.Point max = new Octree.Point(bb.MaxX * m_scale, bb.MaxY * m_scale, bb.MaxZ * m_scale);

                m_octree = new Octree.PointOctree<int>(size, (min + max) * 0.5, size / 100);

                var element_cgs = new Dictionary<int, Point3d>();
                foreach (var element in m_results.Mesh.Elements)
                {
                    var cg = element.Value.GetCG(m_results.Mesh.Nodes);
                    element_cgs.Add(element.Key, new Point3d(cg[0], cg[1], cg[2]) * m_scale);
                    m_octree.Add(element.Key, new Octree.Point(cg[0] * m_scale, cg[1] * m_scale, cg[2] * m_scale));
                }
            }

            var points = new List<Point3d>();
            DA.GetDataList("Query points", points);

            var field_name = "STRESS";
            DA.GetData("Field name", ref field_name);

            var component_name = "SGN-MAX-ABS-PRI";
            DA.GetData("Component name", ref component_name);

            int step_id = 1;
            DA.GetData("Step ID", ref step_id);

            var increments = fres.GetIncrementIds(step_id);
            var last_increment = increments.Last();

            var fdata = fres.GetFieldData(field_name, component_name, step_id, last_increment);

            //var intrp = new ResultsInterpolator(fres.GetAllNodesCellsAndValues(fdata));


            var results = new double[points.Count];

            //Parallel.For(0, points.Count, i =>
            for (int i = 0; i < points.Count; ++i)
            {
                var pt = points[i];
                var indices = m_octree.GetNearby(new Octree.Point(pt.X, pt.Y, pt.Z), max_distance);

                if (indices.Length < 1)
                {
                    //throw new Exception("No indices found!");
                    continue;
                }

                foreach (var index in indices)
                {
                    var element = fres.Mesh.Elements[index];
                    var nodeIds = element.NodeIds;

                    var vecs = new List<Vector3d>();
                    foreach (var nid in nodeIds)
                    {
                        var ncoor = fres.Mesh.Nodes[nid].Coor;
                        vecs.Add(new Vector3d(ncoor[0] * m_scale, ncoor[1] * m_scale, ncoor[2] * m_scale));
                    }

                    double[] weights;
                    if (Utility.TetraBarycentric(vecs[0], vecs[1], vecs[2], vecs[3], new Vector3d(pt), out weights))
                    {
                        var values = fres.GetValues(fdata, nodeIds);
                        for (int j = 0; j < weights.Length; j++)
                        {
                            values[j] = values[j] * (float)weights[j];
                        }

                        var value = values.Sum();

                        results[i] = value;
                        break;
                    }
                }
            }//);

            DA.SetDataList("Values", results);

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
                45094c4f-ef59-4a0a-b30a-6a3fcc21a024
             */
            get { return new Guid("0fe48fc4-68ff-484a-949c-37bae7f882c1"); }
        }
    }
}
