#include "GridBaseAPI.h"
#include "GridBase.h"

namespace DeepSight
{

#pragma region Construct_Init
	GridBase* GridBase_CreateFloat()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::FloatGrid>();
		return grid;
	}

	GridBase* GridBase_CreateDouble()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::DoubleGrid>();
		return grid;
	}

	GridBase* GridBase_CreateInt32()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::Int32Grid>();
		return grid;
	}

	GridBase* GridBase_CreateVec3f()
	{
		GridBase* grid = new GridBase();
		grid->initialize<openvdb::Vec3fGrid>();
		return grid;
	}
#pragma endregion Construct_Init

	void GridBase_Delete(GridBase* grid)
	{
		delete grid;
	}


#pragma region Get_Set

#pragma region FloatGrid

	// Single value
	float FloatGrid_GetValueWs(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value_ws< openvdb::FloatGrid >(Eigen::Vector3d(x, y, z));
	}

	float FloatGrid_GetValueIs(GridBase* ptr, int x, int y, int z)
	{
		return ptr->get_value_is< openvdb::FloatGrid >(Eigen::Vector3i(x, y, z));
	}

	void FloatGrid_SetValue(GridBase* ptr, int x, int y, int z, float v)
	{
		ptr->set_value < openvdb::FloatGrid >(Eigen::Vector3i(x, y, z), v);
	}

	// Multiple values
	void FloatGrid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, float* values)
	{
		std::vector<Eigen::Vector3d> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3d(
				coords[i * 3 + 0],
				coords[i * 3 + 1],
				coords[i * 3 + 2]
			));
		}

		auto res = ptr->get_values_ws< openvdb::FloatGrid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void FloatGrid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, float* values)
	{
		std::vector<Eigen::Vector3i> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		auto res = ptr->get_values_is< openvdb::FloatGrid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void FloatGrid_SetValues(GridBase* ptr, int num_coords, int* coords, float* values)
	{
		std::vector<Eigen::Vector3i> vecs;
		std::vector<float> vals;

		vals.assign(values, values + num_coords);

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		ptr->set_values < openvdb::FloatGrid >(vecs, vals);
	}

	void FloatGrid_GetActiveVoxels(GridBase* ptr, int* coords)
	{
		std::vector<Eigen::Vector3i> vecs = ptr->get_active_voxels<openvdb::FloatGrid>();

		//std::copy(vecs.data()->begin(), vecs.data()->end(), coords);
	}

	void FloatGrid_SetActiveState(GridBase* ptr, int* coord, int state)
	{
		Eigen::Vector3i vec(coord);
		ptr->set_active_state<openvdb::FloatGrid>(vec, state != 0);
	}

	void FloatGrid_SetActiveStates(GridBase* ptr, int num_coords, int* coord, int* state)
	{
		Eigen::Vector3i* coord_ptr = reinterpret_cast<Eigen::Vector3i*>(coord);
		std::vector<Eigen::Vector3i> coord_vec(coord_ptr, coord_ptr + num_coords);
		std::vector<bool> state_vec;

		for (int i = 0; i < num_coords; ++i)
			state_vec.push_back(state[i] != 0);

		ptr->set_active_states<openvdb::FloatGrid>(coord_vec, state_vec);
	}

	void FloatGrid_GetNeighbours(GridBase* ptr, int* coord, float* values)
	{
		auto nbrs = ptr->get_neighbourhood<openvdb::FloatGrid>(Eigen::Vector3i(coord));
		memcpy(values, nbrs.data(), sizeof(float) * 27);
	}

#pragma endregion FloatGrid
#pragma region DoubleGrid

	// Single value
	double DoubleGrid_GetValueWs(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value_ws < openvdb::DoubleGrid >(Eigen::Vector3d(x, y, z));
	}

	double DoubleGrid_GetValueIs(GridBase* ptr, int x, int y, int z)
	{
		return ptr->get_value_is< openvdb::DoubleGrid >(Eigen::Vector3i(x, y, z));
	}

	void DoubleGrid_SetValue(GridBase* ptr, int x, int y, int z, double v)
	{
		ptr->set_value < openvdb::DoubleGrid >(Eigen::Vector3i(x, y, z), v);
	}

	// Multiple values
	void DoubleGrid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, double* values)
	{
		std::vector<Eigen::Vector3d> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3d(
				coords[i * 3 + 0],
				coords[i * 3 + 1],
				coords[i * 3 + 2]
			));
		}

		auto res = ptr->get_values_ws< openvdb::DoubleGrid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void DoubleGrid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, double* values)
	{
		std::vector<Eigen::Vector3i> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		auto res = ptr->get_values_is< openvdb::DoubleGrid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void DoubleGrid_SetValues(GridBase* ptr, int num_coords, int* coords, double* values)
	{
		std::vector<Eigen::Vector3i> vecs;
		std::vector<double> vals;

		vals.assign(values, values + num_coords);

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		ptr->set_values < openvdb::DoubleGrid >(vecs, vals);
	}

	void DoubleGrid_GetActiveVoxels(GridBase* ptr, int* coords)
	{
		std::vector<Eigen::Vector3i> vecs = ptr->get_active_voxels<openvdb::DoubleGrid>();
		//std::copy(vecs.begin(), vecs.end(), coords);
	}

	void DoubleGrid_SetActiveState(GridBase* ptr, int* coord, int state)
	{
		Eigen::Vector3i vec(coord);
		ptr->set_active_state<openvdb::DoubleGrid>(vec, state != 0);
	}

	void DoubleGrid_SetActiveStates(GridBase* ptr, int num_coords, int* coord, int* state)
	{
		Eigen::Vector3i* coord_ptr = reinterpret_cast<Eigen::Vector3i*>(coord);
		std::vector<Eigen::Vector3i> coord_vec(coord_ptr, coord_ptr + num_coords);
		std::vector<bool> state_vec;

		for (int i = 0; i < num_coords; ++i)
			state_vec.push_back(state[i] != 0);

		ptr->set_active_states<openvdb::DoubleGrid>(coord_vec, state_vec);
	}

	void DoubleGrid_GetNeighbours(GridBase* ptr, int* coord, double* values)
	{
		auto nbrs = ptr->get_neighbourhood<openvdb::DoubleGrid>(Eigen::Vector3i(coord));
		memcpy(values, nbrs.data(), sizeof(double) * 27);
	}

#pragma endregion DoubleGrid
#pragma region Int32Grid

	// Single value
	int Int32Grid_GetValueWs(GridBase* ptr, double x, double y, double z)
	{
		return ptr->get_value_ws < openvdb::Int32Grid >(Eigen::Vector3d(x, y, z));
	}

	int Int32Grid_GetValueIs(GridBase* ptr, int x, int y, int z)
	{
		return ptr->get_value_is< openvdb::Int32Grid >(Eigen::Vector3i(x, y, z));
	}

	void Int32Grid_SetValue(GridBase* ptr, int x, int y, int z, int v)
	{
		ptr->set_value < openvdb::Int32Grid >(Eigen::Vector3i(x, y, z), v);
	}

	// Multiple values
	void Int32Grid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, int* values)
	{
		std::vector<Eigen::Vector3d> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3d(
				coords[i * 3 + 0],
				coords[i * 3 + 1],
				coords[i * 3 + 2]
			));
		}

		auto res = ptr->get_values_ws< openvdb::Int32Grid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void Int32Grid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, int* values)
	{
		std::vector<Eigen::Vector3i> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		auto res = ptr->get_values_is< openvdb::Int32Grid >(vecs);
		std::copy(res.begin(), res.end(), values);
	}

	void Int32Grid_SetValues(GridBase* ptr, int num_coords, int* coords, int* values)
	{
		std::vector<Eigen::Vector3i> vecs;
		std::vector<int> vals;

		vals.assign(values, values + num_coords);

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		ptr->set_values < openvdb::Int32Grid >(vecs, vals);
	}

	void Int32Grid_GetActiveVoxels(GridBase* ptr, int* coords)
	{
		std::vector<Eigen::Vector3i> vecs = ptr->get_active_voxels<openvdb::Int32Grid>();
		//std::copy(vecs.begin(), vecs.end(), coords);
	}

	void Int32Grid_SetActiveState(GridBase* ptr, int* coord, int state)
	{
		Eigen::Vector3i vec(coord);
		ptr->set_active_state<openvdb::Int32Grid>(vec, state != 0);
	}

	void Int32Grid_SetActiveStates(GridBase* ptr, int num_coords, int* coord, int* state)
	{
		Eigen::Vector3i* coord_ptr = reinterpret_cast<Eigen::Vector3i*>(coord);
		std::vector<Eigen::Vector3i> coord_vec(coord_ptr, coord_ptr + num_coords);
		std::vector<bool> state_vec;

		for (int i = 0; i < num_coords; ++i)
			state_vec.push_back(state[i] != 0);

		ptr->set_active_states<openvdb::Int32Grid>(coord_vec, state_vec);
	}

	void Int32Grid_GetNeighbours(GridBase* ptr, int* coord, int* values)
	{
		auto nbrs = ptr->get_neighbourhood<openvdb::Int32Grid>(Eigen::Vector3i(coord));
		memcpy(values, nbrs.data(), sizeof(int) * 27);
	}

#pragma endregion Int32Grid

#pragma region Vec3fGrid

	// Single value
	void Vec3fGrid_GetValueWs(GridBase* ptr, double x, double y, double z, float* value)
	{
		float* vec = ptr->get_value_ws < openvdb::Vec3fGrid >(Eigen::Vector3d(x, y, z)).asPointer();
		std::copy(vec, vec + openvdb::Vec3f::size, value);
	}

	void Vec3fGrid_GetValueIs(GridBase* ptr, int x, int y, int z, float* value)
	{
		float* vec = ptr->get_value_is< openvdb::Vec3fGrid >(Eigen::Vector3i(x, y, z)).asPointer();
		std::copy(vec, vec + openvdb::Vec3f::size, value);
	}

	void Vec3fGrid_SetValue(GridBase* ptr, int x, int y, int z, float* v)
	{
		ptr->set_value < openvdb::Vec3fGrid >(Eigen::Vector3i(x, y, z), v);
	}

	// Multiple values
	void Vec3fGrid_GetValuesWs(GridBase* ptr, int num_coords, double* coords, float* values)
	{
		std::vector<Eigen::Vector3d> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3d(
				coords[i * 3 + 0],
				coords[i * 3 + 1],
				coords[i * 3 + 2]
			));
		}

		auto res = ptr->get_values_ws< openvdb::Vec3fGrid >(vecs);
		auto values_ptr = reinterpret_cast<openvdb::Vec3f*>(values);

		std::copy(res.begin(), res.end(), values_ptr);
	}

	void Vec3fGrid_GetValuesIs(GridBase* ptr, int num_coords, int* coords, float* values)
	{
		std::vector<Eigen::Vector3i> vecs;

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		auto res = ptr->get_values_is< openvdb::Vec3fGrid >(vecs);
		auto values_ptr = reinterpret_cast<openvdb::Vec3f*>(values);

		std::copy(res.begin(), res.end(), values_ptr);
	}

	void Vec3fGrid_SetValues(GridBase* ptr, int num_coords, int* coords, float* values)
	{
		std::vector<Eigen::Vector3i> vecs;
		std::vector<openvdb::Vec3f> vals;
		auto values_ptr = reinterpret_cast<openvdb::Vec3f*>(values);

		vals.assign(values_ptr, values_ptr + num_coords);

		for (int i = 0; i < num_coords; ++i)
		{
			vecs.push_back(Eigen::Vector3i(&coords[i * 3]));
		}

		ptr->set_values < openvdb::Vec3fGrid >(vecs, vals);
	}

	void Vec3fGrid_GetActiveVoxels(GridBase* ptr, int* coords)
	{
		std::vector<Eigen::Vector3i> vecs = ptr->get_active_voxels<openvdb::Int32Grid>();
		Eigen::Vector3i* coords_ptr = reinterpret_cast<Eigen::Vector3i*>(coords);
		std::copy(vecs.begin(), vecs.end(), coords_ptr);
	}

	void Vec3fGrid_SetActiveState(GridBase* ptr, int* coord, int state)
	{
		Eigen::Vector3i vec(coord);
		ptr->set_active_state<openvdb::Vec3fGrid>(vec, state != 0);
	}

	void Vec3fGrid_SetActiveStates(GridBase* ptr, int num_coords, int* coord, int* state)
	{
		Eigen::Vector3i* coord_ptr = reinterpret_cast<Eigen::Vector3i*>(coord);
		std::vector<Eigen::Vector3i> coord_vec(coord_ptr, coord_ptr + num_coords);
		std::vector<bool> state_vec;

		for (int i = 0; i < num_coords; ++i)
			state_vec.push_back(state[i] != 0);

		ptr->set_active_states<openvdb::Vec3fGrid>(coord_vec, state_vec);
	}

	void Vec3fGrid_GetNeighbours(GridBase* ptr, int* coord, float* values)
	{
		auto nbrs = ptr->get_neighbourhood<openvdb::Vec3fGrid>(Eigen::Vector3i(coord));
		auto values_ptr = reinterpret_cast<openvdb::Vec3f*>(values);
		//memcpy(values_ptr, nbrs.data(), sizeof(openvdb::Vec3f) * 27);
		std::copy(nbrs.begin(), nbrs.end(), values_ptr);

	}

#pragma endregion Vec3fGrid

#pragma endregion Get_Set

#pragma region Generic

	void GridBase_SetName(GridBase* ptr, const char* name) { ptr->set_name(name); }

	const char* GridBase_GetName(GridBase* ptr)
	{
		std::string name = ptr->get_name();

		ULONG ulSize = strlen(name.c_str()) + sizeof(char);
		char* pszReturn = NULL;
		pszReturn = (char*)::CoTaskMemAlloc(ulSize);
		if (pszReturn != nullptr)
			strcpy_s(pszReturn, ulSize, name.c_str());
		return pszReturn;
	}

	void GridBase_ClipIndex(GridBase* ptr, int* min, int* max)
	{
		ptr->clip_index(min, max);
	}

	void GridBase_ClipWorld(GridBase* ptr, double* min, double* max)
	{
		ptr->clip_world(min, max);
	}

	void GridBase_Prune(GridBase* ptr, float tolerance)
	{
		ptr->prune(tolerance);
	}

	int GridBase_GetGridClass(GridBase* ptr)
	{
		return ptr->get_grid_class();
	}

	void GridBase_SetGridClass(GridBase* ptr, int c)
	{
		ptr->set_grid_class(c);
	}

	int GridBase_GetActiveVoxelCount(GridBase* ptr)
	{
		return ptr->m_grid->activeVoxelCount();
	}

	char* GridBase_GetType(GridBase* ptr)
	{
		std::string type = ptr->get_type();
		ULONG ulSize = strlen(type.c_str()) + sizeof(char);
		char* pszReturn = NULL;

		pszReturn = (char*)::CoTaskMemAlloc(ulSize);
		strcpy_s(pszReturn, ulSize, type.c_str());

		return pszReturn;
	}

	void GridBase_SetTransform(GridBase* ptr, float* xform)
	{
		Eigen::Matrix4f matf(xform);
		ptr->set_transform(matf.cast<double>()); 
	}

	void GridBase_GetTransform(GridBase* ptr, float* xform)
	{
		Eigen::Matrix4f matf = ptr->get_transform().cast<float>();
		std::copy(matf.data(), matf.data() + matf.size(), xform);
	}

#pragma endregion Generic

}
