#ifndef GRIDBASE_API_H
#define GRIDBASE_API_H

#include "GridBase.h"

namespace DeepSight
{
#ifdef __cplusplus
	extern "C" {
#endif
		DEEPSIGHT_EXPORT GridBase* GridBase_CreateFloat();
		DEEPSIGHT_EXPORT GridBase* GridBase_CreateDouble();
		DEEPSIGHT_EXPORT GridBase* GridBase_CreateInt();

		DEEPSIGHT_EXPORT void GridBase_Delete(GridBase* grid);

		DEEPSIGHT_EXPORT void GridBase_SetName(GridBase* ptr, const char* name);
		DEEPSIGHT_EXPORT const char* GridBase_GetName(GridBase* ptr);

		DEEPSIGHT_EXPORT void GridBase_ClipIndex(GridBase* ptr, int* min, int* max);
		DEEPSIGHT_EXPORT void GridBase_ClipWorld(GridBase* ptr, double* min, double* max);
		DEEPSIGHT_EXPORT void GridBase_Prune(GridBase* ptr, float tolerance);

		DEEPSIGHT_EXPORT int GridBase_GetGridClass(GridBase* ptr);
		DEEPSIGHT_EXPORT void GridBase_SetGridClass(GridBase* ptr, int c);
		DEEPSIGHT_EXPORT int GridBase_GetActiveVoxelCount(GridBase* ptr);

		DEEPSIGHT_EXPORT char* GridBase_GetType(GridBase* ptr);
		DEEPSIGHT_EXPORT void GridBase_SetTransform(GridBase* ptr, float* xform);
		DEEPSIGHT_EXPORT void GridBase_GetTransform(GridBase* ptr, float* xform);


#ifndef TEST


#pragma region FloatGrid

		DEEPSIGHT_EXPORT float FloatGrid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT float FloatGrid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void FloatGrid_SetValue(GridBase* ptr, int x, int y, int z, float v);

		DEEPSIGHT_EXPORT void FloatGrid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, float* values);
		DEEPSIGHT_EXPORT void FloatGrid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, float* values);
		DEEPSIGHT_EXPORT void FloatGrid_SetValues(GridBase* ptr, int num_coords, int* coords, float* values);

		DEEPSIGHT_EXPORT void FloatGrid_GetActiveVoxels(GridBase* ptr, int* coords);
		DEEPSIGHT_EXPORT void FloatGrid_SetActiveState(GridBase* ptr, int* coord, int state);

#pragma endregion FloatGrid

#pragma region DoubleGrid

		DEEPSIGHT_EXPORT double DoubleGrid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT double DoubleGrid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void DoubleGrid_SetValue(GridBase* ptr, int x, int y, int z, double v);

		DEEPSIGHT_EXPORT void DoubleGrid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, double* values);
		DEEPSIGHT_EXPORT void DoubleGrid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, double* values);
		DEEPSIGHT_EXPORT void DoubleGrid_SetValues(GridBase* ptr, int num_coords, int* coords, double* values);

		DEEPSIGHT_EXPORT void DoubleGrid_GetActiveVoxels(GridBase* ptr, int* coords);
		DEEPSIGHT_EXPORT void DoubleGrid_SetActiveState(GridBase* ptr, int* coord, int state);

#pragma endregion DoubleGrid

#pragma region Int32Grid

		DEEPSIGHT_EXPORT int Int32Grid_GetValueWs(GridBase* ptr, double x, double y, double z);
		DEEPSIGHT_EXPORT int Int32Grid_GetValueIs(GridBase* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void Int32Grid_SetValue(GridBase* ptr, int x, int y, int z, int v);

		DEEPSIGHT_EXPORT void Int32Grid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, int* values);
		DEEPSIGHT_EXPORT void Int32Grid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, int* values);
		DEEPSIGHT_EXPORT void Int32Grid_SetValues(GridBase* ptr, int num_coords, int* coords, int* values);

		DEEPSIGHT_EXPORT void Int32Grid_GetActiveVoxels(GridBase* ptr, int* coords);
		DEEPSIGHT_EXPORT void Int32Grid_SetActiveState(GridBase* ptr, int* coord, int state);
#pragma endregion Int32Grid

#endif

#ifdef __cplusplus
	}
#endif

}
#endif

