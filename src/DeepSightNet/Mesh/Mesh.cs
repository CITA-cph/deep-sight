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
using System.Runtime.InteropServices;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSight
{
    public class Mesh : IDisposable
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Mesh_Create();

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_Delete(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Mesh_num_vertices(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Mesh_num_tris(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Mesh_num_quads(IntPtr ptr);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_get_vertices(IntPtr ptr, float[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_get_tris(IntPtr ptr, int[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_get_quads(IntPtr ptr, int[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_vertex(IntPtr ptr, float[] vertex);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_tri(IntPtr ptr, int[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_quad(IntPtr ptr, int[] data);

        #endregion

        public IntPtr Ptr;
        private bool m_valid;

        public Mesh(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public Mesh()
        {
            Ptr = Mesh_Create();
        }

        public float[] Vertices
        {
            get
            {
                var verts = new float[Mesh_num_vertices(Ptr) * 3];
                Mesh_get_vertices(Ptr, verts);

                return verts;
            }
        }

        public int[] Quads
        {
            get
            {
                var faces = new int[Mesh_num_quads(Ptr) * 4];
                Mesh_get_quads(Ptr, faces);

                return faces;
            }
        }

        public void AddVertex(float x, float y, float z)
        {
            Mesh_add_vertex(Ptr, new float[] { x, y, z });
        }

        public void AddFace(int[] face)
        {
            if (face.Length == 3)
                Mesh_add_tri(Ptr, face);
            else if (face.Length == 4)
                Mesh_add_quad(Ptr, face);
            else
                throw new ArgumentException("Face must be either a triangle or a quad.");
        }

        public int[] Tris
        {
            get
            {
                var faces = new int[Mesh_num_tris(Ptr) * 3];
                Mesh_get_tris(Ptr, faces);

                return faces;
            }
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

        /// <summary>
        /// protected implementation of dispose pattern
        /// </summary>
        /// <param name="bDisposing">holds value indicating if this was called from dispose or finalizer</param>
        protected virtual void Dispose(bool bDisposing)
        {
            if (this.Ptr != IntPtr.Zero)
            {
                // cleanup everything on the c++ side
                Mesh_Delete(this.Ptr);

                // clear renderer pointer
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
