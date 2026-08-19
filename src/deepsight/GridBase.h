#ifndef GRIDBASE_H
#define GRIDBASE_H

// NOTE: <windows.h> used to be included here. It has been removed: this is a
// core header pulled in by nearly every translation unit, and windows.h drags
// in ~thousands of macros (including min/max, which is why NOMINMAX was needed)
// that collide with OpenVDB and Eigen. The two places that actually need the
// Win32 COM allocator (GridBaseAPI.cpp, ReadWrite.cpp) include it themselves.

#include <openvdb/openvdb.h>
#include <openvdb/tools/Interpolation.h>

#include <stdexcept>
#include <string>
#include <memory>
#include <vector>

#include <Eigen/Geometry>

#include "config.h"

namespace DeepSight
{
	class GridBase
	{
	public:
		openvdb::SharedPtr<openvdb::GridBase> m_grid;

#pragma region Constructor_Init
		GridBase();

		template<typename GridT>
		void initialize(typename GridT::ValueType background);

		GridBase* duplicate();

#pragma endregion Constructor_Init

#pragma region Generic
		void set_name(const std::string& name);
		std::string get_name() const;

		/// Set the grid's index-to-world transform.
		///
		/// IMPORTANT - matrix convention: the Eigen matrix passed here is
		/// interpreted using Eigen's *default column-major storage*, and is
		/// handed to OpenVDB's Mat4 which is *row-major*. The two conventions
		/// cancel out, so a flat 16-element buffer laid out in row-major order
		/// (the layout the C# `Transform` property documents, and the layout
		/// Rhino uses) round-trips correctly through set_transform/get_transform.
		///
		/// The consequence is that the Eigen::Matrix4d objects these two
		/// functions accept and return are the *transpose* of the mathematical
		/// transform. Do not feed the result of get_transform() into Eigen
		/// matrix arithmetic without transposing it first.
		void set_transform(const Eigen::Matrix4d& xform);
		Eigen::Matrix4d get_transform() const;

		void clip_index(const int* min, const int* max);
		void clip_world(const double* min, const double* max);

		void prune(float tolerance = 0.0f);

		int get_grid_class() const;
		void set_grid_class(int c);

		void get_bounding_box(int* min, int* max) const;

		std::string get_type() const;

		/// Number of active voxels. Returns Index64 rather than int: a 2000^3
		/// dense region already exceeds INT_MAX, and CT scan volumes at that
		/// scale are the normal case for this library, not an edge case.
		openvdb::Index64 get_active_voxel_count() const;

#pragma endregion Generic

#pragma region Casting

		/// Downcast m_grid to a concrete grid type, throwing instead of
		/// returning null on failure.
		///
		/// openvdb::gridPtrCast returns a null pointer when the runtime type
		/// does not match, and every call site in this file used to dereference
		/// the result unchecked. That is trivially reachable from managed code:
		/// GridIO.Read() hands back whatever grid types are in the .vdb file,
		/// so calling a FloatGrid method on a file containing a Vec3f grid
		/// dereferenced null and killed the host process. Throwing here turns
		/// that into an error message at the API boundary instead.
		template<typename GridT>
		typename GridT::Ptr grid_as() const
		{
			if (!m_grid)
				throw std::runtime_error("Grid is empty (null tree).");

			typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
			if (!grid)
			{
				throw std::runtime_error(
					"Grid type mismatch: this grid holds '" + m_grid->type() +
					"' but was accessed as '" + GridT::gridType() + "'.");
			}
			return grid;
		}

#pragma endregion Casting

#pragma region Get_Set

		template<typename GridT>
		typename GridT::ValueType get_value_is(const Eigen::Vector3i& xyz);

		template<typename GridT>
		typename GridT::ValueType get_value_ws(const Eigen::Vector3d& xyz);

		template <typename GridT>
		Eigen::Matrix<typename GridT::ValueType, 27, 1> get_neighbourhood(const Eigen::Vector3i& xyz);

		template <typename GridT>
		std::vector<typename GridT::ValueType> get_values_is(const std::vector<Eigen::Vector3i>& xyz);

		template <typename GridT>
		std::vector<typename GridT::ValueType> get_values_ws(const std::vector<Eigen::Vector3d>& xyz);

		template<typename GridT>
		void set_value(const Eigen::Vector3i& xyz, const typename GridT::ValueType& value);

		template<typename GridT>
		void set_values(const std::vector<Eigen::Vector3i>& xyz, const std::vector<typename GridT::ValueType>& values);

		template<typename GridT>
		std::vector<Eigen::Vector3i> get_active_voxels();

		template<typename GridT>
		bool get_active_state(const Eigen::Vector3i& xyz);

		template<typename GridT>
		std::vector<bool> get_active_states(const std::vector<Eigen::Vector3i>& xyz);

		template<typename GridT>
		void set_active_state(const Eigen::Vector3i& xyz, bool state);

		template<typename GridT>
		void set_active_states(const std::vector<Eigen::Vector3i>& xyz, const std::vector<bool>& states);

		template<typename GridT>
		void inactivate_below(const typename GridT::ValueType& min);

#pragma endregion Get_Set

	};


	template<typename GridT>
	void GridBase::initialize(typename GridT::ValueType background)
	{
		m_grid = GridT::create(background);
	}


#pragma region Get_Set

	template<typename GridT>
	typename GridT::ValueType GridBase::get_value_is(const Eigen::Vector3i& xyz)
	{
		// Previously this built an openvdb::tools::GridSampler and called
		// isSample() on integer coordinates. That does a full trilinear
		// interpolation to recover a value that is exact at lattice points --
		// eight tree lookups and a pile of float math per call, and for
		// Int32Grid it round-tripped through floating point. A direct accessor
		// lookup is one traversal and is exact for every value type.
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::ConstAccessor accessor = grid->getConstAccessor();

		return accessor.getValue(openvdb::Coord(xyz.x(), xyz.y(), xyz.z()));
	}

	template<typename GridT>
	typename GridT::ValueType GridBase::get_value_ws(const Eigen::Vector3d& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);

		return sampler.wsSample(openvdb::Vec3R(xyz.x(), xyz.y(), xyz.z()));
	}

	template <typename GridT>
	std::vector<typename GridT::ValueType> GridBase::get_values_is(const std::vector<Eigen::Vector3i>& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::ConstAccessor accessor = grid->getConstAccessor();

		std::vector<typename GridT::ValueType> values;
		values.reserve(xyz.size());		// avoid O(log n) reallocations of a
										// potentially multi-million element buffer

		for (const auto& c : xyz)
		{
			values.push_back(accessor.getValue(openvdb::Coord(c.x(), c.y(), c.z())));
		}
		return values;
	}

	template <typename GridT>
	std::vector<typename GridT::ValueType> GridBase::get_values_ws(const std::vector<Eigen::Vector3d>& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);

		std::vector<typename GridT::ValueType> values;
		values.reserve(xyz.size());

		for (const auto& c : xyz)
		{
			values.push_back(sampler.wsSample(openvdb::Vec3R(c.x(), c.y(), c.z())));
		}
		return values;
	}

	template <typename GridT>
	Eigen::Matrix<typename GridT::ValueType, 27, 1> GridBase::get_neighbourhood(const Eigen::Vector3i& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::ConstAccessor accessor = grid->getConstAccessor();

		const int x = xyz.x(), y = xyz.y(), z = xyz.z();
		Eigen::Matrix<typename GridT::ValueType, 27, 1> neighbourhood;

		// Replaces 27 hand-written, hand-indexed lines. The original was
		// correct, but a single transposed index in that block would have been
		// invisible on review; the loop makes the ordering (x fastest, then y,
		// then z) explicit and impossible to get subtly wrong.
		int n = 0;
		for (int dz = -1; dz <= 1; ++dz)
			for (int dy = -1; dy <= 1; ++dy)
				for (int dx = -1; dx <= 1; ++dx)
					neighbourhood[n++] = accessor.getValue(openvdb::Coord(x + dx, y + dy, z + dz));

		return neighbourhood;
	}

	template<typename GridT>
	void GridBase::set_value(const Eigen::Vector3i& xyz, const typename GridT::ValueType& value)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::Accessor accessor = grid->getAccessor();

		accessor.setValue(openvdb::Coord(xyz.x(), xyz.y(), xyz.z()), value);
	}

	template <typename GridT>
	void GridBase::set_values(const std::vector<Eigen::Vector3i>& xyz, const std::vector<typename GridT::ValueType>& values)
	{
		// The old loop silently stopped at the shorter of the two ranges, so a
		// caller that passed mismatched arrays got a partially written grid and
		// no indication anything was wrong. Fail loudly instead.
		if (xyz.size() != values.size())
			throw std::invalid_argument("set_values: coordinate and value counts differ.");

		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::Accessor accessor = grid->getAccessor();

		for (std::size_t i = 0; i < xyz.size(); ++i)
		{
			accessor.setValue(
				openvdb::Coord(xyz[i].x(), xyz[i].y(), xyz[i].z()),
				values[i]);
		}
	}

	template<typename GridT>
	std::vector<Eigen::Vector3i> GridBase::get_active_voxels()
	{
		typename GridT::Ptr grid = grid_as<GridT>();

		std::vector<Eigen::Vector3i> values;

		for (typename GridT::ValueOnCIter iter = grid->cbeginValueOn(); iter.test(); ++iter)
		{
			if (iter.isVoxelValue())
			{
				const openvdb::Coord c = iter.getCoord();
				values.push_back(Eigen::Vector3i(c.x(), c.y(), c.z()));
			}
			else
			{
				// Active *tiles* represent a whole cube of active voxels with a
				// single tree node. The old code skipped them entirely, which
				// meant get_active_voxels() could return far fewer entries than
				// activeVoxelCount() reported -- and since the C API used that
				// count to size the caller's buffer, the tail of the buffer was
				// left uninitialised. Expand tiles so the two agree.
				openvdb::CoordBBox bbox;
				iter.getBoundingBox(bbox);
				for (const openvdb::Coord& c : bbox)
				{
					values.push_back(Eigen::Vector3i(c.x(), c.y(), c.z()));
				}
			}
		}
		return values;
	}

	template<typename GridT>
	bool GridBase::get_active_state(const Eigen::Vector3i& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::ConstAccessor accessor = grid->getConstAccessor();

		return accessor.isValueOn(openvdb::Coord(xyz.x(), xyz.y(), xyz.z()));
	}

	template<typename GridT>
	void GridBase::inactivate_below(const typename GridT::ValueType& threshold)
	{
		typename GridT::Ptr ptr = grid_as<GridT>();
		const typename GridT::ValueType background = ptr->background();

		for (auto iter = ptr->beginValueOn(); iter; ++iter)
		{
			if (iter.getValue() < threshold)
			{
				iter.setValue(background);
				iter.setValueOff();
			}
		}
	}

	template<typename GridT>
	std::vector<bool> GridBase::get_active_states(const std::vector<Eigen::Vector3i>& xyz)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::ConstAccessor accessor = grid->getConstAccessor();

		std::vector<bool> states;
		states.reserve(xyz.size());

		for (const auto& c : xyz)
		{
			states.push_back(accessor.isValueOn(openvdb::Coord(c.x(), c.y(), c.z())));
		}

		return states;
	}

	template<typename GridT>
	void GridBase::set_active_state(const Eigen::Vector3i& xyz, bool state)
	{
		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::Accessor accessor = grid->getAccessor();

		accessor.setActiveState(openvdb::Coord(xyz.x(), xyz.y(), xyz.z()), state);
	}

	template<typename GridT>
	void GridBase::set_active_states(const std::vector<Eigen::Vector3i>& xyz, const std::vector<bool>& states)
	{
		if (xyz.size() != states.size())
			throw std::invalid_argument("set_active_states: coordinate and state counts differ.");

		typename GridT::Ptr grid = grid_as<GridT>();
		typename GridT::Accessor accessor = grid->getAccessor();

		for (std::size_t i = 0; i < xyz.size(); ++i)
		{
			accessor.setActiveState(
				openvdb::Coord(xyz[i].x(), xyz[i].y(), xyz[i].z()), states[i]);
		}
	}

#pragma endregion Get_Set

// The INSTANTIATE_GRIDBASE macro that used to live here was removed. It was
// dead code (never invoked anywhere in the tree) and it would not have compiled
// if it had been: it declared `initialize<GridT>()` with no arguments when the
// real signature takes a background value, and declared set_value as returning
// ValueT when it returns void. Explicit instantiation is unnecessary anyway --
// these templates are defined in the header and instantiated on use.

}
#endif
