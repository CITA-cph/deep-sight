#pragma once
#pragma managed(push, off)
#include <openvdb/openvdb.h>
#include <openvdb/math/Ray.h>
#include <openvdb/math/Vec3.h>
#include <openvdb/tools/RayIntersector.h>
#include <openvdb/tools/RayTracer.h>
#pragma managed(pop)

#include "AdditiveRender.h"

#include <msclr\marshal_cppstd.h>

using System::IntPtr;
using System::Runtime::InteropServices::Marshal;

namespace DeepSightCommon {


	public ref class VoxelRendererSettings
	{
	public:

		VoxelRendererSettings()
		{
			tx = 0, ty = 0, tz = 0;
			rx = 0, ry = 0, rz = 0;
			aperture = 41.2136, focal = 50.0;
			znear = 1.0e-3, zfar = 1e20;
			scatterX = scatterY = scatterZ = 1.5;
			absorptionX = absorptionY = absorptionZ = 0.1;
			gain = 0.2, cutoff = 0.005;
			lightX = 1.0, lightY = 0, lightZ = 0;
			lightColorX = lightColorY = lightColorZ = 1.0;
			primaryStep = 1.0, shadowStep = 3.0;
			clipValueMin = 0.0, clipValueMax = std::numeric_limits<double>().max();

		}

		double tx, ty, tz;
		double rx, ry, rz;
		double aperture, focal;
		double znear, zfar;
		double scatterX, scatterY, scatterZ;
		double absorptionX, absorptionY, absorptionZ;
		double gain, cutoff;
		double lightX, lightY, lightZ;
		double lightColorX, lightColorY, lightColorZ;
		double primaryStep, shadowStep;

		double clipValueMin, clipValueMax;
	};

	public ref class VoxelRenderer
	{
	public:
		array<float>^ TestRender(System::String^ filepath, int width, int height, VoxelRendererSettings^ opts);
	};
}
