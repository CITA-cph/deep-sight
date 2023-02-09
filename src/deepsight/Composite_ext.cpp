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

	template < typename GridOrTreeT>
	void compIfZero(GridOrTreeT& aTree, GridOrTreeT& bTree)
	{
		using Adapter = openvdb::TreeAdapter<GridOrTreeT>;
		using TreeT = typename Adapter::TreeType;
		struct Local
		{
			static inline void op(openvdb::CombineArgs<typename TreeT::ValueType>& args) {
				args.setResult(args.a() > openvdb::zeroVal<TreeT::ValueType>() ? args.a() : args.b());
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

#ifdef TEMPLATED
	//void compDiff(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree);
	void compDiff(openvdb::Int32Grid& aTree, openvdb::Int32Grid& bTree);
	void compDiff(openvdb::DoubleGrid& aTree, openvdb::DoubleGrid& bTree);
	void compDiff(openvdb::Vec3fGrid& aTree, openvdb::Vec3fGrid& bTree);
#endif
}



