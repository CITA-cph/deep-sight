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
    }
}
