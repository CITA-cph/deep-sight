using System;
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSight
{
    public enum CombineType
    {
        MAX = 0,
        MIN = 1,
        SUM = 2,
        DIFF = 3,
        IFZERO = 4,
        MUL = 5,
        CSG_DIFFERENCE = 6,
        CSG_UNION = 7,
        CSG_INTERSECTION = 8
    }

    public static partial class Tools
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_combine(IntPtr ptr0, IntPtr ptr1, int type);

        #endregion

        public static FloatGrid Combine(FloatGrid grid0, FloatGrid grid1, CombineType type)
        {
            var ngrid0 = grid0.DuplicateGrid();
            var ngrid1 = grid1.DuplicateGrid();

            FloatGrid_combine(ngrid0.Ptr, ngrid1.Ptr, (int)type);

            return ngrid0;
        }

        private static GridApi Sum(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi Maximum(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi Minimum(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi Mean(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi Multiply(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi IfZero(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }

        private static GridApi Diff(GridApi grid0, GridApi grid1)
        {
            return grid0;
        }
    }
}
