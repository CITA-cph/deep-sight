using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class Int32Grid :    GridBase<int>
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GridBase_CreateInt32(int background);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Int32Grid_GetValueWs(IntPtr ptr, double x, double y, double z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Int32Grid_GetValueIs(IntPtr ptr, int x, int y, int z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_SetValue(IntPtr ptr, int x, int y, int z, int v);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_GetValuesWs(IntPtr ptr, int num_coords, double[] coords, int[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_GetValuesIs(IntPtr ptr, int num_coords, int[] coords, int[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_SetValues(IntPtr ptr, int num_coords, int[] coords, int[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_GetActiveVoxels(IntPtr ptr, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Int32Grid_GetNeighbours(IntPtr ptr, int[] coord, int[] values);
        #endregion

        public Int32Grid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public Int32Grid(string name="default", int background=0)
        {
            Ptr = GridBase_CreateInt32(background);
            Name = name;
        }

        public override GridApi Duplicate()
        {
            return new Int32Grid(GridApi.GridBase_Duplicate(Ptr));
        }

        /// <summary>
        /// Duplicate Int32Grid.
        /// </summary>
        /// <returns>A new Int32Grid deep copy.</returns>
        public Int32Grid DuplicateGrid()
        {
            return Duplicate() as Int32Grid;
        }

        public override int GetValueIndex(int[] coordinates)=>
            Int32Grid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override int GetValueWorld(double[] coordinates) => 
            Int32Grid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override void SetValue(int[] coordinates, int value) => 
            Int32Grid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override int[] GetValuesIndex(int[] coordinates)
        {
            int N = coordinates.Length / 3;
            int[] values = new int[N];

            Int32Grid_GetValuesIs(Ptr, N, coordinates, values);
            return values;
        }

        public override int[] GetValuesWorld(double[] coordinates)
        {
            int N = coordinates.Length / 3;
            int[] values = new int[N];

            Int32Grid_GetValuesWs(Ptr, N, coordinates, values);
            return values;
        }

        public override void SetValues(int[] coordinates, int[] values)
        {
            Int32Grid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
        }

        public override int[] GetActiveVoxels()
        {
            int[] coords = new int[GridBase_GetActiveVoxelCount(Ptr) * 3];
            Int32Grid_GetActiveVoxels(Ptr, coords);
            return coords;
        }

        public override int[] GetNeighbours(int[] coordinates)
        {
            var values = new int[27];
            Int32Grid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            Int32Grid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
        }

        public override void SetActiveStates(int[] coordinates, bool[] on)
        {
            Int32Grid_SetActiveStates(Ptr, coordinates.Length / 3, coordinates, on.Select(x => (x ? 1 : 0)).ToArray());
        }

        public override object GetGridValue(int x, int y, int z)
        {
            return (object)this[x, y, z];
        }

        public override string ToString()
        {
            return $"Int32Grid ({Name})";
        }
    }
}
