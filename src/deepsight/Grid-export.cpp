#include "Grid-export.h"

namespace DeepSight
{
	Grid<float>* Grid_Create()
	{

		return new Grid<float>();
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
}


