#ifndef GRIDBASE_API_H
#define GRIDBASE_API_H

#include "GridBase.h"
#include "ApiGuard.h"

// ---------------------------------------------------------------------------
// C ABI surface.
//
// Contract for every function below:
//   * No C++ exception ever escapes. Failures are reported by returning the
//     documented failure value and setting the thread-local error message,
//     which the caller reads with DeepSight_GetLastError().
//   * Every pointer argument is checked for null.
//   * Every function that fills a caller-owned buffer takes the buffer's
//     capacity and never writes past it.
//
// BREAKING CHANGES vs. the previous version (managed side updated to match):
//   * <Type>Grid_GetActiveVoxels now takes a capacity and returns a count.
//   * GridBase_GetActiveVoxelCount returns int64 instead of int.
//   * Buffer-filling calls take element counts so they can validate them.
// ---------------------------------------------------------------------------

namespace DeepSight
{
#ifdef __cplusplus
	extern "C" {
#endif

#pragma region Lifetime

		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL GridBase_CreateFloat(float background);
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL GridBase_CreateDouble(double background);
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL GridBase_CreateInt32(int background);
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL GridBase_CreateVec3f(const float* background);

		/// Returns null on failure.
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL GridBase_Duplicate(GridBase* grid);

		/// Safe to call with null.
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_Delete(GridBase* grid);

#pragma endregion Lifetime

#pragma region Generic

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_SetName(GridBase* ptr, const char* name);

		/// Returns a CoTaskMem-allocated copy that the caller must free
		/// (the .NET LPStr return marshaller does this automatically).
		/// Returns null on failure.
		DEEPSIGHT_EXPORT const char* DEEPSIGHT_CALL GridBase_GetName(GridBase* ptr);
		DEEPSIGHT_EXPORT const char* DEEPSIGHT_CALL GridBase_GetType(GridBase* ptr);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_GetBoundingBoxIndex(GridBase* ptr, int* min, int* max);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_ClipIndex(GridBase* ptr, const int* min, const int* max);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_ClipWorld(GridBase* ptr, const double* min, const double* max);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_Prune(GridBase* ptr, float tolerance);

		/// Returns -1 on failure.
		DEEPSIGHT_EXPORT int DEEPSIGHT_CALL GridBase_GetGridClass(GridBase* ptr);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_SetGridClass(GridBase* ptr, int c);

		/// Returns -1 on failure. int64 because a dense 2000^3 region already
		/// overflows a 32-bit count.
		DEEPSIGHT_EXPORT long long DEEPSIGHT_CALL GridBase_GetActiveVoxelCount(GridBase* ptr);

		/// xform is 16 floats in row-major order.
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_SetTransform(GridBase* ptr, const float* xform);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL GridBase_GetTransform(GridBase* ptr, float* xform);

#pragma endregion Generic

#ifndef TEST

// Common contract for the per-type accessors below:
//   coords      - num_coords * 3 elements
//   values      - num_coords elements (num_coords * 3 for Vec3f)
//   GetActiveVoxels(ptr, capacity, coords)
//               - capacity is the number of XYZ *triplets* coords can hold.
//                 Returns the number of active voxels; writes only if
//                 capacity is large enough. Returns -1 on failure.
//                 Call with capacity 0 and a null buffer to query the size.

#pragma region FloatGrid

		DEEPSIGHT_EXPORT float DEEPSIGHT_CALL FloatGrid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT float DEEPSIGHT_CALL FloatGrid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_SetValue(GridBase* ptr, int x, int y, int z, float v);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, float* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, float* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const float* values);

		DEEPSIGHT_EXPORT long long DEEPSIGHT_CALL FloatGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_SetActiveState(GridBase* ptr, const int* coord, int state);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_GetNeighbours(GridBase* ptr, const int* coord, float* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_InactivateBelow(GridBase* ptr, float threshold);

#pragma endregion FloatGrid

#pragma region DoubleGrid

		DEEPSIGHT_EXPORT double DEEPSIGHT_CALL DoubleGrid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT double DEEPSIGHT_CALL DoubleGrid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_SetValue(GridBase* ptr, int x, int y, int z, double v);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, double* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, double* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const double* values);

		DEEPSIGHT_EXPORT long long DEEPSIGHT_CALL DoubleGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_SetActiveState(GridBase* ptr, const int* coord, int state);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_GetNeighbours(GridBase* ptr, const int* coord, double* values);

#pragma endregion DoubleGrid

#pragma region Int32Grid

		DEEPSIGHT_EXPORT int DEEPSIGHT_CALL Int32Grid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT int DEEPSIGHT_CALL Int32Grid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_SetValue(GridBase* ptr, int x, int y, int z, int v);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, int* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, int* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_SetValues(GridBase* ptr, int num_coords, const int* coords, const int* values);

		DEEPSIGHT_EXPORT long long DEEPSIGHT_CALL Int32Grid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_SetActiveState(GridBase* ptr, const int* coord, int state);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_GetNeighbours(GridBase* ptr, const int* coord, int* values);

#pragma endregion Int32Grid

#pragma region Vec3fGrid

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_GetValueWs(GridBase* ptr, double x, double y, double z, float* value);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_GetValueIs(GridBase* ptr, int x, int y, int z, float* value);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_SetValue(GridBase* ptr, int x, int y, int z, const float* v);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, float* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, float* values);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const float* values);

		DEEPSIGHT_EXPORT long long DEEPSIGHT_CALL Vec3fGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_SetActiveState(GridBase* ptr, const int* coord, int state);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_GetNeighbours(GridBase* ptr, const int* coord, float* values);

#pragma endregion Vec3fGrid

#endif

#ifdef __cplusplus
	}
#endif

}
#endif
