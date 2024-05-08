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
using DeepSight.RhinoCommon;
using System.Collections.Generic;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{
    public class Cmpt_GridFromMesh : GH_Component
    {
        public Cmpt_GridFromMesh()
          : base("Mesh2Grid", "M2G",
              "Convert a mesh to a level-set grid.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh to convert to grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Isovalue", "I", "Isovalue for level-set.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Voxel size", "S", "Size of voxels.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Exterior Bandwidth", "EB", "Max positive distance from mesh.", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Interior Bandwidth", "IB", "Max negative distance from mesh.", GH_ParamAccess.item, 3.0);
            pManager.AddTextParameter("Name", "N", "Name of grid", GH_ParamAccess.item, "default");

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Rhino.Geometry.Mesh mesh = null;

            DA.GetData("Mesh", ref mesh);
            if (mesh == null) return;

            double iso = 0.0, voxel_size = 1.0;
            DA.GetData("Isovalue", ref iso);
            DA.GetData("Voxel size", ref voxel_size);

            string name = "default";
            DA.GetData("Name", ref name);

            Transform inv, xform = Transform.Scale(Point3d.Origin, voxel_size);
            xform.TryGetInverse(out inv);

            mesh.Transform(inv);

            double EB = 3.0f, IB = 3.0f;
            DA.GetData("Exterior Bandwidth", ref EB);
            DA.GetData("Interior Bandwidth", ref IB);

            Grid grid = mesh.ToVolume(xform, (float)iso, (float)EB, (float)IB);
            grid.Name = name;
            //grid.SdfToFog();

            DA.SetData("Grid", new GH_Grid(grid));
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
            get { return new Guid("749cfad2-57cf-4539-9ccc-9981df5ed813"); }
        }
    }
}