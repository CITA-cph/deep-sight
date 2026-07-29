#include "CombineAPI.h"
#include "ApiGuard.h"

#include <stdexcept>

namespace DeepSight
{
	namespace
	{
		// Previously these functions called openvdb::gridPtrCast and, on a type
		// mismatch, silently returned -- the combine appeared to succeed and
		// left the grid untouched. They now throw, and api_guard turns that
		// into a retrievable error instead of a crash.
		GridBase* require_grid(GridBase* p)
		{
			if (p == nullptr) throw std::invalid_argument("Null grid handle.");
			return p;
		}
	}

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

	enum ScalarCombineType
	{
		SCSUM = 0,
		SCDIFF = 1,
		SCMUL = 2,
		SCDIV = 3,
		SCPOW = 4,
		SCMIN = 5,
		SCMAX = 6,
		SCLT = 7,
		SCGT = 8,
		SCEQ = 9,
	};

	void DEEPSIGHT_CALL FloatGrid_combine(GridBase* ptr0, GridBase* ptr1, int type)
	{
		api_guard([&] {
			openvdb::FloatGrid::Ptr grid0 = require_grid(ptr0)->grid_as<openvdb::FloatGrid>();
			openvdb::FloatGrid::Ptr grid1 = require_grid(ptr1)->grid_as<openvdb::FloatGrid>();


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
		});
	}

	void DEEPSIGHT_CALL Scalar_FloatGrid_combine(GridBase* ptr0, float n, int type)
	{
		api_guard([&] {
			openvdb::FloatGrid::Ptr grid0 = require_grid(ptr0)->grid_as<openvdb::FloatGrid>();


			struct SCSUM
			{
				float n;
				SCSUM(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter + n);
				}
			};
			struct SCDIFF
			{
				float n;
				SCDIFF(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter - n);
				}
			};
			struct SCMUL
			{
				float n;
				SCMUL(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter * n);
				}
			};
			struct SCDIV
			{
				float n;
				SCDIV(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter / n);
				}
			};
			struct SCPOW
			{
				float n;
				SCPOW(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(std::pow(*iter, n));
				}
			}; 
			struct SCMIN
			{
				float n;
				SCMIN(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(std::min(*iter, n));
				}
			};
			struct SCMAX
			{
				float n;
				SCMAX(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(std::max(*iter, n));
				}
			};
			struct SCLT
			{
				float n;
				SCLT(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter < n ? 1.0 : 0.0);
				}
			};
			struct SCGT
			{
				float n;
				SCGT(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter > n ? 1.0 : 0.0);
				}
			};
			struct SCEQ
			{
				float n;
				SCEQ(float x) : n(x) {}
				inline void operator()(const openvdb::FloatGrid::ValueOnIter& iter) const {
					iter.setValue(*iter == n ? 1.0 : 0.0);
				}
			};

			switch (type)
			{
				case(ScalarCombineType::SCSUM):
					openvdb::tools::foreach(grid0->beginValueOn(), SCSUM(n));
					break;
				case(ScalarCombineType::SCDIFF):
					openvdb::tools::foreach(grid0->beginValueOn(), SCDIFF(n));
					break;
				case(ScalarCombineType::SCMUL):
					openvdb::tools::foreach(grid0->beginValueOn(), SCMUL(n));
					break;
				case(ScalarCombineType::SCDIV):
					openvdb::tools::foreach(grid0->beginValueOn(), SCDIV(n));
					break;
				case(ScalarCombineType::SCPOW):
					openvdb::tools::foreach(grid0->beginValueOn(), SCPOW(n));
					break;
				case(ScalarCombineType::SCMIN):
					openvdb::tools::foreach(grid0->beginValueOn(), SCMIN(n));
					break;
				case(ScalarCombineType::SCMAX):
					openvdb::tools::foreach(grid0->beginValueOn(), SCMAX(n));
					break;
				case(ScalarCombineType::SCLT):
					openvdb::tools::foreach(grid0->beginValueOn(), SCLT(n));
					break;
				case(ScalarCombineType::SCGT):
					openvdb::tools::foreach(grid0->beginValueOn(), SCGT(n));
					break;
				case(ScalarCombineType::SCEQ):
					openvdb::tools::foreach(grid0->beginValueOn(), SCEQ(n));
					break;
				default:
					break;
			}

		});
	}

	void DEEPSIGHT_CALL Vec3fGrid_combine(GridBase* ptr0, GridBase* ptr1, int type)
	{
		api_guard([&] {
			openvdb::Vec3fGrid::Ptr grid0 = require_grid(ptr0)->grid_as<openvdb::Vec3fGrid>();
			openvdb::Vec3fGrid::Ptr grid1 = require_grid(ptr1)->grid_as<openvdb::Vec3fGrid>();


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
				//DeepSight::compDiff(*grid0, *grid1);
				break;
			case(CombineType::IFZERO):
				//DeepSight::compIfZero(*grid0, *grid1);
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
		});
	}
}
