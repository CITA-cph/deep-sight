using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSight
{
    public static class Weathering
    {
        /// <summary>
        /// Returns the exposure value of a voxel based on its 26 neighbours, calculated
        /// as the inverse sum of their occupancy. If a neighbour value is 0, exposure is
        /// increased.
        /// </summary>
        /// <param name="neighbours">The 27 neighbours of a voxel.</param>
        public static float Exposure(float[] neighbours, float threshold = 0.0f)
        {
            // Sum up the neighbour values, subtract its own value (item at index 13)
            float exp = neighbours.Select(x => (x > threshold ? 1 : 0)).Sum() - (neighbours[13] > threshold ? 1 : 0);
            return 1.0F - (exp / 26.0F);
        }

        /// <summary>
        /// Erode the grid based on its voxel exposure. Subtracts a value rate*exposure 
        /// for every active voxel. If the result is <0, sets the voxel value to 0 and
        /// its state to inactive.
        /// </summary>
        /// <param name="rate">Rate of erosion.</param>
        public static void Erode(FloatGrid grid, float rate = 0.1f, float threshold = 0.0f)
        {
            var killList = new List<int>();
            var killState = new List<bool>();

            var active = grid.GetActiveVoxels();
            var N = active.Length / 3;

            for (int i = 0; i < N; ++i)
            {
                int x = active[i * 3];
                int y = active[i * 3 + 1];
                int z = active[i * 3 + 2];

                var nbrs = grid.GetNeighbours(new int[] {x, y, z });
                var exp = Exposure(nbrs, threshold);

                var v = nbrs[13] - rate * exp; // 13 is the cell itself within its neighbourhood
                if (v < 0.0f)
                {
                    grid[x, y, z] = 0.0f;
                    killList.AddRange(new int[] { x, y, z });
                    killState.Add(false);
                }
                else
                {
                    grid[x, y, z] = v;
                }
            }

            grid.SetActiveStates(killList.ToArray(), killState.ToArray());
        }
    }
}
