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
using Grasshopper.Kernel;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridResample : GH_Component
    {
        public Cmpt_GridResample()
          : base("GridResample", "GRes",
              "Resample a grid to a new cell size.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridResample_01;
        public override Guid ComponentGuid => new Guid("0bf9ba62-b6af-4667-aed1-cfd8179eb911");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Size", "S", "New cell size.", GH_ParamAccess.item, 5);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Resampled grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            double m_size = 0.15;



            DA.GetData("Size", ref m_size);

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
                var ngrid = temp_fgrid.DuplicateGrid();
                var new_grid = Tools.Resample(ngrid, m_size);

                new_grid.Name = temp_fgrid.Name;

                DA.SetData(0, new GH_Grid(new_grid));
            }
            else if (temp_vgrid != null)
            {
                int[] active = temp_vgrid.GetActiveVoxels();
                Vec3<float>[] values = temp_vgrid.GetValuesIndex(active);
                var x_grid = new FGrid("x_grid", 0f);
                var y_grid = new FGrid("y_grid", 0f);
                var z_grid = new FGrid("z_grid", 0f);
                x_grid.SetValues(active, values.Select(x => (float)x.X).ToArray());
                y_grid.SetValues(active, values.Select(x => (float)x.Y).ToArray());
                z_grid.SetValues(active, values.Select(x => (float)x.Z).ToArray());

                Tools.Resample(x_grid, m_size);
                Tools.Resample(y_grid, m_size);
                Tools.Resample(z_grid, m_size);

                VGrid new_grid = temp_vgrid.DuplicateGrid();

                float[] x_values = x_grid.GetValuesIndex(active).Cast<float>().ToArray();
                float[] y_values = y_grid.GetValuesIndex(active).Cast<float>().ToArray();
                float[] z_values = z_grid.GetValuesIndex(active).Cast<float>().ToArray();

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = new Vec3<float>(x_values[i], y_values[i], z_values[i]);
                }

                new_grid.SetValues(active, values);

                DA.SetData(0, new GH_Grid(new_grid));
            }
        }
    }
}