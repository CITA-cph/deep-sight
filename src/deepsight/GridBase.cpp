#include "GridBase.h"

#include <mutex>
#include <stdexcept>

namespace DeepSight
{
	namespace
	{
		// openvdb::initialize() was being called from every GridBase
		// constructor. It is documented as safe to call repeatedly, but it
		// takes an internal lock and walks the type registry each time, so on a
		// file with hundreds of grids -- or a Grasshopper solve that creates
		// thousands of temporaries -- it becomes pure contention. std::call_once
		// makes it exactly once per process and is thread-safe, which the bare
		// call was not guaranteed to be across concurrent GH component solves.
		std::once_flag g_openvdb_init_flag;

		void ensure_openvdb_initialized()
		{
			std::call_once(g_openvdb_init_flag, [] { openvdb::initialize(); });
		}
	}

#pragma region Constructor_Init

	GridBase::GridBase()
	{
		ensure_openvdb_initialized();
	}

	GridBase* GridBase::duplicate()
	{
		if (!m_grid)
			throw std::runtime_error("Cannot duplicate an empty grid.");

		// std::unique_ptr so that if deepCopyGrid() throws (bad_alloc is very
		// plausible -- this doubles the memory footprint of the volume) the
		// half-built GridBase is destroyed rather than leaked.
		std::unique_ptr<GridBase> grid(new GridBase());
		grid->m_grid = m_grid->deepCopyGrid();

		return grid.release();
	}

#pragma endregion Constructor_Init

#pragma region Generic

	void GridBase::set_name(const std::string& name)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		m_grid->setName(name);
	}

	std::string GridBase::get_name() const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		return m_grid->getName();
	}

	Eigen::Matrix4d GridBase::get_transform() const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");

		// baseMap()->getAffineMap() returns null for non-affine (e.g. frustum)
		// transforms. The old code dereferenced it unconditionally.
		auto affine = m_grid->transform().baseMap()->getAffineMap();
		if (!affine)
			throw std::runtime_error("Grid transform is not affine and cannot be expressed as a 4x4 matrix.");

		const auto mat = affine->getMat4();

		// See the convention note in GridBase.h: OpenVDB's Mat4 is row-major,
		// Eigen's default is column-major, so this constructor produces the
		// transpose. That is deliberate -- it is what makes the flat buffer
		// handed to C# come out in row-major order.
		return Eigen::Matrix4d(mat.asPointer());
	}

	void GridBase::set_transform(const Eigen::Matrix4d& mat)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");

		openvdb::Mat4R omat(mat.data());
		openvdb::math::Transform::Ptr linearTransform =
			openvdb::math::Transform::createLinearTransform(omat);

		m_grid->setTransform(linearTransform);
	}

	void GridBase::clip_index(const int* min, const int* max)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		if (min == nullptr || max == nullptr)
			throw std::invalid_argument("clip_index: null bounds.");

		openvdb::CoordBBox bb(
			openvdb::Coord(min[0], min[1], min[2]),
			openvdb::Coord(max[0], max[1], max[2]));

		// An inverted box makes clip() a no-op that silently discards nothing,
		// which reads to the user as "clip is broken". Normalise it instead.


		m_grid->clip(bb);
	}

	void GridBase::clip_world(const double* min, const double* max)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		if (min == nullptr || max == nullptr)
			throw std::invalid_argument("clip_world: null bounds.");

		openvdb::BBoxd bb(
			openvdb::Vec3d(min[0], min[1], min[2]),
			openvdb::Vec3d(max[0], max[1], max[2]));


		m_grid->clipGrid(bb);
	}

	void GridBase::prune(float tolerance)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		m_grid->pruneGrid(tolerance);
	}

	int GridBase::get_grid_class() const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		return static_cast<int>(m_grid->getGridClass());
	}

	void GridBase::set_grid_class(int c)
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");

		// Reject out-of-range values rather than casting arbitrary integers
		// into the enum, which is UB and would leave the grid in a state that
		// crashes later on serialisation.
		if (c < openvdb::GRID_UNKNOWN || c > openvdb::GRID_STAGGERED)
			throw std::invalid_argument("set_grid_class: unrecognised grid class.");

		m_grid->setGridClass(static_cast<openvdb::GridClass>(c));
	}

	void GridBase::get_bounding_box(int* min, int* max) const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		if (min == nullptr || max == nullptr)
			throw std::invalid_argument("get_bounding_box: null output buffer.");

		const auto bb = m_grid->evalActiveVoxelBoundingBox();

		min[0] = bb.min().x();
		min[1] = bb.min().y();
		min[2] = bb.min().z();

		max[0] = bb.max().x();
		max[1] = bb.max().y();
		max[2] = bb.max().z();
	}

	std::string GridBase::get_type() const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		return m_grid->type();
	}

	openvdb::Index64 GridBase::get_active_voxel_count() const
	{
		if (!m_grid) throw std::runtime_error("Grid is empty.");
		return m_grid->activeVoxelCount();
	}

#pragma endregion Generic

}
