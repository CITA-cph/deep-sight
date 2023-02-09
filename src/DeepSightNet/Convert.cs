using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public static class Convert
    {
        #region Api calls
        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr FloatGrid_ToMesh(IntPtr ptr, float isovalue);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr FloatGrid_FromMesh(IntPtr mesh, float[] xform, float isovalue, float exteriorBandWidth, float interiorBandWidth);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr FloatGrid_FromPoints(int num_points, float[] point_data, float radius, float voxel_size);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr DoubleGrid_ToMesh(IntPtr ptr, float isovalue);

        [DllImport(Api.DeepSightApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr Int32Grid_ToMesh(IntPtr ptr, float isovalue);


        #endregion

        public static Mesh VolumeToMesh(FloatGrid grid, float isovalue)
        {
            return new Mesh(FloatGrid_ToMesh(grid.Ptr, isovalue));
        }

        public static FloatGrid MeshToVolume(Mesh mesh, float[] xform, float isovalue=0.5f, float exteriorBandWidth=3.0f, float interiorBandWidth=3.0f)
        {
            return new FloatGrid(FloatGrid_FromMesh(mesh.Ptr, xform, isovalue, exteriorBandWidth, interiorBandWidth));
        }

        public static FloatGrid PointsToVolume(float[] points, float radius, float voxelsize)
        {
            return new FloatGrid(FloatGrid_FromPoints(points.Length/3, points, radius, voxelsize));
        }
    }
}
