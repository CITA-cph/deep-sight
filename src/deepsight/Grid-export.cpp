#ifdef OBSOLETE

#include "Grid-export.h"

namespace DeepSight
{
	const char* get_version()
	{
		return VERSION;
	}

	Grid<float>* Grid_Create(float background)
	{

		return new Grid<float>(background);
	}

	Grid<float>* Grid_duplicate(Grid<float>* ptr)
	{
		return new Grid<float>(*ptr->duplicate());
	}

	void Grid_Delete(Grid<float>* ptr)
	{
		delete ptr;
	}

	Grid<float>* Grid_read(const char* filename)
	{
		//return Grid<float>::read(filename);
		return new Grid<float>(*Grid<float>::read(filename));
	}

	void Grid_write(Grid<float>* ptr, const char* filename, bool half_float)
	{
		ptr->write(filename, half_float);
	}

	SAFEARRAY* Grid_get_some_grids(const char* filename)
	{
		std::vector<std::shared_ptr<Grid<float>>> grid_vec = Grid<float>::from_vdb(filename);
		std::vector<Grid<float>*> grid_ptr_vec(grid_vec.size());
		for (int i = 0; i < grid_vec.size(); ++i)
		{
			grid_ptr_vec.push_back(new Grid<float>(*grid_vec[i]));
		}

		SAFEARRAY* psa = SafeArrayCreateVector(VT_I4, 0, grid_ptr_vec.size());
		void* data;
		SafeArrayAccessData(psa, &data);
		CopyMemory(data, grid_ptr_vec.data(), sizeof(grid_ptr_vec));
		SafeArrayUnaccessData(psa);
		return psa;
	}

	void Grid_evaluate(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3f> xyz(num_coords);
		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
		}

		auto res = ptr->get_interpolated_values(xyz, sample_type);
		std::copy(res.begin(), res.end(), results);
	}

	void Grid_get_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* results, int sample_type)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3f> xyz(num_coords);
		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
		}

		auto res = ptr->get_interpolated_values(xyz, sample_type);
		std::copy(res.begin(), res.end(), results);
	}

	void Grid_set_values_ws(Grid<float>* ptr, int num_coords, float* coords, float* values)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3i> xyz(num_coords);
		std::vector<float> valuesf(num_coords);
		//std::copy(values[0], values[num_coords - 1], valuesf);


		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3i((int)coords[i * 3], (int)coords[i * 3 + 1], (int)coords[i * 3 + 2]);
			valuesf[i] = values[i];
		}

		ptr->set_values(xyz, valuesf);
	}

	float Grid_get_value(Grid<float>* ptr, int x, int y, int z)
	{
		return ptr->get_value(Eigen::Vector3i(x, y, z));
	}

	void Grid_set_value(Grid<float>* ptr, int x, int y, int z, float v)
	{
		ptr->set_value(Eigen::Vector3i(x, y, z), v);
	}

	void Grid_filter(Grid<float>* ptr, int width, int iterations, int type)
	{
		ptr->filter(width, iterations, type);
	}

	void Grid_bounding_box(Grid<float>* ptr, int* min, int* max)
	{
		std::tuple<Eigen::Vector3i, Eigen::Vector3i> bb = ptr->bounding_box();

		Eigen::Vector3i bbmin = std::get<0>(bb);
		Eigen::Vector3i bbmax = std::get<1>(bb);

		min[0] = bbmin.x();
		min[1] = bbmin.y();
		min[2] = bbmin.z();

		max[0] = bbmax.x();
		max[1] = bbmax.y();
		max[2] = bbmax.z();
	}

	void Grid_set_transform(Grid<float>* ptr, float* mat)
	{
		Eigen::Matrix4f matf(mat);
		ptr->set_transform(matf.cast<double>());
		//ptr->set_transform(Eigen::Matrix4d(mat));
	}

	void Grid_get_transform(Grid<float>* ptr, float* mat)
	{
		Eigen::Matrix4f matf = ptr->get_transform().cast<float>();
		std::copy(matf.data(), matf.data() + matf.size(), mat);
		//Eigen::Matrix4d matd = ptr->get_transform();
		//std::copy(matd.data(), matd.data() + matd.size(), mat);
	}

	void Grid_to_mesh(Grid<float>* ptr, QuadMesh* mesh_ptr, float isovalue)
	{
		ptr->to_mesh(isovalue, *(mesh_ptr->vertices), *(mesh_ptr->faces));
	}

	Grid<float>* Grid_resample(Grid<float>* ptr, float scale)
	{
		return ptr->resample(scale);
	}

	void Grid_get_dense(Grid<float>* ptr, int* min, int* max, float* results)
	{
		std::vector<float> dense = ptr->get_dense(Eigen::Vector3i(min[0], min[1], min[2]), Eigen::Vector3i(max[0], max[1], max[2]));
		std::copy(dense.begin(), dense.end(), results);
	}

	char* Grid_get_name(Grid<float>* ptr)
	{
		ULONG ulSize = strlen(ptr->get_name().c_str()) + sizeof(char);
		char* pszReturn = NULL;

		pszReturn = (char*)::CoTaskMemAlloc(ulSize);
		strcpy_s(pszReturn, ulSize, ptr->get_name().c_str());

		return pszReturn;
	}

	void Grid_set_name(Grid<float>* ptr, const char* name)
	{
		ptr->set_name(std::string(name));
	}

	void Grid_ray_intersect(Grid<float>* ptr, float* ray)
	{
		//openvdb::tools::LevelSetRayIntersector<Grid<float>> lsr(*ptr);
		//lsr.intersectsWS(openvdb::math::Ray())
	}

	unsigned long Grid_get_active_values_size(Grid<float>* ptr)
	{
		return ptr->get_active_voxels().size() * 3;
	}

	void Grid_get_active_values(Grid<float>* ptr, int* buffer)
	{
		auto active = ptr->get_active_voxels();

		ULONG ulSize = active.size() * sizeof(Eigen::Vector3i);
		//int* pszReturn = NULL;

		//pszReturn = (int*)::CoTaskMemAlloc(ulSize);
		memcpy(buffer, active.data(), ulSize);
	}

/* ******************************************************* */
	void Grid_erode(Grid<float>* ptr, int iterations)
	{
		ptr->erode(iterations);
	}

	void Grid_get_neighbours(Grid<float>* ptr, int* coords, float* neighbours)
	{
		auto vals = ptr->get_neighbourhood(Eigen::Vector3i(coords));
		memcpy(neighbours, vals.data(), sizeof(float) * 27);
	}

	bool Grid_get_active_state(Grid<float>* ptr, int* xyz)
	{
		return ptr->get_active_state(Eigen::Vector3i(xyz));
	}

	void Grid_get_active_state_many(Grid<float>* ptr, int n, int* xyz, int* states)
	{
		std::vector<Eigen::Vector3i> xyz_vec(n);
		for (size_t i = 0; i < n; ++i)
			xyz_vec[i] = Eigen::Vector3i(xyz[i * 3]);

		auto res = ptr->get_active_state(xyz_vec);
		for (size_t i = 0; i < n; ++i)
			states[i] = res[i] ? 1 : 0;
	}

	void Grid_set_active_state(Grid<float>* ptr, int* xyz, bool state)
	{
		ptr->set_active_state(Eigen::Vector3i(xyz), state);
	}

	void Grid_set_active_state_many(Grid<float>* ptr, int n, int* xyz, int* states)
	{
		std::vector<bool> state_vec(n);
		std::vector<Eigen::Vector3i> xyz_vec(n);
		for (size_t i = 0; i < n; ++i)
		{
			state_vec[i] = states[i] > 0;
			xyz_vec[i] = Eigen::Vector3i(xyz[i * 3], xyz[i * 3 +1], xyz[i * 3 + 2]);
		}

		ptr->set_active_state(xyz_vec, state_vec);
	}

/* ******************************************************* */

	void Grid_difference(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::csgDifference(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_union(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::csgUnion(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_intersection(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::csgIntersection(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_max(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::compMax(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_min(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::compMin(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_sum(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::compSum(*(ptr0->m_grid), *(ptr1->m_grid));
	}

	void Grid_mul(Grid<float>* ptr0, Grid<float>* ptr1)
	{
		openvdb::tools::compMul(*(ptr0->m_grid), *(ptr1->m_grid));
	}

/* ******************************************************* */

	/* ################# VEC3 GRID ##################### */

	Grid<openvdb::Vec3f>* Vec3Grid_Create()
	{

		return new Grid<openvdb::Vec3f>(openvdb::Vec3f(0,0,0));
	}

	Grid<openvdb::Vec3f>* Vec3Grid_duplicate(Grid<openvdb::Vec3f>* ptr)
	{
		return new Grid<openvdb::Vec3f>(*ptr->duplicate());
	}


	void Vec3Grid_Delete(Grid<openvdb::Vec3f>* ptr)
	{
		delete ptr;
	}

	Grid<openvdb::Vec3f>* Vec3Grid_read(const char* filename)
	{
		//return Grid<float>::read(filename);
		return new Grid<openvdb::Vec3f>(*Grid<openvdb::Vec3f>::read(filename));
	}

	void Vec3Grid_write(Grid<openvdb::Vec3f>* ptr, const char* filename, bool half_float)
	{
		ptr->write(filename, half_float);
	}

	SAFEARRAY* Vec3Grid_get_some_grids(const char* filename)
	{
		std::vector<std::shared_ptr<Grid<float>>> grid_vec = Grid<float>::from_vdb(filename);
		std::vector<Grid<float>*> grid_ptr_vec(grid_vec.size());
		for (int i = 0; i < grid_vec.size(); ++i)
		{
			grid_ptr_vec.push_back(new Grid<float>(*grid_vec[i]));
		}

		SAFEARRAY* psa = SafeArrayCreateVector(VT_I4, 0, grid_ptr_vec.size());
		void* data;
		SafeArrayAccessData(psa, &data);
		CopyMemory(data, grid_ptr_vec.data(), sizeof(grid_ptr_vec));
		SafeArrayUnaccessData(psa);
		return psa;
	}

	void Vec3Grid_evaluate(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3f> xyz(num_coords);
		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
		}

		auto res = ptr->get_interpolated_values(xyz, sample_type);
		//std::copy(res.begin(), res.end(), results);
	}

	void Vec3Grid_get_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* results, int sample_type)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3f> xyz(num_coords);
		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
		}

		auto res = ptr->get_interpolated_values(xyz, sample_type);
		//std::copy(res.begin(), res.end(), results);
	}

	void Vec3Grid_set_values_ws(Grid<openvdb::Vec3f>* ptr, int num_coords, float* coords, float* values)
	{
		//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords);
		//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords);
		//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map);

		std::vector<Eigen::Vector3i> xyz(num_coords);
		std::vector<openvdb::Vec3f> valuesf(num_coords);
		//std::copy(values[0], values[num_coords - 1], valuesf);


		for (int i = 0; i < num_coords; ++i)
		{
			xyz[i] = Eigen::Vector3i((int)coords[i * 3], (int)coords[i * 3 + 1], (int)coords[i * 3 + 2]);
			valuesf[i] = openvdb::Vec3f(values[i * 3], values[i * 3+1], values[i * 3+2]);
		}

		ptr->set_values(xyz, valuesf);
	}

	openvdb::Vec3f Vec3Grid_get_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z)
	{
		return ptr->get_value(Eigen::Vector3i(x, y, z));
	}

	void Vec3Grid_set_value(Grid<openvdb::Vec3f>* ptr, int x, int y, int z, float a, float b, float c)
	{
		ptr->set_value(Eigen::Vector3i(x, y, z), openvdb::Vec3f(a, b, c));
	}

	void Vec3Grid_filter(Grid<openvdb::Vec3f>* ptr, int width, int iterations, int type)
	{
		ptr->filter(width, iterations, type);
	}

	void Vec3Grid_bounding_box(Grid<openvdb::Vec3f>* ptr, int* min, int* max)
	{
		std::tuple<Eigen::Vector3i, Eigen::Vector3i> bb = ptr->bounding_box();
		//openvdb::CoordBBox bb = ptr->bounding_box();

		Eigen::Vector3i bbmin = std::get<0>(bb);
		Eigen::Vector3i bbmax = std::get<1>(bb);

		min[0] = bbmin.x();
		min[1] = bbmin.y();
		min[2] = bbmin.z();

		max[0] = bbmax.x();
		max[1] = bbmax.y();
		max[2] = bbmax.z();
	}

	void Vec3Grid_set_transform(Grid<openvdb::Vec3f>* ptr, float* mat)
	{
		Eigen::Matrix4f matf(mat);
		ptr->set_transform(matf.cast<double>());
		//ptr->set_transform(Eigen::Matrix4d(mat));
	}

	void Vec3Grid_get_transform(Grid<openvdb::Vec3f>* ptr, float* mat)
	{
		Eigen::Matrix4f matf = ptr->get_transform().cast<float>();
		std::copy(matf.data(), matf.data() + matf.size(), mat);
		//Eigen::Matrix4d matd = ptr->get_transform();
		//std::copy(matd.data(), matd.data() + matd.size(), mat);
	}

	Grid<openvdb::Vec3f>* Vec3Grid_resample(Grid<openvdb::Vec3f>* ptr, float scale)
	{
		return ptr->resample(scale);
	}

	void Vec3Grid_get_dense(Grid<openvdb::Vec3f>* ptr, int* min, int* max, float* results)
	{
		std::vector<openvdb::Vec3f> dense = ptr->get_dense(Eigen::Vector3i(min[0], min[1], min[2]), Eigen::Vector3i(max[0], max[1], max[2]));
		//std::copy(dense.begin(), dense.end(), results);
	}

	char* Vec3Grid_get_name(Grid<openvdb::Vec3f>* ptr)
	{
		ULONG ulSize = strlen(ptr->get_name().c_str()) + sizeof(char);
		char* pszReturn = NULL;

		pszReturn = (char*)::CoTaskMemAlloc(ulSize);
		strcpy_s(pszReturn, ulSize, ptr->get_name().c_str());

		return pszReturn;
	}

	void Vec3Grid_set_name(Grid<openvdb::Vec3f>* ptr, const char* name)
	{
		ptr->set_name(std::string(name));
	}
}


#endif