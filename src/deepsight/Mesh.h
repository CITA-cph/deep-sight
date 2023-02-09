#pragma once
#include <Eigen/Core>
#include <vector>

namespace DeepSight
{
	class QuadMesh
	{
	public:
		QuadMesh();
		~QuadMesh();

		std::vector<Eigen::Vector3f>* vertices;
		std::vector<Eigen::Vector4i>* faces;
	};

	class Mesh
	{
	public:
		Mesh();
		~Mesh();

		std::vector<Eigen::Vector3f>* vertices;
		std::vector<Eigen::Vector4i>* quads;
		std::vector<Eigen::Vector3i>* tris;
	};

#ifdef __cplusplus
	extern "C" {
#endif
		DEEPSIGHT_EXPORT QuadMesh* QuadMesh_Create();
		DEEPSIGHT_EXPORT void QuadMesh_Delete(QuadMesh* ptr);
		DEEPSIGHT_EXPORT int QuadMesh_num_vertices(QuadMesh* ptr);
		DEEPSIGHT_EXPORT int QuadMesh_num_faces(QuadMesh* ptr);
		DEEPSIGHT_EXPORT void QuadMesh_get_vertices(QuadMesh* ptr, float* data);
		DEEPSIGHT_EXPORT void QuadMesh_get_faces(QuadMesh* ptr, int* data);

		DEEPSIGHT_EXPORT Mesh* Mesh_Create();
		DEEPSIGHT_EXPORT void Mesh_Delete(Mesh* ptr);
		DEEPSIGHT_EXPORT int Mesh_num_vertices(Mesh* ptr);
		DEEPSIGHT_EXPORT int Mesh_num_quads(Mesh* ptr);
		DEEPSIGHT_EXPORT int Mesh_num_tris(Mesh* ptr);
		DEEPSIGHT_EXPORT void Mesh_get_vertices(Mesh* ptr, float* data);
		DEEPSIGHT_EXPORT void Mesh_get_quads(Mesh* ptr, int* data);
		DEEPSIGHT_EXPORT void Mesh_get_tris(Mesh* ptr, int* data);
		DEEPSIGHT_EXPORT void Mesh_add_vertex(Mesh* ptr, float* data);
		DEEPSIGHT_EXPORT void Mesh_add_tri(Mesh* ptr, int* data);
		DEEPSIGHT_EXPORT void Mesh_add_quad(Mesh* ptr, int* data);

#ifdef __cplusplus
	}
#endif
}

