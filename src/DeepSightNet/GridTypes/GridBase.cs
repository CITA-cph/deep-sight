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
using System.Runtime.InteropServices;

namespace DeepSight
{
    /// <summary>
    /// Grid class enumeration.
    /// </summary>
    public enum GridClass
    {
        GRID_UNKNOWN,
        GRID_LEVEL_SET,
        GRID_FOG_VOLUME,
        GRID_STAGGERED
    }

    public abstract class GridBase<T> : GridApi
    {
        public T this[int x, int y, int z]
        {
            get { return GetValueIndex(new int[]{ x, y, z}); }
            set { SetValue(new int[] { x, y, z }, value); }
        }

        public T this[double x, double y, double z]
        {
            get
            {
                return GetValueWorld(new double[] { x, y, z });
            }
            set { throw new NotImplementedException(); 
            }
        }

        public Type ValueType()
        {
            return typeof(T);
        }

        public override object GetGridValue(int x, int y, int z)
        {
            return (object)this[x, y, z];
        }

        /// <summary>
        /// Get a grid value in index-space coordinates.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates ([x, y, z]).</param>
        /// <returns>Value at specified coordinates.</returns>
        public abstract T GetValueIndex(int[] coordinates);

        /// <summary>
        /// Get an interpolated grid value in world-space coordinates.
        /// </summary>
        /// <param name="coordinates">World-space coordinates ([x, y, z]).</param>
        /// <returns>Interpolated value at specified coordinates.</returns>
        public abstract T GetValueWorld(double[] coordinates);

        /// <summary>
        /// Set a grid value in index-space coordinates.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates ([x, y, z]).</param>
        /// <param name="value">Value to set.</param>
        public abstract void SetValue(int[] coordinates, T value);

        /// <summary>
        /// Get grid values for a list of index-space coordinates.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates as repeating XYZ triplets ([x0, y0, z0, x1, y1, z1, x2, y2, z2, ...]).</param>
        /// <returns>List of grid values.</returns>
        public abstract T[] GetValuesIndex(int[] coordinates);

        /// <summary>
        /// Get interpolated grid values for a list of world-space coordinates.
        /// </summary>
        /// <param name="coordinates">World-space coordinates as repeating XYZ triplets ([x0, y0, z0, x1, y1, z1, x2, y2, z2, ...]).</param>
        /// <returns>List of interpolated grid values.</returns>
        public abstract T[] GetValuesWorld(double[] coordinates);

        /// <summary>
        /// Set grid values for a list of index-space coordinates.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates as repeating XYZ triplets ([x0, y0, z0, x1, y1, z1, x2, y2, z2, ...]).</param>
        /// <param name="values">List of values to set.</param>
        public abstract void SetValues(int[] coordinates, T[] values);

        /// <summary>
        /// Get a list of index-space coordinates of active voxels in the grid.
        /// </summary>
        /// <returns>Index-space coordinates for all active voxels as repeating XYZ triplets ([x0, y0, z0, x1, y1, z1, x2, y2, z2, ...]).</returns>
        public abstract int[] GetActiveVoxels();

        /// <summary>
        /// Get all 26 neighbours of a voxel as well as the voxel itself (27 values).
        /// </summary>
        /// <param name="coordinates">Index-space coordinate of cell to retrieve neighbourhood ([x, y, z]).</param>
        /// <returns>Values of all 27 cells in a 3x3x3 neighbourhood.</returns>
        public abstract T[] GetNeighbours(int[] coordinates);

        /// <summary>
        /// Set the active state of a specific cell.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates of cell to activate or deactivate.</param>
        /// <param name="on">Active state of the cell.</param>
        public abstract void SetActiveState(int[] coordinates, bool on);

        /// <summary>
        /// Set the active state of multiple cell.
        /// </summary>
        /// <param name="coordinates">Index-space coordinates of cells to activate or deactivate as repeating XYZ triplets ([x0, y0, z0, x1, y1, z1, x2, y2, z2, ...]).</param>
        /// <param name="on">Active states of the cell.</param>
        public abstract void SetActiveStates(int[] coordinates, bool[] on);
    }

    public abstract class GridApi: IDisposable
    {
        #region Api calls

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GridBase_Duplicate(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GridBase_Delete(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_SetName(IntPtr ptr, string name);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string GridBase_GetName(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_GetBoundingBoxIndex(IntPtr ptr, int[] min, int[] max);
        

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_ClipIndex(IntPtr ptr, int[] min, int[] max);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_ClipWorld(IntPtr ptr, double[] min, double[] max);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_Prune(IntPtr ptr, float tolerance);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GridBase_GetGridClass(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GridBase_SetGridClass(IntPtr ptr, int c);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GridBase_GetActiveVoxelCount(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        internal static extern string GridBase_GetType(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GridBase_SetTransform(IntPtr ptr, float[] xform);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GridBase_GetTransform(IntPtr ptr, float[] xform);
        #endregion

        public IntPtr Ptr { get; protected set; }
        protected bool m_valid;

        /// <summary>
        /// Name of grid.
        /// </summary>
        public string Name
        {
            get
            {
                return GridBase_GetName(Ptr);
            }
            set
            {
                GridBase_SetName(Ptr, value);
            }
        }

        /// <summary>
        /// The active voxel count.
        /// </summary>
        public int ActiveVoxelCount
        {
            get
            {
                return GridBase_GetActiveVoxelCount(Ptr);
            }
        }

        /// <summary>
        /// The grid's type as a string.
        /// </summary>
        public string Type
        {
            get
            {
                return GridBase_GetType(Ptr);
            }
        }

        /// <summary>
        /// The grid's transformation row-major matrix as a flat list of 16 values.
        /// </summary>
        public float[] Transform
        {
            get
            {
                float[] xform = new float[16];
                GridBase_GetTransform(Ptr, xform);
                return xform;
            }
            set
            {
                GridBase_SetTransform(Ptr, value);
            }
        }

        public void BoundingBox(out int[] min, out int[] max)
        {
            min = new int[3];
            max = new int[3];
            GridBase_GetBoundingBoxIndex(Ptr, min, max);
        }

        /// <summary>
        /// Clip the grid to a world-space axis-aligned bounding box.
        /// </summary>
        /// <param name="min">The minimum extents of the bounding box ([x, y, z]).</param>
        /// <param name="max">The maximum extents of the bounding box ([x, y, z]).</param>
        public void ClipWorld(double[] min, double[] max) => GridBase_ClipWorld(Ptr, min, max);

        /// <summary>
        /// Clip the grid to an index-space bounding box.
        /// </summary>
        /// <param name="min">The minimum extents of the bounding box ([x, y, z]).</param>
        /// <param name="max">The maximum extents of the bounding box ([x, y, z]).</param>
        public void ClipIndex(int[] min, int[] max) => GridBase_ClipIndex(Ptr, min, max);

        /// <summary>
        /// Prune the grid tree within a specified tolerance.
        /// </summary>
        /// <param name="tolerance">Tolerance to prune the tree to.</param>
        public void Prune(float tolerance=0.0f) => GridBase_Prune(Ptr, tolerance);

        /// <summary>
        /// Duplicate the grid.
        /// </summary>
        /// <returns>A deep copy of the grid.</returns>
        public abstract GridApi Duplicate();

        /// <summary>
        /// Gets the grid class (unknown, level-set, fog volume, staggered).
        /// </summary>
        public GridClass GridClass
        {
            get
            {
                return (GridClass)GridBase_GetGridClass(Ptr);
            }
            set
            {
                GridBase_SetGridClass(Ptr, (int)value);
            }
        }

        public abstract object GetGridValue(int x, int y, int z);


        #region Dispose
        public void Dispose()
        {
            Dispose(true);
        }

        public override string ToString()
        {
            return string.Format("Grid ({0})", Name);
        }

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

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                GridBase_Delete(this.Ptr);

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
        #endregion
    }
}
