using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeepSight;
using Grid = DeepSight.FloatGrid;

namespace testDeepSightNet
{
    internal class Program
    {
        public static Grid MakeBoxGrid(int[] min, int[] max)
        {
            var grid = new Grid();

            for (int i = min[0]; i < max[0]; ++i)
                for (int j = min[1]; j < max[1]; ++j)
                    for (int k = min[2]; k < max[2]; ++k)
                        grid[i, j, k] = 1.0f;

            return grid;
        }

        public static void TestCSG()
        {
            Console.WriteLine("#######################################");
            Console.WriteLine("TestCSG");
            Console.WriteLine();

            var grid0 = MakeBoxGrid(new int[] { 0, 0, 0 }, new int[] { 20, 20, 20 });
            var grid1 = MakeBoxGrid(new int[] { 10, 10, 10 }, new int[] { 30, 30, 30 });

            Console.WriteLine("grid0 type: {0}", grid0.GridClass.ToString());
            Console.WriteLine("grid1 type: {0}", grid1.GridClass.ToString());
            Console.WriteLine("grid0 active values: {0}", grid0.ActiveVoxelCount);


            grid0.GridClass = GridClass.GRID_LEVEL_SET;
            grid1.GridClass = GridClass.GRID_LEVEL_SET;

            //Grid.Union(grid0, grid1);

            Console.WriteLine("grid0 type: {0}", grid0.GridClass.ToString());
            Console.WriteLine("grid0 active values: {0}", grid0.ActiveVoxelCount);


        }


        public static void TestPoints()
        {
            Console.WriteLine("#######################################");
            Console.WriteLine("TestPoints");
            Console.WriteLine();

            float[] verts = new float[]
            {
                0,0,0,
                1,0,0,
                1,1,0,
                0,1,0,
                0,0,1,
                1,0,1,
                1,1,1,
                0,1,1
            };

            var grid = DeepSight.Convert.PointsToVolume(verts, 3.0f, 0.5f);

            Console.WriteLine("Num active values: {0}", grid.ActiveVoxelCount);
        }


        public static void TestMesh()
        {
            float[] verts = new float[]
            {
                0,0,0,
                1,0,0,
                1,1,0,
                0,1,0,
                0,0,1,
                1,0,1,
                1,1,1,
                0,1,1
            };

            int[] tris = new int[]
            {
                0,1,2,
                0,2,3,
                4,5,6,
                5,6,7,
                0,1,5,
                0,5,4,
                3,0,4,
                3,4,7,
                2,3,7,
                2,7,6,
                1,2,6,
                1,6,5
            };

            float[] xform = new float[] {
                0.1f,0,0,0,
                0,0.1f,0,0,
                0,0,0.1f,0,
                0,0,0,1
            };

            var mesh = new Mesh();
            for (int i = 0; i < verts.Length; i+=3)
            {
                mesh.AddVertex(verts[i], verts[i + 1], verts[i + 2]);
            }

            for (int i = 0; i < tris.Length; i += 3)
            {
                mesh.AddFace(new int[] { tris[i], tris[i + 1], tris[i + 2], 0 });
            }

            var grid = DeepSight.Convert.MeshToVolume(mesh, xform);

            for (int i = 0; i < 16; ++i)
                Console.WriteLine("{0} ", grid.Transform[i]);

            Console.WriteLine("Num active values: {0}", grid.ActiveVoxelCount);
        }

        public static void TestFile()
        {
            Console.WriteLine("#######################################");
            Console.WriteLine("TestFile");
            Console.WriteLine();

            var vdb_path = @"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.154113.DK.feb.log02_char\20220301.154113.DK.feb.log02_char.vdb";
            var grid = GridIO.Read(vdb_path)[0] as Grid;


            var active = grid.GetActiveVoxels();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);


            Console.WriteLine("Eroding...");
            DeepSight.Weathering.Erode(grid);

            active = grid.GetActiveVoxels();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);
        }

        static void Main(string[] args)
        {

            TestMesh();
            TestCSG();
            TestPoints();

            var grid = new FloatGrid();
            grid[0, 0, 0] = 1.0f;
            grid[0, 0, 1] = 1.0f;

            var v = grid[0.0, 0.0, 0.5];

            Console.WriteLine("Grid: {0}", grid);
            Console.WriteLine("Grid name: {0}", grid.Name);
            Console.WriteLine("Value is {0}", v);

            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
