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
        static void Main(string[] args)
        {
            var grid = new Grid();

            grid.Name = "Tommy";
            Console.WriteLine(grid.ToString());

            grid.Name = "Jefferson";
            Console.WriteLine(grid.ToString());

            var vdb_path = @"C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.154113.DK.feb.log02_char\20220301.154113.DK.feb.log02_char.vdb";
            grid = Grid.Read(vdb_path);


            var active = grid.ActiveValues();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);


            Console.WriteLine("Eroding...");
            grid.Erode();

            active = grid.ActiveValues();
            Console.WriteLine(active.Length);
            if (active.Length > 2)
                for (int i = 0; i < 3; ++i)
                    Console.WriteLine(active[i]);


            Console.Write("Press any key to exit.");
            Console.ReadLine();
        }
    }
}
