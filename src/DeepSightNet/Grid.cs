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

    public enum GridClass
    {
        GRID_UNKNOWN,
        GRID_LEVEL_SET,
        GRID_FOG_VOLUME,
        GRID_STAGGERED
    }

    public partial class Grid : GridBase, IDisposable
    {
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_create(float background);
        public Grid(float background=0.0f)
        {
            Ptr = FloatGrid_create(background);
        }

        public Grid(IntPtr ptr)
        {
            Ptr = ptr;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_duplicate(IntPtr ptr);
        public Grid Duplicate()
        {
            var duplicate_ptr = FloatGrid_duplicate(Ptr);
            return new Grid(duplicate_ptr);
        }

        ~Grid()
        {
            Dispose(false);
            Console.WriteLine("Deleting Grid at {0}", Ptr.ToString());
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_read(string filename);
        public static Grid Read(string filename)
        {
            var ptr = FloatGrid_read(filename);
            return new Grid(ptr);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_write(IntPtr ptr, string filename, bool half_float);
        public void Write(string filename, bool float_as_half=false)
        {
            FloatGrid_write(Ptr, filename, float_as_half);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.SafeArray)]
        private static extern IntPtr[] FloatGrid_get_some_grids(string filename);

        public static List<Grid> ReadMultiple(string filename)
        {
            var ptrs = FloatGrid_get_some_grids(filename);
            var grids = new List<Grid>();
            foreach(var ptr in ptrs)
            {
                grids.Add(new Grid(ptr));
            }

            return grids;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_from_points(int num_points, float[] points, float radius, float voxel_size);
        public static Grid FromPoints(float[] coords, float radius, float voxel_size)
        {
            return new Grid(FloatGrid_from_points(coords.Length / 3, coords, radius, voxel_size));
        }

        // #############################

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_difference(IntPtr ptr0, IntPtr ptr1);
        public static void Difference(Grid grid0, Grid grid1)
        {
            FloatGrid_difference(grid0.Ptr, grid1.Ptr);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_union(IntPtr ptr0, IntPtr ptr1);
        public static void Union(Grid grid0, Grid grid1)
        {
            FloatGrid_union(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_intersection(IntPtr ptr0, IntPtr ptr1);
        public static void Intersection(Grid grid0, Grid grid1)
        {
            FloatGrid_intersection(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_max(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids by using the maximum value per cell.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void Maximum(Grid grid0, Grid grid1)
        {
            FloatGrid_max(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_min(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids by using the minimum value per cell.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void Minimum(Grid grid0, Grid grid1)
        {
            FloatGrid_min(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_sum(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids by summing the cell values.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void Sum(Grid grid0, Grid grid1)
        {
            FloatGrid_sum(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_diff(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids by subtracting the second grid values from the first.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void Diff(Grid grid0, Grid grid1)
        {
            FloatGrid_diff(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_ifzero(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids if the first grid cell value is <= 0.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void IfZero(Grid grid0, Grid grid1)
        {
            FloatGrid_ifzero(grid0.Ptr, grid1.Ptr);
        }
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_mul(IntPtr ptr0, IntPtr ptr1);
        /// <summary>
        /// Combines cells from two grids by multiplying the cell values.
        /// </summary>
        /// <param name="grid0">First grid.</param>
        /// <param name="grid1">Second grid.</param>
        public static void Multiply(Grid grid0, Grid grid1)
        {
            FloatGrid_mul(grid0.Ptr, grid1.Ptr);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_from_mesh(int num_verts, float[] verts, int num_faces, int[] faces, float[] xform, float isovalue, float exteriorBandWidth, float interiorBandWidth);
        /// <summary>
        /// Create a level-set grid from a mesh.
        /// </summary>
        /// <param name="vertices">Array of flattened vertex data.</param>
        /// <param name="tris">Array of flattened triangle index data.</param>
        /// <param name="xform">Flattened 4x4 transformation matrix.</param>
        /// <param name="isovalue">Isovalue.</param>
        /// <param name="exteriorBandWidth">Width of the exterior level-set band in voxels.</param>
        /// <param name="interiorBandWidth">Width of the interior level-set band in voxels.</param>
        /// <returns></returns>
        public static Grid FromMesh(float[] vertices, int[] tris, float[] xform, float isovalue = 0.5f, float exteriorBandWidth = 0.5f, float interiorBandWidth = 0.5f)
        {
            return new Grid(FloatGrid_from_mesh(vertices.Length / 3, vertices, tris.Length / 3, tris, xform, isovalue, exteriorBandWidth, interiorBandWidth));
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_get_active_values(IntPtr ptr, int[] buffer);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong FloatGrid_get_active_values_size(IntPtr ptr);
        public int[] ActiveVoxels()
        {
            var size = FloatGrid_get_active_values_size(Ptr);
            var buffer = new int[size];
            FloatGrid_get_active_values(Ptr, buffer);
            return buffer;

        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_erode(IntPtr ptr, int iterations);
        private void ErodeDeprec(int iterations = 1)
        {
            FloatGrid_erode(Ptr, iterations);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_evaluate(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] Evaluate(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            FloatGrid_evaluate(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_get_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] results, int sample_type);
        public float[] GetValuesWS(float[] coords, int sample_type)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            FloatGrid_get_values_ws(Ptr, N, coords, values, sample_type);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_values_ws(IntPtr ptr, int num_coords, float[] coords, float[] values);
        public float[] SetValuesWS(float[] coords, float[] values)
        {
            if (coords.Length != values.Length * 3) throw new ArgumentException("Coordinate and value array lengths don't match.");
            int N = coords.Length / 3;

            FloatGrid_set_values_ws(Ptr, N, coords, values);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_get_values(IntPtr ptr, int num_coords, int[] coords, float[] results);
        public float[] GetValues(int[] coords)
        {
            int N = coords.Length / 3;
            float[] values = new float[N];

            FloatGrid_get_values(Ptr, N, coords, values);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_values(IntPtr ptr, int num_coords, int[] coords, float[] values);
        public float[] SetValues(int[] coords, float[] values)
        {
            if (coords.Length != values.Length * 3) throw new ArgumentException("Coordinate and value array lengths don't match.");
            int N = coords.Length / 3;

            FloatGrid_set_values(Ptr, N, coords, values);
            //Marshal.Copy(buffer, values, 0, N);

            return values;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_value(IntPtr ptr, int x, int y, int z, float v);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern float FloatGrid_get_value(IntPtr ptr, int x, int y, int z);

        public float this[int x, int y, int z]
        {
            get { return FloatGrid_get_value(Ptr, x, y, z); }
            set { FloatGrid_set_value(Ptr, x, y, z, value); }
        }

        public float this[float x, float y, float z]
        {
            get { 
                var results = new float[1];
                FloatGrid_get_values_ws(Ptr, 1, new float[] { x, y, z }, results, 1); 
                return results[0]; 
            }
            set { FloatGrid_set_values_ws(Ptr, 1, new float[] { x, y, z }, new float[] { value }); }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool FloatGrid_get_active_state(IntPtr ptr, int[] xyz);
        public bool GetActiveState(int x, int y, int z)
        {
            return FloatGrid_get_active_state(Ptr, new int[] { x, y, z });
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_active_state(IntPtr ptr, int[] xyz, int state);
        public void SetActiveState(int x, int y, int z, bool state)
        {
            FloatGrid_set_active_state(Ptr, new int[] { x, y, z }, state ? 1 : 0);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool FloatGrid_get_active_state_many(IntPtr ptr, int N, int[] xyz, int[] states);
        public bool[] GetActiveState(int[] xyz)
        {
            var states = new int[xyz.Length];
            FloatGrid_get_active_state_many(Ptr, xyz.Length, xyz, states);
            return states.Select(x =>  x > 0).ToArray();
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_active_state_many(IntPtr ptr, int N, int[] xyz, int[] state);
        public void SetActiveState(int[] xyz, bool[] states)
        {
            int N = Math.Min(xyz.Length/3, states.Length);
            FloatGrid_set_active_state_many(Ptr, N, xyz, states.Select(x => x ? 1 : 0).ToArray());
        }



        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_name(IntPtr ptr, string name);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string FloatGrid_get_name(IntPtr ptr);

        public string Name
        {
            get 
            {
                return FloatGrid_get_name(Ptr);
            }
            set 
            {
                FloatGrid_set_name(Ptr, value); 
            }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        private static extern string FloatGrid_get_type(IntPtr ptr);
        public string GridType
        {
            get
            {
                return FloatGrid_get_type(Ptr);
            }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_grid_class(IntPtr ptr, int gclass);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int FloatGrid_get_grid_class(IntPtr ptr);

        public GridClass Class
        {
            get
            {
                return (GridClass)FloatGrid_get_grid_class(Ptr);
            }
            set
            {
                FloatGrid_set_grid_class(Ptr, (int)value);
            }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_offset(IntPtr ptr, float amount);
        public void Offset(float amount)
        {
            FloatGrid_offset(Ptr, amount);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_filter(IntPtr ptr, int width, int iterations, int type);
        public void Filter(int width, int iterations, int type)
        {
            FloatGrid_filter(Ptr, width, iterations, type);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_bounding_box(IntPtr ptr, int[] min, int[] max);
        public void BoundingBox(out int[] min, out int[] max)
        {
            min = new int[3];
            max = new int[3];
            FloatGrid_bounding_box(Ptr, min, max);
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_get_transform(IntPtr ptr, float[] mat);
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_set_transform(IntPtr ptr, float[] mat);

        public float[] Transform
        {
            get
            {
                var mat = new float[16];
                FloatGrid_get_transform(Ptr, mat);
                return mat;
            }
            set
            {
                if (value.Length < 16) throw new ArgumentException("Transform array must have at least 16 values.");
                FloatGrid_set_transform(Ptr, value);
            }
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_to_mesh(IntPtr ptr, IntPtr mesh_ptr, float isovalue);
        public QuadMesh ToMesh(float isovalue)
        {
            var qm = new QuadMesh();
            FloatGrid_to_mesh(Ptr, qm.Ptr, isovalue);

            return qm;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_resample(IntPtr ptr, float scale);
        public Grid Resample(double scale)
        {
            return new Grid(FloatGrid_resample(Ptr, (float)scale));
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr FloatGrid_get_dense(IntPtr ptr, int[] min, int[] max, float[] results);
        public float[] GetDenseGrid(int[] min, int[] max)
        {
            int Nx = max[0] - min[0] + 1, Ny = max[1] - min[1] + 1, Nz = max[2] - min[2] + 1;
            var results = new float[Nx * Ny * Nz];
            FloatGrid_get_dense(Ptr, min, max, results);
            return results;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_get_neighbours(IntPtr ptr, int[] coords, float[] neighbours);

        public float[] GetNeighbours(int x, int y, int z)
        {
            float[] neighbours = new float[27];
            FloatGrid_get_neighbours(Ptr, new int[] { x, y, z }, neighbours);

            return neighbours;
        }

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_sdf_to_fog(IntPtr ptr, float cutoff);
        public void SdfToFog(float cutoff = float.MaxValue)
        {
            FloatGrid_sdf_to_fog(Ptr, cutoff);
        }


        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FloatGrid_prune(IntPtr ptr, float tolerance);
        public void Prune(float tolerance=0.0f)
        {
            FloatGrid_prune(Ptr, tolerance);
        }

        public void Clean(float threshold)
        {
            var active = this.ActiveVoxels();
            var N = active.Length / 3;

            int ii, jj, kk;
            for (int i = 0; i < N; ++i)
            {
                ii = active[i * 3 + 0]; jj = active[i * 3 + 1]; kk = active[i * 3 + 2];
                if (this[ii, jj, kk] < threshold)
                {
                    this[ii, jj, kk] = 0.0f;
                    this.SetActiveState(ii, jj, kk, false);
                }
            }
            Prune();
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
        private static extern void FloatGrid_delete(IntPtr ptr);

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                FloatGrid_delete(this.Ptr);

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

// ##############################################################

        /// <summary>
        /// Returns the exposure value of a voxel based on its 26 neighbours, calculated
        /// as the inverse sum of their occupancy. If a neighbour value is 0, exposure is
        /// increased.
        /// </summary>
        /// <param name="neighbours">The 27 neighbours of a voxel.</param>
        public float Exposure(float[] neighbours, float threshold=0.0f)
        {
            // Sum up the neighbour values, subtract its own value (item at index 13)
            float exp = neighbours.Select(x => (x > threshold ? 1 : 0)).Sum() - (neighbours[13] > threshold ? 1 : 0);
            return 1.0F - (exp / 26.0F);
        }

        /// <summary>
        /// Erode the grid based on its voxel exposure. Subtracts a value rate*exposure 
        /// for every active voxel. If the result is <0, sets the voxel value to 0 and
        /// its state to inactive.
        /// </summary>
        /// <param name="rate">Rate of erosion.</param>
        public void Erode(float rate = 0.1f, float threshold = 0.0f)
        {
            var killList = new List<int>();
            var killState = new List<bool>();

            var active = ActiveVoxels();
            var N = active.Length / 3;

            for (int i = 0; i < N; ++i)
            {
                int x = active[i * 3];
                int y = active[i * 3 + 1];
                int z = active[i * 3 + 2];

                var nbrs = GetNeighbours(x, y, z);
                var exp = Exposure(nbrs, threshold);

                var v = nbrs[13] - rate * exp; // 13 is the cell itself within its neighbourhood
                if (v < 0.0f)
                {
                    this[x, y, z] = 0.0f;
                    killList.AddRange(new int[] { x, y, z });
                    killState.Add(false);
                }
                else
                {
                    this[x, y, z] = v;
                }
            }

            SetActiveState(killList.ToArray(), killState.ToArray());
        }

    }
}
