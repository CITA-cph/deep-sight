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

#ifdef __cplusplus
	extern "C" {
#endif
		RAWLAM_EXPORT QuadMesh* QuadMesh_Create();
		RAWLAM_EXPORT void QuadMesh_Delete(QuadMesh* ptr);
		RAWLAM_EXPORT int QuadMesh_num_vertices(QuadMesh* ptr);
		RAWLAM_EXPORT int QuadMesh_num_faces(QuadMesh* ptr);
		RAWLAM_EXPORT void QuadMesh_get_vertices(QuadMesh* ptr, float* data);
		RAWLAM_EXPORT void QuadMesh_get_faces(QuadMesh* ptr, int* data);
		
#ifdef __cplusplus
	}
#endif
}

