﻿/*
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

namespace DeepSight.GH.Components
{
    public class Cmpt_GridMesh : GH_Component
    {
        public Cmpt_GridMesh()
          : base("Grid2Mesh", "G2M",
              "Get the isomesh of a grid.",
              DeepSight.GH.Api.ComponentCategory, "Create")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GridMesh_01;
        public override Guid ComponentGuid => new Guid("90a8da23-75a6-4ebc-a83c-405bb725b5cf");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Isovalue", "I", "Isovalue for meshing.", GH_ParamAccess.item, 0.15);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Isomesh of grid.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            GridApi temp_grid = null;
            double m_threshold = 0.15;

            DA.GetData(0, ref m_grid);
            if (m_grid is GridApi)
                temp_grid = m_grid as GridApi;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value;
            else
                return;

            DA.GetData(1, ref m_threshold);

            var fgrid = temp_grid as FloatGrid;
            if (fgrid == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Only scalar grids can be converted to a mesh.");
                return;
            }
            Rhino.Geometry.Mesh rhino_mesh = Convert.VolumeToMesh(temp_grid as FloatGrid, (float)m_threshold).ToRhinoMesh();
            //Rhino.Geometry.Mesh rhino_mesh = temp_grid.ToMesh((float)m_threshold).ToRhinoMesh();
            rhino_mesh.Normals.ComputeNormals();
            rhino_mesh.Faces.CullDegenerateFaces();

            DA.GetData(1, ref m_threshold);
            DA.SetData(0, rhino_mesh);
        }
    }
}