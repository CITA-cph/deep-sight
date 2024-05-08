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
using Rhino.Geometry;

using DeepSight.RhinoCommon;
using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;
using System.Linq.Expressions;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridSet: GH_Component
    {
        public Cmpt_GridSet()
          : base("GridSet", "GSet",
              "Set grid values.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddPointParameter("Sample points", "SP", "Coordinates to set in the grid.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Sample values", "SV", "Values to set at the coordinates in the grid.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Grid space", "GS", "Coordinates in grid-space (true) or world-space (false).", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Output grid.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var points = new List<GH_Point>();
            DA.GetDataList("Sample points", points);
            var objects = new List<object>();
            DA.GetDataList("Sample values", objects);
            bool gridspace = false;
            DA.GetData("Grid space", ref gridspace);

            object m_grid = null;
            DA.GetData("Grid", ref m_grid);

            if (!gridspace)
            {
                Transform imat;
                Transform mat = (m_grid as GH_Grid).Value.Transform.ToRhinoTransform();
                mat.TryGetInverse(out imat);

                for (int i = 0; i < points.Count; i++)
                {
                    points[i] = (GH_Point)points[i].Transform(imat);
                }
            }

            var fpoints = new int[points.Count * 3];
            for (int i = 0; i < points.Count; i++)
            {
                fpoints[i * 3] = (int)Math.Floor(points[i].Value.X + 0.5);
                fpoints[i * 3 + 1] = (int)Math.Floor(points[i].Value.Y + 0.5);
                fpoints[i * 3 + 2] = (int)Math.Floor(points[i].Value.Z + 0.5);
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
                List<GH_Number> values = null;
                try
                {
                    values = objects.Cast<GH_Number>().ToList();
                }
                catch
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not convert input values to numbers.");
                    return;
                }
                FGrid new_fgrid = new FGrid();
                new_fgrid.Transform = temp_fgrid.Transform;
                new_fgrid.SetValues(fpoints, values.Select(x => (float)x.Value).ToArray());

                DA.SetData("Grid", new GH_Grid(new_fgrid));
                DA.SetDataList("debug", debug);
            }
            else if (temp_vgrid != null)
            {
                List<GH_Vector> values = new List<GH_Vector>();
                try
                {
                    for(int i=0; i<objects.Count; i++)
                    {
                        GH_Vector vec = new GH_Vector();
                        GH_Convert.ToGHVector(objects[i], GH_Conversion.Both, ref vec);
                        values.Add(vec);
                    }
                    //values = objects.Cast<GH_Vector>().ToList();
                } catch
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not convert input values to vectors.");
                    return;
                }

                VGrid new_vgrid = new VGrid();
                new_vgrid.Transform = temp_vgrid.Transform;
                new_vgrid.SetValues(fpoints, values.Select(x => new Vec3<float>((float)(x.Value.X), (float)(x.Value.Y), (float)(x.Value.Z))).ToArray());

                DA.SetData("Grid", new GH_Grid(new_vgrid));
                DA.SetDataList("debug", debug);
            }
            else return;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSet_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("efff82d2-afbb-4a99-84cc-59c6002a617f"); }
        }
    }
}