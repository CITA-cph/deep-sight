using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace RawLamb
{
    public struct Knot
    {
        public Line Axis;
        public double Radius;
        public double Length;
        public double Volume;
        public int Index;
        public double DeadKnotRadius;

        public Knot(int index, Line axis, double dead_knot_radius, double radius, double length, double volume)
        {
            Axis = axis;
            Radius = radius;
            Length = length;
            Volume = volume;
            Index = index;
            DeadKnotRadius = dead_knot_radius;
        }

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

    }
}
