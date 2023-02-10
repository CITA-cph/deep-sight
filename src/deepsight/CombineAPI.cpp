#include "CombineAPI.h"

namespace DeepSight
{
	enum CombineType
	{
		MAX = 0,
		MIN = 1,
		SUM = 2,
		DIFF = 3,
		IFZERO = 4,
		MUL = 5,
		CSG_DIFFERENCE = 6,
		CSG_UNION = 7,
		CSG_INTERSECTION = 8
	};
	void FloatGrid_combine(GridBase* ptr0, GridBase* ptr1, int type)
	{
		openvdb::FloatGrid::Ptr grid0 = openvdb::gridPtrCast<openvdb::FloatGrid>(ptr0->m_grid);
		openvdb::FloatGrid::Ptr grid1 = openvdb::gridPtrCast<openvdb::FloatGrid>(ptr1->m_grid);

		if (grid0 == nullptr || grid1 == nullptr)
			return;

		switch (type)
		{
		case(CombineType::MAX):
			openvdb::tools::compMax(*grid0, *grid1);
			break;
		case(CombineType::MIN):
			openvdb::tools::compMin(*grid0, *grid1);
			break;
		case(CombineType::SUM):
			openvdb::tools::compSum(*grid0, *grid1);
			break;
		case(CombineType::DIFF):
			DeepSight::compDiff(*grid0, *grid1);
			break;
		case(CombineType::IFZERO):
			DeepSight::compIfZero(*grid0, *grid1);
			break;
		case(CombineType::MUL):
			openvdb::tools::compMul(*grid0, *grid1);
			break;
		case(CombineType::CSG_DIFFERENCE):
			openvdb::tools::csgDifference(*grid0, *grid1);
			break;
		case(CombineType::CSG_UNION):
			openvdb::tools::csgUnion(*grid0, *grid1);
			break;
		case(CombineType::CSG_INTERSECTION):
			openvdb::tools::csgIntersection(*grid0, *grid1);
			break;
		default:
			break;
		}
	}
}
