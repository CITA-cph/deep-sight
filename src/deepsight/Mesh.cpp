#include "Mesh.h"

namespace DeepSight
{
	QuadMesh::QuadMesh()
	{
		vertices = new std::vector<Eigen::Vector3f>();
		faces = new std::vector<Eigen::Vector4i>();
	}

	QuadMesh::~QuadMesh()
	{
		delete vertices;
		delete faces;
	}
	
	QuadMesh* QuadMesh_Create()
	{
		return new QuadMesh();
	}
	void QuadMesh_Delete(QuadMesh* ptr)
	{
		delete ptr;
	}
	int QuadMesh_num_vertices(QuadMesh* ptr)
	{
		return ptr->vertices->size();
	}

	int QuadMesh_num_faces(QuadMesh* ptr)
	{
		return ptr->faces->size();
	}
	void QuadMesh_get_vertices(QuadMesh* ptr, float* data)
	{
		std::copy((float*)ptr->vertices->data(), (float*)ptr->vertices->data() + ptr->vertices->size() * 3, data);
	}
	void QuadMesh_get_faces(QuadMesh* ptr, int* data)
	{
		std::copy((int*)ptr->faces->data(), (int*)ptr->faces->data() + ptr->faces->size() * 4, data);
	}
}