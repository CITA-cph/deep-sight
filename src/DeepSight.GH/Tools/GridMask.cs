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
using Eto.Forms;
using Grasshopper.Kernel;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridMask: GH_Component
    {
        public Cmpt_GridMask()
          : base("GridMask", "GMask",
              "Mask a grid with the active values of another.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridFilter_01;
        public override Guid ComponentGuid => new Guid("2406FA31-D1C6-4F73-8669-62525F13E43C");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mask", "M", "Match active states to this grid.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Resulting grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            FGrid temp_fgrid = null, mask_fgrid = null;
            VGrid temp_vgrid = null, mask_vgrid = null;
            //Grid temp_grid, mask_grid = null;

            DA.GetData("Mask", ref m_grid);
            if (m_grid is FGrid)
                mask_fgrid = m_grid as FGrid;
            else if (m_grid is VGrid)
                mask_vgrid = m_grid as VGrid;
            else if (m_grid is GH_Grid)
                if ((m_grid as GH_Grid).Value is FGrid)
                    mask_fgrid = (m_grid as GH_Grid).Value as FGrid;
                else if ((m_grid as GH_Grid).Value is VGrid)
                    mask_vgrid = (m_grid as GH_Grid).Value as VGrid;
                else
                    return;
            else
                return;

            int[] active = null;
            if (mask_fgrid != null) {
                active = mask_fgrid.GetActiveVoxels();
            } else if (mask_vgrid != null)
            {
                active = mask_vgrid.GetActiveVoxels();
            }

            DA.GetData("Grid", ref m_grid);
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
                var new_grid = new FGrid("new_grid", 0f);
                new_grid.Transform = temp_fgrid.Transform;

                float[] values = temp_fgrid.GetValuesIndex(active);
                new_grid.SetValues(active, values);

                DA.SetData(0, new GH_Grid(new_grid));

            }
            else if (temp_vgrid != null)
            {
                var new_grid = new VGrid("new_grid", new float[3] { 0, 0, 0 });
                new_grid.Transform = temp_vgrid.Transform;

                Vec3<float>[] values = temp_vgrid.GetValuesIndex(active);
                new_grid.SetValues(active, values);

                DA.SetData(0, new GH_Grid(new_grid));

            }
        }
    }
}