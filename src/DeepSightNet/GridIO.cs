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
        internal static extern IntPtr[] ReadWrite_ReadVdb(string path);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReadWrite_WriteVdb(string path, int num_grids, IntPtr[] grids, int float_as_half);

        
        #endregion

        public static GridApi[] Read(string filepath)
        {
            IntPtr[] ptrs = ReadWrite_ReadVdb(filepath);
            var grids = new GridApi[ptrs.Length];

            Console.WriteLine("Num grids: {0}", grids.Length);

            for(int i = 0; i < ptrs.Length; i++)
            {
                Console.WriteLine($"Pointer: {ptrs[i]}");
                string type = GridApi.GridBase_GetType(ptrs[i]);
                var tokens = type.Split('_');
                //Console.WriteLine($"Found grid type: {type}");
                switch(tokens[1])
                {
                    case ("float"):
                        grids[i] = new FloatGrid(ptrs[i]);
                        break;
                    case ("double"):
                        grids[i] = new DoubleGrid(ptrs[i]);
                        break;
                    case ("int32"):
                        grids[i] = new Int32Grid(ptrs[i]);
                        break;
                    case ("vec3s"):
                        grids[i] = new Vec3fGrid(ptrs[i]);
                        break;
                    default:
                        //Console.WriteLine($"Unknown grid: {type}");
                        //GridApi.GridBase_Delete(ptrs[i]);
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
