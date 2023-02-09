#ifndef TOOLS_H
#define TOOLS_H

#include "GridBase.h"
#include "ParticleList.h"
#include <openvdb/tools/ParticlesToLevelSet.h>

#include <openvdb/tools/Filter.h>
#include <openvdb/tools/GridOperators.h>
#include <openvdb/tools/GridTransformer.h>
#include <openvdb/tools/Dense.h>
#include <openvdb/tree/ValueAccessor.h>
#include <openvdb/tools/Morphology.h>

namespace DeepSight
{

#pragma region Filter_Tools

	template<typename GridT>
	GridBase* filter(GridBase* grid, int width, int iterations, int type);

	template<typename GridT>
	GridBase* resample(GridBase* grid, float scale);

	template<typename GridT>
	GridBase* mean_curvature(GridBase* grid);

	template<typename GridT>
	void erode(GridBase* grid, int iterations);

	template<typename GridT>
	void dilate(GridBase* grid, int iterations);

#pragma endregion Filter_Tools

#pragma region Conversion_Tools

	template<typename GridT>
	void volume_to_mesh(
		GridBase* grid, float isovalue,
		std::vector<Eigen::Vector3f>& verts,
		std::vector<Eigen::Vector4i>& quads,
		std::vector<Eigen::Vector3i>& tris);

	GridBase* volume_from_mesh(
		std::vector<openvdb::Vec3f> vert_data,
		std::vector<openvdb::Vec4I> faces,
		float* xform_data, float isovalue,
		float exteriorBandWidth, float interiorBandWidth);

	GridBase* volume_from_points(
		int num_points, float* points, 
		float radius, float voxelsize);

#pragma endregion Conversion_Tools





}

#endif