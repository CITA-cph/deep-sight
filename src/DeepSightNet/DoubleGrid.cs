using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class DoubleGrid :    GridBase<double>
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GridBase_CreateDouble();

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern double DoubleGrid_GetValueWs(IntPtr ptr, double x, double y, double z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern double DoubleGrid_GetValueIs(IntPtr ptr, int x, int y, int z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetValue(IntPtr ptr, int x, int y, int z, double v);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_GetValuesWs(IntPtr ptr, int num_coords, double[] coords, double[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_GetValuesIs(IntPtr ptr, int num_coords, int[] coords, double[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetValues(IntPtr ptr, int num_coords, int[] coords, double[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_GetActiveVoxels(IntPtr ptr, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_GetNeighbours(IntPtr ptr, int[] coord, double[] values);
        #endregion

        public DoubleGrid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public DoubleGrid(string name="default", float background=0.0f)
        {
            Ptr = GridBase_CreateDouble();
            Name = name;
        }

        public override double GetValueIndex(int[] coordinates)=>
            DoubleGrid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override double GetValueWorld(double[] coordinates) => 
            DoubleGrid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override void SetValue(int[] coordinates, double value) => 
            DoubleGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override double[] GetValuesIndex(int[] coordinates)
        {
            int N = coordinates.Length / 3;
            double[] values = new double[N];

            DoubleGrid_GetValuesIs(Ptr, N, coordinates, values);
            return values;
        }

        public override double[] GetValuesWorld(double[] coordinates)
        {
            int N = coordinates.Length / 3;
            double[] values = new double[N];

            DoubleGrid_GetValuesWs(Ptr, N, coordinates, values);
            return values;
        }

        public override void SetValues(int[] coordinates, double[] values)
        {
            DoubleGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
        }

        public override int[] GetActiveVoxels()
        {
            int[] coords = new int[GridBase_GetActiveVoxelCount(Ptr)];
            DoubleGrid_GetActiveVoxels(Ptr, coords);
            return coords;
        }

        public override double[] GetNeighbours(int[] coordinates)
        {
            var values = new double[27];
            DoubleGrid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            DoubleGrid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
        }

        public override void SetActiveStates(int[] coordinates, bool[] on)
        {
            DoubleGrid_SetActiveStates(Ptr, coordinates.Length / 3, coordinates, on.Select(x => (x ? 1 : 0)).ToArray());
        }

        public override object GetGridValue(int x, int y, int z)
        {
            return (object)this[x, y, z];
        }

        public override string ToString()
        {
            return $"DoubleGrid ({Name})";
        }
    }
}
