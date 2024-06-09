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
using Rhino.Geometry;
using DeepSight.RhinoCommon;
using System.Collections.Generic;
using System.Threading.Tasks;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    /// <summary>
    /// Uses the approach from Dendro (https://github.com/ryein/dendro) to 
    /// discretize curves into a set of points and then rasterize those.
    /// </summary>
    public class Cmpt_Curve2Grid : GH_Component
    {
        public Cmpt_Curve2Grid()
          : base("Curve2Grid", "C2G",
              "Convert curves to volume grid.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridMesh_01;
        public override Guid ComponentGuid => new Guid("5b28e18d-2eca-429f-b3b6-9c28d8f5f351");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Curve", "C", "Curve to convert.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Thickness", "T", "Thickness of volumetric curve.", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Voxel size", "S", "Size of voxels.", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("Name", "N", "Name of grid.", GH_ParamAccess.item, "default");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<Curve>();
            DA.GetDataList("Curve", curves);
            if (curves.Count < 1) return;

            double thickness = 3.0, voxel_size = 3.0;
            DA.GetData("Thickness", ref thickness);
            DA.GetData("Voxel size", ref voxel_size);

            string name = "default";
            DA.GetData("Name", ref name);

            if (voxel_size <= 0.0) throw new ArgumentException("Voxel size must be above 0.");
            if (thickness <= 0.0) throw new ArgumentException("Thickness must be above 0.");

            var inv_voxel_size = 1 / voxel_size;

            var points = new List<Point3d>();

            for (int i = 0; i < curves.Count; i++)
            {
                var crv = curves[i];
                double[] tt = crv.DivideByLength(voxel_size, true);
                points.AddRange(tt.Select(x => crv.PointAt(x)));
            }

            var coords = new float[points.Count * 3];

            Parallel.For(0, points.Count, i =>
            {
                coords[i * 3 + 0] = (float)(points[i].X);
                coords[i * 3 + 1] = (float)(points[i].Y);
                coords[i * 3 + 2] = (float)(points[i].Z);
            });

            var grid = Convert.PointsToVolume(coords, (float)thickness, (float)voxel_size);
            grid.Name = name;

            DA.SetData("Grid", new GH_Grid(grid));
        }
    }
}