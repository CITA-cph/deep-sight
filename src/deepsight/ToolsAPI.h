#ifndef TOOLS_API_H
#define TOOLS_API_H

#include "Tools.h"
#include "ApiGuard.h"
#include "GridBase.h"

namespace DeepSight
{

#ifdef __cplusplus
	extern "C" {
#endif
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL FloatGrid_Resample(GridBase* ptr, float scale);
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL DoubleGrid_Resample(GridBase* ptr, float scale);
		DEEPSIGHT_EXPORT GridBase* DEEPSIGHT_CALL Int32Grid_Resample(GridBase* ptr, float scale);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_Filter(GridBase* ptr, int width, int iterations, int type);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_Filter(GridBase* ptr, int width, int iterations, int type);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_Filter(GridBase* ptr, int width, int iterations, int type);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_SdfToFog(GridBase* ptr, float cutoffDistance);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_SdfToFog(GridBase* ptr, float cutoffDistance);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_SdfToFog(GridBase* ptr, float cutoffDistance);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_Erode(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_Erode(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_Erode(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_Erode(GridBase* ptr, int iterations);

		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL FloatGrid_Dilate(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL DoubleGrid_Dilate(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Int32Grid_Dilate(GridBase* ptr, int iterations);
		DEEPSIGHT_EXPORT void DEEPSIGHT_CALL Vec3fGrid_Dilate(GridBase* ptr, int iterations);

#ifdef __cplusplus
	}
#endif

};

#endif
