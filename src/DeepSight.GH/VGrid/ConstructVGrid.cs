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
using Grasshopper.Kernel.Types;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridConstruct : GH_Component
    {
        public Cmpt_VGridConstruct()
          : base("VGridConstruct", "ConVGrid",
              "Combine three float grids into a VGrid.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.VGridNew_01;
        public override Guid ComponentGuid => new Guid("9088B78F-7E86-4A1E-94AE-351D05ABE386");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("X Grid", "X", "X-channel grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Y Grid", "Y", "Y-channel grid.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Z Grid", "Z", "Z-channel grid.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);

            pManager.AddGenericParameter("Grid", "G", "Constructed VGrid.", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            object X_in = null, Y_in = null, Z_in = null;

            DA.GetData("X Grid", ref X_in);
            FGrid X_grid;
            if (X_in is FGrid)
                X_grid = X_in as FGrid;
            else if (X_in is GH_Grid)
                X_grid = (X_in as GH_Grid).Value as FGrid;
            else if (X_in is GH_Number)
                X_grid = new FGrid("X Grid", (float)(X_in as GH_Number).Value);
            else
                return;

            DA.GetData("Y Grid", ref Y_in);
            FGrid Y_grid;
            if (Y_in is FGrid)
                Y_grid = Y_in as FGrid;
            else if (Y_in is GH_Grid)
                Y_grid = (Y_in as GH_Grid).Value as FGrid;
            else if (Y_in is GH_Number)
                Y_grid = new FGrid("Y Grid", (float)(Y_in as GH_Number).Value);
            else
                return;

            DA.GetData("Z Grid", ref Z_in);
            FGrid Z_grid;
            if (Z_in is FGrid)
                Z_grid = Z_in as FGrid;
            else if (Z_in is GH_Grid)
                Z_grid = (Z_in as GH_Grid).Value as FGrid;
            else if (Z_in is GH_Number)
                Z_grid = new FGrid("Z Grid", (float)(Z_in as GH_Number).Value);
            else
                return;

            /*
            X_grid.SetA
            List<Vec3<float>> all_active = new List<Vec3<float>>();

            int[] active = X_grid.GetActiveVoxels();
            for(int i=0; i<active.Length; i+=3)
            {
                all_active.Add(new Vec3<float>(active[i], active[i + 1], active[i + 2]));
            }
            int[] active = X_grid.GetActiveVoxels();
            for (int i = 0; i < active.Length; i += 3)
            {
                all_active.Add(new Vec3<float>(active[i], active[i + 1], active[i + 2]));
            }
            int[] active = X_grid.GetActiveVoxels();
            for (int i = 0; i < active.Length; i += 3)
            {
                all_active.Add(new Vec3<float>(active[i], active[i + 1], active[i + 2]));
            }

            all_active = all_active.Distinct().ToList();
            */
            
            //FGrid mask = new FGrid("Mask", 0);

            VGrid new_grid = new VGrid("new_grid", new float[3] { 0, 0, 0 });
            new_grid.SetActiveStates(X_grid.GetActiveVoxels(), Enumerable.Repeat(true, X_grid.ActiveVoxelCount).ToArray());
            new_grid.SetActiveStates(Y_grid.GetActiveVoxels(), Enumerable.Repeat(true, Y_grid.ActiveVoxelCount).ToArray());
            new_grid.SetActiveStates(Z_grid.GetActiveVoxels(), Enumerable.Repeat(true, Z_grid.ActiveVoxelCount).ToArray());

            int[] active = new_grid.GetActiveVoxels();
            Vec3<float>[] values = new Vec3<float>[new_grid.ActiveVoxelCount];
            float[] X_values = X_grid.GetValuesIndex(active);
            float[] Y_values = Y_grid.GetValuesIndex(active);
            float[] Z_values = Z_grid.GetValuesIndex(active);

            for (int i=0; i<values.Length; i++)
            {
                values[i] = new Vec3<float>(X_values[i], Y_values[i], Z_values[i]);
            }
            new_grid.SetValues(active, values);

            DA.SetDataList(0, debug);
            DA.SetData("Grid", new GH_Grid(new_grid));

        }
    }
}