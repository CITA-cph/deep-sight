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
        private static extern IntPtr GridBase_CreateDouble(double background);

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
        private static extern long DoubleGrid_GetActiveVoxels(IntPtr ptr, long capacity, int[] coords);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetActiveState(IntPtr ptr, int[] coord, int state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_SetActiveStates(IntPtr ptr, int num_boords, int[] coord, int[] state);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DoubleGrid_GetNeighbours(IntPtr ptr, int[] coord, double[] values);
        #endregion

        public DoubleGrid(IntPtr ptr)
        {
            // Was a bare `Ptr = ptr;`. A null handle (which every native
            // factory now returns on failure) produced an object that looked
            // usable and crashed on first use.
            Adopt(ptr, "DoubleGrid");
        }

        public DoubleGrid(string name="default", double background=0.0)
        {
            Adopt(GridBase_CreateDouble(background), "DoubleGrid");
            Name = name;
        }

        public override GridApi Duplicate()
        {
            ThrowIfDisposed();
            return new DoubleGrid(GridApi.GridBase_Duplicate(Ptr));
        }

        /// <summary>
        /// Duplicate DoubleGrid.
        /// </summary>
        /// <returns>A new DoubleGrid deep copy.</returns>
        public DoubleGrid DuplicateGrid()
        {
            return Duplicate() as DoubleGrid;
        }

        public override double GetValueIndex(int[] coordinates)=>
            DoubleGrid_GetValueIs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override double GetValueWorld(double[] coordinates) => 
            DoubleGrid_GetValueWs(Ptr, coordinates[0], coordinates[1], coordinates[2]);

        public override void SetValue(int[] coordinates, double value) => 
            DoubleGrid_SetValue(Ptr, coordinates[0], coordinates[1], coordinates[2], value);

        public override double[] GetValuesIndex(int[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            double[] values = new double[N];

            DoubleGrid_GetValuesIs(Ptr, N, coordinates, values);
            return values;
        }

        public override double[] GetValuesWorld(double[] coordinates)
        {
            ThrowIfDisposed();
            if (coordinates == null) throw new ArgumentNullException(nameof(coordinates));
            if (coordinates.Length % 3 != 0)
                throw new ArgumentException("Coordinates must be XYZ triplets (length divisible by 3).", nameof(coordinates));
            int N = coordinates.Length / 3;
            double[] values = new double[N];

            DoubleGrid_GetValuesWs(Ptr, N, coordinates, values);
            return values;
        }

        public override void SetValues(int[] coordinates, double[] values)
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
            DoubleGrid_SetValues(Ptr, coordinates.Length / 3, coordinates, values);
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

            long count = DoubleGrid_GetActiveVoxels(Ptr, 0, null);
            if (count < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (count == 0) return new int[0];

            long elements = count * 3;
            if (elements > int.MaxValue)
                throw new InvalidOperationException(
                    $"Grid has {count} active voxels, too many to return in a single array.");

            int[] coords = new int[elements];

            long written = DoubleGrid_GetActiveVoxels(Ptr, count, coords);
            if (written < 0) NativeError.ThrowIfFailed("GetActiveVoxels");
            if (written > count)
                throw new InvalidOperationException("Grid was modified while reading active voxels.");

            return coords;
        }

        public override double[] GetNeighbours(int[] coordinates)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            var values = new double[27];
            DoubleGrid_GetNeighbours(Ptr, coordinates, values);
            return values;
        }

        public override void SetActiveState(int[] coordinates, bool on)
        {
            ThrowIfDisposed();
            RequireXyz(coordinates, nameof(coordinates));
            DoubleGrid_SetActiveState(Ptr, coordinates, on ? 1 : 0);
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
