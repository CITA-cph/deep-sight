#ifndef CONVERT_API_H
#define CONVERT_API_H

#include "Tools.h"
#include "Mesh.h"

namespace DeepSight
{
#ifdef __cplusplus
	extern "C" {
#endif
	DEEPSIGHT_EXPORT Mesh* FloatGrid_ToMesh(GridBase* ptr, float isovalue);
	DEEPSIGHT_EXPORT GridBase* FloatGrid_FromMesh(Mesh* mesh, float* xform, float isovalue, float exteriorBandWidth, float interiorBandWidth);
	DEEPSIGHT_EXPORT GridBase* FloatGrid_FromPoints(int num_points, float* point_data, float radius, float voxelsize);
	DEEPSIGHT_EXPORT Mesh* DoubleGrid_ToMesh(GridBase* ptr, float isovalue);
	DEEPSIGHT_EXPORT Mesh* Int32Grid_ToMesh(GridBase* ptr, float isovalue);

#ifdef __cplusplus
	}
#endif
}

#endif