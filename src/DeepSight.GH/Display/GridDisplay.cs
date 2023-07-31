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

using DeepSight.Rhino;

using Grid = DeepSight.FloatGrid;

namespace DeepSight.GH.Components
{

    public class Cmpt_GridDisplay : GH_Component
    {
        public Cmpt_GridDisplay()
          : base("GridDisplay", "GDis",
              "Visualize a grid as a pointcloud.",
              DeepSight.GH.Api.ComponentCategory, "Display")
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
            pManager.AddGenericParameter("Grid", "G", "Volume grid.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Threshold", "T", "Threshold for displaying values.", GH_ParamAccess.item, 0.15);
            pManager.AddIntegerParameter("Step", "S", "Step size (list of 3 numbers for X, Y, and Z).", GH_ParamAccess.list);
            pManager[2].Optional = true;

            pManager.AddBooleanParameter("Update", "U", "Update display with each change (can be slow)", GH_ParamAccess.item, false);
            pManager[3].Optional = true;

            pManager.AddColourParameter("Color", "C", "Colour of display points.", GH_ParamAccess.item, Color.White);
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
            double m_threshold = 0.15;
            var steps = new List<int>();
            bool m_update = false;

            DA.GetData(3, ref m_update);

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

            DA.GetData(1, ref m_threshold);
            if (!DA.GetDataList(2, steps))
                steps = new List<int>() { 2, 2, 2 };

            for (int i = steps.Count; i <= 3; ++i)
                steps.Add(1);

            Color color = Color.White;
            DA.GetData("Color", ref color);

            GridTransform = Grid.Transform.ToRhinoTransform();

            if (Values == null)
            {
                Min = new int[] { 0, 0, 0 };
                Max = new int[] { 0, 0, 0 };
                //Grid.BoundingBox(out Min, out Max);
                //Values = Grid.GetDenseGrid(Min, Max);
            }

            var size = new int[]{
              Max[0] - Min[0] + 1,
              Max[1] - Min[1] + 1,
              Max[2] - Min[2] + 1};


            Cloud = new PointCloud();

            var active = Grid.GetActiveVoxels();

            for (int i = 0; i < active.Length; i += 3)
            {
                if (Math.Abs(active[i]) % steps[0] != 0 || Math.Abs(active[i + 1]) % steps[1] != 0 || Math.Abs(active[i + 2]) % steps[2] != 0)
                    continue;
                var pt = new Point3d(active[i], active[i + 1], active[i + 2]);
                var value = Grid[active[i], active[i + 1], active[i + 2]];
                if (value < m_threshold) continue;

                value = Math.Max(0, Math.Min(1.0f, value));

                //var grey = Math.Max(0, Math.Min(255, (int)(value * 255)));
                //Cloud.Add(pt, System.Drawing.Color.FromArgb((int)(grey * Alpha), grey, grey, grey));

                Cloud.Add(pt, Color.FromArgb(color.A, 
                    (int)(color.R * value),
                    (int)(color.G * value),
                    (int)(color.B * value)
                    ));
            }

            Cloud.Transform(GridTransform);
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
            get { return new Guid("a111ca6d-14f0-4257-8d76-967592ca7c3c"); }
        }
    }

    public class ButtonRefreshComponentAttributes : GH_ComponentAttributes
    {
        private bool _selected;
        Rectangle ButtonBounds { get; set; }

        public ButtonRefreshComponentAttributes(GH_Component owner) : base(owner) { }

        public override bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                //Owner.Params.ToList().ForEach(p => p.Attributes.Selected = value);
                _selected = value;
            }
        }

        protected override void Layout()
        {
            base.Layout();

            var baseRec = GH_Convert.ToRectangle(Bounds);
            baseRec.Height += 26;

            var btnRec = baseRec;
            btnRec.Y = btnRec.Bottom - 26;
            btnRec.Height = 26;
            btnRec.Inflate(-2, -2);

            Bounds = baseRec;
            ButtonBounds = btnRec;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            var state = "friendly";

            if (channel == GH_CanvasChannel.Objects)
            {

                var palette = (state == "expired" || state == "up_to_date") ? GH_Palette.Black : GH_Palette.Transparent;
                //NOTE: progress set to indeterminate until the TotalChildrenCount is correct
                //var text = state == "sending" ? $"{((SendComponent)Owner).OverallProgress}" : "Send";
                var text = state == "sending" ? $"Refreshing..." : "Refresh";

                var button = GH_Capsule.CreateTextCapsule(ButtonBounds, ButtonBounds, palette, text, 2,
                    state == "expired" ? 10 : 0);
                button.Render(graphics, Selected, Owner.Locked, false);
                button.Dispose();
            }
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (((RectangleF)ButtonBounds).Contains(e.CanvasLocation))
                {

                    //if (((NewVariableInputSendComponent)Owner).CurrentComponentState == "sending")
                    //{
                    //    return GH_ObjectResponse.Handled;
                    //}

                    //((NewVariableInputSendComponent)Owner).CurrentComponentState = "primed_to_send";
                    ((Cmpt_GridDisplay)Owner).m_refresh = true;
                    Owner.ExpireSolution(true);

                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

    }

}