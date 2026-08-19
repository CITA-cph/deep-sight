#include "ToolsAPI.h"
#include "ApiGuard.h"

// Every export here forwards into OpenVDB, all of which can throw. Without the
// api_guard wrapper those exceptions unwound out of an extern "C" frame into
// managed code, which terminates the host process. See ApiGuard.h.

namespace DeepSight
{
	namespace
	{
		GridBase* require_grid(GridBase* p)
		{
			if (p == nullptr) throw std::invalid_argument("Null grid handle.");
			return p;
		}
	}

	extern "C"
	{
		GridBase* DEEPSIGHT_CALL FloatGrid_Resample(GridBase* ptr, float scale)
		{
			return api_guard([&] { return resample<openvdb::FloatGrid>(require_grid(ptr), scale); },
				static_cast<GridBase*>(nullptr));
		}

		GridBase* DEEPSIGHT_CALL DoubleGrid_Resample(GridBase* ptr, float scale)
		{
			return api_guard([&] { return resample<openvdb::DoubleGrid>(require_grid(ptr), scale); },
				static_cast<GridBase*>(nullptr));
		}

		GridBase* DEEPSIGHT_CALL Int32Grid_Resample(GridBase* ptr, float scale)
		{
			return api_guard([&] { return resample<openvdb::Int32Grid>(require_grid(ptr), scale); },
				static_cast<GridBase*>(nullptr));
		}


		void DEEPSIGHT_CALL FloatGrid_Filter(GridBase* ptr, int width, int iterations, int type)
		{
			api_guard([&] { filter<openvdb::FloatGrid>(require_grid(ptr), width, iterations, type); });
		}

		void DEEPSIGHT_CALL DoubleGrid_Filter(GridBase* ptr, int width, int iterations, int type)
		{
			api_guard([&] { filter<openvdb::DoubleGrid>(require_grid(ptr), width, iterations, type); });
		}

		void DEEPSIGHT_CALL Int32Grid_Filter(GridBase* ptr, int width, int iterations, int type)
		{
			api_guard([&] { filter<openvdb::Int32Grid>(require_grid(ptr), width, iterations, type); });
		}

		void DEEPSIGHT_CALL FloatGrid_SdfToFog(GridBase* ptr, float cutoffDistance)
		{
			api_guard([&] { sdf_to_fog<openvdb::FloatGrid>(require_grid(ptr), cutoffDistance); });
		}

		void DEEPSIGHT_CALL DoubleGrid_SdfToFog(GridBase* ptr, float cutoffDistance)
		{
			api_guard([&] { sdf_to_fog<openvdb::DoubleGrid>(require_grid(ptr), cutoffDistance); });
		}

		void DEEPSIGHT_CALL Int32Grid_SdfToFog(GridBase* ptr, float cutoffDistance)
		{
			api_guard([&] { sdf_to_fog<openvdb::Int32Grid>(require_grid(ptr), cutoffDistance); });
		}

		void DEEPSIGHT_CALL FloatGrid_Erode(GridBase* ptr, int iterations)
		{
			api_guard([&] { erode<openvdb::FloatGrid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL DoubleGrid_Erode(GridBase* ptr, int iterations)
		{
			api_guard([&] { erode<openvdb::DoubleGrid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL Int32Grid_Erode(GridBase* ptr, int iterations)
		{
			api_guard([&] { erode<openvdb::Int32Grid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL Vec3fGrid_Erode(GridBase* ptr, int iterations)
		{
			api_guard([&] { erode<openvdb::Vec3fGrid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL FloatGrid_Dilate(GridBase* ptr, int iterations)
		{
			api_guard([&] { dilate<openvdb::FloatGrid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL DoubleGrid_Dilate(GridBase* ptr, int iterations)
		{
			api_guard([&] { dilate<openvdb::DoubleGrid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL Int32Grid_Dilate(GridBase* ptr, int iterations)
		{
			api_guard([&] { dilate<openvdb::Int32Grid>(require_grid(ptr), iterations); });
		}

		void DEEPSIGHT_CALL Vec3fGrid_Dilate(GridBase* ptr, int iterations)
		{
			api_guard([&] { dilate<openvdb::Vec3fGrid>(require_grid(ptr), iterations); });
		}

	}
}
