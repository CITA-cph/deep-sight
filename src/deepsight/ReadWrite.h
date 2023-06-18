#ifndef READ_WRITE_H
#define READ_WRITE_H

#define NOMINMAX
#include <windows.h>

#include <openvdb/openvdb.h>
#include <openvdb/tree/ValueAccessor.h>
#include <openvdb/tools/Interpolation.h>
#include <openvdb/tools/Filter.h>
#include <openvdb/tools/Dense.h>
#include <openvdb/tools/GridTransformer.h>
#include <openvdb/tools/GridOperators.h>
#include <openvdb/tools/LevelSetTracker.h>

#include <openvdb/math/Math.h>
#include <openvdb/math/Mat.h>
#include <openvdb/math/Mat3.h>

#include "Grid.h"
#include "InfoLog.h"
#include "GridBase.h"

#include <map>
#include <vector>
#include <tuple>
#include <memory>
#include <filesystem>
#include "config.h"

#include "tiff.h"
#include "tiffio.h"

#include <Eigen/Geometry>


namespace DeepSight
{
	//template <typename T>
	Grid<float>::Ptr load_scalar_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0, bool verbose = false);
	//std::shared_ptr<Grid<openvdb::Vec3f>> load_vector_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0);

	Grid<openvdb::Vec3f>::Ptr load_vector_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0);
	DEEPSIGHT_EXPORT RawLam::InfoLog::Ptr load_infolog(const std::string path, bool verbose = false);

	std::vector<GridBase*> read_vdb(const std::string path);

#ifdef __cplusplus
	extern "C" {
#endif
	DEEPSIGHT_EXPORT void ReadWrite_ReadVdb(const char* path, int* num_grids, GridBase** grid_ptrs);
	DEEPSIGHT_EXPORT void ReadWrite_WriteVdb(const char* path, int num_grids, GridBase** grids, int float_as_half);

#ifdef __cplusplus
	}
#endif
}
#endif