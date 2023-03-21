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
        internal static Dictionary<string, string> settings = new Dictionary<string, string>();

        public static Grid MakeBoxGrid(int[] min, int[] max)
        {
            var grid = new Grid();

            for (int i = min[0]; i < max[0]; ++i)
                for (int j = min[1]; j < max[1]; ++j)
                    for (int k = min[2]; k < max[2]; ++k)
                        grid[i, j, k] = 1.0f;

            return grid;
        }

        public static void LoadSettings(string path)
        {
            if (System.IO.File.Exists(path))
            {
                var lines = System.IO.File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    var tok = line.Split('=');
                    if (tok.Length != 2)
                        continue;
                    var key = tok[0].Trim();
                    var value = tok[1].Trim();

                    settings.Add(key, value);
                }
            }
        }

        public static void TestCSG()
        {
            Console.WriteLine();
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
            Console.WriteLine();
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
            Console.WriteLine();
            Console.WriteLine("#######################################");
            Console.WriteLine("TestMesh");
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

            var active = grid.GetActiveVoxels();
            var N = Math.Min(20, active.Length / 3);
            for (int i = 0; i < N; ++i)
            {
                Console.WriteLine($"[{active[i * 3]}, {active[i * 3 + 1]}, {active[i * 3 + 2]}]");
            }
        }

        public static void TestFile()
        {
            Console.WriteLine();
            Console.WriteLine("#######################################");
            Console.WriteLine("TestFile");
            Console.WriteLine();

            string vdb_path = "";

            if (settings.ContainsKey("test_vdb_path"))
                vdb_path = settings["test_vdb_path"];
            else
            {
                Console.WriteLine($"Couldn't find 'test_vdb_path' in settings.");
                return;
            }

            if (!System.IO.File.Exists(vdb_path))
            {
                Console.WriteLine($"File '{vdb_path}' doesn't exist.");
                return;
            }

            Console.WriteLine($"Loading grids from '{vdb_path}'...");
            
            var grids = GridIO.Read(vdb_path);
            if (grids == null || grids.Length < 1)
            {
                Console.WriteLine("Couldn't load any grids...");
                return;
            }

            Grid grid;
            if (grids[0] != null && grids[0] is Grid)
                grid = grids[0] as Grid;
            else
                return;

            Console.WriteLine($"Found {grid}.");

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

        public static void TestBasic()
        {
            Console.WriteLine();
            Console.WriteLine("#######################################");
            Console.WriteLine("TestBasic");
            Console.WriteLine();

            var grid = new FloatGrid();
            grid[0, 0, 0] = 1.0f;
            grid[0, 0, 1] = 1.0f;

            var v = grid[0.0, 0.0, 0.5];

            Console.WriteLine("Grid: {0}", grid);
            Console.WriteLine("Grid name: {0}", grid.Name);
            Console.WriteLine("Value is {0}", v);
        }

        public static void TestGridTypes()
        {
            Console.WriteLine();
            Console.WriteLine("#######################################");
            Console.WriteLine("TestGridTypes");
            Console.WriteLine();

            var fgrid = new FloatGrid();
            fgrid[0,0,0] = 1.0f;

            var igrid = new Int32Grid();
            igrid[0, 0, 0] = 50;

            var dgrid = new DoubleGrid();
            dgrid[0, 0, 0] = 30.0;

            var dgrid2 = new DoubleGrid();
            dgrid[0, 0, 0] = 30.0;

            var vgrid = new Vec3fGrid();
            vgrid[0, 0, 0] = new Vec3<float> (3.0f, 0.0f, 1.3f);

            Console.WriteLine($"{fgrid}");
            Console.WriteLine($"{igrid}");
            Console.WriteLine($"{dgrid}");
            Console.WriteLine($"{vgrid}");

            var path = "C:/tmp/grids.vdb";
            Console.Write("Writing grids... ");
            GridIO.Write(path, new GridApi[] { vgrid, igrid, dgrid, dgrid2, fgrid });
            Console.WriteLine("OK.");

            Console.Write("Reading grids... ");
            var grids = GridIO.Read(path);
            Console.WriteLine($"OK. Found {grids.Length} grids.");
            Console.WriteLine();

            foreach (var grid in grids)
            {
                Console.WriteLine($"{grid}");
                Console.WriteLine($"    {grid} value is {grid.GetGridValue(0, 0, 0)}");

            }
        }

        static void Main(string[] args)
        {
            LoadSettings("test.ini");
            TestBasic();
            TestMesh();

            //TestCSG();
            //TestPoints();
            TestFile();
            //TestGridTypes();


            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
