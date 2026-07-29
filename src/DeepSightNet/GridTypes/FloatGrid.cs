using System;
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
        private static extern IntPtr GridBase_CreateFloat(float background);

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
        private static extern long FloatGrid_GetActiveVoxels(IntPtr ptr, long capacity, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_GetNeighbours(IntPtr ptr, int[] coord, float[] values);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_InactivateBelow(IntPtr ptr, float threshold);

        #endregion

        public FloatGrid(IntPtr ptr)
        {
            // Was a bare `Ptr = ptr;`. A null handle (which every native
            // factory now returns on failure) produced an object that looked
            // usable and crashed on first use.
            Adopt(ptr, "FloatGrid");
        }

        public FloatGrid(string name="default", float background=0.0f)
        {
            Adopt(GridBase_CreateFloat(background), "FloatGrid");
            Name = name;
        }

        public override GridApi Duplicate()
        {
            ThrowIfDisposed();
            return new FloatGrid(GridApi.GridBase_Duplicate(Ptr));
        }

        /// <summary>
        /// Duplicate FloatGrid.
        /// </summary>
        /// <returns>A new FloatGrid deep copy.</returns>
        public FloatGrid DuplicateGrid()
        {
            return Duplicate() as FloatGrid;
        }

        public override float GetValueIndex(int[] coordinates)=>
            FloatGrid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override float GetValueWorld(double[] coordinates) => 
            FloatGrid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override void SetValue(int[] coordinates, float value) => 
            FloatGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override float[] GetValuesIndex(int[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            float[] values = new float[N];

            FloatGrid_GetValuesIs(Ptr, N, coordinates, values);
            return values;
        }

        public override float[] GetValuesWorld(double[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            float[] values = new float[N];

            FloatGrid_GetValuesWs(Ptr, N, coordinates, values);
            return values;
        }

        public override void SetValues(int[] coordinates, float[] values)
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
            FloatGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
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

            long count = FloatGrid_GetActiveVoxels(Ptr, 0, null);
            if (count < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (count == 0) return new int[0];

            long elements = count * 3;
            if (elements > int.MaxValue)
                throw new InvalidOperationException(
                    $"Grid has {count} active voxels, too many to return in a single array.");

            int[] coords = new int[elements];

            long written = FloatGrid_GetActiveVoxels(Ptr, count, coords);
            if (written < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (written > count)
                throw new InvalidOperationException("Grid was modified while reading active voxels.");

            return coords;
        }

        public override float[] GetNeighbours(int[] coordinates)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            var values = new float[27];
            FloatGrid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            FloatGrid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
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
            FloatGrid_SetActiveStates(Ptr, coordinates.Length / 3, coordinates, on.Select(x => (x ? 1 : 0)).ToArray());
        }

        public override object GetGridValue(int x, int y, int z)
        {
            return (object)this[x, y, z];
        }

        public void InactivateBelow(float threshold)
        {
            FloatGrid_InactivateBelow(Ptr, threshold);
        }

        public override string ToString()
        {
            return $"FloatGrid ({Name})";
        }
    }
}
