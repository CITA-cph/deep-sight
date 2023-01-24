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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public abstract class GridBase
    {
        public IntPtr Ptr { get; protected set; }
        protected bool m_valid;
    }

    public class Grid : GridBase, IDisposable
    {
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_Create();
        public Grid()
        {
            Ptr = Grid_Create();
        }

        public Grid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_read(string filename);
        public static Grid Read(string filename)
        {
            var ptr = Grid_read(filename);
            return new Grid(ptr);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_write(IntPtr ptr, string filename, bool half_float);
        public void Write(string filename, bool float_as_half=false)
        {
            Grid_write(Ptr, filename, float_as_half);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        private static extern IntPtr[] Grid_get_some_grids(string filename);

        public static List<Grid> ReadMultiple(string filename)
        {
            var ptrs = Grid_get_some_grids(filename);
            var grids = new List<Grid>();
            foreach(var ptr in ptrs)
            {
                grids.Add(new Grid(ptr));
            }

            return grids;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_difference(IntPtr ptr0, IntPtr ptr1);
        public static void Difference(Grid grid0, Grid grid1)
        {
            Grid_difference(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_union(IntPtr ptr0, IntPtr ptr1);
        public static void Union(Grid grid0, Grid grid1)
        {
            Grid_union(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_intersection(IntPtr ptr0, IntPtr ptr1);
        public static void Intersection(Grid grid0, Grid grid1)
        {
            Grid_intersection(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_max(IntPtr ptr0, IntPtr ptr1);
        public static void Maximum(Grid grid0, Grid grid1)
        {
            Grid_max(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_min(IntPtr ptr0, IntPtr ptr1);
        public static void Minimum(Grid grid0, Grid grid1)
        {
            Grid_min(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_sum(IntPtr ptr0, IntPtr ptr1);
        public static void Sum(Grid grid0, Grid grid1)
        {
            Grid_sum(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_mul(IntPtr ptr0, IntPtr ptr1);
        public static void Multiply(Grid grid0, Grid grid1)
        {
            Grid_mul(grid0.Ptr, grid1.Ptr);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.ByValArray)]
        private static extern void Grid_get_active_values(IntPtr ptr, int[] buffer);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong Grid_get_active_values_size(IntPtr ptr);
        public int[] ActiveValues()
        {
            var size = Grid_get_active_values_size(Ptr);
            var buffer = new int[size];
            Grid_get_active_values(Ptr, buffer);
            return buffer;

        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_erode(IntPtr ptr, int iterations);
        public void Erode(int iterations=1)
        {
            Grid_erode(Ptr, iterations);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_evaluate(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] Evaluate(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            Grid_evaluate(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_get_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] GetValuesWS(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            Grid_get_values_ws(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] values);
        public float[] SetValuesWS(float[] coords, float[] values)
        {
            if (coords.Length != values.Length * 3) throw new ArgumentException("Coordinate and value array lengths don't match.");
            int N = coords.Length / 3;

            Grid_set_values_ws(Ptr, N, coords, values);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_value(IntPtr ptr, int x, int y, int z, float v);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern float Grid_get_value(IntPtr ptr, int x, int y, int z);

        public float this[int x, int y, int z]
        {
            get { return Grid_get_value(Ptr, x, y, z); }
            set { Grid_set_value(Ptr, x, y, z, value); }
        }

        public float this[float x, float y, float z]
        {
            get { 
                var results = new float[1]; 
                Grid_get_values_ws(Ptr, 1, new float[] { x, y, z }, results, 1); 
                return results[0]; 
            }
            set { Grid_set_values_ws(Ptr, 1, new float[] { x, y, z }, new float[] { value }); }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Grid_get_active_state(IntPtr ptr, int[] xyz);
        public bool GetActiveState(int x, int y, int z)
        {
            return Grid_get_active_state(Ptr, new int[] { x, y, z });
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_active_state(IntPtr ptr, int[] xyz, int state);
        public void SetActiveState(int x, int y, int z, bool state)
        {
            Grid_set_active_state(Ptr, new int[] { x, y, z }, state ? 1 : 0);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Grid_get_active_state_many(IntPtr ptr, int N, int[] xyz, int[] states);
        public bool[] GetActiveState(int[] xyz)
        {
            var states = new int[xyz.Length];
            Grid_get_active_state_many(Ptr, xyz.Length, xyz, states);
            return states.Select(x =>  x > 0).ToArray();
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_active_state_many(IntPtr ptr, int N, int[] xyz, int[] state);
        public void SetActiveState(int[] xyz, bool[] states)
        {
            int N = Math.Min(xyz.Length, states.Length);
            Grid_set_active_state_many(Ptr, N, xyz, states.Select(x => x ? 1 : 0).ToArray());
        }



        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_set_name(IntPtr ptr, string name);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_offset(IntPtr ptr, float amount);
        public void Offset(float amount)
        {
            Grid_offset(Ptr, amount);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_filter(IntPtr ptr, int width, int iterations, int type);
        public void Filter(int width, int iterations, int type)
        {
            Grid_filter(Ptr, width, iterations, type);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_bounding_box(IntPtr ptr, int[] min, int[] max);
        public void BoundingBox(out int[] min, out int[] max)
        {
            min = new int[3];
            max = new int[3];
            Grid_bounding_box(Ptr, min, max);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_get_transform(IntPtr ptr, float[] mat);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
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

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_to_mesh(IntPtr ptr, IntPtr mesh_ptr, float isovalue);
        public QuadMesh ToMesh(float isovalue)
        {
            var qm = new QuadMesh();
            Grid_to_mesh(Ptr, qm.Ptr, isovalue);

            return qm;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_resample(IntPtr ptr, float scale);
        public Grid Resample(double scale)
        {
            return new Grid(Grid_resample(Ptr, (float)scale));
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Grid_get_dense(IntPtr ptr, int[] min, int[] max, float[] results);
        public float[] GetDenseGrid(int[] min, int[] max)
        {
            int Nx = max[0] - min[0] + 1, Ny = max[1] - min[1] + 1, Nz = max[2] - min[2] + 1;
            var results = new float[Nx * Ny * Nz];
            Grid_get_dense(Ptr, min, max, results);
            return results;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Grid_get_neighbours(IntPtr ptr, int[] coords, float[] neighbours);

        public float[] GetNeighbours(int x, int y, int z)
        {
            float[] neighbours = new float[27];
            Grid_get_neighbours(Ptr, new int[] { x, y, z }, neighbours);

            return neighbours;
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

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
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
