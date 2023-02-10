#include "ToolsAPI.h"

namespace DeepSight
{
	GridBase* FloatGrid_Resample (GridBase* ptr, float scale) { return resample<openvdb::FloatGrid>(ptr, scale); }
	GridBase* DoubleGrid_Resample(GridBase* ptr, float scale) { return resample<openvdb::DoubleGrid>(ptr, scale); }
	GridBase* Int32Grid_Resample (GridBase* ptr, float scale) { return resample<openvdb::Int32Grid>(ptr, scale); }

	void FloatGrid_Filter(GridBase* ptr, int width, int iterations, int type) { filter<openvdb::FloatGrid>(ptr, width, iterations, type); }
	void DoubleGrid_Filter(GridBase* ptr, int width, int iterations, int type) { filter<openvdb::DoubleGrid>(ptr, width, iterations, type); }
	void Int32Grid_Filter(GridBase* ptr, int width, int iterations, int type) { filter<openvdb::Int32Grid>(ptr, width, iterations, type); }

	void FloatGrid_SdfToFog(GridBase* ptr, float cutoffDistance) { sdf_to_fog<openvdb::FloatGrid>(ptr, cutoffDistance); }
	void DoubleGrid_SdfToFog(GridBase* ptr, float cutoffDistance) { sdf_to_fog<openvdb::DoubleGrid>(ptr, cutoffDistance); }
	void Int32Grid_SdfToFog(GridBase* ptr, float cutoffDistance) { sdf_to_fog<openvdb::Int32Grid>(ptr, cutoffDistance); }

}
