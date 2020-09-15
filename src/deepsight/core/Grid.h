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

namespace DeepSight
{
	class Grid
	{
	public:
		Grid::Grid();
		~Grid();

		using Ptr = std::shared_ptr<Grid>;

		std::map<std::string, openvdb::FloatGrid::Ptr> grids;

		void read(std::string path);
		void write(std::string grid_name, std::string path);
		void from_multipage_tiff(std::string path, std::string id, double threshold=1.0e-3);

		std::vector<std::string> grid_names();
		float getValue(std::string grid_name, Eigen::Vector3i xyz);
		std::vector<float> getDense(std::string grid_name, Eigen::Vector3i min, Eigen::Vector3i max);
		float getInterpolatedValue(std::string grid_name, Eigen::Vector3f xyz);
		std::vector<float> getValues(std::string grid_name, std::vector<Eigen::Vector3i> &xyz);
		std::vector<float> getInterpolatedValues(std::string grid_name, std::vector<Eigen::Vector3f> &xyz);
		std::tuple<Eigen::Vector3i, Eigen::Vector3i> getBoundingBox(std::string grid_name);

	};

}

#endif
