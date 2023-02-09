#define GRIDBASE
#ifdef GRIDBASE
#include "GridBase.h"

namespace DeepSight
{
#pragma region Constructor_Init

	GridBase::GridBase()
	{
		openvdb::initialize();
	}

#pragma endregion Constructor_Init

#pragma region Generic

	void GridBase::set_name(std::string name)
	{
		m_grid->setName(name);
	}

	std::string GridBase::get_name()
	{
		return m_grid->getName();
	}

	Eigen::Matrix4d GridBase::get_transform()
	{
		auto mat = m_grid->transform().baseMap()->getAffineMap()->getMat4();

		return Eigen::Matrix4d(mat.asPointer());
	}

	void GridBase::set_transform(Eigen::Matrix4d mat)
	{
		openvdb::Mat4R omat(mat.data());
		openvdb::math::Transform::Ptr linearTransform =
			openvdb::math::Transform::createLinearTransform(omat);

		m_grid->setTransform(linearTransform);
	}

	void GridBase::clip_index(int* min, int* max)
	{
		openvdb::CoordBBox bb(min[0], min[1], min[2], max[0], max[1], max[2]);
		m_grid->clip(bb);
	}

	void GridBase::clip_world(double* min, double* max)
	{
		openvdb::BBoxd bb(openvdb::Vec3d(min[0], min[1], min[2]), openvdb::Vec3d(max[0], max[1], max[2]));
		m_grid->clipGrid(bb);
	}

	void GridBase::prune(float tolerance)
	{
		m_grid->pruneGrid(tolerance);
	}

	int GridBase::get_grid_class()
	{
		return (int)m_grid->getGridClass();
	}

	void GridBase::set_grid_class(int c)
	{
		m_grid->setGridClass((openvdb::GridClass)c);
	}

	std::string GridBase::get_type()
	{
		return m_grid->type();
	}
#pragma endregion Generic





}
#endif