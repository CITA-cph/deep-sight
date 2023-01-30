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
using Grasshopper.Kernel;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridConvert : GH_Component
    {
        public Cmpt_GridConvert()
          : base("GridConvert", "GCon",
              "Convert a grid from level-set to density.",
              DeepSight.GH.Api.ComponentCategory, "Grid")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "T", "Threshold below which to deactivate voxels.", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Fog/density grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid grid = null;

            DA.GetData("Grid", ref m_grid);
            if (m_grid is Grid)
                grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                grid = (m_grid as GH_Grid).Value;
            else
                return;

            double threshold = 0.0;
            DA.GetData("Threshold", ref threshold);

            /*
            var ngrid = new Grid(0.0f);
            ngrid.Class = GridClass.GRID_FOG_VOLUME;
            ngrid.Name = grid.Name;
            ngrid.Transform = grid.Transform;

            var active = grid.ActiveVoxels();
            var N = active.Length / 3;
            int ii, jj, kk;
            for (int i = 0; i < N; ++i)
            {
                ii = active[i * 3 + 0];
                jj = active[i * 3 + 1];
                kk = active[i * 3 + 2];
                float v = grid[ii, jj, kk];
                if (v < threshold)
                {
                    ngrid[ii, jj, kk] = Math.Abs(v);
                }
            }
            */

            var ngrid = grid.Duplicate();
            ngrid.Name = grid.Name;
            ngrid.SdfToFog(1.0f);

            var active = ngrid.ActiveVoxels();
            var N = active.Length / 3;

            int ii, jj, kk;
            for (int i = 0; i < N; ++i)
            {
                ii = active[i * 3 + 0]; jj = active[i * 3 + 1]; kk = active[i * 3 + 2];
                if (ngrid[ii, jj, kk] < threshold)
                {
                    ngrid[ii, jj, kk] = 0.0f;
                    ngrid.SetActiveState(ii, jj, kk, false);
                }
            }

            DA.SetData(0, new GH_Grid(ngrid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridResample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("795d171e-6df9-4058-8d2e-a7c0ed8fd38e"); }
        }
    }
}