using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{

    using Vec3f = Vec3<float>;

    public class Vec3fGrid :    GridBase<Vec3f>
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GridBase_CreateVec3f();

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetValueWs(IntPtr ptr, double x, double y, double z, float[] value);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetValueIs(IntPtr ptr, int x, int y, int z, float[] value);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetValue(IntPtr ptr, int x, int y, int z, float[] v);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetValuesWs(IntPtr ptr, int num_coords, double[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetValuesIs(IntPtr ptr, int num_coords, int[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetValues(IntPtr ptr, int num_coords, int[] coords, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetActiveVoxels(IntPtr ptr, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetNeighbours(IntPtr ptr, int[] coord, Vec3f[] values);
        #endregion

        public Vec3fGrid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public Vec3fGrid(string name="default", float background=0.0f)
        {
            Ptr = GridBase_CreateVec3f();
            Name = name;
        }

        public override Vec3f GetValueIndex(int[] coordinates)
        {
            var v = new float[3];
            Vec3fGrid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2], v);
            return new Vec3f(v);
        }

        public override Vec3f GetValueWorld(double[] coordinates)
        {
            var v = new float[3];
            Vec3fGrid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2], v);
            return new Vec3f(v);
        }


        public override void SetValue(int[] coordinates, Vec3f value) => 
            Vec3fGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value.Data);

        public override Vec3f[] GetValuesIndex(int[] coordinates)
        {
            int N = coordinates.Length / 3;
            Vec3f[] vecs = new Vec3f[N];
            float[] values = new float[N * 3];

            Vec3fGrid_GetValuesIs(Ptr, N, coordinates, values);

            for (int i = 0; i < vecs.Length; ++i)
                vecs[i] = new Vec3f(
                    values[i + 3 + 0],
                    values[i + 3 + 1],
                    values[i + 3 + 2]);
            
            return vecs;
        }

        public override Vec3f[] GetValuesWorld(double[] coordinates)
        {
            int N = coordinates.Length / 3;
            Vec3f[] vecs = new Vec3f[N];
            float[] values = new float[N * 3];

            Vec3fGrid_GetValuesWs(Ptr, N, coordinates, values);

            for (int i = 0; i < vecs.Length; ++i)
                vecs[i] = new Vec3f(
                    values[i + 3 + 0],
                    values[i + 3 + 1],
                    values[i + 3 + 2]);

            return vecs;
        }

        public override void SetValues(int[] coordinates, Vec3f[] values)
        {
            float[] values_raw = new float[values.Length * 3];
            for(int i = 0; i < values.Length; ++i)
            {
                for (int j = 0; j < 3; ++j)
                    values_raw[i * 3 + j] = values[i].Data[j];
            }    

            Vec3fGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values_raw);
        }

        public override int[] GetActiveVoxels()
        {
            int[] coords = new int[GridBase_GetActiveVoxelCount(Ptr)];
            Vec3fGrid_GetActiveVoxels(Ptr, coords);
            return coords;
        }

        public override Vec3f[] GetNeighbours(int[] coordinates)
        {
            var values = new Vec3f[27];
            Vec3fGrid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            Vec3fGrid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
        }

        public override void SetActiveStates(int[] coordinates, bool[] on)
        {
            Vec3fGrid_SetActiveStates(Ptr, coordinates.Length / 3, coordinates, on.Select(x => (x ? 1 : 0)).ToArray());
        }

        public override object GetGridValue(int x, int y, int z)
        {
            return (object)this[x, y, z];
        }

        public override string ToString()
        {
            return $"Vec3fGrid ({Name})";
        }
    }
}
