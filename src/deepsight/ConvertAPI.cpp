#include "ConvertAPI.h"
#include "ApiGuard.h"

#include <memory>
#include <stdexcept>

namespace DeepSight
{
	namespace
	{
		GridBase* require_grid(GridBase* p)
		{
			if (p == nullptr) throw std::invalid_argument("Null grid handle.");
			return p;
		}

		Mesh* require_mesh(Mesh* m)
		{
			if (m == nullptr || m->vertices == nullptr || m->quads == nullptr || m->tris == nullptr)
				throw std::invalid_argument("Null or incomplete mesh.");
			return m;
		}

		// `new Mesh()` followed by a call that can throw leaked the Mesh on
		// every failure. unique_ptr + release() keeps the success path
		// identical and cleans up on the failure path.
		template<typename GridT>
		Mesh* to_mesh(GridBase* ptr, float isovalue)
		{
			std::unique_ptr<Mesh> mesh(new Mesh());
			volume_to_mesh<GridT>(require_grid(ptr), isovalue,
				*mesh->vertices, *mesh->quads, *mesh->tris);
			return mesh.release();
		}
	}

	extern "C"
	{
		Mesh* DEEPSIGHT_CALL FloatGrid_ToMesh(GridBase* ptr, float isovalue)
		{
			return api_guard([&] { return to_mesh<openvdb::FloatGrid>(ptr, isovalue); },
				static_cast<Mesh*>(nullptr));
		}

		Mesh* DEEPSIGHT_CALL DoubleGrid_ToMesh(GridBase* ptr, float isovalue)
		{
			return api_guard([&] { return to_mesh<openvdb::DoubleGrid>(ptr, isovalue); },
				static_cast<Mesh*>(nullptr));
		}

		Mesh* DEEPSIGHT_CALL Int32Grid_ToMesh(GridBase* ptr, float isovalue)
		{
			return api_guard([&] { return to_mesh<openvdb::Int32Grid>(ptr, isovalue); },
				static_cast<Mesh*>(nullptr));
		}

		GridBase* DEEPSIGHT_CALL FloatGrid_FromMesh(Mesh* mesh, float* xform, float isovalue,
			float exteriorBandWidth, float interiorBandWidth)
		{
			return api_guard([&]() -> GridBase* {
				require_mesh(mesh);
				if (xform == nullptr) throw std::invalid_argument("Null transform.");

				std::vector<openvdb::Vec3f> verts;
				std::vector<openvdb::Vec4I> faces;

				// reserve() added: these loops used to grow the vectors one
				// push_back at a time over meshes that routinely have millions
				// of elements.
				faces.reserve(mesh->quads->size() + mesh->tris->size());
				verts.reserve(mesh->vertices->size());

				// Loop counters were `int` compared against size_t size(),
				// which both warns and wraps on meshes above 2^31 elements.
				for (std::size_t i = 0; i < mesh->quads->size(); ++i)
				{
					const auto& face = (*mesh->quads)[i];
					faces.emplace_back(face.x(), face.y(), face.z(), face.w());
				}

				for (std::size_t i = 0; i < mesh->tris->size(); ++i)
				{
					const auto& face = (*mesh->tris)[i];
					faces.emplace_back(face.x(), face.y(), face.z(), openvdb::util::INVALID_IDX);
				}

				for (std::size_t i = 0; i < mesh->vertices->size(); ++i)
				{
					const auto& vert = (*mesh->vertices)[i];
					verts.emplace_back(vert.x(), vert.y(), vert.z());
				}

				return volume_from_mesh(verts, faces, xform, isovalue,
					exteriorBandWidth, interiorBandWidth);
			}, static_cast<GridBase*>(nullptr));
		}

		GridBase* DEEPSIGHT_CALL FloatGrid_FromPoints(int num_points, float* point_data,
			float radius, float voxelsize)
		{
			return api_guard([&]() -> GridBase* {
				if (num_points < 0) throw std::invalid_argument("Negative point count.");
				if (num_points > 0 && point_data == nullptr) throw std::invalid_argument("Null point buffer.");
				if (voxelsize <= 0.0f) throw std::invalid_argument("Voxel size must be positive.");

				return volume_from_points(num_points, point_data, radius, voxelsize);
			}, static_cast<GridBase*>(nullptr));
		}
	}
}
