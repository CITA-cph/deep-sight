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
using Rhino;

namespace RawLamb.GH.Components
{

    public class Cmpt_GraphCreate : GH_Component
    {
        public Cmpt_GraphCreate()
          : base("GraphCreate", "GNew",
              "Connect to a graph.",
              "RawLamb", "Graph")
        {
            Attributes = new ButtonConnectComponentAttributes(this);
        }

        public bool m_connect = false;
        private N4jGraph Graph = null;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Login", "L", "Login details (uri,username,password)", GH_ParamAccess.item, "bolt://localhost:7687,neo4j,rawlam");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Graph", "G", "Connection to graph.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (m_connect)
            {
                m_connect = false;

                string login_string = "";

                DA.GetData("Login", ref login_string);

                var login = login_string.Split(',');
                if (login.Length != 3)
                    throw new ArgumentException("Invalid login triplet. Must be comma-separated.");

                Graph = new N4jGraph(login[0], login[1], login[2]);
            }
 
            if (Graph != null)
                DA.SetData("Graph", new GH_N4jGraph(Graph));

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0759fda7-dc12-4d32-af72-4943292b26c2"); }
        }
    }

    public class ButtonConnectComponentAttributes : GH_ComponentAttributes
    {
        private bool _selected;
        Rectangle ButtonBounds { get; set; }

        public ButtonConnectComponentAttributes(GH_Component owner) : base(owner) { }

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
                var text = state == "sending" ? $"Connecting..." : "Connect";

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
                    ((Cmpt_GraphCreate)Owner).m_connect = true;
                    Owner.ExpireSolution(true);

                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDown(sender, e);
        }

    }


}