#ifndef TOOLS_API_H
#define TOOLS_API_H

#include "Tools.h"
#include "GridBase.h"

namespace DeepSight
{

#ifdef __cplusplus
	extern "C" {
#endif
		DEEPSIGHT_EXPORT GridBase* FloatGrid_Resample(GridBase* ptr, float scale);
		DEEPSIGHT_EXPORT GridBase* DoubleGrid_Resample(GridBase* ptr, float scale);
		DEEPSIGHT_EXPORT GridBase* Int32Grid_Resample(GridBase* ptr, float scale);

		DEEPSIGHT_EXPORT void FloatGrid_Filter(GridBase* ptr, int width, int iterations, int type);
		DEEPSIGHT_EXPORT void DoubleGrid_Filter(GridBase* ptr, int width, int iterations, int type);
		DEEPSIGHT_EXPORT void Int32Grid_Filter(GridBase* ptr, int width, int iterations, int type);

		DEEPSIGHT_EXPORT void FloatGrid_SdfToFog(GridBase* ptr, float cutoffDistance);
		DEEPSIGHT_EXPORT void DoubleGrid_SdfToFog(GridBase* ptr, float cutoffDistance);
		DEEPSIGHT_EXPORT void Int32Grid_SdfToFog(GridBase* ptr, float cutoffDistance);

#ifdef __cplusplus
	}
#endif

};

#endif
