using System;
using System.Collections.Generic;
using System.Diagnostics;

using Rhino.Geometry;
using DeepSight;
using DeepSight.RhinoCommon;

using Grid = DeepSight.FloatGrid;

namespace RawLamb
{
    public class LogYard
    {
        //public Dictionary<string, CtLog> CtLogs;
        //public Dictionary<string, Mesh> MeshLogs;
        //public Dictionary<string, Polyline> Piths;
        //public Dictionary<string, List<Knot>> Knots;
        //public Dictionary<string, BoundingBox> LogBounds;

        public Dictionary<string, Log> Logs;

        public List<string> Messages;

        public LogYard()
        {
            //CtLogs = new Dictionary<string, CtLog>();
            //MeshLogs = new Dictionary<string, Mesh>();
            //Piths = new Dictionary<string, Polyline>();
            //Knots = new Dictionary<string, List<Knot>>();
            //LogBounds = new Dictionary<string, BoundingBox>();
            Logs = new Dictionary<string, Log>();

            Messages = new List<string>();
        }

        public void Load(string directory, IList<string> names)
        {
            if (!System.IO.Directory.Exists(directory)) throw new Exception("Invalid directory!");
            Messages = new List<string>();


            //List<CtLog> logs = new List<CtLog>();
            //List<Mesh> log_meshes = new List<Mesh>();
            //List<string> names = new List<string>();

            foreach (string name in names)
            {
                if (!Logs.ContainsKey(name))
                {
                    Logs[name] = new Log();
                    Logs[name].Name = name;
                }

                var log = Logs[name];

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var path = System.IO.Path.Combine(directory, name, name + ".vdb");
                /*
                        if (CtLogs.ContainsKey(name))
                        {
                          logs.Add(CtLogs[name]);
                        }
                */
                //if (log.CtLog == null)
                if (!log.Grids.ContainsKey("density"))
                {
                    //var ctlog = new RawLamNet.CtLog();
                    Grid ctlog = null;

                    try
                    {
                        ctlog = GridIO.Read(path)[0] as Grid;
                        if (ctlog == null)
                            throw new ArgumentException("Couldn't find suitable grid.");
                    }
                    catch (Exception e)
                    {
                        Messages.Add(e.Message);
                        continue;
                    }
                    
                    //Print("Loaded {0}: {1}s", name, stopwatch.ElapsedMilliseconds / 1000);

                    Transform scale = Transform.Scale(Plane.WorldXY, 1, 1, 10);
                    Transform move = Transform.Translation(new Vector3d(-449, -449, 0) * 0.5);
                    Transform xform = Transform.Multiply(scale, move);
                    //xform = scale;
                    ctlog.Transform = xform.ToFloatArray(false);

                    int[] min, max;
                    ctlog.BoundingBox(out min, out max);

                    Point3d minBB = new Point3d(min[0], min[1], min[2]);
                    Point3d maxBB = new Point3d(max[0], max[1], max[2]);

                    var bb = new BoundingBox(minBB, maxBB);
                    bb.Transform(xform);
                    log.BoundingBox = bb;
                    //LogBounds[name] = bb;

                    log.Grids["density"] = ctlog;
                    //CtLogs[name] = ctlog;
                    //logs.Add(CtLogs[name]);
                }
                /*
                        if (MeshLogs.ContainsKey(name))
                        {
                          log_meshes.Add(MeshLogs[name]);
                        }
                */
                if (log.Mesh == null && !log.Grids.ContainsKey("density"))
                //if (!MeshLogs.ContainsKey(name) && CtLogs.ContainsKey(name))
                {
                    // ***********************
                    // Check for existing mesh
                    // ***********************

                    var rhino_path = System.IO.Path.Combine(directory, name, name + ".3dm");
                    if (System.IO.File.Exists(rhino_path))
                    {
                        Messages.Add("Loading from existing mesh.");
                        var rfile = Rhino.FileIO.File3dm.Read(rhino_path);

                        foreach (var obj in rfile.Objects)
                        {
                            if (obj.Geometry is Rhino.Geometry.Mesh)
                            {
                                //Messages.Add(obj.Geometry.ToString());
                                log.Mesh = obj.Geometry as Rhino.Geometry.Mesh;
                                //MeshLogs[name] = obj.Geometry as Mesh;
                                break;
                            }
                        }
                        Messages.Add(string.Format("Loaded {0}.3dm: {1}s", name, stopwatch.ElapsedMilliseconds / 1000));

                    }
                    // ****************************************
                    // Otherwise resample and get isosurface...
                    // ****************************************
                    else
                    {

                        var rlog = Tools.Resample(log.Grids["density"] as Grid, 34.0);
                        Messages.Add(string.Format("Resampled {0}: {1}s", name, stopwatch.ElapsedMilliseconds / 1000));
                        
                        var qm = DeepSight.Convert.VolumeToMesh(rlog, 0.25f);
                        var log_mesh = qm.ToRhinoMesh();
                        log_mesh.Vertices.CombineIdentical(true, true);
                        log_mesh.Faces.CullDegenerateFaces();
                        log_mesh.Vertices.CullUnused();

                        var pieces = log_mesh.SplitDisjointPieces();

                        double max_volume = 0;
                        int index = -1;

                        for (int i = 0; i < pieces.Length; ++i)
                        {
                            var vmp = VolumeMassProperties.Compute(pieces[i]);
                            if (vmp.Volume > max_volume)
                            {
                                max_volume = vmp.Volume;
                                index = i;
                            }
                        }
                        if (index >= 0)
                            log_mesh = pieces[index];
                        log_mesh.Compact();

                        Messages.Add(string.Format("Converted {0}: {1}s", name, stopwatch.ElapsedMilliseconds / 1000));

                        //MeshLogs[name] = log_mesh;
                        log.Mesh = log_mesh;
                        //log_meshes.Add(log_mesh);
                    }
                }

                // ******************
                // Load piths
                // ******************
                if (log.Pith == null || log.Pith.Count < 1)
                //if (!Piths.ContainsKey(name) && CtLogs.ContainsKey(name))
                {
                    var pith_path = System.IO.Path.Combine(directory, name, "pith@" + name + ".txt");
                    if (System.IO.File.Exists(pith_path))
                    {
                        Messages.Add("Loading pith.");
                        var pith_lines = System.IO.File.ReadAllLines(pith_path);

                        var pith = new Polyline();

                        double z = 0;
                        foreach (var line in pith_lines)
                        {
                            var tok = line.Split();
                            double x = double.Parse(tok[0]) * 0.1;
                            double y = double.Parse(tok[1]) * 0.1;

                            pith.Add(new Point3d(x, y, z));
                            z += 10.0;
                        }

                        log.Pith = pith;
                        //Piths[name] = pith;
                    }
                }

                // ******************
                // Load knots
                // ******************
                if (log.Knots == null || log.Knots.Count < 1)
                //if (!Knots.ContainsKey(name) && CtLogs.ContainsKey(name))
                {
                    var knot_path = System.IO.Path.Combine(directory, name, "knots@" + name + ".txt");
                    if (System.IO.File.Exists(knot_path))
                    {
                        Messages.Add("Loading knots.");
                        var knot_lines = System.IO.File.ReadAllLines(knot_path);

                        var knots = new List<Knot>();

                        double z = 0;
                        int index = 0;
                        foreach (var line in knot_lines)
                        {
                            var tok = line.Split();
                            var start = new Point3d(
                              double.Parse(tok[1]),
                              -double.Parse(tok[2]),
                              double.Parse(tok[3]));

                            var end = new Point3d(
                              double.Parse(tok[4]),
                              -double.Parse(tok[5]),
                              double.Parse(tok[6]));

                            var dead_border = double.Parse(tok[7]);
                            var radius = double.Parse(tok[8]);
                            var length = double.Parse(tok[9]);
                            var volume = double.Parse(tok[10]);

                            var knot = new Knot(index, new Line(start, end), dead_border, radius, length, volume);

                            knots.Add(knot);
                            index++;
                        }
                        log.Knots = knots;
                        //Knots[name] = knots;
                    }
                }
            }
        }
    }

}
