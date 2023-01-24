using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DeepSight
{
    public class RLGeom
    {
        [DllImport(Api.RawLamGeomApiPath, SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RawLam_create_board_STEP(float[] board_plane, float[] board_dims, int num_knots, float[] knot_data, string output_path);
        public static void ExportBoard2Step(float[] board_plane, float[] board_dims, int num_knots, float[] knot_data, string output_path)
        {
            RawLam_create_board_STEP(board_plane, board_dims, num_knots, knot_data, output_path);
        }
    }
}
