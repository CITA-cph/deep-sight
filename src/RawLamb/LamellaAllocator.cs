using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using GluLamb;

namespace RawLamb
{
     /// <summary>
    /// Class to allocate Lamellas to Boards. IN PROGRESS. Needs to be integrated with SortingHat.
    /// </summary>
    public class Alligator
    {
        public Plane LamellaPlane;
        public Plane BoardPlane;

        public List<Board> Boards;
        public List<int> BoardLogIndices;
        public List<Lamella> Lamellas;

        public List<Plane> Planes;
        public List<Guid> BoardIds;

        public double MaxRotation = 0.1;
        public bool UseRotation = false;

        public int Tries = 300;
        public int MaxTries = 500;

        // Debugging
        public List<string> Log;

        private Random rnd;

        public Alligator()
        {
            LamellaPlane = Plane.WorldXY;
            BoardPlane = Plane.WorldXY;
            Boards = new List<Board>();
            BoardLogIndices = new List<int>();
            Lamellas = new List<Lamella>();

            Planes = new List<Plane>();
            BoardIds = new List<Guid>();

            Log = new List<string>();

            rnd = new Random();
        }

        public List<LamellaPlacement> Alligate()
        {
            // Check for stupid inputs
            if (Boards.Count < 1) throw new Exception("No boards to alligate onto.");
            if (Lamellas.Count < 1) throw new Exception("No lamellas to alligate.");

            // Initialize geometry, boards, and lamellas
            var bps = new List<BoardPlacement>();
            var lrecs = new List<Rectangle3d>();
            var lps = new List<LamellaPlacement>();

            for (int i = 0; i < Boards.Count; ++i)
            {
                var centre = Boards[i].Centre.Duplicate();
                centre.Transform(Transform.PlaneToPlane(Boards[i].Plane, Plane.WorldXY));

                var bp = new BoardPlacement(centre);
                bps.Add(bp);
            }

            for (int i = 0; i < Lamellas.Count; ++i)
            {
                lps.Add(new LamellaPlacement());
                lrecs.Add(new Rectangle3d(
                  Plane.WorldXY,
                  new Interval(Lamellas[i].Width * -0.5, Lamellas[i].Width * 0.5),
                  new Interval(0, Lamellas[i].Length)
                  ));
            }

            // Initialize allocator variables
            int N = 300;
            int max_tries = N * 2;
            bool flag = false;
            int index = -1;

            // Metrics
            int counter = 0;
            int num_lamellas_placed = 0;

            // Alligate the alligator
            for (int i = 0; i < lrecs.Count; ++i)
            {
                for (int j = 0; j < Tries; ++j)
                {
                    counter++;

                    index = rnd.Next(0, bps.Count);
                    var board = bps[index];

                    if (Lamellas[i].Thickness >= Boards[index].Thickness)
                        continue;
                    if (Lamellas[i].Thickness + 15.0 < Boards[index].Thickness)
                        continue;

                    //Log.Add(string.Format("Lamella {0} (t{2:0.0}): matching to board {1} ({3})",
                    //  i, index, Lamellas[i].Thickness, j));

                    var lam = lrecs[i];

                    double xRange = board.RangeX - lam.Width;
                    double yRange = board.RangeY - lam.Height;

                    var x = rnd.NextDouble() * xRange + board.MinX;
                    var y = rnd.NextDouble() * yRange + board.MinY;

                    //Plane plane = LamellaPlane;
                    Plane plane = Lamellas[i].Plane;

                    if (UseRotation)
                    {
                        plane.Transform(Transform.Rotation(rnd.NextDouble() * MaxRotation, Point3d.Origin));
                    }

                    plane.Transform(Transform.Translation(new Vector3d(x, y, 0)));
                    //xform = Transform.Multiply(xform, Transform.Translation(new Vector3d(x, y, 0)));

                    lam.Transform(Transform.PlaneToPlane(Plane.WorldXY, plane));

                    if (!IsInside(bps[index].Outline.ToNurbsCurve(), lam))
                        continue;

                    flag = false;

                    foreach (var p in bps[index].Lamellas)
                    {
                        if (IsOverlapping(p, lam))
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (!flag)
                    {
                        bps[index].Lamellas.Add(lam);
                        plane.Transform(Transform.PlaneToPlane(Plane.WorldXY, Boards[index].Plane));
                        Planes.Add(plane);
                        BoardIds.Add(Boards[index].BoardId);
                        lrecs[i] = lam;

                        lps[i].Plane = plane;
                        lps[i].Placed = true;
                        lps[i].BoardIndex = index;
                        lps[i].LogIndex = BoardLogIndices[index];
                        lps[i].Name = Lamellas[i].Name;
                         
                        var mesh = Lamellas[i].Mesh.DuplicateMesh();
                        mesh.Transform(Transform.PlaneToPlane(new Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.ZAxis), plane));

                        num_lamellas_placed++;
                        Log.Add("Success!");
                        break;
                    }
                    else
                    {
                        bps[index].FalseTries++;
                        if (bps[index].FalseTries > MaxTries)
                        {
                            //bps[index].Full = true;
                        }
                    }
                }

                if (flag)
                {
                    //Log.Add(string.Format("Ran out of tries: {0}", counter));
                    //Print("Ran out of tries!");
                    //boards[index].Full = true;
                    //break;
                }

                Log.Add(string.Format("Moving on from lamella {0}", i));
            }

            Log.Add(string.Format("Placed {0} lamellas on {1} boards.", num_lamellas_placed, Boards.Count));

            return lps;
        }

        public bool IsOverlapping(Rectangle3d r1, Rectangle3d r2)
        {
            if (!UseRotation) // Use simple, axis-aligned method
            {
                // If one rectangle is on left side of other
                if (r1.BoundingBox.Min.X >= r2.BoundingBox.Max.X ||
                  r2.BoundingBox.Min.X >= r1.BoundingBox.Max.X)
                    return false;

                // If one rectangle is above other
                if (r1.BoundingBox.Min.Y >= r2.BoundingBox.Max.Y ||
                  r2.BoundingBox.Min.Y >= r1.BoundingBox.Max.Y)
                    return false;

                return true;
            }

            var res = Rhino.Geometry.Intersect.Intersection.CurveCurve(r1.ToNurbsCurve(), r2.ToNurbsCurve(), 0.1, 0.1);
            return res.Count > 0;
        }

        public bool IsInside(Curve crv, Rectangle3d r)
        {
            for (int i = 0; i < 4; ++i)
                if (crv.Contains(r.Corner(i), Plane.WorldXY, 0.1) != PointContainment.Inside)
                    return false;
            return true;
        }
    }



    /// <summary>
    /// Class to define board placement.
    /// </summary>
    public class BoardPlacement
    {
        public BoardPlacement(Polyline poly)
        {
            Outline = poly;
            BoundingBox = poly.BoundingBox;

            RangeX = BoundingBox.Max.X - BoundingBox.Min.X;
            RangeY = BoundingBox.Max.Y - BoundingBox.Min.Y;

            MinX = BoundingBox.Min.X;
            MinY = BoundingBox.Min.Y;

            Lamellas = new List<Rectangle3d>();
            LamellaMeshes = new List<Mesh>();
            Full = false;
        }

        public List<Rectangle3d> Lamellas;
        public List<Mesh> LamellaMeshes;
        public bool Full;
        public BoundingBox BoundingBox;
        public Polyline Outline;

        public double RangeX;
        public double RangeY;

        public double MinX;
        public double MinY;

        public int FalseTries = 0;
    }
}
