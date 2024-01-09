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
using System.Diagnostics;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;
using Grasshopper.Kernel.Types;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridSlice : GH_Component
    {
        public Cmpt_GridSlice()
          : base("GridSlice", "GSlc",
              "Create an image slice throug a grid.",
              DeepSight.GH.Api.ComponentCategory, "Display")
        {
        }



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Grid to inspect.", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Point to make slice at.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Axis", "A", "Axis to make slice through (0 = X, 1 = Y, 2 = Z).", GH_ParamAccess.item, 0);

            pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Colored mesh of the grid slice.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Values", "V", "Grid values for each vertex of grid slice mesh.", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            Grid temp_grid = null;

            DA.GetData(0, ref m_grid);

            if (m_grid is Grid)
                temp_grid = m_grid as FloatGrid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as FloatGrid;
            else
                return;

            Point3d point = Point3d.Origin;
            DA.GetData("Point", ref point);

            int axis = 0;
            DA.GetData("Axis", ref axis);

            var grid = temp_grid;
            if (grid == null) return;

            int[] min, max;

            grid.BoundingBox(out min, out max);


            var xform = grid.Transform.ToRhinoTransform();
            var scaleX = new Vector3d(xform.M00, xform.M01, xform.M02).Length;
            var scaleY = new Vector3d(xform.M10, xform.M11, xform.M12).Length;
            var scaleZ = new Vector3d(xform.M20, xform.M21, xform.M22).Length;

            var scale = new double[] { scaleX, scaleY, scaleZ };

            Transform inv;
            xform.TryGetInverse(out inv);

            point.Transform(inv);

            var xyz = new double[] { point.X, point.Y, point.Z };

            int[] axes;
            switch (axis)
            {
                case (1): // YZ plane
                    axes = new int[] { 1, 2, 0 };
                    break;
                case (2): // XZ plane
                    axes = new int[] { 0, 2, 1 };
                    break;
                default: // XY plane
                    axes = new int[] { 0, 1, 2 };
                    break;
            }

            int z = (int)xyz[axes[2]];
            double aspect = scale[axes[1]] / scale[axes[0]];

            var ijk = new int[3];

            Rhino.Geometry.Mesh m = new Rhino.Geometry.Mesh();

            int stride = max[axes[1]] - min[axes[1]] + 1;

            var xrange = max[axes[0]] - min[axes[0]];
            var yrange = max[axes[1]] - min[axes[1]];


            //var bmp = new System.Drawing.Bitmap(xrange + 1, yrange + 1);
            int ix = 0, iy = yrange;

            var outputValues = new List<double>();

            for (int x = min[axes[0]]; x <= max[axes[0]]; ++x)
            {
                for (int y = min[axes[1]]; y <= max[axes[1]]; ++y)
                {
                    ijk[axes[0]] = x;
                    ijk[axes[1]] = y;
                    ijk[axes[2]] = z;

                    m.Vertices.Add((float)ijk[0], (float)ijk[1], (float)ijk[2]);
                    outputValues.Add(grid[ijk[0], ijk[1], ijk[2]]);

                    //var v = (int)(255 - grid[ijk[0], ijk[1], ijk[2]] * 255);
                    //var v = (int)(grid[ijk[0], ijk[1], ijk[2]] * 255);
                    //m.VertexColors.Add(v, v, v);
                    //bmp.SetPixel(ix, iy, System.Drawing.Color.FromArgb(255, v, v, v));

                    iy -= 1;
                }

                iy = yrange;
                ix += 1;
            }

            int row = 0, rowp = stride;
            for (int x = 0; x < xrange; ++x)
            {
                for (int y = 0; y < yrange; ++y)
                {
                    m.Faces.AddFace(row + y, row + y + 1, rowp + y + 1, rowp + y);
                }

                row += stride;
                rowp += stride;
            }

            m.Transform(xform);

            DA.SetData("Mesh", m);
            DA.SetDataList("Values", outputValues);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridDisplay_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("bf169c5f-c5b6-4bc2-94b8-6d8515b33ca7"); }
        }
    }
}