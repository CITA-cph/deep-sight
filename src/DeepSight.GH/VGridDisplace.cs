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

using Grid = DeepSight.Vec3fGrid;
using Rhino.Geometry;
using DeepSight.RhinoCommon;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridDisplace : GH_Component
    {
        public Cmpt_VGridDisplace()
          : base("VGridDisplace", "VGDisp",
              "Displace one vector grid using another as a map.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displace Map", "D", "Displacement map, with vec3 offsets as voxel values", GH_ParamAccess.item);
            pManager.AddNumberParameter("Interpolation", "t", "Interpolation parameter", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Grid space", "GS", "Coordinates in grid-space (true) or world-space (false).", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Displaced Grid", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            object m_grid = null;
            DA.GetData("Grid", ref m_grid);

            Grid temp_grid;
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            if (temp_grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unsupported grid type ({m_grid})");
                return;
            }

            m_grid = null;
            DA.GetData("Displace Map", ref m_grid);

            Grid disp_grid;
            if (m_grid is Grid)
                disp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                disp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            if (disp_grid == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unsupported grid type ({m_grid})");
                return;
            }

            double t = 1.0;
            DA.GetData("Interpolation", ref t);

            int[] active = temp_grid.GetActiveVoxels();
            Vec3<float>[] values = temp_grid.GetValuesIndex(active);

            double[] coord = active.Select(i => (double)i).ToArray();

            bool gridspace = false;
            DA.GetData(3, ref gridspace);
            if (!gridspace)
            {
                Transform imat;
                Transform mat = temp_grid.Transform.ToRhinoTransform();
                mat.TryGetInverse(out imat);

                for (int i = 0; i < active.Length; i += 3)
                {
                    Point3d pt = new Point3d(coord[i], coord[i + 1], coord[i + 2]);
                    pt.Transform(imat);
                    coord[i] = pt.X;
                    coord[i + 1] = pt.Y;
                    coord[i + 2] = pt.Z;
                }
            }

            Vec3<float>[] disp = disp_grid.GetValuesWorld(coord);

            int[] new_coord = new int[coord.Length];

            for (int i = 0; i < active.Length; i+=3)
            {
                Vec3<float> pt = new Vec3<float>((float)coord[i], (float)coord[i + 1], (float)coord[i + 2]);
                
                new_coord[i] = (int)Math.Round(pt.X + t * disp[i / 3].X);
                new_coord[i+1] = (int)Math.Round(pt.Y + t * disp[i / 3].Y);
                new_coord[i+2] = (int)Math.Round(pt.Z + t * disp[i / 3].Z);
            }

            Grid new_grid = new Grid("newgrid", new float[3] { 0, 0, 0 });
            new_grid.Transform = temp_grid.Transform;

            debug.Add("New coordinates match values: " + new_coord.Length.ToString() + ", " + values.Length.ToString());
            new_grid.SetValues(new_coord, values);

            DA.SetDataList("debug", debug);
            DA.SetData("Grid", new GH_Grid(new_grid));

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
            get { return new Guid("C0E8C0E5-1659-48FD-9775-FDA83CB0DDD6"); }
        }
    }
}