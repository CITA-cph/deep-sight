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
        private static extern IntPtr GridBase_CreateVec3f(float[] background);

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
        private static extern long Vec3fGrid_GetActiveVoxels(IntPtr ptr, long capacity, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Vec3fGrid_GetNeighbours(IntPtr ptr, int[] coord, Vec3f[] values);
        #endregion

        public Vec3fGrid(IntPtr ptr)
        {
            // Was a bare `Ptr = ptr;`. A null handle (which every native
            // factory now returns on failure) produced an object that looked
            // usable and crashed on first use.
            Adopt(ptr, "Vec3fGrid");
        }

        public Vec3fGrid(string name="default", float[] background=null)
        {
            if (background == null) background = new float[] {0.0f, 0.0f, 0.0f};
            Adopt(GridBase_CreateVec3f(background), "Vec3fGrid");
            Name = name;
        }

        public override GridApi Duplicate()
        {
            ThrowIfDisposed();
            return new Vec3fGrid(GridApi.GridBase_Duplicate(Ptr));
        }

        /// <summary>
        /// Duplicate Int32Grid.
        /// </summary>
        /// <returns>A new Vec3fGrid deep copy.</returns>
        public Vec3fGrid DuplicateGrid()
        {
            return Duplicate() as Vec3fGrid;
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

        public void SetValue(int[] coordinates, float[] value) => 
            Vec3fGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override Vec3f[] GetValuesIndex(int[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            Vec3f[] vecs = new Vec3f[N];
            float[] values = new float[N * 3];

            Vec3fGrid_GetValuesIs(Ptr, N, coordinates, values);

            for (int i = 0; i < vecs.Length; ++i)
                vecs[i] = new Vec3f(
                    values[i * 3 + 0],
                    values[i * 3 + 1],
                    values[i * 3 + 2]);
            
            return vecs;
        }

        public override Vec3f[] GetValuesWorld(double[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            Vec3f[] vecs = new Vec3f[N];
            float[] values = new float[N * 3];

            Vec3fGrid_GetValuesWs(Ptr, N, coordinates, values);

            for (int i = 0; i < vecs.Length; ++i)
                vecs[i] = new Vec3f(
                    values[i * 3 + 0],
                    values[i * 3 + 1],
                    values[i * 3 + 2]);

            return vecs;
        }

        public override void SetValues(int[] coordinates, Vec3f[] values)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length != coordinates.Length / 3)
                throw new ArgumentException(
                    "Expected one value per coordinate triplet; the native side reads that many "
                    + "regardless of the array's actual length.", nameof(values));
            float[] values_raw = new float[values.Length * 3];
            for(int i = 0; i < values.Length; ++i)
            {
                for (int j = 0; j < 3; ++j)
                    values_raw[i * 3 + j] = values[i].Data[j];
            }    

            Vec3fGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values_raw);
        }

        public void SetValues(int[] coordinates, float[] values)
        {
            Vec3fGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
        }

        /// <summary>
        /// Index-space coordinates of every active voxel, as XYZ triplets.
        /// </summary>
        /// <remarks>
        /// Rewritten to use a size-then-fill handshake. The previous version
        /// allocated a buffer from GridBase_GetActiveVoxelCount() and then
        /// called a native function that took no capacity argument and wrote
        /// 3 * activeVoxelCount ints into it on trust. Those two numbers did
        /// not have to agree: activeVoxelCount() counts voxels inside active
        /// tiles, which the native enumeration loop skipped, so the tail of the
        /// array was left uninitialised (and the reverse ordering of the two
        /// calls, or a grid mutated in between, overran it). The native side
        /// now expands tiles and refuses to write past the capacity it is told.
        /// </remarks>
        public override int[] GetActiveVoxels()
        {
            ThrowIfDisposed();

            long count = Vec3fGrid_GetActiveVoxels(Ptr, 0, null);
            if (count < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (count == 0) return new int[0];

            long elements = count * 3;
            if (elements > int.MaxValue)
                throw new InvalidOperationException(
                    $"Grid has {count} active voxels, too many to return in a single array.");

            int[] coords = new int[elements];

            long written = Vec3fGrid_GetActiveVoxels(Ptr, count, coords);
            if (written < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (written > count)
                throw new InvalidOperationException("Grid was modified while reading active voxels.");

            return coords;
        }

        public override Vec3f[] GetNeighbours(int[] coordinates)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            var values = new Vec3f[27];
            Vec3fGrid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            Vec3fGrid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
        }

        public override void SetActiveStates(int[] coordinates, bool[] on)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            if (on == null) throw new ArgumentNullException(nameof(on));
            if (on.Length != coordinates.Length / 3)
                throw new ArgumentException("Expected one state per coordinate triplet.", nameof(on));
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
