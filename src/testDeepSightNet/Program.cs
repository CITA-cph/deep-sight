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


            Console.ReadLine();
        }
    }
}
