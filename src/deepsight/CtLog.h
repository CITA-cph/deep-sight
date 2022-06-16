#pragma once
#define _USE_MATH_DEFINES
#define NOMINMAX
#include <cmath>

#include "openvdb/openvdb.h"
#include <openvdb/tools/Interpolation.h>
#include <openvdb/tools/Filter.h>
#include <openvdb/tools/VolumeToMesh.h>
#include <openvdb/tools/GridTransformer.h>
#include <openvdb/tools/Composite.h>

#include "openvdb/Types.h"

#include <Eigen/Core>

#include "Mesh.h"

using GridType = typename openvdb::FloatGrid;
using ValueT = typename GridType::ValueType;

namespace DeepSight
{
	class CtLog
	{
		GridType::Ptr m_grid;

		bool initialized = false;

	public:
		CtLog();
		~CtLog();

		void readVDB(const std::string filename);
		std::vector<ValueT> evaluate(std::vector<Eigen::Vector3f> coords, int sample_type = 0);
		void filter(int width, int iterations, int type = 0);
		void offset(ValueT amount);
		openvdb::math::CoordBBox bounding_box();
		Eigen::Matrix4d get_transform();
		void set_transform(Eigen::Matrix4d mat);
		void to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces);
		CtLog* resample(float scale);

	};


#ifdef __cplusplus
	extern "C" {
#endif

		int debug_grid_counter = 0;

		RAWLAM_EXPORT CtLog* CtLog_Create();
		RAWLAM_EXPORT void CtLog_Delete(CtLog* ptr);
		RAWLAM_EXPORT void CtLog_readVDB(CtLog* ptr, const char* filename);
		RAWLAM_EXPORT void CtLog_evaluate(CtLog* ptr, int num_coords, float* coords, ValueT* results, int sample_type);
		RAWLAM_EXPORT void CtLog_filter(CtLog* ptr, int width, int iterations, int type);
		RAWLAM_EXPORT void CtLog_offset(CtLog* ptr, ValueT amount);
		RAWLAM_EXPORT void CtLog_bounding_box(CtLog* ptr, float* min, float* max);
		RAWLAM_EXPORT void CtLog_set_transform(CtLog* ptr, float* mat);
		RAWLAM_EXPORT void CtLog_get_transform(CtLog* ptr, float* mat);
		RAWLAM_EXPORT void CtLog_to_mesh(CtLog* ptr, DeepSight::QuadMesh* mesh_ptr, float isovalue);
		RAWLAM_EXPORT CtLog* CtLog_resample(CtLog* ptr, float scale);
		RAWLAM_EXPORT int CtLog_debug_grid_counter();

#ifdef __cplusplus
	}
#endif
}