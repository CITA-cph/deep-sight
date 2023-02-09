#include "openvdb/openvdb.h"

#include <openvdb/Platform.h>
#ifndef COMPOSITE_EXT_H
#define COMPOSITE_EXT_H

#include <openvdb/Exceptions.h>
#include <openvdb/Types.h>
#include <openvdb/Grid.h>
#include <openvdb/math/Math.h> // for isExactlyEqual()
#include <openvdb/openvdb.h>


namespace DeepSight
{
#define NOT_TEMPLATED
#ifdef TEMPLATED
	template <typename GridOrTreeT>
	void compDiff(GridOrTreeT& aTree, GridOrTreeT& bTree);

	template <typename GridOrTreeT>
	void compIfZero(GridOrTreeT& aTree, GridOrTreeT& bTree);

#else
	void compDiff(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree);
	void compIfZero(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree);

#endif
}

#endif