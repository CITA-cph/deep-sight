using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public static class GridIO
    {

        #region Api calls

        // The bogus [return: MarshalAs(UnmanagedType.SafeArray)] attribute that
        // used to sit on ReadWrite_ReadVdb has been removed. The function
        // returns void, so there was nothing for it to marshal; it was a
        // leftover from an abandoned SAFEARRAY design (the dead code for which
        // is still commented out in ReadWrite.cpp).
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReadWrite_ReadVdb(string path, out int num_grids, out IntPtr grid_ptrs);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReadWrite_WriteVdb(string path, int num_grids, IntPtr[] grids, int float_as_half);

        #endregion

        /// <summary>
        /// Read every supported grid from a .vdb file.
        /// </summary>
        public static GridApi[] Read(string filepath)
        {
            if (filepath == null) throw new ArgumentNullException(nameof(filepath));

            // Checked here rather than letting the native side discover it.
            // A missing path used to raise openvdb::IoError inside an
            // extern "C" function, which unwound into the CLR and killed the
            // host process. The native side now traps that too, but failing
            // early gives a much better message.
            if (!File.Exists(filepath))
                throw new FileNotFoundException("VDB file not found.", filepath);

            IntPtr ptr;
            int num_grids;
            ReadWrite_ReadVdb(filepath, out num_grids, out ptr);
            NativeError.ThrowIfFailed($"Reading '{filepath}'");

            if (num_grids <= 0 || ptr == IntPtr.Zero)
                return new GridApi[0];

            var grid_ptrs = new IntPtr[num_grids];
            try
            {
                Marshal.Copy(ptr, grid_ptrs, 0, num_grids);
            }
            finally
            {
                // In a finally block so the CoTaskMem array is released even if
                // Marshal.Copy throws.
                Marshal.FreeCoTaskMem(ptr);
            }

            var grids = new List<GridApi>(num_grids);

            for (int i = 0; i < grid_ptrs.Length; i++)
            {
                if (grid_ptrs[i] == IntPtr.Zero) continue;

                GridApi grid = null;
                try
                {
                    grid = Wrap(grid_ptrs[i]);
                }
                catch
                {
                    // Never leak a native grid because we could not classify
                    // it, and never abandon the grids already wrapped.
                    GridApi.GridBase_Delete(grid_ptrs[i]);
                    foreach (var g in grids) g.Dispose();
                    throw;
                }

                if (grid == null)
                {
                    // Unrecognised type: release it rather than leaking, which
                    // the original did correctly, and keep going.
                    GridApi.GridBase_Delete(grid_ptrs[i]);
                    continue;
                }

                grids.Add(grid);
            }

            return grids.ToArray();
        }

        /// <summary>
        /// Wrap a native handle in the managed type matching its value type,
        /// or return null if the type is not supported.
        /// </summary>
        private static GridApi Wrap(IntPtr handle)
        {
            string type = GridApi.GridBase_GetType(handle);
            NativeError.ThrowIfFailed("Reading grid type");

            if (string.IsNullOrEmpty(type))
                return null;

            // The original did `type.Split('_')[1]`, which throws
            // IndexOutOfRangeException on any grid whose type name has no
            // underscore. OpenVDB type names look like "Tree_float_5_4_3", but
            // that is a convention, not a guarantee -- a custom or future grid
            // type in someone's .vdb file crashed the read instead of being
            // skipped as unsupported.
            var tokens = type.Split('_');
            if (tokens.Length < 2)
            {
                Console.WriteLine($"Unrecognised grid type name: {type}");
                return null;
            }

            switch (tokens[1])
            {
                case "float":  return new FloatGrid(handle);
                case "double": return new DoubleGrid(handle);
                case "int32":  return new Int32Grid(handle);
                case "vec3s":  return new Vec3fGrid(handle);
                default:
                    Console.WriteLine($"Unknown grid: {type}");
                    return null;
            }
        }

        /// <summary>
        /// Write grids to a .vdb file.
        /// </summary>
        public static void Write(string filepath, GridApi[] grids, bool float_as_half = false)
        {
            if (filepath == null) throw new ArgumentNullException(nameof(filepath));
            if (grids == null) throw new ArgumentNullException(nameof(grids));

            // A null or disposed entry used to be dereferenced natively.
            for (int i = 0; i < grids.Length; i++)
            {
                if (grids[i] == null || grids[i].Ptr == IntPtr.Zero)
                    throw new ArgumentException($"Grid at index {i} is null or disposed.", nameof(grids));
            }

            ReadWrite_WriteVdb(filepath, grids.Length, grids.Select(x => x.Ptr).ToArray(), float_as_half ? 1 : 0);
            NativeError.ThrowIfFailed($"Writing '{filepath}'");
        }
    }
}
