/*
 * RawLamb
 * Copyright 2022 Tom Svilans
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
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class Grid : IDisposable
    {
        public IntPtr Ptr;
        private bool m_valid;

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_Create();
        public Grid()
        {
            Ptr = Grid_Create();
        }

        public Grid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_duplicate(IntPtr ptr);
        public Grid Duplicate()
        {
            var duplicate_ptr = Grid_duplicate(Ptr);
            return new Grid(duplicate_ptr);
        }

        ~Grid()
        {
            Dispose(false);
            Console.WriteLine("Deleting Grid at {0}", Ptr.ToString());
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_read(string filename);
        public static Grid Read(string filename)
        {
            var ptr = Grid_read(filename);
            return new Grid(ptr);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_write(IntPtr ptr, string filename, bool half_float);
        public void Write(string filename, bool float_as_half=false)
        {
            Grid_write(Ptr, filename, float_as_half);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        private static extern IntPtr[] Grid_get_some_grids(string filename);

        public List<Grid> ReadMultiple(string filename)
        {
            var ptrs = Grid_get_some_grids(filename);
            var grids = new List<Grid>();
            foreach(var ptr in ptrs)
            {
                grids.Add(new Grid(ptr));
            }

            return grids;
        }


        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_evaluate(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] Evaluate(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            Grid_evaluate(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_get_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] GetValuesWS(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            Grid_get_values_ws(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] values);
        public float[] SetValuesWS(float[] coords, float[] values)
        {
            if (coords.Length != values.Length * 3) throw new ArgumentException("Coordinate and value array lengths don't match.");
            int N = coords.Length / 3;

            Grid_set_values_ws(Ptr, N, coords, values);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_value(IntPtr ptr, int x, int y, int z, float v);
        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern float Grid_get_value(IntPtr ptr, int x, int y, int z);

        public float this[int x, int y, int z]
        {
            get { return Grid_get_value(Ptr, x, y, z); }
            set { Grid_set_value(Ptr, x, y, z, value); }
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_name(IntPtr ptr, string name);
        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string Grid_get_name(IntPtr ptr);

        public string Name
        {
            get 
            {
                return Grid_get_name(Ptr);
            }
            set 
            { 
                Grid_set_name(Ptr, value); 
            }
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_offset(IntPtr ptr, float amount);
        public void Offset(float amount)
        {
            Grid_offset(Ptr, amount);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_filter(IntPtr ptr, int width, int iterations, int type);
        public void Filter(int width, int iterations, int type)
        {
            Grid_filter(Ptr, width, iterations, type);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_bounding_box(IntPtr ptr, int[] min, int[] max);
        public void BoundingBox(out int[] min, out int[] max)
        {
            min = new int[3];
            max = new int[3];
            Grid_bounding_box(Ptr, min, max);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_get_transform(IntPtr ptr, float[] mat);
        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_transform(IntPtr ptr, float[] mat);

        public float[] Transform
        {
            get
            {
                var mat = new float[16];
                Grid_get_transform(Ptr, mat);
                return mat;
            }
            set
            {
                if (value.Length < 16) throw new ArgumentException("Transform array must have at least 16 values.");
                Grid_set_transform(Ptr, value);
            }
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_to_mesh(IntPtr ptr, IntPtr mesh_ptr, float isovalue);
        public QuadMesh ToMesh(float isovalue)
        {
            var qm = new QuadMesh();
            Grid_to_mesh(Ptr, qm.Ptr, isovalue);

            return qm;
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_resample(IntPtr ptr, float scale);
        public Grid Resample(double scale)
        {
            return new Grid(Grid_resample(Ptr, (float)scale));
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_get_dense(IntPtr ptr, int[] min, int[] max, float[] results);
        public float[] GetDenseGrid(int[] min, int[] max)
        {
            int Nx = max[0] - min[0] + 1, Ny = max[1] - min[1] + 1, Nz = max[2] - min[2] + 1;
            var results = new float[Nx * Ny * Nz];
            Grid_get_dense(Ptr, min, max, results);
            return results;
        }

        /// <summary>
        /// validity property
        /// </summary>
        /// <returns>boolean value of validity</returns>
        public bool IsValid
        {
            get
            {
                if (this.Ptr == IntPtr.Zero) return false;
                return this.m_valid;
            }
            private set
            {
                this.m_valid = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public override string ToString()
        {
            return string.Format("Grid ({0})", Name);
        }

        [DllImport(API.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_Delete(IntPtr ptr);

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                Grid_Delete(this.Ptr);

                // clear pointer
                this.Ptr = IntPtr.Zero;
                this.IsValid = false;
            }

            // finalize garbage collection
            if (bDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }




    }
}
