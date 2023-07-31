#if OBSOLETE

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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel;
using DeepSight.RhinoCommon;


namespace DeepSight.GH.Components
{
    public class Cmpt_GridFromToolpath : GH_Component
    {
        public Cmpt_GridFromToolpath()
          : base("Toolpath2Grid", "T2G",
              "Convert a printing path to a voxel grid.",
              DeepSight.GH.Api.ComponentCategory, "Biopol")
        {
        }

        protected double Radius = 2.0;
        protected double Density = 1.0;
        protected double DensityVariance = 1.0;
        protected Random Rand = null;



        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Toolpath", "T", "Toolpath as list of curves.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "R", "Printing bead radius.", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Voxel scale", "VS", "Voxel size in document units.", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Density", "D", "Overall density of material.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Density variance", "DV", "Amount of random variation in density of the material.", GH_ParamAccess.item, 0.3);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Grid", "G", "Eroded grid.", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "d", "debugging", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (Rand == null) Rand = new Random();
            var crvs = new List<Curve>();
            var debug = new List<string>();


            DA.GetDataList("Toolpath", crvs);
            if (crvs.Count < 1) return;

            double radius = 0.0, voxel_size = 1.0, density = 1.0, density_variance = 0.1;
            DA.GetData("Radius", ref radius);
            DA.GetData("Voxel scale", ref voxel_size);
            DA.GetData("Density", ref density);
            DA.GetData("Density variance", ref density_variance);

            radius = Math.Max(0, radius);
            voxel_size = Math.Max(0, voxel_size);
            density = Math.Max(0, voxel_size);
            density_variance = Math.Max(0, voxel_size);

            // Create transform
            Transform inv_scale, scale = Transform.Scale(Point3d.Origin, 1 / voxel_size);
            scale.TryGetInverse(out inv_scale);

            // Create concurrent dictionary
            var values = new ConcurrentDictionary<Point3i, float>(System.Environment.ProcessorCount * 2, 2000);

            // Initialize variables
            int irad = (int)Math.Ceiling(radius + 1);
            double margin = 2.0;
            double ti = radius - margin, to = radius;

            // Find bounds of toolpath
            BoundingBox bounds = BoundingBox.Empty;

            foreach (Curve crv in crvs)
                bounds.Union(crv.GetBoundingBox(true));

            int i_min = (int)Math.Floor(bounds.Min.X - radius - margin),
                j_min = (int)Math.Floor(bounds.Min.Y - radius - margin),
                k_min = (int)Math.Floor(bounds.Min.Z - radius - margin);

            int i_max = (int)Math.Ceiling(bounds.Max.X + radius + margin),
                j_max = (int)Math.Ceiling(bounds.Max.Y + radius + margin),
                k_max = (int)Math.Ceiling(bounds.Max.Z + radius + margin);

            debug.Add(string.Format("min I{0} J{1}, K{2}", i_min, j_min, k_min));
            debug.Add(string.Format("max I{0} J{1}, K{2}", i_max, j_max, k_max));

            // Fragment toolpath into line segments
            var polylines = new List<Polyline>();
            for(int i = 0; i < crvs.Count; i++)
            {
                var crv = crvs[i];
                crv.Transform(scale);
                if (crv.IsPolyline())
                {
                    Polyline polyline;
                    crv.TryGetPolyline(out polyline);
                    polylines.Add(polyline);
                }
                else
                {
                    var polycurve = crv.ToPolyline(0.1, 0.1, voxel_size, 0.0);
                    polylines.Add(polycurve.ToPolyline());
                }
            }

            RTree tree = new RTree();

            var fragments = FragmentToolpath(polylines);
            for (int i = 0; i < fragments.Length; ++i)
            {
                var bb = fragments[i].BoundingBox;
                bb.Inflate((radius + margin) * 2);
                tree.Insert(bb, i);
            }

            // Search tree
            Parallel.For(i_min, i_max, i =>
            {
                for (int j = j_min; j < j_max; ++j)
                    for (int k = k_min; k < k_max; ++k)
                    {
                        var sd = new SearchData() { Dic = values, I = i, J = j, K = k, Lines = fragments, Radius = radius}; 
                        tree.Search(new Sphere(
                            new Point3d(i, j, k), radius), SearchCallback, sd);
                    }
            });

            var grid = new Grid();
            grid.Name = "density";
            grid.Class = GridClass.GRID_FOG_VOLUME;

            //Parallel.ForEach(values, (KeyValuePair<Point3i, float> pair) =>
            //{
            foreach(var pair in values)
                grid[pair.Key.X, pair.Key.Y, pair.Key.Z] = pair.Value;
            //});

            grid.Transform = inv_scale.ToFloatArray(true);

            DA.SetData("Grid", new GH_Grid(grid));
            DA.SetDataList("debug", debug);
        }

        public Line[] FragmentToolpath(List<Polyline> polylines)
        {
            var fragments = new ConcurrentBag<Line>();
            Parallel.For(0, polylines.Count, i =>
            {
                foreach (var segment in polylines[i].GetSegments())
                    fragments.Add(segment);
            });

            return fragments.ToArray();
        }

        public class SearchData
        {
            public SearchData() { }
            public ConcurrentDictionary<Point3i, float> Dic { get;  set; }
            public int I { get; set; }
            public int J { get; set; }
            public int K { get; set; }
            public Line[] Lines { get; set; }
            public int Index { get; set; }
            public double Radius { get; set; }
        }

        public struct Point3i
        {
            public Point3i(int x, int y, int z) 
            {
                X = x;
                Y = y;
                Z = z;
            }
            public int X { get; set; }
            public int Y { get; set; }
            public int Z { get; set; }
        }

        void SearchCallback(object sender, RTreeEventArgs e)
        {
            var data = e.Tag as SearchData;
            if (data == null) return;

            var point = new Point3i(data.I, data.J, data.K);
            var pointd = new Point3d(data.I, data.J, data.K);
            var dist = data.Lines[data.Index].MinimumDistanceTo(pointd);

            if (dist < data.Radius)
                data.Dic[point] = (float)(Density * Rand.NextDouble() * DensityVariance);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.GridSample_01;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("955976fd-9f90-40f0-85ed-537cf16e0a48"); }
        }
    }
}
#endif