#ifndef GRID_EXPORT_H
#define GRID_EXPORT_H

#include "Grid.h"
#include "Mesh.h"
#include <tuple>

namespace DeepSight
{

#ifdef __cplusplus
	extern "C" {
#endif

		RAWLAM_EXPORT Grid<float>* Grid_Create();
		RAWLAM_EXPORT Grid<float>* Grid_duplicate(Grid<float> *ptr);
		RAWLAM_EXPORT void Grid_Delete(Grid<float>* ptr);
		RAWLAM_EXPORT Grid<float>* Grid_read(const char* filename);

		RAWLAM_EXPORT void Grid_evaluate(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type);

		RAWLAM_EXPORT float Grid_get_value(Grid<float>* ptr, int x, int y, int z);
		RAWLAM_EXPORT void Grid_set_value(Grid<float>* ptr, int x, int y, int z, float v);

		RAWLAM_EXPORT char* Grid_get_name(Grid<float>* ptr);
		RAWLAM_EXPORT void Grid_set_name(Grid<float>* ptr, const char* name);

		RAWLAM_EXPORT void Grid_get_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type);
		RAWLAM_EXPORT void Grid_set_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* values);

		RAWLAM_EXPORT void Grid_filter(Grid<float>* ptr, int width, int iterations, int type);
		//RAWLAM_EXPORT void Grid_offset(Grid<float>* ptr, float amount);
		RAWLAM_EXPORT void Grid_bounding_box(Grid<float>* ptr, int* min, int* max);
		RAWLAM_EXPORT void Grid_set_transform(Grid<float>* ptr, float* mat);
		RAWLAM_EXPORT void Grid_get_transform(Grid<float>* ptr, float* mat);
		RAWLAM_EXPORT void Grid_to_mesh(Grid<float>* ptr, QuadMesh* mesh_ptr, float isovalue);
		RAWLAM_EXPORT Grid<float>* Grid_resample(Grid<float>* ptr, float scale);
		RAWLAM_EXPORT void Grid_get_dense(Grid<float>* ptr, int* min, int* max, float* results);
		RAWLAM_EXPORT SAFEARRAY* Grid_get_some_grids(const char* filename);

#ifdef __cplusplus
	}
#endif
}
#endif
