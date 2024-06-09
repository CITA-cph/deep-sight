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
using System.Threading.Tasks;
using System.Diagnostics;
using Rhino.Geometry;
using Grasshopper.Kernel;

using VGrid = DeepSight.Vec3fGrid;
using FGrid = DeepSight.FloatGrid;
using Eto.Forms;
using DeepSight.RhinoCommon;
using System.Linq;
using System.Collections.Generic;
using Grasshopper.Kernel.Types;
using System.Runtime.InteropServices;

namespace DeepSight.GH.Components
{

public class Cmpt_WarpSrf : GH_Component
    {
        public Cmpt_WarpSrf()
          : base("GridWarp", "GridWarp",
              "Warp a grid to surface UVW coordinates.",
              DeepSight.GH.Api.ComponentCategory, "Grid")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.VGridResample_01;
        public override Guid ComponentGuid => new Guid("54D9A22F-3B2F-4AEE-AA5F-52035C7A4E3B");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to warp.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Source Surface", "S", "Surface to warp from.", GH_ParamAccess.item);
            pManager.AddSurfaceParameter("Target Surface", "T", "Surface to warp to.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Resulting Grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();


            Surface tosrf = null;
            DA.GetData("Target Surface", ref tosrf);
            if (tosrf == null) return;

            double x = tosrf.Domain(0).T1;
            double y = tosrf.Domain(1).T1;
            Plane xy = new Plane(new Point3d(0, 0, 0), new Vector3d(0, 0, 1));
            Surface fromsrf = new Rhino.Geometry.PlaneSurface(xy, new Rhino.Geometry.Interval(0, x), new Rhino.Geometry.Interval(0, y));

            DA.GetData("Source Surface", ref fromsrf);
            Rhino.Geometry.Morphs.SporphSpaceMorph S = new Rhino.Geometry.Morphs.SporphSpaceMorph(fromsrf, tosrf);


            object m_grid = null;
            DA.GetData("Grid", ref m_grid);

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
                int[] active = temp_fgrid.GetActiveVoxels();
                float[] values = temp_fgrid.GetValuesIndex(active);

                int[] active_warped = new int[active.Length];

                for (int i = 0; i < active.Length; i += 3)
                {
                    Point3d pt = new Point3d(active[i], active[i + 1], active[i + 2]);
                    Point3d npt = S.MorphPoint(pt);
                    active_warped[i] = (int)npt.X;
                    active_warped[i + 1] = (int)npt.Y;
                    active_warped[i + 2] = (int)npt.Z;
                }

                FGrid new_grid = new FGrid("warped_grid", 0);
                new_grid.SetValues(active_warped, values);
                debug.Add(string.Format("{0} : Set values.", stopwatch.ElapsedMilliseconds));

                DA.SetDataList("debug", debug);
                DA.SetData("Grid", new GH_Grid(new_grid));
            } 
            else if (temp_vgrid != null)
            {
                int[] active = temp_vgrid.GetActiveVoxels();
                Vec3<float>[] values = temp_vgrid.GetValuesIndex(active);

                int[] active_warped = new int[active.Length];

                for (int i = 0; i < active.Length; i += 3)
                {
                    Point3d pt = new Point3d(active[i], active[i + 1], active[i + 2]);
                    Point3d npt = S.MorphPoint(pt);
                    active_warped[i] = (int)npt.X;
                    active_warped[i + 1] = (int)npt.Y;
                    active_warped[i + 2] = (int)npt.Z;
                }

                VGrid new_grid = new VGrid("warped_grid", new float[3] { 0, 0, 0 });
                new_grid.SetValues(active_warped, values);
                debug.Add(string.Format("{0} : Set values.", stopwatch.ElapsedMilliseconds));

                DA.SetDataList("debug", debug);
                DA.SetData("Grid", new GH_Grid(new_grid));
            }
            

        }
    }
}