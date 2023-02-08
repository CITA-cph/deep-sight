#ifdef OBSOLETE
#ifndef GRID_EXPORT_H
#define GRID_EXPORT_H

#include "Grid.h"
#include <openvdb/tools/Composite.h>
#include "Mesh.h"
#include <tuple>

#define XSTR(x) STR(x)
#define STR(x) #x
#define _VERSION XSTR(_DEEPSIGHT_VERSION)
#pragma message ("deepsight v" _VERSION)

namespace DeepSight
{
	const char* VERSION = _VERSION;

#ifdef __cplusplus
	extern "C" {
#endif

		// FloatGrid
		DEEPSIGHT_EXPORT Grid<float>* Grid_Create();
		DEEPSIGHT_EXPORT Grid<float>* Grid_duplicate(Grid<float> *ptr);
		DEEPSIGHT_EXPORT void Grid_Delete(Grid<float>* ptr);
		DEEPSIGHT_EXPORT Grid<float>* Grid_read(const char* filename);
		DEEPSIGHT_EXPORT void Grid_write(Grid<float>* ptr, const char* filename, bool half_float);

		DEEPSIGHT_EXPORT void Grid_evaluate(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type);

		DEEPSIGHT_EXPORT float Grid_get_value(Grid<float>* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void Grid_set_value(Grid<float>* ptr, int x, int y, int z, float v);

		DEEPSIGHT_EXPORT char* Grid_get_name(Grid<float>* ptr);
		DEEPSIGHT_EXPORT void Grid_set_name(Grid<float>* ptr, const char* name);

		DEEPSIGHT_EXPORT void Grid_get_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type);
		DEEPSIGHT_EXPORT void Grid_set_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* values);

		DEEPSIGHT_EXPORT void Grid_filter(Grid<float>* ptr, int width, int iterations, int type);
		//DEEPSIGHT_EXPORT void Grid_offset(Grid<float>* ptr, float amount);
		DEEPSIGHT_EXPORT void Grid_bounding_box(Grid<float>* ptr, int* min, int* max);
		DEEPSIGHT_EXPORT void Grid_set_transform(Grid<float>* ptr, float* mat);
		DEEPSIGHT_EXPORT void Grid_get_transform(Grid<float>* ptr, float* mat);
		DEEPSIGHT_EXPORT void Grid_to_mesh(Grid<float>* ptr, QuadMesh* mesh_ptr, float isovalue);
		DEEPSIGHT_EXPORT Grid<float>* Grid_resample(Grid<float>* ptr, float scale);
		DEEPSIGHT_EXPORT void Grid_get_dense(Grid<float>* ptr, int* min, int* max, float* results);
		DEEPSIGHT_EXPORT SAFEARRAY* Grid_get_some_grids(const char* filename);
		DEEPSIGHT_EXPORT unsigned long Grid_get_active_values_size(Grid<float>* ptr);
		DEEPSIGHT_EXPORT void Grid_get_active_values(Grid<float>* ptr, int* buffer);
		DEEPSIGHT_EXPORT void Grid_erode(Grid<float>* ptr, int iterations);
		DEEPSIGHT_EXPORT void Grid_get_neighbours(Grid<float>* ptr, int* coords, float* neighbours);

		DEEPSIGHT_EXPORT bool Grid_get_active_state(Grid<float>* ptr, int* xyz);
		DEEPSIGHT_EXPORT void Grid_get_active_state_many(Grid<float>* ptr, int n, int* xyz, int* states);
		DEEPSIGHT_EXPORT void Grid_set_active_state(Grid<float>* ptr, int* xyz, bool state);
		DEEPSIGHT_EXPORT void Grid_set_active_state_many(Grid<float>* ptr, int n, int* xyz, int* states);

		DEEPSIGHT_EXPORT void Grid_difference(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_union(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_intersection(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_max(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_min(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_sum(Grid<float>* ptr0, Grid<float>* ptr1);
		DEEPSIGHT_EXPORT void Grid_mul(Grid<float>* ptr0, Grid<float>* ptr1);

		// Vec3Grid
		DEEPSIGHT_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_Create();
		DEEPSIGHT_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_duplicate(Grid<openvdb::Vec3f>* ptr);
		DEEPSIGHT_EXPORT void Vec3Grid_Delete(Grid<openvdb::Vec3f>* ptr);
		DEEPSIGHT_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_read(const char* filename);
		DEEPSIGHT_EXPORT void Vec3Grid_write(Grid<openvdb::Vec3f>* ptr, const char* filename, bool half_float);

		DEEPSIGHT_EXPORT void Vec3Grid_evaluate(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type);

		DEEPSIGHT_EXPORT openvdb::Vec3f Vec3Grid_get_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z);
		DEEPSIGHT_EXPORT void Vec3Grid_set_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z, float a, float b, float c);

		DEEPSIGHT_EXPORT char* Vec3Grid_get_name(Grid<openvdb::Vec3f>* ptr);
		DEEPSIGHT_EXPORT void Vec3Grid_set_name(Grid<openvdb::Vec3f>* ptr, const char* name);

		DEEPSIGHT_EXPORT void Vec3Grid_get_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type);
		DEEPSIGHT_EXPORT void Vec3Grid_set_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* values);

		DEEPSIGHT_EXPORT void Vec3Grid_filter(Grid<openvdb::Vec3f>* ptr, int width, int iterations, int type);
		DEEPSIGHT_EXPORT void Vec3Grid_bounding_box(Grid<openvdb::Vec3f>* ptr, int* min, int* max);
		DEEPSIGHT_EXPORT void Vec3Grid_set_transform(Grid<openvdb::Vec3f>* ptr, float* mat);
		DEEPSIGHT_EXPORT void Vec3Grid_get_transform(Grid<openvdb::Vec3f>* ptr, float* mat);
		DEEPSIGHT_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_resample(Grid<openvdb::Vec3f>* ptr, float scale);
		DEEPSIGHT_EXPORT void Vec3Grid_get_dense(Grid<openvdb::Vec3f>* ptr, int* min, int* max, float* results);
		DEEPSIGHT_EXPORT SAFEARRAY* Vec3Grid_get_some_grids(const char* filename);

#ifdef __cplusplus
	}
#endif
}
#endif
#endif