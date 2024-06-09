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

using Grid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridFilter : GH_Component
    {
        public Cmpt_VGridFilter()
          : base("VGridFilter", "VGFil",
              "Filter a vector grid.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Width", "W", "Width of filtering kernel.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Iterations", "I", "Number of filter iterations.", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Mode", "M", "Filter mode (0 = Gaussian, 1 = Mean, 2 = Median).", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Filtered grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid = null;
            int width = 1, iterations = 1, type = 0;

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            DA.GetData(1, ref width);
            DA.GetData(2, ref iterations);
            DA.GetData(3, ref type);

            int[] active = temp_grid.GetActiveVoxels();
            Vec3<float>[] values = temp_grid.GetValuesIndex(active);
            var x_grid = new FloatGrid("x_grid", 0f);
            var y_grid = new FloatGrid("y_grid", 0f);
            var z_grid = new FloatGrid("z_grid", 0f);
            x_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.X,2)).ToArray());
            y_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.Y,2)).ToArray());
            z_grid.SetValues(active, values.Select(x => (float)Math.Pow(x.Z,2)).ToArray());

            Tools.Filter(x_grid, iterations, width, (FilterType)type);
            Tools.Filter(y_grid, iterations, width, (FilterType)type);
            Tools.Filter(z_grid, iterations, width, (FilterType)type);

            Grid new_grid = temp_grid.DuplicateGrid();

            float[] x_values = x_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray();
            float[] y_values = y_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray(); ;
            float[] z_values = z_grid.GetValuesIndex(active).Select(x => (float)Math.Sqrt(x)).ToArray(); ;

            for (int i=0; i<values.Length; i++)
            {
                values[i] = new Vec3<float>(x_values[i], y_values[i], z_values[i]);
            }

            new_grid.SetValues(active, values);


            DA.SetData(0, new GH_Grid(new_grid));
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.VGridResample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("691C3FAB-E4C7-4C10-AEEE-AA2E1D199CD1"); }
        }
    }
}