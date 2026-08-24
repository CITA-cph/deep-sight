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
    public class Cmpt_GridCombine : GH_Component
    {
        public Cmpt_GridCombine()
          : base("GridCombine", "GCom",
              "Combine two grids together (min, max, sum, mult, diff). Works on float and vector grids.",
              DeepSight.GH.Api.ComponentCategory, "Tools")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Voxel_Combine;
        public override Guid ComponentGuid => new Guid("1461a574-faea-4210-8fbe-4609184ae059");

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid 1", "G1", "First grid to combine.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Grid 2", "G2", "Second grid to combine.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Mode", "M", "Mode to combine grids. 0=max, 1=min, 2=sum, 3=diff, 4=if zero, 5=mul.", GH_ParamAccess.item, 1);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Output of combine.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object m_grid = null;
            List<object> m_grids = new List<object>();

            if (!DA.GetData("Grid 1", ref m_grid)) return;
            DA.GetDataList("Grid 2", m_grids);

            int mode = 1;
            DA.GetData("Mode", ref mode);

            // Unwrap the first grid to whatever concrete type it actually is.
            // The original hard-cast everything to FloatGrid, so a vector grid
            // came out null and hit "Unsupported grid types". Now we branch on
            // the real type and dispatch to the matching Combine overload, both
            // of which already exist in DeepSightNet (float and Vec3f).
            GridApi first = GH_Grid.ParseStructure(m_grid);
            if (first == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Grid 1 is not a grid.");
                return;
            }

            if (first is FGrid)
            {
                CombineFloat(DA, first as FGrid, m_grids, mode);
            }
            else if (first is VGrid)
            {
                CombineVector(DA, first as VGrid, m_grids, mode);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Grid 1 is an unsupported grid type. Only float and vector grids can be combined.");
            }
        }

        private void CombineFloat(IGH_DataAccess DA, FGrid grid0, List<object> others, int mode)
        {
            var grid1 = new List<FGrid>();
            foreach (var obj in others)
            {
                FGrid g = GH_Grid.ParseStructure(obj) as FGrid;
                if (g != null) grid1.Add(g);
            }

            if (grid1.Count < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Grid 2 must contain at least one float grid to combine with a float Grid 1.");
                return;
            }

            FGrid ngrid = grid0;
            for (int i = 0; i < grid1.Count; i++)
                ngrid = Tools.Combine(ngrid, grid1[i], (CombineType)mode);

            ngrid.Prune();
            DA.SetData("Grid", new GH_Grid(ngrid));
        }

        private void CombineVector(IGH_DataAccess DA, VGrid grid0, List<object> others, int mode)
        {
            var grid1 = new List<VGrid>();
            foreach (var obj in others)
            {
                VGrid g = GH_Grid.ParseStructure(obj) as VGrid;
                if (g != null) grid1.Add(g);
            }

            if (grid1.Count < 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Grid 2 must contain at least one vector grid to combine with a vector Grid 1. " +
                    "You cannot combine a vector grid with a float grid.");
                return;
            }

            // Not every mode is meaningful for vectors. Sum/diff act per
            // component (which is what you want for adding or subtracting
            // colours); min/max compare per component too. Multiply and 'if
            // zero' are unusual for a colour vector but the native op accepts
            // them, so they are allowed with a note rather than blocked.
            if (mode == 4 || mode == 5)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
                    "Modes 'if zero' and 'mul' apply per vector component; check the result makes sense for your data.");
            }

            VGrid ngrid = grid0;
            for (int i = 0; i < grid1.Count; i++)
                ngrid = Tools.Combine(ngrid, grid1[i], (CombineType)mode);

            ngrid.Prune();
            DA.SetData("Grid", new GH_Grid(ngrid));
        }
    }
}
