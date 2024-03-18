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
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;

using FGrid = DeepSight.FloatGrid;
using VGrid = DeepSight.Vec3fGrid;
using System.Diagnostics;
using System.Linq;
using Eto.Forms;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridFromPointCloud : GH_Component
    {
        public Cmpt_GridFromPointCloud()
          : base("PointCloud2Grid", "PC2G",
              "Convert a pointcloud to a grid (with colors if present).",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PointCloud", "PC", "PointCloud to convert to grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Voxel size", "S", "Size of voxels.", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("Name", "N", "Name of grid", GH_ParamAccess.item, "default");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "Debugging messages.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            var debug = new List<string>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            PointCloud pc = null;

            DA.GetData("PointCloud", ref pc);
            if (pc == null) return;

            double voxel_size = 1.0;
            DA.GetData("Voxel size", ref voxel_size);

            string name = "default";
            DA.GetData("Name", ref name);

            Transform inv, xform = Transform.Scale(Point3d.Origin, voxel_size);
            xform.TryGetInverse(out inv);

            pc.Transform(inv);

            Point3d[] points = pc.GetPoints();
            int[] fpoints = new int[points.Length * 3];

            //Parallel.For(0, points.Length - 1, i =>
            //{
            //    fpoints[i * 3] = (int)Math.Floor(points[i].X + 0.5);
            //    fpoints[i * 3 + 1] = (int)Math.Floor(points[i].Y + 0.5);
            //    fpoints[i * 3 + 2] = (int)Math.Floor(points[i].Z + 0.5);
            //});

            for (int i=0; i<points.Length; i++)
            {
                fpoints[i * 3] = (int)Math.Floor(points[i].X + 0.5);
                fpoints[i * 3 + 1] = (int)Math.Floor(points[i].Y + 0.5);
                fpoints[i * 3 + 2] = (int)Math.Floor(points[i].Z + 0.5);
            }

            debug.Add(string.Format("{0} : Flattened list of samples.", stopwatch.ElapsedMilliseconds));


            if (pc.ContainsColors)
            {
                VGrid grid = new VGrid();
                grid.Name = name;
                grid.Transform = xform.ToFloatArray(true);

                System.Drawing.Color[] colors = pc.GetColors();
                Vec3<float>[] values = new Vec3<float>[points.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    System.Drawing.Color c = colors[i];
                    values[i] = new Vec3<float>((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
                }

                grid.SetValues(fpoints, values);
                DA.SetData("Grid", new GH_Grid(grid));
            } else
            {
                FGrid grid = new FGrid();
                grid.Name = name;
                grid.Transform = xform.ToFloatArray(true);

                float[] values = Enumerable.Repeat(1f, points.Length).ToArray();

                grid.SetValues(fpoints, values);
                DA.SetData("Grid", new GH_Grid(grid));

            }

            DA.SetDataList(0, debug);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridMesh_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("441990E4-8F4B-4AEC-8DCA-E4DA6767B844"); }
        }
    }
}