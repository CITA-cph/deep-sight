#include "pch.h"

#include "DeepSightCommon.h"

using System::IntPtr;
using System::Runtime::InteropServices::Marshal;

namespace DeepSightCommon
{
	array<float>^ VoxelRenderer::TestRender(System::String^ filepath, int width, int height, VoxelRendererSettings^ opts)
	{
		openvdb::initialize();

		std::string filepathNative = msclr::interop::marshal_as<std::string>(filepath);

		openvdb::io::File file(filepathNative);

		file.open();

		std::vector<openvdb::GridBase::Ptr> grids;

		//auto grid_ptr_vec = file.getGrids();

		//for(auto iter=grid_ptr_vec->begin(); iter != grid_ptr_vec->end(); ++iter)

		for (openvdb::io::File::NameIterator nameIter = file.beginName();
			nameIter != file.endName(); ++nameIter)
		{
			//std::cout << "Reading " << (*iter)->type() << std::endl;
			//grid->m_grid = openvdb::GridBase::Ptr( * iter);
			//std::cout << "Grid set: " << grid->m_grid->type() << std::endl;
			//std::cout << "Grid ptr: " << grid << std::endl;
			grids.push_back(file.readGrid(nameIter.gridName()));
		}

		file.close();

		if (grids.size() < 1) return nullptr;

		openvdb::FloatGrid::Ptr fgrid = openvdb::gridPtrCast<openvdb::FloatGrid>(grids[0]);

		openvdb::tools::Film film(width, height);

		std::unique_ptr<openvdb::tools::BaseCamera> camera;
		camera.reset(new openvdb::tools::PerspectiveCamera(
			film,
			openvdb::math::Vec3d(opts->rx, opts->ry, opts->rz),
			openvdb::math::Vec3d(opts->tx, opts->ty, opts->tz),
			opts->focal, opts->aperture, opts->znear, opts->zfar));

		std::unique_ptr<openvdb::tools::BaseShader> shader;
		//shader.reset(new openvdb::tools::MatteShader<>());
		shader.reset(new openvdb::tools::DiffuseShader<>());


		using IntersectorType = openvdb::tools::VolumeRayIntersector<openvdb::FloatGrid>;
		IntersectorType intersector(*fgrid);

		openvdb::tools::VolumeRender<IntersectorType> renderer(intersector, *camera);

		renderer.setLightDir(opts->lightX, opts->lightY, opts->lightZ);
		renderer.setLightColor(opts->lightColorX, opts->lightColorY, opts->lightColorZ);
		renderer.setPrimaryStep(opts->primaryStep);
		renderer.setShadowStep(opts->shadowStep);
		renderer.setScattering(opts->scatterX, opts->scatterY, opts->scatterZ);
		renderer.setAbsorption(opts->absorptionX, opts->absorptionY, opts->absorptionZ);
		renderer.setLightGain(opts->gain);
		renderer.setCutOff(opts->cutoff);

		AdditiveRender<IntersectorType> renderer2(intersector, *camera);

		renderer2.set_clip_min(opts->clipValueMin);
		renderer2.set_clip_max(opts->clipValueMax);

		renderer2.render(true);

		//renderer.render(true);

		array<float>^ buffer = gcnew array<float>(width * height * 4);

		Marshal::Copy(IntPtr((float*)film.pixels()), buffer, 0, width * height * 4);

		openvdb::uninitialize();

		return buffer;

		/*

		openvdb::tools::VolumeRayIntersector<openvdb::FloatGrid> inter(*fgrid);

		openvdb::math::Ray ray;

		bool hit = inter.setWorldRay(ray);

		if (!hit)
		{
			return;
		}

		// Now you can begin the ray-marching using consecutive calls to VolumeRayIntersector::march
		double t0 = 0, t1 = 0;// note the entry and exit times are with respect to the INDEX ray
		while (inter.march(t0, t1))
		{
			// perform line-integration between t0 and t1
		}
		*/
	}
}