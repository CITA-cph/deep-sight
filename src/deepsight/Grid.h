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
#include <openvdb/tools/RayIntersector.h>

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

//typedef openvdb::FloatGrid openvdb_grid;


namespace DeepSight
{
	template<typename T>
	class Grid
	{
	public:

		using Ptr = std::shared_ptr<Grid<T>>;

		using TreeT = openvdb::tree::Tree< openvdb::tree::RootNode< openvdb::tree::InternalNode< openvdb::tree::InternalNode< openvdb::tree::LeafNode< T, 3 >, 4 >, 5 >>>;
		using GridT = openvdb::Grid<TreeT>;
		using ValueT = typename GridT::ValueType;

		Grid();
		Grid(T background);
		~Grid();

		openvdb::SharedPtr<GridT> m_grid;
		//GridT* m_grid;

		static std::vector<Ptr> from_vdb(const std::string path);
		static Ptr read(const std::string filename, double threshold = 1.0e-3, unsigned int crop = 0);

		void write(const std::string path, bool float_as_half = false);

		static void write(const std::string path, std::vector<Grid*> grids, bool float_as_half = false);
		//static void write_many(const std::string path, std::vector<Grid<T>> grids, bool float_as_half);

		T get_value(Eigen::Vector3i xyz);
		std::vector<T> get_values(std::vector<Eigen::Vector3i>& xyz);

		void set_value(Eigen::Vector3i xyz, T value);
		void set_values(std::vector<Eigen::Vector3i>& xyz, std::vector<T> values);

		bool get_active_state(Eigen::Vector3i xyz);
		std::vector<bool> get_active_state(std::vector<Eigen::Vector3i>& xyz);

		void set_active_state(Eigen::Vector3i xyz, bool state);
		void set_active_state(std::vector<Eigen::Vector3i>& xyz, std::vector<bool>& states);

		std::vector<T> get_dense(Eigen::Vector3i min, Eigen::Vector3i max);
		std::vector<Eigen::Vector3i> get_active_voxels();

		T get_interpolated_value(Eigen::Vector3f xyz);
		std::vector<T> get_interpolated_values(std::vector<Eigen::Vector3f>& xyz, unsigned int sample_type = 1);

		Eigen::Matrix<T, 27, 1> get_neighbourhood(Eigen::Vector3i xyz);

		void prune(T value);

		const std::tuple<Eigen::Vector3i, Eigen::Vector3i> bounding_box();
		void transform_grid(Eigen::Matrix4d xform);
		void set_transform(Eigen::Matrix4d xform);
		Eigen::Matrix4d get_transform();

		std::string get_name();
		void set_name(std::string name);

		void dense_fill(Eigen::Vector3i min, Eigen::Vector3i max, T value, bool active = true);

		void gradient();
		Ptr laplacian();
		Ptr mean_curvature();
		void normalize();
		void filter(int type = 0, int width = 1, int iterations = 1);

		void dilate(int iterations = 1);
		void erode(int iterations = 1);

		void to_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces);
		//void from_mesh(float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& faces);

		Ptr duplicate();

		Grid<T>* resample(float scale);

	protected:
		static bool has_suffix(const std::string& str, const std::string& suffix);

	};

	Grid<float>::Ptr from_mesh(std::vector<openvdb::Vec3f> verts, std::vector <openvdb::Vec4I> faces, openvdb::math::Transform xform, float isovalue, float exteriorBandWidth, float interiorBandWidth);

}

#endif
