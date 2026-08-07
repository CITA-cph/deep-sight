#ifndef COMBINE_API_H
#define COMBINE_API_H
#include <openvdb/tools/Composite.h>
#include "Composite_ext.h"

#include "GridBase.h"

namespace DeepSight
{
#ifdef __cplusplus
	extern "C" {
#endif
	DEEPSIGHT_EXPORT void FloatGrid_combine(GridBase* ptr0, GridBase* ptr1, int type);
	DEEPSIGHT_EXPORT void Scalar_FloatGrid_combine(GridBase* ptr0, float n, int type);
	DEEPSIGHT_EXPORT void Vec3fGrid_combine(GridBase* ptr0, GridBase* ptr1, int type);

#ifdef __cplusplus
	}
#endif
};

#endif