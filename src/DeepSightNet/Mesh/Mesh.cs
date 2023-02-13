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
    /// <summary>
    /// Simple interface for passing mesh data back and forth between .NET and the native API.
    /// </summary>
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
        private static extern void Mesh_add_vertices(IntPtr ptr, int num_verts, float[] vert_data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_tri(IntPtr ptr, int[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_quad(IntPtr ptr, int[] data);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Mesh_add_faces(IntPtr ptr, int num_faces, int[] face_data);

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

        /// <summary>
        /// Return a flattened array of vertex data as repeating XYZ triplets.
        /// </summary>
        public float[] Vertices
        {
            get
            {
                var verts = new float[Mesh_num_vertices(Ptr) * 3];
                Mesh_get_vertices(Ptr, verts);

                return verts;
            }
        }

        /// <summary>
        /// Return a flattened array of quad faces.
        /// </summary>
        public int[] Quads
        {
            get
            {
                var faces = new int[Mesh_num_quads(Ptr) * 4];
                Mesh_get_quads(Ptr, faces);

                return faces;
            }
        }

        /// <summary>
        /// Return a flattened array of triangle faces.
        /// </summary>
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
        /// Add a single vertex from its XYZ coordinates.
        /// </summary>
        /// <param name="x">The X-component of the vertex location.</param>
        /// <param name="y">The Y-component of the vertex location.</param>
        /// <param name="z">The Z-component of the vertex location.</param>
        public void AddVertex(float x, float y, float z)
        {
            Mesh_add_vertex(Ptr, new float[] { x, y, z });
        }

        /// <summary>
        /// Add multiple vertices from flattened float data as repeating
        /// XYZ triplets ([x0, y0, z0, x1, y1, z1, ...]).
        /// </summary>
        /// <param name="data">Vertex data as list of floats.</param>
        public void AddVertices(float[] data)
        {
            Mesh_add_vertices(Ptr, data.Length / 3, data);
        }

        /// <summary>
        /// Add a tri or quad face.
        /// </summary>
        /// <param name="face">A list of the face indices. Tris are length 3, quads are length 4.</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddFace(int[] face)
        {
            if (face.Length == 3)
                Mesh_add_tri(Ptr, face);
            else if (face.Length == 4)
                Mesh_add_quad(Ptr, face);
            else
                throw new ArgumentException("Face must be either a triangle or a quad.");
        }

        /// <summary>
        /// Add multiple tri or quad faces as flat list of indices. 
        /// Each face always has 4 indices. 
        /// Quads just list their indices (i.e. [0, 1, 2, 3]).
        /// Tris repeat the last index (i.e. [0, 1, 3, 3]).
        /// </summary>
        /// <param name="data">Flattened list of face indices, padded to 4 values.</param>
        public void AddFaces(int[] data)
        {
            Mesh_add_faces(Ptr, data.Length / 4, data);
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
