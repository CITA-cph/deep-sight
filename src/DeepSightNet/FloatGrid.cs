﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class FloatGrid :    GridBase<float>
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GridBase_CreateFloat();

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern float FloatGrid_GetValueWs(IntPtr ptr, double x, double y, double z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern float FloatGrid_GetValueIs(IntPtr ptr, int x, int y, int z);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_SetValue(IntPtr ptr, int x, int y, int z, float v);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_GetValuesWs(IntPtr ptr, int num_coords, double[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_GetValuesIs(IntPtr ptr, int num_coords, int[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_SetValues(IntPtr ptr, int num_coords, int[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_GetActiveVoxels(IntPtr ptr, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_SetActiveState(IntPtr ptr, int[] coord, int state);
        #endregion

        public FloatGrid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public FloatGrid(string name="default", float background=0.0f)
        {
            Ptr = GridBase_CreateFloat();
            Name = name;
        }

        public override float GetValueIndex(int[] coordinates)=>
            FloatGrid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override float GetValueWorld(double[] coordinates) => 
            FloatGrid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override void SetValue(int[] coordinates, float value) => 
            FloatGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override float[] GetValuesIndex(int[] coordinates)
        {
            int N = coordinates.Length / 3;
            float[] values = new float[N];

            FloatGrid_GetValuesIs(Ptr, N, coordinates, values);
            return values;
        }

        public override float[] GetValuesWorld(double[] coordinates)
        {
            int N = coordinates.Length / 3;
            float[] values = new float[N];

            FloatGrid_GetValuesWs(Ptr, N, coordinates, values);
            return values;
        }

        public override void SetValues(int[] coordinates, float[] values)
        {
            FloatGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
        }

        public override int[] GetActiveVoxels()
        {
            int[] coords = new int[GridBase_GetActiveVoxelCount(Ptr)];
            FloatGrid_GetActiveVoxels(Ptr, coords);
            return coords;
        }

        public override float[] GetNeighbours(int[] coordinates)
        {
            throw new NotImplementedException();
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            throw new NotImplementedException();
        }

        public override void SetActiveStates(int[] coordinates, bool[] on)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"FloatGrid ({Name})";
        }
    }
}
