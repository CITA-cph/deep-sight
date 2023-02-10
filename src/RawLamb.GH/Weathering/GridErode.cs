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

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;
using Grasshopper.Kernel.Types;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridErode : GH_Component
    {
        public Cmpt_GridErode()
          : base("GridErode", "GErode",
              "Erode a grid based on how exposed its active voxels are.",
              DeepSight.GH.Api.ComponentCategory, "Biopol")
        {
        }

        Grid ErosionGrid = null;
        int Iterations = 0;

        Vector3d[] Compass = new Vector3d[]
        {
            new Vector3d(-0.5774, -0.5774, -0.5774),
            new Vector3d(0.0000, -0.7071, -0.7071),
            new Vector3d(0.5774, -0.5774, -0.5774),
            new Vector3d(-0.7071, 0.0000, -0.7071),
            new Vector3d(0.0000, 0.0000, -1.0000),
            new Vector3d(0.7071, 0.0000, -0.7071),
            new Vector3d(-0.5774, 0.5774, -0.5774),
            new Vector3d(0.0000, 0.7071, -0.7071),
            new Vector3d(0.5774, 0.5774, -0.5774),
            new Vector3d(-0.7071, -0.7071, 0.0000),
            new Vector3d(0.0000, -1.0000, 0.0000),
            new Vector3d(0.7071, -0.7071, 0.0000),
            new Vector3d(-1.0000, 0.0000, 0.0000),
            new Vector3d(0.0000, 0.0000, 0.0000),
            new Vector3d(1.0000, 0.0000, 0.0000),
            new Vector3d(-0.7071, 0.7071, 0.0000),
            new Vector3d(0.0000, 1.0000, 0.0000),
            new Vector3d(0.7071, 0.7071, 0.0000),
            new Vector3d(-0.5774, -0.5774, 0.5774),
            new Vector3d(0.0000, -0.7071, 0.7071),
            new Vector3d(0.5774, -0.5774, 0.5774),
            new Vector3d(-0.7071, 0.0000, 0.7071),
            new Vector3d(0.0000, 0.0000, 1.0000),
            new Vector3d(0.7071, 0.0000, 0.7071),
            new Vector3d(-0.5774, 0.5774, 0.5774),
            new Vector3d(0.0000, 0.7071, 0.7071),
            new Vector3d(0.5774, 0.5774, 0.5774)
        };

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to erode.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rate", "R", "Damage per iteration per time step.", GH_ParamAccess.item, 0.1);
            pManager.AddIntegerParameter("Iterations", "I", "Number of time steps.", GH_ParamAccess.item, 5);
            pManager.AddGenericParameter("Strategy", "S", "Decay strategy. A Vector will decay voxels facing its direction more; a Plane will decay voxels below.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "W", "Weighting between normal decay and the decay strategy.", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Threshold", "T", "Threshold of exposure.", GH_ParamAccess.item, 0.1);
            pManager.AddBooleanParameter("Reset", "R", "Reset erosion.", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Eroded grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
            DA.GetData("Reset", ref reset);
            if (reset || ErosionGrid == null)
            {
                object m_grid = null;
                Grid grid = null;

                DA.GetData(0, ref m_grid);

                if (m_grid is Grid)
                    grid = m_grid as Grid;
                else if (m_grid is GH_Grid)
                    grid = (m_grid as GH_Grid).Value as Grid;
                else
                    return;

                if (grid == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unsupported grid type.");
                    return;
                }

                ErosionGrid = grid.DuplicateGrid();
                Iterations = 0;
                Message = "Reset";
                return;
            }

            double rate = 0.1;
            DA.GetData("Rate", ref rate);
            if (rate < 0.0) return;

            int iter = 1;
            DA.GetData("Iterations", ref iter);
            if (iter < 1)
                DA.SetData("Grid", new GH_Grid(ErosionGrid));

            double threshold = 0.1;
            DA.GetData("Threshold", ref threshold);
            threshold = Math.Max(0, threshold);

            // Get erosion strategies
            double weight = 0.0;
            Vector3d vector = Vector3d.Unset;
            Plane plane = Plane.Unset;

            object strategy = null;
            if (DA.GetData("Strategy", ref strategy))
            {
                DA.GetData("Weight", ref weight);
                weight = Math.Max(0, weight);

                if (strategy is Vector3d)
                    vector = (Vector3d)strategy;
                else if (strategy is GH_Vector)
                    vector = (strategy as GH_Vector).Value;
                else if (strategy is Plane)
                    plane = (Plane)strategy;
                else if (strategy is GH_Plane)
                    plane = (strategy as GH_Plane).Value;
            }

            Message = String.Format("V: {0}, P: {1}", vector.IsValid, plane.IsValid);
            Message = String.Format("Weight: {0}", weight);

            Erode(ErosionGrid, vector, plane, (float)weight, (float)rate, (float)threshold);

            Iterations++;

            DA.SetData("Grid", new GH_Grid(ErosionGrid));
        }

        public float DirectionalExposure(Vector3d vec, float[] neighbours, Vector3d[] vdirs, float threshold)
        {
            float exp = 0;
            for (int i = 0; i < 27; ++i)
            {
                if (i == 13) continue;
                if (neighbours[i] < threshold)
                    exp += (float)Math.Max(0, -vec * vdirs[i]);
            }
            return exp / 26.0F;
        }

        /// <summary>
        /// Erode the grid based on its voxel exposure. Subtracts a value rate*exposure 
        /// for every active voxel. If the result is <0, sets the voxel value to 0 and
        /// its state to inactive.
        /// </summary>
        /// <param name="rate">Rate of erosion.</param>
        public void Erode(Grid grid, Vector3d vector, Plane plane, float weight=0.1f, float rate = 0.1f, float threshold = 0.0f)
        {
            var killList = new List<int>();
            var killState = new List<bool>();

            var active = grid.GetActiveVoxels();
            var N = active.Length / 3;

            /*
            var vdirs = new Vector3d[27];
            int ii = 0;
            for (int z = -1; z < 2; ++z)
                for (int y = -1; y < 2; ++y)
                    for (int x = -1; x < 2; ++x)
                    {
                        vdirs[ii] = new Vector3d(x, y, z);
                        vdirs[ii].Unitize();
                        ii++;
                    }
            */
            Transform xform = grid.Transform.ToRhinoTransform();

            float decay = 0.0f;
            for (int i = 0; i < N; ++i)
            {
                decay = 0.0f;
                int x = active[i * 3];
                int y = active[i * 3 + 1];
                int z = active[i * 3 + 2];

                var nbrs = grid.GetNeighbours(new int[] {x, y, z});
                var exp = Weathering.Exposure(nbrs, threshold);

                if (vector.IsValid)
                {
                    var dexp = DirectionalExposure(vector, nbrs, Compass, threshold);
                    decay = dexp * weight;
                }
                if (plane.IsValid)
                {
                    var pt = new Point3d(x, y, z);
                    pt.Transform(xform);

                    Vector3d vv = pt - plane.Origin;
                    if (vv * plane.ZAxis < 0)
                        decay = exp * weight;
                }

                var v = nbrs[13] - rate * exp - decay; // 13 is the cell itself within its neighbourhood
                if (v < 0.0f)
                {
                    grid[x, y, z] = 0.0f;
                    killList.AddRange(new int[] { x, y, z });
                    killState.Add(false);
                }
                else
                {
                    grid[x, y, z] = v;
                }
            }

            grid.SetActiveStates(killList.ToArray(), killState.ToArray());
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
            get { return new Guid("72a25459-db79-4563-8b79-077a1d40bc89"); }
        }
    }
}