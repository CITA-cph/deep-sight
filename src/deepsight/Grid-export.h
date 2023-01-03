#ifndef GRID_EXPORT_H
#define GRID_EXPORT_H

#include "Grid.h"
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
		RAWLAM_EXPORT Grid<float>* Grid_Create();
		RAWLAM_EXPORT Grid<float>* Grid_duplicate(Grid<float> *ptr);
		RAWLAM_EXPORT void Grid_Delete(Grid<float>* ptr);
		RAWLAM_EXPORT Grid<float>* Grid_read(const char* filename);
		RAWLAM_EXPORT void Grid_write(Grid<float>* ptr, const char* filename, bool half_float);

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

		// Vec3Grid
		RAWLAM_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_Create();
		RAWLAM_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_duplicate(Grid<openvdb::Vec3f>* ptr);
		RAWLAM_EXPORT void Vec3Grid_Delete(Grid<openvdb::Vec3f>* ptr);
		RAWLAM_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_read(const char* filename);
		RAWLAM_EXPORT void Vec3Grid_write(Grid<openvdb::Vec3f>* ptr, const char* filename, bool half_float);

		RAWLAM_EXPORT void Vec3Grid_evaluate(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type);

		RAWLAM_EXPORT openvdb::Vec3f Vec3Grid_get_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z);
		RAWLAM_EXPORT void Vec3Grid_set_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z, float a, float b, float c);

		RAWLAM_EXPORT char* Vec3Grid_get_name(Grid<openvdb::Vec3f>* ptr);
		RAWLAM_EXPORT void Vec3Grid_set_name(Grid<openvdb::Vec3f>* ptr, const char* name);

		RAWLAM_EXPORT void Vec3Grid_get_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type);
		RAWLAM_EXPORT void Vec3Grid_set_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* values);

		RAWLAM_EXPORT void Vec3Grid_filter(Grid<openvdb::Vec3f>* ptr, int width, int iterations, int type);
		RAWLAM_EXPORT void Vec3Grid_bounding_box(Grid<openvdb::Vec3f>* ptr, int* min, int* max);
		RAWLAM_EXPORT void Vec3Grid_set_transform(Grid<openvdb::Vec3f>* ptr, float* mat);
		RAWLAM_EXPORT void Vec3Grid_get_transform(Grid<openvdb::Vec3f>* ptr, float* mat);
		RAWLAM_EXPORT Grid<openvdb::Vec3f>* Vec3Grid_resample(Grid<openvdb::Vec3f>* ptr, float scale);
		RAWLAM_EXPORT void Vec3Grid_get_dense(Grid<openvdb::Vec3f>* ptr, int* min, int* max, float* results);
		RAWLAM_EXPORT SAFEARRAY* Vec3Grid_get_some_grids(const char* filename);

#ifdef __cplusplus
	}
#endif
}
#endif
