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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using DeepSight.RhinoCommon;

using Grid = DeepSight.Vec3fGrid;
using System.Diagnostics.Eventing.Reader;

namespace DeepSight.GH.Components
{

    public class Cmpt_VGridDisplay : GH_Component
    {
        public Cmpt_VGridDisplay()
          : base("VGridDisplay", "VGDis",
              "Visualize a vector grid as a pointcloud.",
              DeepSight.GH.Api.ComponentCategory, "VGrid")
        {
            //Attributes = new ButtonRefreshComponentAttributes(this);
        }

        public bool m_refresh = false;


        public static float Alpha = 0.5f;
        public static float PointSize = 10;
        public static int MaxPoints = 10000000;

        private Grid Grid = null;
        private PointCloud Cloud = null;
        Transform GridTransform;
        float[] Values;
        int[] Min, Max;

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (Cloud != null)
                    return Cloud.GetBoundingBox(true);
                return BoundingBox.Empty;
            }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Vector grid.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Step", "S", "Step size (list of 3 numbers for X, Y, and Z)", GH_ParamAccess.list);
            pManager[1].Optional = true;

            pManager.AddBooleanParameter("Update", "U", "Update display with each change (can be slow)", GH_ParamAccess.item, false);
            pManager[2].Optional = true;

            pManager.AddNumberParameter("Size", "P", "Point size for point cloud display", GH_ParamAccess.item, 10);
            pManager[3].Optional = true;

            pManager.AddBooleanParameter("Grid space", "GS", "Coordinates in grid-space (true) or world-space (false).", GH_ParamAccess.item, false);
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "d", "debugging data", GH_ParamAccess.list);
        }

        //Draw all meshes in this method.
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (Cloud == null) return;
            args.Display.DrawPointCloud(Cloud, PointSize);
        }

        //Draw all wires and points in this method.
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            DrawViewportMeshes(args);
        }

        protected override void ValuesChanged()
        {
            base.ValuesChanged();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Message = "";

            object m_grid = null;
            Grid temp_grid = null;
            var steps = new List<int>();
            bool m_update = false;

            DA.GetData(2, ref m_update);
            double dPointSize = 10.0;
            if (DA.GetData(3, ref dPointSize)){
                PointSize = (float)(dPointSize);
            }

            DA.GetData(0, ref m_grid);
            if (m_grid is Grid)
                temp_grid = m_grid as Grid;
            else if (m_grid is GH_Grid)
                temp_grid = (m_grid as GH_Grid).Value as Grid;
            else
                return;

            if (temp_grid != Grid || m_update)
            {
                Grid = temp_grid;
                Values = null;
                Message = "Updating";
            }

            if (!DA.GetDataList(1, steps))
                steps = new List<int>() { 1, 1, 1 };

            for (int i = steps.Count; i <= 3; ++i)
                steps.Add(1);



            if (Values == null)
            {
                Min = new int[] { 0, 0, 0 };
                Max = new int[] { 0, 0, 0 };
            }

            var size = new int[]{
              Max[0] - Min[0] + 1,
              Max[1] - Min[1] + 1,
              Max[2] - Min[2] + 1};

            Cloud = new PointCloud();

            var active = Grid.GetActiveVoxels();

            GridTransform = Grid.Transform.ToRhinoTransform();

            bool gridspace = false;
            DA.GetData(4, ref gridspace);

            for (int i = 0; i < active.Length; i += 3)
            {
                if (Math.Abs(active[i]) % steps[0] != 0 || Math.Abs(active[i + 1]) % steps[1] != 0 || Math.Abs(active[i + 2]) % steps[2] != 0)
                    continue;
                var pt = new Point3d(active[i], active[i + 1], active[i + 2]);
                var value = Grid[active[i], active[i + 1], active[i + 2]];

                Cloud.Add(pt, Color.FromArgb(255, 
                    (int)(value.X),
                    (int)(value.Y),
                    (int)(value.Z)
                    ));
            }

            if(!gridspace) Cloud.Transform(GridTransform);
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
            get { return new Guid("C128E42A-31CA-45D0-A8C9-FB9BB080D7F6"); }
        }
    }
    
}