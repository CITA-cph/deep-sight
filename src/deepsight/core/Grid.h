#ifndef GRID_H
#define GRID_H

#define NOMINMAX
#include <windows.h>

#include <openvdb/openvdb.h>
#include <openvdb/tree/ValueAccessor.h>
#include <openvdb/tools/Interpolation.h>
#include <openvdb/tools/Dense.h>


#include <map>
#include <vector>
#include <tuple>
#include <memory>
#include <filesystem>
#include "config.h"

#include "tiff.h"
#include "tiffio.h"

#include <Eigen/Geometry>

typedef openvdb::FloatGrid openvdb_grid;

namespace DeepSight
{
	class Grid
	{
	public:
		Grid();
		~Grid();

		openvdb_grid::Ptr m_grid;	

		using Ptr = std::shared_ptr<Grid>;

		static Ptr from_multipage_tiff(const std::string path, double threshold=1.0e-3);
		static std::vector<Ptr> from_vdb(const std::string path);
		static Ptr read(const std::string path);

		// std::map<std::string, openvdb::FloatGrid::Ptr> grids;

		// void read(std::string path);
		void write(const std::string path);
		void write_as_vdb(const std::string path);
		void write_as_multipage_tiff(const std::string path);

		// void from_multipage_tiff(std::string path, std::string id, double threshold=1.0e-3);

		float getValue(Eigen::Vector3i xyz);
		std::vector<float> getDense(Eigen::Vector3i min, Eigen::Vector3i max);
		float getInterpolatedValue(Eigen::Vector3f xyz);
		std::vector<float> getValues(std::vector<Eigen::Vector3i> &xyz);
		std::vector<float> getInterpolatedValues(std::vector<Eigen::Vector3f> &xyz);
		std::tuple<Eigen::Vector3i, Eigen::Vector3i> getBoundingBox();

		std::string name;

	protected:
		static bool has_suffix(const std::string &str, const std::string &suffix);

	};

	// class MultiGrid
	// {
	// public:
	// 	MultiGrid();
	// 	~MultiGrid();

	// 	using Ptr = std::shared_ptr<MultiGrid>;
	// 	std::map<std::string, Grid::Ptr> grids;

	// }

}

#endif
