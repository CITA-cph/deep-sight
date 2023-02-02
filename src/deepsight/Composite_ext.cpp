#include "Composite_ext.h"

namespace DeepSight
{
#ifdef TEMPLATED
	template < typename GridOrTreeT>
	void compDiff(GridOrTreeT& aTree, GridOrTreeT& bTree)
	{
		using Adapter = openvdb::TreeAdapter<GridOrTreeT>;
		using TreeT = typename Adapter::TreeType;
		struct Local
		{
			static inline void op(openvdb::CombineArgs<typename TreeT::ValueType>& args) {
				args.setResult(args.a() - args.b());
			}

		};

		Adapter::tree(aTree).combineExtended(Adapter::tree(bTree), Local::op, /*prune=*/false);
	}
#else
	void compDiff(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree)
	{
		using Adapter = openvdb::TreeAdapter<openvdb::FloatGrid>;
		using TreeT = typename Adapter::TreeType;
		struct Local
		{
			static inline void op(openvdb::CombineArgs<typename TreeT::ValueType>& args) {
				args.setResult(args.a() - args.b());
			}

		};

		Adapter::tree(aTree).combineExtended(Adapter::tree(bTree), Local::op, /*prune=*/false);
	}

	void compIfZero(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree)
	{
		using Adapter = openvdb::TreeAdapter<openvdb::FloatGrid>;
		using TreeT = typename Adapter::TreeType;
		struct Local
		{
			static inline void op(openvdb::CombineArgs<typename TreeT::ValueType>& args) {
				args.setResult(args.a() > openvdb::zeroVal<TreeT::ValueType>() ? args.a() : args.b());
			}

		};

		Adapter::tree(aTree).combineExtended(Adapter::tree(bTree), Local::op, /*prune=*/false);
	}
#endif
}

#if TEMPLATED
void DeepSight::compNegSum(openvdb::Grid<float>& aTree, openvdb::Grid<float>& bTree);
void DeepSight::compNegSum(openvdb::Grid<int>& aTree, openvdb::Grid<int>& bTree);
void DeepSight::compNegSum(openvdb::Grid<double>& aTree, openvdb::Grid<double>& bTree);
void DeepSight::compNegSum(openvdb::Grid<openvdb::Vec3f>& aTree, openvdb::Grid<openvdb::Vec3f>& bTree);
#endif

