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
using System.Diagnostics;

using Grasshopper.Kernel;

using VGrid = DeepSight.Vec3fGrid;
using FGrid = DeepSight.FloatGrid;
using Eto.Forms;
using DeepSight.RhinoCommon;
using System.Linq;
using System.Collections.Generic;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridDeconstruct : GH_Component
    {
        public Cmpt_VGridDeconstruct()
          : base("VGridDeconstruct", "DeconVGrid",
              "Separate a VGrid into three float grids.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);

            pManager.AddGenericParameter("X Grid", "X", "X-channel grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Y Grid", "Y", "Y-channel grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Z Grid", "Z", "Z-channel grid.", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            object m_grid = null;
            DA.GetData(0, ref m_grid);

            VGrid temp_grid;
            if (m_grid is VGrid)
                temp_grid = m_grid as VGrid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as VGrid;
            else
                return;


            FGrid X_grid = new FGrid("X Grid", 0);
            X_grid.Transform = temp_grid.Transform;

            FGrid Y_grid = new FGrid("Y Grid", 0);
            Y_grid.Transform = temp_grid.Transform;

            FGrid Z_grid = new FGrid("Z Grid", 0);
            Z_grid.Transform = temp_grid.Transform;

            debug.Add(string.Format("{0} : Initialized output XYZ grids.", stopwatch.ElapsedMilliseconds));


            int[] active = temp_grid.GetActiveVoxels();
            Vec3<float>[] values = temp_grid.GetValuesIndex(active);

            debug.Add(string.Format("{0} : Got active and vector values.", stopwatch.ElapsedMilliseconds));


            float[] X_values = new float[values.Length];
            float[] Y_values = new float[values.Length];
            float[] Z_values = new float[values.Length];

            X_values = values.Select(x => x.X).ToArray();
            Y_values = values.Select(x => x.Y).ToArray();
            Z_values = values.Select(x => x.Z).ToArray();

            debug.Add(string.Format("{0} : Split vector array.", stopwatch.ElapsedMilliseconds));


            X_grid.SetValues(active, X_values);
            Y_grid.SetValues(active, Y_values);
            Z_grid.SetValues(active, Z_values);

            debug.Add(string.Format("{0} : Set values.", stopwatch.ElapsedMilliseconds));


            DA.SetDataList(0, debug);
            DA.SetData("X Grid", new GH_Grid(X_grid));
            DA.SetData("Y Grid", new GH_Grid(Y_grid));
            DA.SetData("Z Grid", new GH_Grid(Z_grid));

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
            get { return new Guid("712C18B0-EC8B-471F-955D-5451238AA16D"); }
        }
    }
}