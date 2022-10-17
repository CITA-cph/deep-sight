using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

namespace RawLamb
{
    public class Board
    {
        public Guid BoardId;
        public Guid LogId;

        public Log Log = null;

        public string Name;
        public List<Polyline> Top;
        public Polyline Centre;
        public List<Polyline> Bottom;
        public Plane Plane;
        public Plane TopPlane;
        public Plane BottomPlane;
        public double Thickness;

        public Board(string name = "Board") : this(null, name)
        {
        }

        public static void ExportSTEP(Plane board_plane, double l, double w, double t, List<Knot> knots, string output_path)
        {
            var board_dims = new float[] {(float)l, (float)w, (float)t};

            var board_plane_flat = new float[9];

            board_plane_flat[0] = (float)board_plane.OriginX;
            board_plane_flat[1] = (float)board_plane.OriginY;
            board_plane_flat[2] = (float)board_plane.OriginZ;

            board_plane_flat[3] = (float)board_plane.XAxis.X;
            board_plane_flat[4] = (float)board_plane.XAxis.Y;
            board_plane_flat[5] = (float)board_plane.XAxis.Z;

            board_plane_flat[6] = (float)board_plane.ZAxis.X;
            board_plane_flat[7] = (float)board_plane.ZAxis.Y;
            board_plane_flat[8] = (float)board_plane.ZAxis.Z;

            var knot_data = new float[knots.Count * 11];
            for (int i = 0; i < knots.Count; ++i)
            {
                int ii = i * 11;
                var knot_plane = new Plane(knots[i].Axis.From, knots[i].Axis.Direction);

                knot_data[ii + 0] = (float)knots[i].Axis.From.X;
                knot_data[ii + 1] = (float)knots[i].Axis.From.Y;
                knot_data[ii + 2] = (float)knots[i].Axis.From.Z;

                knot_data[ii + 3] = (float)knot_plane.XAxis.X;
                knot_data[ii + 4] = (float)knot_plane.XAxis.Y;
                knot_data[ii + 5] = (float)knot_plane.XAxis.Z;

                knot_data[ii + 6] = (float)knot_plane.ZAxis.X;
                knot_data[ii + 7] = (float)knot_plane.ZAxis.Y;
                knot_data[ii + 8] = (float)knot_plane.ZAxis.Z;

                knot_data[ii + 9] = (float)knots[i].Radius;
                knot_data[ii + 10] = (float)knots[i].Axis.Length;
            }

            DeepSight.RLGeom.ExportBoard2Step(board_plane_flat, board_dims, knots.Count, knot_data, output_path);
        }

        public Board(Log log, string name = "Board", double thickness = 24.0)
        {
            Log = log;
            //LogId = log.LogId;
            Thickness = thickness;
            Name = name;

            Top = new List<Polyline>();
            Centre = new Polyline();
            Bottom = new List<Polyline>();
            Plane = Plane.WorldXY;

            BoardId = Guid.NewGuid();
        }

        public void Transform(Transform xform)
        {
            for (int i = 0; i < Top.Count; ++i)
            {
                Top[i].Transform(xform);
            }
            Centre.Transform(xform);

            for (int i = 0; i < Bottom.Count; ++i)
            {
                Bottom[i].Transform(xform);
            }
            Plane.Transform(xform);
        }

        public Board Duplicate()
        {
            var board = new Board()
            {
                Name = Name,
                Centre = Centre.Duplicate(),
                Plane = Plane,
                Thickness = Thickness,
                Log = Log

            };

            for (int i = 0; i < Top.Count; ++i)
            {
                board.Top.Add(Top[i].Duplicate());
            }

            for (int i = 0; i < Bottom.Count; ++i)
            {
                board.Bottom.Add(Bottom[i].Duplicate());
            }

            return board;
        }
    }
}
