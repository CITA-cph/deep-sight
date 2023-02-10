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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridMaterial : GH_Component
    {
        public Cmpt_GridMaterial()
          : base("GridMaterial", "GMat",
              "Set material parameters for a grid.",
              DeepSight.GH.Api.ComponentCategory, "Biopol")
        {
        }

        protected double Density = 1.0;
        protected double DensityVariance = 1.0;
        protected Random Rand = null;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to set to material.", GH_ParamAccess.item);
            pManager.AddTextParameter("Material name", "MN", "Name of material.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Material density", "MD", "Density of material.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Material variance", "MV", "Variance of material density.", GH_ParamAccess.item, 0.1);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to assign material properties to.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "debugging", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (Rand == null) Rand = new Random();
            var debug = new List<string>();

            object m_grid = null;
            Grid grid = null;

            DA.GetData(0, ref m_grid);

            if (m_grid is Grid)
                grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            string mat_name = "Material";
            double density = 1.0, variance = 0.1;

            DA.GetData("Material name", ref mat_name);
            DA.GetData("Material density", ref density);
            DA.GetData("Material variance", ref variance);

            var ngrid = new Grid(mat_name, 0.0f);
            ngrid.Transform = grid.Transform;

            var active = grid.GetActiveVoxels();

            var N = active.Length / 3;

            var values = new float[N];
            var random = new Random();

            for (int i = 0; i < N; ++i)
            {
                values[i] = (float)(density - random.NextDouble() * variance);
            }

            ngrid.SetValues(active, values);

            DA.SetData("Grid", new GH_Grid(ngrid));
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
            get { return new Guid("4017e195-251e-4e75-bc2c-9a6d0d6328bf"); }
        }
    }
}