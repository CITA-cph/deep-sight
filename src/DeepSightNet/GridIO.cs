using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public static class GridIO
    {

        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
        internal static extern void ReadWrite_ReadVdb(string path, out int num_grids, out IntPtr grid_ptrs);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReadWrite_WriteVdb(string path, int num_grids, IntPtr[] grids, int float_as_half);

        
        #endregion

        public static GridApi[] Read(string filepath)
        {
            IntPtr ptr;
            int num_grids;
            ReadWrite_ReadVdb(filepath, out num_grids, out ptr);
            var grid_ptrs = new IntPtr[num_grids];

            Marshal.Copy(ptr, grid_ptrs, 0, num_grids);
            Marshal.FreeCoTaskMem(ptr);

            var grids = new GridApi[grid_ptrs.Length];

            for(int i = 0; i < grid_ptrs.Length; i++)
            {
                string type = GridApi.GridBase_GetType(grid_ptrs[i]);
                var tokens = type.Split('_');

                switch(tokens[1])
                {
                    case ("float"):
                        grids[i] = new FloatGrid(grid_ptrs[i]);
                        break;
                    case ("double"):
                        grids[i] = new DoubleGrid(grid_ptrs[i]);
                        break;
                    case ("int32"):
                        grids[i] = new Int32Grid(grid_ptrs[i]);
                        break;
                    case ("vec3s"):
                        grids[i] = new Vec3fGrid(grid_ptrs[i]);
                        break;
                    default:
                        Console.WriteLine($"Unknown grid: {type}");
                        GridApi.GridBase_Delete(grid_ptrs[i]);
                        break;
                }
            }

            return grids.Where(x => x != null).ToArray();
        }

        public static void Write(string filepath, GridApi[] grids, bool float_as_half=false)
        {
            ReadWrite_WriteVdb(filepath, grids.Length, grids.Select(x => x.Ptr).ToArray(), float_as_half ? 1 : 0);
        }
    }
}
