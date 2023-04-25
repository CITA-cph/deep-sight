﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace RawLamb
{
    public abstract class LogModel
    {
        protected Plane m_plane;
        protected Transform World2Log;
        protected Transform Log2World;

        public Plane Plane
        {
            get { return m_plane; }
            set { 
                m_plane = value;
                World2Log = Transform.PlaneToPlane(m_plane, Plane.WorldXY);
                Log2World = Transform.PlaneToPlane(Plane.WorldXY, m_plane);
            }
        }
        public abstract Plane GetMaterialDirection(Point3d pt);
    }

    public class SimpleLogModel : LogModel
    {
        public double Length;
        public double StartRadius;
        public double EndRadius;

        private double m_spiral_angle;
        private double m_conical_angle;

        public double ConicalAngle
        {
            get { return m_conical_angle; }
            set
            {
                m_conical_angle = value;
                TConicalAngle = Transform.Identity;
                TConicalAngle.M00 = Math.Cos(m_conical_angle);
                TConicalAngle.M01 = -Math.Sin(m_conical_angle);
                TConicalAngle.M10 = Math.Sin(m_conical_angle);
                TConicalAngle.M11 = Math.Cos(m_conical_angle);
            }
        }

        public double SpiralAngle
        {
            get { return m_spiral_angle; }
            set
            {
                m_spiral_angle = value;
                TSpiralGrain = Transform.Identity;
                TSpiralGrain.M00 = Math.Cos(m_spiral_angle);
                TSpiralGrain.M02 = -Math.Sin(m_spiral_angle);
                TSpiralGrain.M20 = Math.Sin(m_spiral_angle);
                TSpiralGrain.M22 = Math.Cos(m_spiral_angle);
            }
        }

        private Transform TSpiralGrain;
        private Transform TConicalAngle;

        public SimpleLogModel() : this(Plane.Unset) { }

        public SimpleLogModel(
            Plane plane, double length = 4000, 
            double start_radius = 200, double end_radius = 200, 
            double spiral_angle = 0, double conical_angle = 0)
        {
            if (plane == Plane.Unset)
                Plane = Plane.WorldXY;
            else
                Plane = plane;

            Length = length;
            StartRadius = start_radius;
            EndRadius = end_radius;

            SpiralAngle = spiral_angle;
            ConicalAngle = conical_angle;
        }

        public override Plane GetMaterialDirection(Point3d pt)
        {
            pt.Transform(World2Log);

            var m = Plane.WorldXY;

            m.Transform(TSpiralGrain);
            m.Transform(TConicalAngle);

            // If the log is oriented along the X-axis of its base plane...
            // var L = Vector3d.XAxis;
            // var R = new Vector3d(0, pt.Y, pt.Z);

            // If the log is oriented along the Z-axis of its base plane...
            var L = Vector3d.ZAxis;
            var R = new Vector3d(pt.X, pt.Y, 0);

            Transform Local2Global = Transform.PlaneToPlane(
              Plane.WorldXY,
              new Plane(pt, L, R));

            m.Transform(Local2Global);

            m.Transform(Log2World);
            return m;
        }

        public GeometryBase ToGeometry()
        {
            if (Math.Abs(m_conical_angle) < Rhino.RhinoMath.Epsilon)
                return new Cylinder(
                    new Circle(Plane, StartRadius), 
                    Length).ToBrep(true, true);


            Circle bottom_circle = new Circle(this.Plane.Origin, StartRadius);
            Circle top_circle = new Circle(this.Plane.Origin + this.Plane.ZAxis * Length, EndRadius);

            LineCurve shapeCurve = new LineCurve(bottom_circle.PointAt(0), top_circle.PointAt(0));
            Line axis = new Line(bottom_circle.Center, top_circle.Center);
            RevSurface revsrf = RevSurface.Create(shapeCurve, axis);
            return Brep.CreateFromRevSurface(revsrf, true, true);
        }
    }
}
