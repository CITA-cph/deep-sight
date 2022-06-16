#ifndef IO_H
#define IO_H

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
	Grid<float>::Ptr load_scalar_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0);
	//std::shared_ptr<Grid<openvdb::Vec3f>> load_vector_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0);

	Grid<openvdb::Vec3f>::Ptr load_vector_tiff(const std::string path, double threshold = 1.0e-3, unsigned int crop = 0);
	RawLam::InfoLog::Ptr load_infolog(const std::string path, bool verbose = false);

}
#endif