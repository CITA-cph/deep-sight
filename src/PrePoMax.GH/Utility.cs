using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Management;
using System.IO;
using System.Threading;
using System.ComponentModel;

using Rhino.Geometry;
using CaeResults;

namespace PrePoMax.GH
{
    public static class Utility
    {
        public static Mesh[] ToMesh(this FeResults fres, out int[][] nodeIds, double scale = 1.0, string[] parts = null)
        {
            if (parts == null)
                parts = fres.Mesh.Parts.Keys.ToArray();

            Mesh[] meshes = new Mesh[parts.Length];
            nodeIds = new int[parts.Length][];

            int i = 0;

            foreach (string part in parts)
            {
                meshes[i] = new Mesh();

                Double[][] nodeCoor;
                Int32[] cellIds;
                Int32[][] cells;
                Int32[] cellTypes;

                fres.Mesh.GetVisualizationNodesAndCells(fres.Mesh.Parts[part], out nodeIds[i], out nodeCoor, out cellIds, out cells, out cellTypes);
                var faces = fres.Mesh.GetVisualizationFaceIds(nodeIds[i], cellIds, false, false, true);

                foreach (var node in nodeCoor)
                    meshes[i].Vertices.Add(
                        new Point3d(
                        node[0] * scale,
                        node[1] * scale,
                        node[2] * scale));

                foreach (var cell in cells)
                    meshes[i].Faces.AddFace(cell[0], cell[1], cell[2]);

                meshes[i].SetUserString("part_name", part);

                ++i;
            }

            return meshes;
        }

        public static Mesh DeformMesh(this FeResults fres, Mesh mesh, int[] nodeIds, int stepId = 1, double scale = 1.0)
        {
            if (mesh.Vertices.Count != nodeIds.Length) throw new Exception("Mesh does not match node ID list.");

            var dmesh = mesh.DuplicateMesh();

            var increments = fres.GetIncrementIds(stepId);
            var last_increment = increments.Last();

            var dfdata = fres.GetFieldData("DISP", "ALL", stepId, last_increment);
            var dfield = fres.GetField(dfdata);
            //var du = new float[][] { dfield.GetComponentValues("U1"), dfield.GetComponentValues("U2"), dfield.GetComponentValues("U2") };
            //var du1 = dfield.GetComponentValues("U1");
            //var du2 = dfield.GetComponentValues("U2");
            //var du3 = dfield.GetComponentValues("U3");

            var du = fres.GetNodalMeshDeformations(stepId, last_increment);

            int i = 0;
            foreach (var id in nodeIds)
            {
                var dvec = new Vector3f(
                  du[0][id - 1],
                  du[1][id - 1],
                  du[2][id - 1]);

                dmesh.Vertices[i] = dmesh.Vertices[i] + dvec * (float)scale;
                ++i;
            }

            return dmesh;
        }

        public static bool TetraBarycentric(Vector3d a, Vector3d b, Vector3d c, Vector3d d, Vector3d p, out double[] weights)
        {
            Vector3d vap = p - a;
            Vector3d vbp = p - b;

            Vector3d vab = b - a;
            Vector3d vac = c - a;
            Vector3d vad = d - a;

            Vector3d vbc = c - b;
            Vector3d vbd = d - b;

            // Triple products...
            double va6 = Vector3d.CrossProduct(vbp, vbd) * vbc;
            double vb6 = Vector3d.CrossProduct(vap, vac) * vad;
            double vc6 = Vector3d.CrossProduct(vap, vad) * vab;
            double vd6 = Vector3d.CrossProduct(vap, vab) * vac;
            double v6 = 1.0 / (Vector3d.CrossProduct(vab, vac) * vad);

            weights = new double[] { va6 * v6, vb6 * v6, vc6 * v6, vd6 * v6 };

            if (va6 < 0 || vb6 < 0 || vc6 < 0 || vd6 < 0)
                return false;
            return true;
        }

        public static List<string> AddEngineeringConstants()
        {
            List<string> file = new List<string>();
            var ec = new EngineeringConstants(new double[] { 9700e6, 400e6, 220e6 }, new double[] {0.35, 0.6, 0.55 }, new double[] {400e6, 250e6, 24e6 });
            file.Add(ec.GetDataString());
            return file;

            file.Add("*Material, Name=Wood");
            file.Add("*ELASTIC, TYPE=ENGINEERING CONSTANTS");

            file.Add("9700e6,400e6,220e6,0.35,0.6,0.55,400e6,250e6,");
            file.Add("25e6, 20");
            //file.Add("600e6,600e6,12000e6,0.558,0.038,0.015,40e6,700e6,");
            //file.Add("700e6, 20");
            file.Add("*DENSITY");
            file.Add("450");
            file.Add("*EXPANSION, TYPE=ORTHO, ZERO=13");
            file.Add("0.03, 0.36, 0.78");
            file.Add("*Conductivity");
            file.Add("200");
            file.Add("*Specific heat");
            file.Add("900");

            return file;
        }

        public static void LaunchCCX(string _executable, string _argument, string _workDirectory, List<string> _log)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.CreateNoWindow = true;
            psi.FileName = _executable;
            psi.Arguments = _argument;
            psi.WorkingDirectory = _workDirectory;
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;

            psi.EnvironmentVariables["OMP_NUM_THREADS"] = "8";
            psi.EnvironmentVariables["CCX_NPROC_STIFFNESS"] = "8";
            psi.EnvironmentVariables["NUMBER_OF_CPUS"] = "8";


            var _exe = new Process();
            _exe.StartInfo = psi;
            //
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                _exe.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        // the safe wait handle closes on kill
                        if (!outputWaitHandle.SafeWaitHandle.IsClosed) outputWaitHandle.Set();
                    }
                    else
                    {
                        _log.Add(e.Data);
                    }
                };
                //
                _exe.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        // the safe wait handle closes on kill
                        if (!errorWaitHandle.SafeWaitHandle.IsClosed) errorWaitHandle.Set();
                    }
                    else
                    {
                        //File.AppendAllText(_errorFileName, e.Data + Environment.NewLine);
                        _log.Add(e.Data);
                    }
                };
                //
                _exe.Start();
                //
                _exe.BeginOutputReadLine();
                _exe.BeginErrorReadLine();
                int ms = 1000 * 3600 * 24 * 7 * 3; // 3 weeks
                if (_exe.WaitForExit(ms) && outputWaitHandle.WaitOne(ms) && errorWaitHandle.WaitOne(ms))
                {
                    // Process completed. Check process.ExitCode here.
                    // after Kill() _jobStatus is Killed
                    //if (_jobStatus != JobStatus.Killed) _jobStatus = CaeJob.JobStatus.OK;
                }
                _exe.Close();
            }
        }
    }
}
