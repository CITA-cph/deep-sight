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
using System.Xml.Schema;
using Grasshopper.Kernel;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridFilter : GH_Component
    {
        public Cmpt_GridFilter()
          : base("GridFilter", "GFil",
              "Filter a float or vector grid.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Float or Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Width of filtering kernel.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Iterations", "I", "Number of filter iterations.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Mode", "M", "Filter mode. 0 = gaussian, 1 = mean, 2 = median", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Filtered grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            int width = 1, iterations = 1, type = 0;
            DA.GetData(1, ref width);
            DA.GetData(2, ref iterations);
            DA.GetData(3, ref type);

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
                var new_grid = temp_fgrid.DuplicateGrid();

                Tools.Filter(new_grid, iterations, width, (FilterType)type);

                DA.SetData(0, new GH_Grid(new_grid));
            }
            else if (temp_vgrid != null)
            {
                int[] active = temp_vgrid.GetActiveVoxels();
                Vec3<float>[] values = temp_vgrid.GetValuesIndex(active);
                var x_grid = new FGrid("x_grid", 0f);
                var y_grid = new FGrid("y_grid", 0f);
                var z_grid = new FGrid("z_grid", 0f);
                x_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.X, 2)).ToArray());
                y_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.Y, 2)).ToArray());
                z_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.Z, 2)).ToArray());

                Tools.Filter(x_grid, iterations, width, (FilterType)type);
                Tools.Filter(y_grid, iterations, width, (FilterType)type);
                Tools.Filter(z_grid, iterations, width, (FilterType)type);

                VGrid new_grid = temp_vgrid.DuplicateGrid();

                float[] x_values = x_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray();
                float[] y_values = y_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray(); ;
                float[] z_values = z_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray(); ;

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = new Vec3<float>(x_values[i], y_values[i], z_values[i]);
                }

                new_grid.SetValues(active, values);

                DA.SetData(0, new GH_Grid(new_grid));
            }
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
            get { return new Guid("2D0A9F8B-8C0C-4890-921E-C291F780B7D6"); }
        }
    }
}