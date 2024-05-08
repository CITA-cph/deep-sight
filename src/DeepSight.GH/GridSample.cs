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
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using DeepSight.RhinoCommon;
using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;
using Rhino.Geometry;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridSample : GH_Component
    {
        public Cmpt_GridSample()
          : base("GridSample", "GSmp",
              "Query a point in the grid.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddPointParameter("Sample points", "SP", "Points to query in the grid.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Grid space", "GS", "Coordinates in grid-space (true) or world-space (false).", GH_ParamAccess.item, false);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Values", "V", "Grid value at each sampling point.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var points = new List<GH_Point>();
            DA.GetDataList("Sample points", points);
            var coords = new double[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                coords[i * 3] = points[i].Value.X;
                coords[i * 3 + 1] = points[i].Value.Y;
                coords[i * 3 + 2] = points[i].Value.Z;
            }

            bool gridspace = false;
            DA.GetData("Grid space", ref gridspace);

            object m_grid = null;
            DA.GetData("Grid", ref m_grid);

            if (!gridspace)
            {
                Transform imat;
                Transform mat = (m_grid as GH_Grid).Value.Transform.ToRhinoTransform();
                mat.TryGetInverse(out imat);

                for (int i = 0; i < points.Count; ++i)
                {
                    points[i] = (GH_Point)points[i].Transform(imat);
                }
            }

            FGrid temp_fgrid = null;
            VGrid temp_vgrid = null;

            if (m_grid is FGrid)
                temp_fgrid = m_grid as FGrid;
            else if (m_grid is VGrid)
                temp_vgrid = m_grid as VGrid;
            else if (m_grid is GH_Grid)
                if ((m_grid as GH_Grid).Value is FGrid)
                    temp_fgrid = (m_grid as GH_Grid).Value as FGrid;
                else if ((m_grid as GH_Grid).Value is VGrid)
                    temp_vgrid = (m_grid as GH_Grid).Value as VGrid;
                else
                    return;
            else
                return;

            if (temp_fgrid != null)
            {
                var values = temp_fgrid.GetValuesWorld(coords);

                DA.SetDataList("Values", values.Select(x => new GH_Number(x)));

                DA.SetDataList("debug", debug);
            } 
            else if (temp_vgrid != null)
            {
                var values = temp_vgrid.GetValuesWorld(coords);

                DA.SetDataList("Values", values.Select(x => new GH_Vector(new Rhino.Geometry.Vector3d(x.X,x.Y,x.Z))));

                DA.SetDataList("debug", debug);
            }
            else return;

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f6b26d6b-0962-4e4d-a632-33d227af57df"); }
        }
    }
}