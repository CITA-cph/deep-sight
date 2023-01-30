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
#ifdef TEMPLATED
	template < typename GridOrTreeT>
	void compDiff(GridOrTreeT& aTree, GridOrTreeT& bTree);
#else
	void compDiff(openvdb::FloatGrid& aTree, openvdb::FloatGrid& bTree);
#endif
}

#endif