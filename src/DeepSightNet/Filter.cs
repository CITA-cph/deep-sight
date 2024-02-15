/*
 * DeepSight
 * Copyright 2023 Tom Svilans
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public enum FilterType
    {
        GAUSSIAN,
        MEAN,
        MEDIAN
    }

    public static partial class Tools
    {
        #region Api calls

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr FloatGrid_Resample(IntPtr ptr, float scale);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr DoubleGrid_Resample(IntPtr ptr, float scale);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Int32Grid_Resample(IntPtr ptr, float scale);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void FloatGrid_Filter(IntPtr ptr, int width, int iterations, int type);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void DoubleGrid_Filter(IntPtr ptr, int width, int iterations, int type);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Int32Grid_Filter(IntPtr ptr, int width, int iterations, int type);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void FloatGrid_Erode(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void DoubleGrid_Erode(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Int32Grid_Erode(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Vec3fGrid_Erode(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void FloatGrid_Dilate(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void DoubleGrid_Dilate(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Int32Grid_Dilate(IntPtr ptr, int iterations);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Vec3fGrid_Dilate(IntPtr ptr, int iterations);

        #endregion

        public static FloatGrid Resample(FloatGrid grid, double scale)
        {
            return new FloatGrid(FloatGrid_Resample(grid.Ptr, (float)scale));
        }

        public static DoubleGrid Resample(DoubleGrid grid, double scale)
        {
            return new DoubleGrid(DoubleGrid_Resample(grid.Ptr, (float)scale));
        }

        public static Int32Grid Resample(Int32Grid grid, double scale)
        {
            return new Int32Grid(Int32Grid_Resample(grid.Ptr, (float)scale));
        }

        public static void Filter(FloatGrid grid, int width, int iterations, FilterType type)
        {
            FloatGrid_Filter(grid.Ptr, width, iterations, (int)type);
        }

        public static void Filter(DoubleGrid grid, int width, int iterations, FilterType type)
        {
            DoubleGrid_Filter(grid.Ptr, width, iterations, (int)type);
        }

        public static void Filter(Int32Grid grid, int width, int iterations, FilterType type)
        {
            Int32Grid_Filter(grid.Ptr, width, iterations, (int)type);
        }

        public static void Erode(FloatGrid grid, int iterations)
        {
            FloatGrid_Erode(grid.Ptr, iterations);
        }

        public static void Erode(DoubleGrid grid, int iterations)
        {
            DoubleGrid_Erode(grid.Ptr, iterations);
        }

        public static void Erode(Int32Grid grid, int iterations)
        {
            Int32Grid_Erode(grid.Ptr, iterations);
        }

        public static void Erode(Vec3fGrid grid, int iterations)
        {
            Vec3fGrid_Erode(grid.Ptr, iterations);
        }

        public static void Dilate(FloatGrid grid, int iterations)
        {
            FloatGrid_Dilate(grid.Ptr, iterations);
        }

        public static void Dilate(DoubleGrid grid, int iterations)
        {
            DoubleGrid_Dilate(grid.Ptr, iterations);
        }

        public static void Dilate(Int32Grid grid, int iterations)
        {
            Int32Grid_Dilate(grid.Ptr, iterations);
        }

        public static void Dilate(Vec3fGrid grid, int iterations)
        {
            Vec3fGrid_Dilate(grid.Ptr, iterations);
        }

        private static void Gaussian(GridApi grid, int iterations, int width)
        {

        }

        private static void Mean(GridApi grid, int iterations, int width)
        {

        }

        private static void Median(GridApi grid, int iterations, int width)
        {

        }
    }
}
