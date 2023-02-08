#define GRIDBASE
#ifdef GRIDBASE
#include "GridBase.h"

namespace DeepSight
{
	GridBase::GridBase()
	{
		//openvdb::initialize();
	}
	
	template<typename GridT>
	void GridBase::initialize()
	{
		//m_grid = GridT::create(openvdb::zeroVal<GridT::ValueType>());
		//m_grid = GridT::create();
	}

	
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

	template<typename GridT, typename T>
	T GridBase::get_value(double x, double y, double z)
	{
		typename GridT::Ptr grid = openvdb::gridPtrCast<GridT>(m_grid);
		openvdb::tools::GridSampler<GridT, openvdb::tools::BoxSampler> sampler(*grid);
		T worldValue = (T)sampler.wsSample(openvdb::Vec3R(x, y, z));

		return worldValue;
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

	void GridBase::get_grid_class(int c)
	{
		m_grid->setGridClass((openvdb::GridClass)c);
	}

	
	/* ################# API FUNCTIONS ################# */

	GridBase* GridBase_CreateFloat()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::FloatGrid>();
		return grid;
	}

	GridBase* GridBase_CreateDouble() 
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::DoubleGrid>();
		return grid;
	}

	GridBase* GridBase_CreateInt()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::Int32Grid>();
		return grid;
	}

	float GridBase_GetValueFloat(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value < openvdb::FloatGrid, openvdb::FloatGrid::ValueType > (x, y, z);
	}

	double GridBase_GetValueDouble(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value < openvdb::DoubleGrid, openvdb::DoubleGrid::ValueType >(x, y, z);
	}

	int GridBase_GetValueInt32(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value < openvdb::Int32Grid, openvdb::Int32Grid::ValueType >(x, y, z);
	}
	
	void GridBase_SetName(GridBase* ptr, const char* name) { ptr->set_name(name); }

	const char * GridBase_GetName(GridBase* ptr) 
	{ 
		ULONG ulSize = strlen(ptr->get_name().c_str()) + sizeof(char);
		char* pszReturn = NULL; 
		pszReturn = (char*)::CoTaskMemAlloc(ulSize);
		if (pszReturn != nullptr)
			strcpy_s(pszReturn, ulSize, ptr->get_name().c_str());
		return pszReturn;
	}

	void GridBase_ClipIndex(GridBase* ptr, int* min, int* max)
	{
		ptr->clip_index(min, max);
	}

	void GridBase_ClipWorld(GridBase* ptr, double* min, double* max)
	{
		ptr->clip_world(min, max);
	}

	void GridBase_Prune(GridBase* ptr, float tolerance)
	{
		ptr->prune(tolerance);
	}

	int GridBase_GetGridClass(GridBase* ptr)
	{
		return ptr->get_grid_class();
	}

	void GridBase_SetGridClass(GridBase* ptr, int c)
	{
		ptr->set_grid_class(c);
	}

}
#endif