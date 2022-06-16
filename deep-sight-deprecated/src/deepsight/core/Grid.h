#ifndef GRID_H
#define GRID_H

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
	template<class T>
	class Grid
	{
	public:
		Grid();
		~Grid();

		using Ptr = std::shared_ptr<Grid<T>>;

		using TreeT = openvdb::tree::Tree< openvdb::tree::RootNode< openvdb::tree::InternalNode< openvdb::tree::InternalNode< openvdb::tree::LeafNode< T, 3 >, 4 >, 5 >>>;
		using GridT = openvdb::Grid<TreeT>;

		openvdb::SharedPtr<GridT> m_grid;	

		static Ptr from_multipage_tiff(const std::string path, double threshold=1.0e-3, unsigned int crop=0);
		static Ptr from_many_tiffs(std::vector<std::string> paths, double threshold=1.0e-3, unsigned int crop=0);
		static std::vector<Ptr> from_vdb(const std::string path);
		static Ptr read(const std::string path);

		void write(const std::string path, bool float_as_half=false);
		//void write_as_vdb(const std::string path);
		//void write_as_multipage_tiff(const std::string path);

		T getValue(Eigen::Vector3i xyz);
		std::vector<T> getDense(Eigen::Vector3i min, Eigen::Vector3i max);
		T getInterpolatedValue(Eigen::Vector3f xyz);
		std::vector<T> getValues(std::vector<Eigen::Vector3i> &xyz);
		std::vector<T> getInterpolatedValues(std::vector<Eigen::Vector3f> &xyz);
		const std::tuple<Eigen::Vector3i, Eigen::Vector3i> getBoundingBox();
		void transform_grid(Eigen::Matrix4d xform);
		void set_transform(Eigen::Matrix4d xform);
		Eigen::Matrix4d get_transform();

		std::string get_name();
		void set_name(std::string name);

		void denseFill(Eigen::Vector3i min, Eigen::Vector3i max, double value, bool active=true);

		void gradient();
		Ptr laplacian();
		Ptr mean_curvature();
		void normalize();
		void filter(int width=1, int iterations=1);

		void dilate(int iterations=1);
		void erode(int iterations=1);



	protected:
		static bool has_suffix(const std::string &str, const std::string &suffix);

	};

	typedef Grid<float> FloatGrid;
	typedef std::shared_ptr<FloatGrid> FloatGridPtr;

	// typedef Grid<openvdb::math::Vec3<float>> VecGrid;
	// typedef std::shared_ptr<VecGrid> VecGridPtr;
}

//#include "Grid.cpp"

#endif
