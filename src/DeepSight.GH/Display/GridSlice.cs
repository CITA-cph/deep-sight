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
using System.Collections.Generic;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;
using Grasshopper.Kernel.Types;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;

namespace DeepSight.GH.Components
{
    using Mesh = Rhino.Geometry.Mesh;

    public class Cmpt_GridSlice : GH_Component
    {
        public Cmpt_GridSlice()
          : base("GridSlice", "GSlc",
              "Create an image slice through a grid. Works on float grids (value output) " +
              "and vector grids (coloured slice).",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridSlice;
        public override Guid ComponentGuid => new Guid("bf169c5f-c5b6-4bc2-94b8-6d8515b33ca7");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to inspect (float or vector).", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Point to make slice at.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Axis", "A", "Axis to make slice through (0 = X, 1 = Y, 2 = Z).", GH_ParamAccess.item, 0);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh of the grid slice (coloured for vector grids).", GH_ParamAccess.item);
            pManager.AddNumberParameter("Values", "V", "Grid values per vertex (float grids only).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            DA.GetData(0, ref m_grid);

            GridApi baseGrid = GH_Grid.ParseStructure(m_grid);
            if (baseGrid == null || !baseGrid.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input is not a valid grid.");
                return;
            }

            Point3d point = Point3d.Origin;
            DA.GetData("Point", ref point);

            int axis = 0;
            DA.GetData("Axis", ref axis);

            // Shared slice geometry setup, then a per-type inner loop. The
            // original hard-cast to FloatGrid, so a vector grid became null and
            // the component silently returned - which is why slicing never
            // worked on vector grids.
            int[] min, max;
            baseGrid.BoundingBox(out min, out max);

            var xform = baseGrid.Transform.ToRhinoTransform();
            Transform inv;
            xform.TryGetInverse(out inv);

            Point3d ip = point;
            ip.Transform(inv);
            var xyz = new double[] { ip.X, ip.Y, ip.Z };

            int[] axes;
            switch (axis)
            {
                case 1: axes = new int[] { 1, 2, 0 }; break; // YZ
                case 2: axes = new int[] { 0, 2, 1 }; break; // XZ
                default: axes = new int[] { 0, 1, 2 }; break; // XY
            }

            int z = (int)xyz[axes[2]];
            int stride = max[axes[1]] - min[axes[1]] + 1;
            int xrange = max[axes[0]] - min[axes[0]];
            int yrange = max[axes[1]] - min[axes[1]];

            var mesh = new Mesh();
            var ijk = new int[3];
            var outputValues = new List<double>();

            var fgrid = baseGrid as FGrid;
            var vgrid = baseGrid as VGrid;

            // Build vertices; colour them if vector, record values if float.
            for (int x = min[axes[0]]; x <= max[axes[0]]; ++x)
            {
                for (int y = min[axes[1]]; y <= max[axes[1]]; ++y)
                {
                    ijk[axes[0]] = x;
                    ijk[axes[1]] = y;
                    ijk[axes[2]] = z;

                    mesh.Vertices.Add((float)ijk[0], (float)ijk[1], (float)ijk[2]);

                    if (vgrid != null)
                    {
                        Vec3<float> c = vgrid[ijk[0], ijk[1], ijk[2]];
                        mesh.VertexColors.Add(
                            Clamp255((int)Math.Round(c.X * 255)),
                            Clamp255((int)Math.Round(c.Y * 255)),
                            Clamp255((int)Math.Round(c.Z * 255)));
                    }
                    else if (fgrid != null)
                    {
                        outputValues.Add(fgrid[ijk[0], ijk[1], ijk[2]]);
                    }
                }
            }

            int row = 0, rowp = stride;
            for (int x = 0; x < xrange; ++x)
            {
                for (int y = 0; y < yrange; ++y)
                    mesh.Faces.AddFace(row + y, row + y + 1, rowp + y + 1, rowp + y);
                row += stride;
                rowp += stride;
            }

            mesh.Transform(xform);

            DA.SetData("Mesh", mesh);
            DA.SetDataList("Values", outputValues);
        }

        private static int Clamp255(int v) { return v < 0 ? 0 : (v > 255 ? 255 : v); }
    }
}
