using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace RawLamb
{
    public static class Geometry
    {
        /// <summary>
        /// Determines whether or not a point is inside a cone.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Normalized axis of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="pt">Point to check.</param>
        /// <returns>1 if the point is inside the cone, 0 if it is on the cone, and -1 if it is outside the cone.</returns>
        public static int PointInConeSquared(Point3d apex, Vector3d axis, double height, double radius, Point3d pt)
        {
            var A = pt - apex;      // Vector from sample point to apex
            var dot = A * axis;     // Length of axis at closest point
            var t = dot / height;   // Proportion of length at closest point
            var r = radius * t;     // Radius of cone at closest point
            var B = pt - (apex + axis * dot); // Vector from sample point to closest point

            // Compare length of vector B to the radius at closest point
            return Math.Sign(r * r - (B.X * B.X + B.Y * B.Y + B.Z * B.Z));
        }

        /// <summary>
        /// Creates a truncated conde.
        /// </summary>
        /// <param name="plane">Base plane of cone.</param>
        /// <param name="height">Height of cone.</param>
        /// <param name="radius1">Bottom radius of cone.</param>
        /// <param name="radius2">Top radius of cone.</param>
        /// <param name="cap_bottom">True to cap the bottom of the cone.</param>
        /// <param name="cap_top">True to cap the top of the cone.</param>
        /// <returns></returns>
        public static Brep CreateTruncatedCone(Plane plane, double height, double radius1, double radius2, bool cap_bottom = false, bool cap_top = false)
        {
            var plane2 = new Plane(plane.Origin + plane.ZAxis * height, plane.ZAxis);
            Circle bottom_circle = new Circle(plane, radius1);
            Circle top_circle = new Circle(plane2, radius2);

            LineCurve shapeCurve = new LineCurve(bottom_circle.PointAt(0), top_circle.PointAt(0));
            Line axis = new Line(bottom_circle.Center, top_circle.Center);
            RevSurface revsrf = RevSurface.Create(shapeCurve, axis);
            return Brep.CreateFromRevSurface(revsrf, cap_bottom, cap_top);
        }

        /// <summary>
        /// Tests if a point is inside a cone or not.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Axis of the cone. Its length determines the length of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="point">Point to test.</param>
        /// <returns>+1 if the point is inside the cone, -1 if it is outside the cone, and 0 if it is on the surface of the cone.</returns>
        public static int PointInCone(Point3d apex, Vector3d axis, double radius, Point3d point)
        {
            double height = axis.Length; // Get height from axis vector.
            axis.Unitize();

            var A = point - apex;      // Vector from sample point to apex
            var dot = A * axis;     // Length of axis at closest point
            var t = dot / height;   // Proportion of length at closest point
            var r = radius * t;     // Radius of cone at closest point
            var B = point - (apex + axis * dot); // Vector from sample point to closest point

            // Compare length of vector B to the radius at closest point
            return Math.Sign(r * r - (B.X * B.X + B.Y * B.Y + B.Z * B.Z));
        }

        /// <summary>
        /// Tests if a point is inside a cone or not.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Normalized axis of the cone. Its length determines the length of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="point">Point to test.</param>
        /// <returns>+1 if the point is inside the cone, -1 if it is outside the cone, and 0 if it is on the surface of the cone.</returns>
        public static int PointInCone(Point3d apex, Vector3d axis, double height, double radius, Point3d point)
        {
            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t = dot / height;   // Proportion of length at closest point
            double r = radius * t;     // Radius of cone at closest point
            Vector3d B = point - (apex + axis * dot); // Vector from sample point to closest point

            // Compare length of vector B to the radius at closest point
            return Math.Sign(r * r - (B.X * B.X + B.Y * B.Y + B.Z * B.Z));
        }

        /// <summary>
        /// Gets closest radius of a cone to a point.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Normalized axis of the cone. Its length determines the length of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="point">Point to test.</param>
        /// <returns>Radius of cone at closest point.</returns>
        public static double ClosestRadiusOfCone(Point3d apex, Vector3d axis, double height, double radius, Point3d point)
        {
            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t = dot / height;   // Proportion of length at closest point
            return radius * t;     // Radius of cone at closest point
        }

        /// <summary>
        /// Tests if a point is inside a cone or not.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Normalized axis of the cone. Its length determines the length of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="point">Point to test.</param>
        /// <param name="cpoint">Closest point on cone axis.</param>
        /// <param name="cradius">Radius of cone at closest point.</param>
        /// <returns>Nothing.</returns>
        public static void ClosestParametersOfCone(Point3d apex, Vector3d axis, double height, double radius, Point3d point, out Point3d cpoint, out double cradius)
        {
            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t = dot / height;   // Proportion of length at closest point
            cradius = radius * t;     // Radius of cone at closest point
            cpoint = apex + axis * dot; // Closest point on cone axis
        }

        /// <summary>
        /// Tests if a point is inside a cone or not.
        /// </summary>
        /// <param name="apex">Apex point of the cone.</param>
        /// <param name="axis">Normalized axis of the cone. Its length determines the length of the cone.</param>
        /// <param name="height">Height of the cone.</param>
        /// <param name="radius">Radius of the cone.</param>
        /// <param name="point">Point to test.</param>
        /// <param name="cpoint">Closest point on cone axis.</param>
        /// <param name="cradius">Radius of cone at closest point.</param>
        /// <returns>Nothing.</returns>
        public static void ClosestParametersOfCone(Point3d apex, Vector3d axis, double radius, Point3d point, out Point3d cpoint, out double cradius)
        {
            double height = axis.Length; // Get height from axis vector.
            axis.Unitize();

            Vector3d A = point - apex;      // Vector from sample point to apex
            double dot = A * axis;     // Length of axis at closest point
            double t = dot / height;   // Proportion of length at closest point
            cradius = radius * t;     // Radius of cone at closest point
            cpoint = apex + axis * dot; // Closest point on cone axis
        }
    }
}
