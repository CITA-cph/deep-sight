using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace RawLamb
{
    public class Knot
    {
        public Line Axis;
        public double Radius;
        public double Length;
        public double Volume;
        public int Index;
        public double DeadKnotRadius;

        public Knot() { }

        public Knot(int index, Line axis, double dead_knot_radius, double radius, double length, double volume)
        {
            Axis = axis;
            Radius = radius;
            Length = length;
            Volume = volume;
            Index = index;
            DeadKnotRadius = dead_knot_radius;
        }

        public Knot(Knot knot)
        {
            Index = knot.Index;
            Axis = knot.Axis;
            DeadKnotRadius = knot.DeadKnotRadius;
            Radius = knot.Radius;
            Length = knot.Length;
            Volume = knot.Volume;
        }

        public Knot Duplicate() => new Knot(this);
        public void Transform(Transform xform) => Axis.Transform(xform);
        public int Contains(Point3d point) => RawLamb.Geometry.PointInCone(Axis.From, Axis.Direction, Radius, point);
        public Brep ToBrep() => new Cone(new Plane(Axis.From, Axis.Direction), Axis.Length, Radius).ToBrep(true);

        /// <summary>
        /// Create Fibre Deviation Region (FDR) or TR (Transition Region) knot regions as per 
        /// Lukacevic, M., Kandler, G., Hu, M., Olsson, 
        /// A., & Füssl, J. (2019). A 3D model for knots and related fiber deviations in sawn 
        /// timber for prediction of mechanical properties of boards. Materials and Design, 166. 
        /// https://doi.org/https://doi.org/10.1016/j.matdes.2019.107617.
        /// Depending on the knot length and radius, this will either create a simple region
        /// (single cone) or a complex region (compound cone) consisting of a simple cone and a
        /// truncated cone.
        /// </summary>
        /// <param name="axis">Knot axis. The axis length corresponds to the knot length.</param>
        /// <param name="radius">Knot radius.</param>
        /// <param name="factor">Multiplication factor of apex angle (6 for FDR, 7 for TR).</param>
        /// <param name="radius_limit">Maximum thickness of region (25 for FDR, 30 for TR).</param>
        /// <param name="radius1">Radius of cone if simple; first radius of compound cone if not.</param>
        /// <param name="radius2">0 if simple; second radius of compound cone if not.</param>
        /// <param name="t">Proportion of the knot length at which the first radius occurs. If 
        /// it is a simple region, this will be 1.0.</param>
        /// <returns>Region geometry as array of Breps.</returns>
        public static Brep[] CreateRegion(Line axis, double radius, double factor, double radius_limit,
            out double radius1, out double radius2, out double t)
        {
            double length = axis.Length;
            double slope = radius / length;

            double angle = Math.Atan(slope);
            double slope_start = Math.Tan(angle * factor);

            /*
            Solve the pair of linear equations (y = mx + b) where:
              y = slope * x + radius_limit -> the offset cone with the same slope
              y = slope_start * x -> the wide cone with the same apex
            Solve for x...
            */
            double x = radius_limit / (slope_start - slope);
            radius1 = radius + radius_limit;

            var plane0 = new Plane(axis.From, axis.Direction);

            // If the intersection of the two lines is within the range of
            // the knot axis...
            if (length > x)
            {
                radius2 = x * slope_start;
                var plane1 = new Plane(plane0.Origin + plane0.ZAxis * x, plane0.ZAxis);
                t = x / length;
                return new Brep[]{
                    new Cone(plane0, x, radius2).ToBrep(false),
                    Geometry.CreateTruncatedCone(plane1, length - x, radius2, radius1, false, true)
                };
            }
            // Otherwise, the intersection is past the end of the knot and
            // doesn't matter...
            else
            {
                t = 1.0;
                radius2 = 0.0;
                return new Brep[] { new Cone(plane0, length, radius1).ToBrep(true) };
            }
        }

        public static int PointInKnotRegion(Point3d apex, Vector3d axis, double height, double radius1, double radius2, double t, Point3d point)
        {
            if (t == 1.0)
                return RawLamb.Geometry.PointInCone(apex, axis, height, radius1, point);

            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t2 = dot / height;   // Proportion of length at closest point

            if (dot < height * t) // if the closest point is in the cone region of the axis...
                return RawLamb.Geometry.PointInCone(apex, axis, height * t, radius2, point);
            else if (dot > height) return -1; // if closest point is past the end of the knot...

            double r = (radius2 - radius1) * (1 - t2) / (1 - t) + radius1; // Radius of truncated cone at closest point
            Vector3d B = point - (apex + axis * dot); // Vector from sample point to closest point

            // Compare length of vector B to the radius at closest point
            return Math.Sign(r * r - (B.X * B.X + B.Y * B.Y + B.Z * B.Z));
        }
    }

    public class KnotRegion
    {
        Knot Knot;
        public double Parameter;
        public double Radius1;
        public double Radius2;
        private double m_length;

        public double Length
        {
            get { return m_length; }
        }

        public KnotRegion(Knot knot, double apex_factor, double thickness)
        {
            Knot = knot;
            m_length = knot.Axis.Length;

            double slope = Knot.Radius / m_length;

            double angle = Math.Atan(slope);
            double slope_start = Math.Tan(angle * apex_factor);

            /*
            Solve the pair of linear equations (y = mx + b) where:
              y = slope * x + radius_limit -> the offset cone with the same slope
              y = slope_start * x -> the wide cone with the same apex
            Solve for x...
            */
            double x = thickness / (slope_start - slope);
            Radius1 = knot.Radius + thickness;

            //var plane0 = new Plane(knot.Axis.From, knot.Axis.Direction);

            // If the intersection of the two lines is within the range of
            // the knot axis...
            if (m_length > x)
            {
                Radius2 = x * slope_start;
                //var plane1 = new Plane(plane0.Origin + plane0.ZAxis * x, plane0.ZAxis);
                Parameter = x / m_length;
                //return new Brep[]{
                //    new Cone(plane0, x, Radius2).ToBrep(false),
                //    Geometry.CreateTruncatedCone(plane1, m_length - x, Radius2, Radius1, false, true)
                //};
            }
            // Otherwise, the intersection is past the end of the knot and
            // doesn't matter...
            else
            {
                Parameter = 1.0;
                Radius2 = 0.0;
                //return new Brep[] { new Cone(plane0, m_length, Radius1).ToBrep(true) };
            }
        }

        public Brep ToBrep()
        {
            var plane = new Plane(Knot.Axis.From, Knot.Axis.Direction);
            if (Parameter < 1.0)
            {
                double x = m_length * Parameter;
                var plane1 = new Plane(plane.Origin + plane.ZAxis * x, plane.ZAxis);

                return Brep.JoinBreps(new Brep[]{
                        new Cone(plane, x, Radius2).ToBrep(false),
                        Geometry.CreateTruncatedCone(plane1, m_length - x, Radius2, Radius1, false, true) }, 0.01)[0];
            }
            return Brep.JoinBreps(new Brep[] { new Cone(plane, m_length, Radius1).ToBrep(true) }, 0.01)[0];
        }

        public int Contains(Point3d point)
        {
            var axis = Knot.Axis.Direction;
            axis.Unitize();
            var apex = Knot.Axis.From;

            if (Parameter == 1.0)
                return RawLamb.Geometry.PointInCone(apex, axis, m_length, Radius1, point);

            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t2 = dot / m_length;   // Proportion of length at closest point

            if (dot < m_length * Parameter) // if the closest point is in the cone region of the axis...
                return RawLamb.Geometry.PointInCone(apex, axis, m_length * Parameter, Radius2, point);
            else if (dot > m_length) return -1; // if closest point is past the end of the knot...

            double r = (Radius2 - Radius1) * (1 - t2) / (1 - Parameter) + Radius1; // Radius of truncated cone at closest point
            Vector3d B = point - (apex + axis * dot); // Vector from sample point to closest point

            // Compare length of vector B to the radius at closest point
            return Math.Sign(r * r - (B.X * B.X + B.Y * B.Y + B.Z * B.Z));
        }
    }
}
