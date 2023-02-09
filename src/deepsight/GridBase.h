#ifndef GRIDBASE_H
#define GRIDBASE_H

#define NOMINMAX
#include <windows.h>

#include <openvdb/openvdb.h>
#include <openvdb/tools/Interpolation.h>

#include <string>
#include <memory>


#include <Eigen/Geometry>


namespace DeepSight
{
	class GridBase
	{
	public:
		openvdb::SharedPtr<openvdb::GridBase> m_grid;

#pragma region Constructor_Init
		GridBase();

		template<typename GridT>
		void initialize();

#pragma endregion Constructor_Init

#pragma region Generic
		void set_name(std::string name);
		std::string get_name();

		void set_transform(Eigen::Matrix4d xform);
		Eigen::Matrix4d get_transform();

		void clip_index(int* min, int* max);
		void clip_world(double* min, double* max);

		void prune(float tolerance=0.0f);

		int get_grid_class();
		void set_grid_class(int c);

		void get_bounding_box();

		std::string get_type();

#pragma endregion Generic


#pragma region Get_Set

		template<typename GridT>
		typename GridT::ValueType get_value_is(Eigen::Vector3i xyz);

		template<typename GridT>
		typename GridT::ValueType get_value_ws(Eigen::Vector3d xyz);

		//template <typename GridT>
		//typename GridT::ValueType get_interpolated_value(Eigen::Vector3f xyz);

		template <typename GridT>
		Eigen::Matrix<typename GridT::ValueType, 27, 1> get_neighbourhood(Eigen::Vector3i xyz);

		template <typename GridT>
		std::vector<typename GridT::ValueType> get_values_is(std::vector<Eigen::Vector3i>& xyz);

		template <typename GridT>
		std::vector<typename GridT::ValueType> get_values_ws(std::vector<Eigen::Vector3d>& xyz);

		template<typename GridT>
		void set_value(Eigen::Vector3i xyz, typename GridT::ValueType value);

		template<typename GridT>
		void set_values(std::vector<Eigen::Vector3i>& xyz, std::vector<typename GridT::ValueType> values);

		template<typename GridT>
		std::vector<Eigen::Vector3i> get_active_voxels();

		template<typename GridT>
		bool get_active_state(Eigen::Vector3i xyz);

		template<typename GridT>
		std::vector<bool> get_active_states(std::vector<Eigen::Vector3i>& xyz);

		template<typename GridT>
		void set_active_state(Eigen::Vector3i xyz, bool state);

		template<typename GridT>
		void set_active_states(std::vector<Eigen::Vector3i>& xyz, std::vector<bool>& states);

#pragma endregion Get_Set

	};


	template<typename GridT>
	void GridBase::initialize()
	{
		m_grid = GridT::create(openvdb::zeroVal<GridT::ValueType>());
	}

#pragma region Get_Set

	template<typename GridT>
	typename GridT::ValueType GridBase::get_value_is(Eigen::Vector3i xyz)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);
		typename GridT::ValueType indexValue = (typename GridT::ValueType)sampler.isSample(openvdb::Vec3i(xyz.x(), xyz.y(), xyz.z()));

		return indexValue;
	}

	template<typename GridT>
	typename GridT::ValueType GridBase::get_value_ws(Eigen::Vector3d xyz)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);
		typename GridT::ValueType worldValue = (typename GridT::ValueType)sampler.wsSample(openvdb::Vec3R(xyz.x(), xyz.y(), xyz.z()));

		return worldValue;
	}
	/*
	template <typename GridT>
	typename GridT::ValueType GridBase::get_interpolated_value(Eigen::Vector3f xyz)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();
		return openvdb::tools::BoxSampler::sample(
			accessor,
			openvdb::Vec3R(
				xyz.x(),
				xyz.y(),
				xyz.z()
			));
	}
	*/
	template <typename GridT>
	std::vector<typename GridT::ValueType> GridBase::get_values_is(std::vector<Eigen::Vector3i>& xyz)
	{
		std::vector<typename GridT::ValueType> values;

		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		for (auto iter = xyz.begin();
			iter != xyz.end();
			iter++)
		{
			values.push_back(
				accessor.getValue(
					openvdb::math::Coord(
						iter->x(),
						iter->y(),
						iter->z()
					)
				)
			);
		}
		return values;
	}

	template <typename GridT>
	std::vector<typename GridT::ValueType> GridBase::get_values_ws(std::vector<Eigen::Vector3d>& xyz)
	{
		std::vector<typename GridT::ValueType> values;
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);

		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);

		for (auto iter = xyz.begin();
			iter != xyz.end();
			iter++)
		{
			values.push_back(sampler.wsSample(openvdb::Vec3R(iter->x(), iter->y(), iter->z())));
		}
		return values;
	}

	template <typename GridT>
	Eigen::Matrix<typename GridT::ValueType, 27, 1> GridBase::get_neighbourhood(Eigen::Vector3i xyz)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		int x = xyz[0], y = xyz[1], z = xyz[2];
		Eigen::Matrix<typename GridT::ValueType, 27, 1> neighbourhood;

		neighbourhood[0] = accessor.getValue(openvdb::Coord(x - 1, y - 1, z - 1));
		neighbourhood[1] = accessor.getValue(openvdb::Coord(x, y - 1, z - 1));
		neighbourhood[2] = accessor.getValue(openvdb::Coord(x + 1, y - 1, z - 1));

		neighbourhood[3] = accessor.getValue(openvdb::Coord(x - 1, y, z - 1));
		neighbourhood[4] = accessor.getValue(openvdb::Coord(x, y, z - 1));
		neighbourhood[5] = accessor.getValue(openvdb::Coord(x + 1, y, z - 1));

		neighbourhood[6] = accessor.getValue(openvdb::Coord(x - 1, y + 1, z - 1));
		neighbourhood[7] = accessor.getValue(openvdb::Coord(x, y + 1, z - 1));
		neighbourhood[8] = accessor.getValue(openvdb::Coord(x + 1, y + 1, z - 1));

		neighbourhood[9] = accessor.getValue(openvdb::Coord(x - 1, y - 1, z));
		neighbourhood[10] = accessor.getValue(openvdb::Coord(x, y - 1, z));
		neighbourhood[11] = accessor.getValue(openvdb::Coord(x + 1, y - 1, z));

		neighbourhood[12] = accessor.getValue(openvdb::Coord(x - 1, y, z));
		neighbourhood[13] = accessor.getValue(openvdb::Coord(x, y, z));
		neighbourhood[14] = accessor.getValue(openvdb::Coord(x + 1, y, z));

		neighbourhood[15] = accessor.getValue(openvdb::Coord(x - 1, y + 1, z));
		neighbourhood[16] = accessor.getValue(openvdb::Coord(x, y + 1, z));
		neighbourhood[17] = accessor.getValue(openvdb::Coord(x + 1, y + 1, z));

		neighbourhood[18] = accessor.getValue(openvdb::Coord(x - 1, y - 1, z + 1));
		neighbourhood[19] = accessor.getValue(openvdb::Coord(x, y - 1, z + 1));
		neighbourhood[20] = accessor.getValue(openvdb::Coord(x + 1, y - 1, z + 1));

		neighbourhood[21] = accessor.getValue(openvdb::Coord(x - 1, y, z + 1));
		neighbourhood[22] = accessor.getValue(openvdb::Coord(x, y, z + 1));
		neighbourhood[23] = accessor.getValue(openvdb::Coord(x + 1, y, z + 1));

		neighbourhood[24] = accessor.getValue(openvdb::Coord(x - 1, y + 1, z + 1));
		neighbourhood[25] = accessor.getValue(openvdb::Coord(x, y + 1, z + 1));
		neighbourhood[26] = accessor.getValue(openvdb::Coord(x + 1, y + 1, z + 1));

		return neighbourhood;
	}

	template<typename GridT>
	void GridBase::set_value(Eigen::Vector3i xyz, typename GridT::ValueType value)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		accessor.setValue(openvdb::math::Coord(xyz.x(), xyz.y(), xyz.z()), value);
	}

	template <typename GridT>
	void GridBase::set_values(std::vector<Eigen::Vector3i>& xyz, std::vector<typename GridT::ValueType> values)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		for (auto iter = std::make_pair(xyz.cbegin(), values.cbegin());
			iter.first != xyz.end() && iter.second != values.end();
			++iter.first, ++iter.second)
		{
			accessor.setValue(
				openvdb::math::Coord(
					(*iter.first).x(),
					(*iter.first).y(),
					(*iter.first).z()
				), *iter.second
			);
		}
	}

	template<typename GridT>
	std::vector<Eigen::Vector3i> GridBase::get_active_voxels()
	{
		std::vector<Eigen::Vector3i> values;
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);

		for (typename GridT::ValueOnCIter iter = grid->cbeginValueOn(); iter.test(); ++iter)
		{
			if (iter.isVoxelValue())
			{
				values.push_back(Eigen::Vector3i(iter.getCoord().data()));
			}
		}
		return values;
	}

	template<typename GridT>
	bool GridBase::get_active_state(Eigen::Vector3i xyz)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		return accessor.isValueOn(openvdb::math::Coord(xyz.data()));
	}

	template<typename GridT>
	std::vector<bool> GridBase::get_active_states(std::vector<Eigen::Vector3i>& xyz)
	{
		std::vector<bool> states;

		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		for (auto iter = xyz.begin(); iter != xyz.end(); ++iter)
		{
			states.push_back(accessor.isValueOn(openvdb::math::Coord((*iter).data())));
		}

		return states;
	}

	template<typename GridT>
	void GridBase::set_active_state(Eigen::Vector3i xyz, bool state)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		accessor.setActiveState(
			openvdb::math::Coord(xyz.data()), state);
	}

	template<typename GridT>
	void GridBase::set_active_states(std::vector<Eigen::Vector3i>& xyz, std::vector<bool>& states)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		typename GridT::Accessor accessor = grid->getAccessor();

		for (auto iter = std::make_pair(xyz.cbegin(), states.cbegin());
			iter.first != xyz.end() && iter.second != states.end();
			++iter.first, ++iter.second)
		{
			accessor.setActiveState(
				openvdb::math::Coord((*iter.first).data()), *iter.second);
		}
	}

#pragma endregion Get_Set

#define INSTANTIATE_GRIDBASE(GridT, ValueT) \
	template<> void GridBase::initialize<GridT>();\
	template<> ValueT GridBase::get_value_is<GridT>(Eigen::Vector3i xyz);\
	template<> ValueT GridBase::get_value_ws<GridT>(Eigen::Vector3d xyz);\
	template<> std::vector<ValueT> GridBase::get_values_is<GridT>(std::vector<Eigen::Vector3i>& xyz);\
	template<> std::vector<ValueT> GridBase::get_values_ws<GridT>(std::vector<Eigen::Vector3d>& xyz);\
	template<> ValueT GridBase::set_value<GridT>(Eigen::Vector3i xyz, ValueT value);\
	template<> void GridBase::set_values<GridT>(std::vector<Eigen::Vector3i>& xyz, std::vector<ValueT> values);\
	template<> std::vector<Eigen::Vector3i> GridBase::get_active_voxels<GridT>();\
	template<> bool GridBase::get_active_state<GridT>(Eigen::Vector3i xyz);\
	template<> std::vector<bool> GridBase::get_active_states<GridT>(std::vector<Eigen::Vector3i>& xyz);\
	template<> void GridBase::set_active_state<GridT>(Eigen::Vector3i xyz, bool state);\
	template<> void GridBase::set_active_states<GridT>(std::vector<Eigen::Vector3i>& xyz, std::vector<bool>& states);


}
#endif
