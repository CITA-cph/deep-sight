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

        public Knot(Line axis, double radius, double length, double volume)
        {
            Axis = axis;
            Radius = radius;
            Length = length;
            Volume = volume;
        }
    }
}
