using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeepSight;

namespace testDeepSightNet
{
    internal class Program
    {
        public static Grid MakeBoxGrid(int[] min, int[] max)
        {
            var grid = new Grid(1f);

            for (int i = min[0]; i < max[0]; ++i)
                for (int j = min[1]; j < max[1]; ++j)
                    for (int k = min[2]; k < max[2]; ++k)
                        grid[i, j, k] = 1.0f;

            return grid;
        }

        public static void TestCSG()
        {
            var grid0 = MakeBoxGrid(new int[] { 0, 0, 0 }, new int[] { 20, 20, 20 });
            var grid1 = MakeBoxGrid(new int[] { 10, 10, 10 }, new int[] { 30, 30, 30 });

            Console.WriteLine("grid0 type: {0}", grid0.Class.ToString());
            Console.WriteLine("grid1 type: {0}", grid1.Class.ToString());
            Console.WriteLine("grid0 active values: {0}", grid0.ActiveVoxels().Length);


            //grid0.Class = GridClass.GRID_LEVEL_SET;
            //grid1.Class = GridClass.GRID_LEVEL_SET;

            Grid.Union(grid0, grid1);

            Console.WriteLine("grid0 type: {0}", grid0.Class.ToString());
            Console.WriteLine("grid0 active values: {0}", grid0.ActiveVoxels().Length);


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
                0.5f,0,0,1,
                0,0.5f,0,2,
                0,0,5f,0,
                2,3,4,1
            };

            var grid = Grid.FromMesh(verts, tris, xform);

            for (int i = 0; i < 16; ++i)
                Console.WriteLine("{0} ", grid.Transform[i]);

            Console.WriteLine("Num active values: {0}", grid.ActiveVoxels().Length);
        }

        public static void TestFile()
        {
            var vdb_path = @"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.154113.DK.feb.log02_char\20220301.154113.DK.feb.log02_char.vdb";
            var grid = Grid.Read(vdb_path);


            var active = grid.ActiveVoxels();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);


            Console.WriteLine("Eroding...");
            grid.Erode();

            active = grid.ActiveVoxels();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);
        }

        static void Main(string[] args)
        {

            //TestMesh();
            TestCSG();

            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
