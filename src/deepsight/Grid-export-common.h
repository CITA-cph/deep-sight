#ifndef GRID_EXPORT_COMMON_H
#define GRID_EXPORT__COMMON_H

#include "Grid.h"
#include <openvdb/tools/Composite.h>
#include "Composite_ext.h"
#include <openvdb/tools/ParticlesToLevelSet.h>
#include <openvdb/tools/LevelSetUtil.h>
#include "Mesh.h"
#include "ParticleList.h"
#include <tuple>

#define EXPORT_SCALAR_H(TypeName, Type) \
_declspec(dllexport) void TypeName##Grid_to_mesh			(Grid<Type>* ptr, QuadMesh* mesh_ptr, float isovalue); \

#define EXPORT_COMMON_H(TypeName, Type)					\
_declspec(dllexport) Grid<Type>* TypeName##Grid_create(Type background);						\
_declspec(dllexport) Grid<Type>* TypeName##Grid_duplicate(Grid<Type> *ptr);	\
_declspec(dllexport) void TypeName##Grid_delete			(Grid<Type> *ptr);		\
_declspec(dllexport) Grid<Type>* TypeName##Grid_read	(const char* filename);	\
_declspec(dllexport) void TypeName##Grid_write			(Grid<Type> *ptr, const char* filename, bool half_float);		\
_declspec(dllexport) Type TypeName##Grid_get_value			(Grid<Type> *ptr, int x, int y, int z);		\
_declspec(dllexport)  void TypeName##Grid_set_value			(Grid<Type>* ptr, int x, int y, int z, Type v); \
_declspec(dllexport)  void TypeName##Grid_get_background	(Grid<Type>* ptr, Type* v); \
_declspec(dllexport) char* TypeName##Grid_get_name			(Grid<Type>* ptr);	\
_declspec(dllexport) void TypeName##Grid_set_name			(Grid<Type>* ptr, const char* name);	\
_declspec(dllexport) int TypeName##Grid_get_grid_class(Grid<Type>* ptr); \
_declspec(dllexport) void TypeName##Grid_set_grid_class(Grid<Type>* ptr, int gclass); \
_declspec(dllexport) char* TypeName##Grid_get_type(Grid<Type>* ptr);\
_declspec(dllexport) void TypeName##Grid_get_values_ws		(Grid<Type>* ptr, int num_coords, float* coords, Type* results, int sample_type);	\
_declspec(dllexport) void TypeName##Grid_set_values_ws		(Grid<Type>* ptr, int num_coords, float* coords, Type* values);	\
_declspec(dllexport) void TypeName##Grid_get_values		(Grid<Type>* ptr, int num_coords, int* coords, Type* results);	\
_declspec(dllexport) void TypeName##Grid_set_values		(Grid<Type>* ptr, int num_coords, int* coords, Type* values);	\
_declspec(dllexport) void TypeName##Grid_filter				(Grid<Type>* ptr, int width, int iterations, int type); \
_declspec(dllexport) void TypeName##Grid_bounding_box		(Grid<Type>* ptr, int* min, int* max); \
_declspec(dllexport) void TypeName##Grid_set_transform		(Grid<Type>* ptr, float* mat); \
_declspec(dllexport) void TypeName##Grid_get_transform		(Grid<Type>* ptr, float* mat); \
_declspec(dllexport) Grid<Type>* TypeName##Grid_resample	(Grid<Type>* ptr, float scale); \
_declspec(dllexport) void TypeName##Grid_get_dense			(Grid<Type>* ptr, int* min, int* max, Type* results); \
_declspec(dllexport) SAFEARRAY* TypeName##Grid_get_some_grids				(const char* filename); \
_declspec(dllexport) unsigned long TypeName##Grid_get_active_values_size	(Grid<Type>* ptr); \
_declspec(dllexport) void TypeName##Grid_get_active_values		(Grid<Type>* ptr, int* buffer); \
_declspec(dllexport) void TypeName##Grid_erode					(Grid<Type>* ptr, int iterations); \
_declspec(dllexport) void TypeName##Grid_get_neighbours			(Grid<Type>* ptr, int* coords, Type* neighbours); \
_declspec(dllexport) bool TypeName##Grid_get_active_state		(Grid<Type>* ptr, int* xyz); \
_declspec(dllexport) void TypeName##Grid_get_active_state_many	(Grid<Type>* ptr, int n, int* xyz, int* states); \
_declspec(dllexport) void TypeName##Grid_set_active_state		(Grid<Type>* ptr, int* xyz, bool state); \
_declspec(dllexport) void TypeName##Grid_set_active_state_many	(Grid<Type>* ptr, int n, int* xyz, int* states); \
_declspec(dllexport) void TypeName##Grid_difference		(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_union			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_intersection	(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_max			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_min			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_sum			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_diff			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_ifzero			(Grid<Type>* ptr0, Grid<Type>* ptr1); \
_declspec(dllexport) void TypeName##Grid_mul			(Grid<Type>* ptr0, Grid<Type>* ptr1); \

namespace DeepSight
{
#ifdef __cplusplus
extern "C" {
#endif
	EXPORT_COMMON_H(Float, float)
	EXPORT_SCALAR_H(Float, float)

	//EXPORT_COMMON_H(Int, int)

	//EXPORT_COMMON_H(Double, double)
	//EXPORT_SCALAR_H(Double, double)

	DEEPSIGHT_EXPORT Grid<float>* FloatGrid_from_mesh(int num_verts, float* verts, int num_faces, int* faces, float* transform, float isovalue, float exteriorBandWidth, float interiorBandWidth);
	DEEPSIGHT_EXPORT Grid<float>* FloatGrid_from_points(int num_points, float* points, float radius, float voxelsize);
	DEEPSIGHT_EXPORT void FloatGrid_sdf_to_fog(Grid<float>* ptr, float cutoffDistance);
	DEEPSIGHT_EXPORT void FloatGrid_prune(Grid<float>* ptr, float tolerance);
	DEEPSIGHT_EXPORT void Grid_write(const char* filepath, int num_grids, Grid<float>** grids, int save_float_as_half);

#ifdef __cplusplus
	}
#endif
}
#endif
