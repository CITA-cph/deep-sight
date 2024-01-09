#include "Tools.h"

namespace DeepSight
{
#pragma region Filter_Tools

	template<typename GridT>
	void filter(GridBase* grid, int width, int iterations, int type)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);

		openvdb::tools::Filter<GridT> tool(*source);
		switch (type)
		{
		case(1):
			tool.mean(width, iterations);
			break;
		case(2):
			tool.median(width, iterations);
			break;
		default:
			tool.gaussian(width, iterations);
		}
	}

	template<typename GridT>
	GridBase* resample(GridBase* grid, float scale)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);
		typename GridT::Ptr target = openvdb::gridPtrCast<GridT>(source->deepCopyGrid());
		target->clear();

		target->setTransform(
			openvdb::math::Transform::createLinearTransform(scale));

		const openvdb::math::Transform
			& sourceXform = grid->m_grid->transform(),
			& targetXform = target->transform();

		openvdb::Mat4R xform =
			sourceXform.baseMap()->getAffineMap()->getMat4() *
			targetXform.baseMap()->getAffineMap()->getMat4().inverse();

		openvdb::tools::GridTransformer transformer(xform);

		// Resample using triquadratic interpolation.
		//transformer.transformGrid<openvdb::tools::QuadraticSampler, GridType>(
		//	*m_grid, *target);

		//target->tree().prune();

		openvdb::tools::resampleToMatch<openvdb::tools::QuadraticSampler>(*source, *target);

		GridBase* new_grid = new GridBase();
		new_grid->m_grid = target;

		return new_grid;
	}

	template<typename GridT>
	GridBase* mean_curvature(GridBase* grid)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);
		GridBase* new_grid = new GridBase();
		new_grid->m_grid = openvdb::tools::meanCurvature(*source);
	}

	template<typename GridT>
	void erode(GridBase* grid, int iterations)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);
		openvdb::tools::erodeActiveValues(source->tree(), iterations, openvdb::tools::NearestNeighbors::NN_FACE_EDGE_VERTEX);
	}

	template<typename GridT>
	void dilate(GridBase* grid, int iterations)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);
		openvdb::tools::dilateActiveValues(source->tree(), iterations, openvdb::tools::NearestNeighbors::NN_FACE_EDGE_VERTEX);
	}

	template<typename GridT>
	void gradient(GridBase* grid)
	{
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);
		//openvdb::tools::

	}

#pragma endregion Filter_Tools

#pragma region Conversion_Tools

	template<typename GridT>
	void volume_to_mesh(GridBase* grid, float isovalue, std::vector<Eigen::Vector3f>& verts, std::vector<Eigen::Vector4i>& quads, std::vector<Eigen::Vector3i>& tris)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);
		typename GridT::Ptr source = openvdb::gridPtrCast<GridT>(grid->m_grid);

		mesher(*source);

		openvdb::Coord ijk;

		for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
		{
			const openvdb::Vec3s& vert = mesher.pointList()[i];
			verts.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
		}

		// Copy primitives
		openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();

		for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i) 
		{
			const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
			for (size_t j = 0, I = polygons.numQuads(); j < I; ++j) 
			{
				const openvdb::Vec4I& quad = polygons.quad(j);
				quads.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
			}

			for (size_t j = 0, I = polygons.numTriangles(); j < I; ++j)
			{
				const openvdb::Vec3I& tri = polygons.triangle(j);
				tris.push_back(Eigen::Vector3i((int)tri.x(), (int)tri.y(), (int)tri.z()));
			}
		}
	}

	GridBase* volume_from_mesh(
		std::vector<openvdb::Vec3f> verts,
		std::vector<openvdb::Vec4I> faces,
		float* xform_data, float isovalue, 
		float exteriorBandWidth, float interiorBandWidth)
	{
		openvdb::tools::VolumeToMesh mesher(isovalue);

		using MeshType = openvdb::tools::QuadAndTriangleDataAdapter<openvdb::Vec3f, openvdb::Vec4I>;
		MeshType mesh(verts, faces);

		openvdb::Mat4d mat(xform_data);
		openvdb::math::Transform::Ptr xform = openvdb::math::Transform::createLinearTransform(mat);

		openvdb::FloatGrid::Ptr new_grid = openvdb::tools::meshToVolume<openvdb::FloatGrid, MeshType>
			(mesh, *xform, exteriorBandWidth, interiorBandWidth);

		GridBase* grid = new GridBase();
		grid->m_grid = new_grid->deepCopy();

		return grid;
	}

	GridBase* volume_from_points(int num_points, float* points, float radius, float voxelsize)
	{
		ParticleList plist;

		for (int i = 0; i < num_points; ++i)
		{
			plist.add(openvdb::Vec3R(points[i * 3], points[i * 3 + 1], points[i * 3 + 2]), radius);
		}

		auto grid = openvdb::createLevelSet<openvdb::FloatGrid>(voxelsize);
		grid->setTransform(openvdb::math::Transform::createLinearTransform(voxelsize));

		openvdb::tools::ParticlesToLevelSet<openvdb::FloatGrid> pgrid(*grid);

		pgrid.setGrainSize(4);
		pgrid.rasterizeSpheres(plist);
		pgrid.finalize();

		auto dgrid = new GridBase();
		dgrid->m_grid = grid;
		return dgrid;
	}
#pragma endregion Conversion_Tools
#pragma region Template_specialization

	template GridBase* resample<openvdb::FloatGrid>(GridBase* grid, float isovalue);
	template GridBase* resample<openvdb::DoubleGrid>(GridBase* grid, float isovalue);
	template GridBase* resample<openvdb::Int32Grid>(GridBase* grid, float isovalue);
	
	template void filter<openvdb::FloatGrid>(GridBase* grid, int width, int iterations, int type);
	template void filter<openvdb::DoubleGrid>(GridBase* grid, int width, int iterations, int type);
	template void filter<openvdb::Int32Grid>(GridBase* grid, int width, int iterations, int type);
	
	template void sdf_to_fog<openvdb::FloatGrid>(GridBase* grid, float cutoffDistance);
	template void sdf_to_fog<openvdb::DoubleGrid>(GridBase* grid, float cutoffDistance);
	template void sdf_to_fog<openvdb::Int32Grid>(GridBase* grid, float cutoffDistance);

	template void volume_to_mesh<openvdb::FloatGrid>(
		GridBase* grid, float isovalue,
		std::vector<Eigen::Vector3f>& verts,
		std::vector<Eigen::Vector4i>& quads,
		std::vector<Eigen::Vector3i>& tris);

	template void volume_to_mesh<openvdb::DoubleGrid>(
		GridBase* grid, float isovalue,
		std::vector<Eigen::Vector3f>& verts,
		std::vector<Eigen::Vector4i>& quads,
		std::vector<Eigen::Vector3i>& tris);

	template void volume_to_mesh<openvdb::Int32Grid>(
		GridBase* grid, float isovalue,
		std::vector<Eigen::Vector3f>& verts,
		std::vector<Eigen::Vector4i>& quads,
		std::vector<Eigen::Vector3i>& tris);

#pragma endregion Template_specialization


}