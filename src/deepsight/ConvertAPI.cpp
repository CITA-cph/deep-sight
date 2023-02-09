#include "ConvertAPI.h"
namespace DeepSight
{
	
	Mesh* FloatGrid_ToMesh(GridBase* ptr, float isovalue)
	{
		auto mesh = new Mesh();
		volume_to_mesh<openvdb::FloatGrid>(ptr, isovalue, *mesh->vertices, *mesh->quads, *mesh->tris);
		return mesh;
	}

	GridBase* FloatGrid_FromMesh(Mesh* mesh, float* xform, float isovalue, float exteriorBandWidth, float interiorBandWidth)
	{
		std::vector<openvdb::Vec3f> verts;
		std::vector<openvdb::Vec4I> faces;
		
		for (int i = 0; i < mesh->quads->size(); ++i)
		{
			auto face = (*mesh->quads)[i];
			faces.push_back(openvdb::Vec4I(face.x(), face.y(), face.z(), face.w()));
		}

		for (int i = 0; i < mesh->tris->size(); ++i)
		{
			auto face = (*mesh->tris)[i];
			faces.push_back(openvdb::Vec4I(face.x(), face.y(), face.z(), openvdb::util::INVALID_IDX));
		}

		for (int i = 0; i < mesh->vertices->size(); ++i)
		{
			auto vert = (*mesh->vertices)[i];
			verts.push_back(openvdb::Vec3f(vert.x(), vert.y(), vert.z()));
		}

		return volume_from_mesh(
			verts, faces,
			xform, isovalue, exteriorBandWidth, interiorBandWidth);
	}

	GridBase* FloatGrid_FromPoints(int num_points, float* point_data, float radius, float voxelsize)
	{
		return volume_from_points(num_points, point_data, radius, voxelsize);
	}

	Mesh* DoubleGrid_ToMesh(GridBase* ptr, float isovalue)
	{
		auto mesh = new Mesh();
		volume_to_mesh<openvdb::DoubleGrid>(ptr, isovalue, *mesh->vertices, *mesh->quads, *mesh->tris);
		return mesh;
	}

	Mesh* Int32Grid_ToMesh(GridBase* ptr, float isovalue)
	{
		auto mesh = new Mesh();
		volume_to_mesh<openvdb::Int32Grid>(ptr, isovalue, *mesh->vertices, *mesh->quads, *mesh->tris);
		return mesh;
	}
}