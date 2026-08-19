#ifndef READ_WRITE_H
#define READ_WRITE_H

#define NOMINMAX
#include <windows.h>	// CoTaskMemAlloc

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
#include "ApiGuard.h"

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

	/// Reads every grid in a .vdb file. Caller owns the returned GridBase*
	/// objects. Throws openvdb::IoError if the file cannot be read.
	std::vector<GridBase*> read_vdb(const std::string& path);

#ifdef __cplusplus
	extern "C" {
#endif
	/// On success sets *num_grids and *grid_ptrs, where *grid_ptrs is a
	/// CoTaskMem-allocated array of *num_grids GridBase* handles that the
	/// caller must free with CoTaskMemFree. On failure sets *num_grids to 0,
	/// *grid_ptrs to null, and records the reason in DeepSight_GetLastError().
	///
	/// The third parameter is now spelled GridBase*** rather than GridBase**.
	/// The ABI is unchanged (it was always an out-parameter receiving an
	/// array); the old spelling just made the intent unreadable.
	DEEPSIGHT_EXPORT void DEEPSIGHT_CALL ReadWrite_ReadVdb(const char* path, int* num_grids, GridBase*** grid_ptrs);
	DEEPSIGHT_EXPORT void DEEPSIGHT_CALL ReadWrite_WriteVdb(const char* path, int num_grids, GridBase* const* grids, int float_as_half);

#ifdef __cplusplus
	}
#endif
}
#endif