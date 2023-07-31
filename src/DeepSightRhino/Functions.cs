using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using GluLamb;

namespace RawLambLib
{

    public static class Functions
    {
        public static List<Lamella> ExtractUnbentLamellas(Glulam g, double resolution = 50.0, double extra_tolerance = 0.0)
        {
            var lams = new List<Lamella>();
            var lamcrvs = g.GetLamellaeCurves();

            double hw = g.Width / 2;
            double hh = g.Height / 2;

            double lw = g.Data.LamWidth;
            double lh = g.Data.LamHeight;

            double hlw = lw / 2;
            double hlh = lh / 2;

            int l = 0;
            for (int x = 0; x < g.Data.NumWidth; ++x)
            {
                for (int y = 0; y < g.Data.NumHeight; ++y)
                {
                    var length = lamcrvs[l].GetLength();
                    var lam = new Lamella(g.Id, g, lh, x, y);
                    Mesh lmesh = GluLamb.Utility.Create3dMeshGrid(lw + extra_tolerance, lh, length + extra_tolerance, resolution);

                    lam.Mesh = lmesh;
                    lam.Length = length;
                    lam.Width = g.Data.LamWidth;
                    //lam.Thickness = g.Data.LamHeight;
                    //lam.StackPositionX = x;
                    //lam.StackPositionY = y;
                    lam.Plane = new Plane(
                      new Point3d(
                          -hw + lw * x + hlw,
                          -hh + lh * y + hlh,
                          0),
                      Vector3d.XAxis,
                      Vector3d.YAxis);
                    lams.Add(lam);
                    l++;

                }
            }

            return lams;
        }
    }

    /*
     Glulam g = ...
     Log l = ...
     var lamellas = Functions.ExtractUnbentLamellas(g, resolution);
     var sh = new SortingHat();
     sh.AddBoards(l.Boards);
     sh.AddLamellas(lamellas);

     sh.MatchLamellasAndBoards();
     // profit

     */

    /// <summary>
    /// Sort boards and lamellas, find appropriate boards for each lamella.
    /// </summary>
    public class SortingHat
    {
        public List<Log> Logs;
        public List<GluLamb.Glulam> Glulams;

        public Dictionary<Board, HashSet<Lamella>> BoardMatches;
        public Dictionary<Lamella, HashSet<Board>> LamellaMatches;

        /* -- Parameters -- */
        double MaxExtraThickness = 3.0;

        public SortingHat()
        {
            Logs = new List<Log>();
            Glulams = new List<GluLamb.Glulam>();
        }

        public void AddBoards(IEnumerable<Board> boards)
        {
            foreach (var brd in boards)
                BoardMatches.Add(brd, new HashSet<Lamella>());
        }

        public void AddLamellas(IEnumerable<Lamella> lamellas)
        {
            foreach (var lam in lamellas)
                LamellaMatches.Add(lam, new HashSet<Board>());
        }

        /// <summary>
        /// Matches boards and lamellas based on their thickness, with some defined tolerance for boards being thicker.
        /// </summary>
        public void MatchLamellasAndBoards()
        {
            // Clear already matched pairs
            foreach (var brd in BoardMatches.Keys)
                BoardMatches[brd].Clear();
            foreach (var lam in LamellaMatches.Keys)
                LamellaMatches[lam].Clear();

            // Match based on thickness
            foreach (var lam in LamellaMatches.Keys)
            {
                foreach (var brd in BoardMatches.Keys)
                {
                    if (brd.Thickness >= lam.Thickness && brd.Thickness <= lam.Thickness + MaxExtraThickness)
                    {
                        LamellaMatches[lam].Add(brd);
                        BoardMatches[brd].Add(lam);
                    }
                }
            }
        }
    }



}