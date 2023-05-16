using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;

using GluLamb;

namespace RawLamb
{
    public abstract class KnotFibreOrientationModel
    {
        public abstract Vector3d CalculateFibreOrientation(Point3d point, Knot knot, Vector3d stem_axis);
    }

    public class ArrayComparer : IComparer<double[]>
    {

        public int Compare(double[] x, double[] y)
        {
            return x[0].CompareTo(y[0]);
        }

    }

    public class FlowlineKnotFibreOrientationModel : KnotFibreOrientationModel
    {
        /// <summary>
        /// Velocity of the rectilinear flow.
        /// </summary>
        public double U;
        /// <summary>
        /// Flow rate from the source and sink
        /// </summary>
        public double q;
        /// <summary>
        /// Ratio of U/q
        /// </summary>
        public double G;
        /// <summary>
        /// Short length of the Rankine oval.
        /// </summary>
        public double Bk;
        /// <summary>
        /// Long length of the Rankin oval.
        /// </summary>
        public double Lk;
        /// <summary>
        /// Distance of the source and the sink to the origin of the coordinate system.
        /// </summary>
        public double a;

        public double aa;

        /// <summary>
        /// Look-up table to speed up getting G for every sample.
        /// </summary>
        private List<double[]> GLUT;

        public FlowlineKnotFibreOrientationModel()
        {

        }

        public void Initialize(double radius_min, double radius_max, double step)
        {
            int N = (int)Math.Ceiling((radius_max - radius_min) / step);
            GLUT = new List<double[]>();

            for (int i = 0; i < N; ++i)
            {
                GLUT.Add(new double[5]);
                GLUT[i][0] = radius_min + i * step;
                GLUT[i][1] = radius_min + i * step;

                ApproximateG(ref GLUT[i][0], ref GLUT[i][1], out GLUT[i][2], out GLUT[i][3], out GLUT[i][4]);
            }
        }

        /// <summary>
        /// Approximate G for a given Bk and Lk.
        /// </summary>
        /// <returns></returns>
        public static void ApproximateG(ref double Bk, ref double Lk, out double G, out double a, out double aa)
        {
            // Do some sort of magic here...
            double acc01 = 0.01;
            int acc02 = 8999;
            List<double> table = new List<double>();
            a = 0;

            for (int i = 49; i < acc02; ++i)
            {
                double deg = acc01 * i; // Angle in degrees
                if (deg <= 0 || deg == 1)
                {
                    table.Add(100.0);
                    continue;
                }

                double degR = deg * Math.PI / 180; // Angle in radians
                degR = RhinoMath.ToRadians(deg);
                Lk = Math.Atan(degR);

                a = Bk / degR;
                double R = -(a * Bk) / (Bk * Bk - a * a);
                table.Add(Math.Abs(Lk - R));
            }

            double minT = table.Min();
            double minI = table.IndexOf(minT);
            double minD = 0.1 * (minI + 50);
            aa = Bk / minD;
            //aa = Br;

            // U: the velocity of the rectilinear flow
            // q: the flow rate in m2/s from the source and sink
            // a: distance of the source and sink to the origin of the coordinate system
            // G: ratio of q to U (q/U)
            // bk: short dimension of oval
            // lk: long dimension of oval
            // bk/2 + G/pi * arctan(bk/2a) - g/2 = 0


            //bk / 2 + G / Math.PI * Math.Atan2(bk / (2 * a) - G / 2 = 0;

            G = Math.Abs(Math.PI * (Bk * Bk - aa * aa) / aa);
        }

        public void GetVelocity(double strength, double xs, double ys, double x, double y, out double u, out double v)
        {
            var a = Math.Pow(x - xs, 2) + Math.Pow(y - ys, 2);
            u = strength / (2 * Math.PI) * (x - xs) / a;
            v = strength / (2 * Math.PI) * (y - ys) / a;
        }

        public double GetStreamFunction(double strength, double xs, double ys, double x, double y)
        {
            return strength / (2 * Math.PI) * Math.Atan2(y - ys, x - xs);
        }

        public override Vector3d CalculateFibreOrientation(Point3d point, Knot knot, Vector3d stem_axis)
        {
            // Find knot radius at closest point
            Geometry.ClosestParametersOfCone(knot.Axis.From, knot.Axis.Direction, knot.Radius, point, out Point3d cp, out Bk);

            // Transform point and stem_axis to local CS
            var knot_plane = new Plane(cp, knot.Axis.Direction);
            var projected = stem_axis - knot_plane.ZAxis * (knot_plane.ZAxis * stem_axis);

            projected = knot_plane.Project(stem_axis);
            projected.Unitize();

            var yaxis = Vector3d.CrossProduct(knot_plane.ZAxis, projected);
            knot_plane = new Plane(cp, projected, yaxis);

            var xform = Transform.PlaneToPlane(knot_plane, Plane.WorldXY);
            point.Transform(xform);
            stem_axis.Transform(xform);


            // Freestream speed
            double u_inf = 1.0;
            stem_axis.Unitize();
            double ux = stem_axis.X;
            double uy = stem_axis.Y;

            // beta pattern for late wood
            double aboveA = 1, aboveB = 0.0;
            double belowA = 1, belowB = 0.0;

            /* MAIN */
            double Lr;

            Lk = Bk * 2;

            /* *************************** */
            // Look up G
            /* *************************** */

            var parameters = new double[] { Bk, 0, 0, 0, 0 };
            int index = GLUT.BinarySearch(parameters, new ArrayComparer());
            if (index < 0)
            {
                index = ~index;
                if (index >= GLUT.Count)
                    parameters = GLUT[GLUT.Count - 1];
                else
                {
                    var before = GLUT[index];
                    var after = GLUT[index + 1];

                    var mu = (Bk - before[0]) / (before[0] - after[0]);
                    for (int i = 1; i < parameters.Length; i++)
                    {
                        parameters[i] = GluLamb.Interpolation.Lerp(before[i], after[i], mu);
                    }
                }
            }
            else
                parameters = GLUT[index];

            Lk = parameters[1];
            G = parameters[2];
            a = parameters[3];
            aa = parameters[4];

            /* *************************** */

            double Yn = Bk + G / (2 * Math.PI) * (Math.Atan(Lk / (point.X + aa)) - Math.Atan(Lk / (point.X - aa)));

            if (point.X > 0) // assuming above
                Lr = Lk * aboveA * (1 + aboveB * Yn);
            else // assuming below
                Lr = Lk * belowA * (1 + belowB * Yn);

            double Gnew = G * Lr / Lk;
            double anew = aa * Lr / Lk;

            double strength_source = Gnew;
            double strength_sink = -Gnew;

            double x_source = -anew;
            double y_source = 0.0;    // location of the source

            double x_sink = anew;
            double y_sink = 0.0;    // location of the sink

            // compute the velocity field
            GetVelocity(strength_source, x_source, y_source, point.X, point.Y, out double u_source, out double v_source);

            // compute the stream-function
            double psi_source = GetStreamFunction(strength_source, x_source, y_source, point.X, point.Y);

            // compute the velocity field on the mesh grid
            GetVelocity(strength_sink, x_sink, y_sink, point.X, point.Y, out double u_sink, out double v_sink);

            // compute the stream-function on the grid mesh
            double psi_sink = GetStreamFunction(strength_sink, x_sink, y_sink, point.X, point.Y);

            // superposition of a source and a sink on the freestream
            double u = ux + u_source + u_sink;
            double v = uy + v_source + v_sink;

            var dir = new Vector3d(u, v, stem_axis.Z);
            //var dir = new Vector3d(u, v, 0);

            var xform2 = Transform.PlaneToPlane(Plane.WorldXY, knot_plane);

            //xform.TryGetInverse(out Transform xform2);
            dir.Transform(xform2);
            return dir;
        }
    }
}
