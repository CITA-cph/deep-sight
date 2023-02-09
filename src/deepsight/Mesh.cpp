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

	Mesh::Mesh()
	{
		vertices = new std::vector<Eigen::Vector3f>();
		quads = new std::vector<Eigen::Vector4i>();
		tris = new std::vector<Eigen::Vector3i>();
	}

	Mesh::~Mesh()
	{
		delete vertices;
		delete quads;
		delete tris;
	}

	Mesh* Mesh_Create()
	{
		return new Mesh();
	}

	void Mesh_Delete(Mesh* ptr)
	{
		delete ptr;
	}

	int Mesh_num_vertices(Mesh* ptr)
	{
		return ptr->vertices->size();
	}
	int Mesh_num_quads(Mesh* ptr)
	{
		return ptr->quads->size();
	}
	int Mesh_num_tris(Mesh* ptr)
	{
		return ptr->tris->size();
	}
	void Mesh_get_vertices(Mesh* ptr, float* data)
	{
		std::copy((float*)ptr->vertices->data(), (float*)ptr->vertices->data() + ptr->vertices->size() * 3, data);
	}
	void Mesh_get_quads(Mesh* ptr, int* data)
	{
		std::copy((int*)ptr->quads->data(), (int*)ptr->quads->data() + ptr->quads->size() * 4, data);
	}
	void Mesh_get_tris(Mesh* ptr, int* data)
	{
		std::copy((int*)ptr->tris->data(), (int*)ptr->tris->data() + ptr->tris->size() * 3, data);
	}

	void Mesh_add_vertex(Mesh* ptr, float* data)
	{
		ptr->vertices->push_back(Eigen::Vector3f(data));
	}

	void Mesh_add_tri(Mesh* ptr, int* data)
	{
		ptr->tris->push_back(Eigen::Vector3i(data));
	}

	void Mesh_add_quad(Mesh* ptr, int* data)
	{
		ptr->quads->push_back(Eigen::Vector4i(data));
	}
}