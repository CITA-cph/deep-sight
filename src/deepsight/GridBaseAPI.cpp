#include "GridBaseAPI.h"

#define NOMINMAX
#include <windows.h>	// CoTaskMemAlloc, for strings handed to the CLR marshaller

#include <algorithm>
#include <cstring>
#include <memory>
#include <stdexcept>
#include <vector>

namespace DeepSight
{
	namespace
	{
		// -------------------------------------------------------------------
		// Argument validation helpers.
		//
		// Every one of these used to be absent: the API dereferenced whatever
		// pointer managed code handed it. A disposed GridApi in C# leaves
		// Ptr == IntPtr.Zero, and a GH component that reads from a disposed
		// grid would take the process down with an access violation rather
		// than surfacing an error.
		// -------------------------------------------------------------------

		GridBase* require_grid(GridBase* p)
		{
			if (p == nullptr)
				throw std::invalid_argument("Null grid handle.");
			return p;
		}

		void require_buffer(const void* p, const char* name)
		{
			if (p == nullptr)
				throw std::invalid_argument(std::string("Null buffer: ") + name);
		}

		void require_count(int n)
		{
			if (n < 0)
				throw std::invalid_argument("Negative element count.");
		}

		std::vector<Eigen::Vector3i> to_index_coords(int num_coords, const int* coords)
		{
			require_count(num_coords);
			if (num_coords > 0) require_buffer(coords, "coords");

			std::vector<Eigen::Vector3i> vecs;
			vecs.reserve(static_cast<std::size_t>(num_coords));
			for (int i = 0; i < num_coords; ++i)
			{
				// Constructed element-wise rather than by reinterpret_casting
				// the caller's int* to an Eigen::Vector3i*. That cast was a
				// strict-aliasing violation, and it silently assumed
				// sizeof(Eigen::Vector3i) == 12 with no padding -- true today,
				// but nothing in the language guarantees it.
				vecs.emplace_back(coords[i * 3 + 0], coords[i * 3 + 1], coords[i * 3 + 2]);
			}
			return vecs;
		}

		std::vector<Eigen::Vector3d> to_world_coords(int num_coords, const double* coords)
		{
			require_count(num_coords);
			if (num_coords > 0) require_buffer(coords, "coords");

			std::vector<Eigen::Vector3d> vecs;
			vecs.reserve(static_cast<std::size_t>(num_coords));
			for (int i = 0; i < num_coords; ++i)
			{
				vecs.emplace_back(coords[i * 3 + 0], coords[i * 3 + 1], coords[i * 3 + 2]);
			}
			return vecs;
		}

		std::vector<bool> to_bools(int n, const int* state)
		{
			require_count(n);
			if (n > 0) require_buffer(state, "state");

			std::vector<bool> v;
			v.reserve(static_cast<std::size_t>(n));
			for (int i = 0; i < n; ++i) v.push_back(state[i] != 0);
			return v;
		}

		/// Copy a std::string into a CoTaskMem block for the CLR's LPStr return
		/// marshaller (which frees it with CoTaskMemFree).
		const char* to_cotaskmem(const std::string& s)
		{
			const std::size_t size = s.size() + 1;
			char* buffer = static_cast<char*>(::CoTaskMemAlloc(size));

			// GridBase_GetType did not check this. On allocation failure it
			// called strcpy_s on a null destination.
			if (buffer == nullptr)
				throw std::bad_alloc();

			std::memcpy(buffer, s.c_str(), size);
			return buffer;
		}

		// -------------------------------------------------------------------
		// Shared implementations. The four grid families had four verbatim
		// copies of each of these; a fix applied to one (e.g. Vec3f's
		// GetActiveVoxels used std::copy while the others used memcpy) did not
		// propagate to the rest. One implementation, four thin wrappers.
		// -------------------------------------------------------------------

		template<typename GridT>
		long long get_active_voxels_impl(GridBase* ptr, long long capacity, int* coords)
		{
			std::vector<Eigen::Vector3i> vecs =
				require_grid(ptr)->get_active_voxels<GridT>();

			const long long count = static_cast<long long>(vecs.size());

			// Query mode: capacity 0 just reports how much room is needed.
			if (capacity <= 0)
				return count;

			// This is the bug that made the old signature unsafe: there was no
			// capacity parameter at all, so the native side memcpy'd
			// 3 * activeVoxelCount ints into whatever the caller had allocated
			// and trusted the two numbers to agree. Refuse to write instead of
			// overrunning; the caller can re-query and retry.
			if (capacity < count)
				return count;

			require_buffer(coords, "coords");

			for (long long i = 0; i < count; ++i)
			{
				const Eigen::Vector3i& v = vecs[static_cast<std::size_t>(i)];
				coords[i * 3 + 0] = v.x();
				coords[i * 3 + 1] = v.y();
				coords[i * 3 + 2] = v.z();
			}

			return count;
		}

		template<typename GridT, typename ValueT>
		void get_values_ws_impl(GridBase* ptr, int num_coords, const double* coords, ValueT* values)
		{
			if (num_coords > 0) require_buffer(values, "values");

			auto res = require_grid(ptr)->get_values_ws<GridT>(to_world_coords(num_coords, coords));
			std::copy(res.begin(), res.end(), values);
		}

		template<typename GridT, typename ValueT>
		void get_values_is_impl(GridBase* ptr, int num_coords, const int* coords, ValueT* values)
		{
			if (num_coords > 0) require_buffer(values, "values");

			auto res = require_grid(ptr)->get_values_is<GridT>(to_index_coords(num_coords, coords));
			std::copy(res.begin(), res.end(), values);
		}

		template<typename GridT, typename ValueT>
		void set_values_impl(GridBase* ptr, int num_coords, const int* coords, const ValueT* values)
		{
			if (num_coords > 0) require_buffer(values, "values");

			std::vector<ValueT> vals(values, values + std::max(0, num_coords));
			require_grid(ptr)->set_values<GridT>(to_index_coords(num_coords, coords), vals);
		}

		template<typename GridT, typename ValueT>
		void get_neighbours_impl(GridBase* ptr, const int* coord, ValueT* values)
		{
			require_buffer(coord, "coord");
			require_buffer(values, "values");

			auto nbrs = require_grid(ptr)->get_neighbourhood<GridT>(
				Eigen::Vector3i(coord[0], coord[1], coord[2]));

			std::copy(nbrs.data(), nbrs.data() + 27, values);
		}

		template<typename GridT>
		void set_active_state_impl(GridBase* ptr, const int* coord, int state)
		{
			require_buffer(coord, "coord");
			require_grid(ptr)->set_active_state<GridT>(
				Eigen::Vector3i(coord[0], coord[1], coord[2]), state != 0);
		}

		template<typename GridT>
		void set_active_states_impl(GridBase* ptr, int num_coords, const int* coord, const int* state)
		{
			auto states = to_bools(num_coords, state);
			require_grid(ptr)->set_active_states<GridT>(to_index_coords(num_coords, coord), states);
		}
	}

	extern "C"
	{

#pragma region Lifetime

		GridBase* DEEPSIGHT_CALL GridBase_CreateFloat(float background)
		{
			return api_guard([&]() -> GridBase* {
				// unique_ptr so a throw from initialize() (bad_alloc) does not
				// leak the GridBase that was already allocated.
				std::unique_ptr<GridBase> grid(new GridBase());
				grid->initialize<openvdb::FloatGrid>(background);
				return grid.release();
			}, nullptr);
		}

		GridBase* DEEPSIGHT_CALL GridBase_CreateDouble(double background)
		{
			return api_guard([&]() -> GridBase* {
				std::unique_ptr<GridBase> grid(new GridBase());
				grid->initialize<openvdb::DoubleGrid>(background);
				return grid.release();
			}, nullptr);
		}

		GridBase* DEEPSIGHT_CALL GridBase_CreateInt32(int background)
		{
			return api_guard([&]() -> GridBase* {
				std::unique_ptr<GridBase> grid(new GridBase());
				grid->initialize<openvdb::Int32Grid>(background);
				return grid.release();
			}, nullptr);
		}

		GridBase* DEEPSIGHT_CALL GridBase_CreateVec3f(const float* background)
		{
			return api_guard([&]() -> GridBase* {
				require_buffer(background, "background");
				std::unique_ptr<GridBase> grid(new GridBase());
				grid->initialize<openvdb::Vec3fGrid>(
					openvdb::Vec3f(background[0], background[1], background[2]));
				return grid.release();
			}, nullptr);
		}

		GridBase* DEEPSIGHT_CALL GridBase_Duplicate(GridBase* grid)
		{
			return api_guard([&] { return require_grid(grid)->duplicate(); },
				static_cast<GridBase*>(nullptr));
		}

		void DEEPSIGHT_CALL GridBase_Delete(GridBase* grid)
		{
			// delete on null is well-defined; no guard needed for that, but the
			// destructor releases an OpenVDB tree and could in principle throw.
			api_guard([&] { delete grid; });
		}

#pragma endregion Lifetime

#pragma region Generic

		void DEEPSIGHT_CALL GridBase_SetName(GridBase* ptr, const char* name)
		{
			api_guard([&] {
				require_buffer(name, "name");
				require_grid(ptr)->set_name(name);
			});
		}

		const char* DEEPSIGHT_CALL GridBase_GetName(GridBase* ptr)
		{
			return api_guard([&]() -> const char* {
				return to_cotaskmem(require_grid(ptr)->get_name());
			}, static_cast<const char*>(nullptr));
		}

		const char* DEEPSIGHT_CALL GridBase_GetType(GridBase* ptr)
		{
			return api_guard([&]() -> const char* {
				return to_cotaskmem(require_grid(ptr)->get_type());
			}, static_cast<const char*>(nullptr));
		}

		void DEEPSIGHT_CALL GridBase_GetBoundingBoxIndex(GridBase* ptr, int* min, int* max)
		{
			api_guard([&] { require_grid(ptr)->get_bounding_box(min, max); });
		}

		void DEEPSIGHT_CALL GridBase_ClipIndex(GridBase* ptr, const int* min, const int* max)
		{
			api_guard([&] { require_grid(ptr)->clip_index(min, max); });
		}

		void DEEPSIGHT_CALL GridBase_ClipWorld(GridBase* ptr, const double* min, const double* max)
		{
			api_guard([&] { require_grid(ptr)->clip_world(min, max); });
		}

		void DEEPSIGHT_CALL GridBase_Prune(GridBase* ptr, float tolerance)
		{
			api_guard([&] { require_grid(ptr)->prune(tolerance); });
		}

		int DEEPSIGHT_CALL GridBase_GetGridClass(GridBase* ptr)
		{
			return api_guard([&] { return require_grid(ptr)->get_grid_class(); }, -1);
		}

		void DEEPSIGHT_CALL GridBase_SetGridClass(GridBase* ptr, int c)
		{
			api_guard([&] { require_grid(ptr)->set_grid_class(c); });
		}

		long long DEEPSIGHT_CALL GridBase_GetActiveVoxelCount(GridBase* ptr)
		{
			return api_guard([&] {
				return static_cast<long long>(require_grid(ptr)->get_active_voxel_count());
			}, -1LL);
		}

		void DEEPSIGHT_CALL GridBase_SetTransform(GridBase* ptr, const float* xform)
		{
			api_guard([&] {
				require_buffer(xform, "xform");
				Eigen::Matrix4f matf(xform);
				require_grid(ptr)->set_transform(matf.cast<double>());
			});
		}

		void DEEPSIGHT_CALL GridBase_GetTransform(GridBase* ptr, float* xform)
		{
			api_guard([&] {
				require_buffer(xform, "xform");
				Eigen::Matrix4f matf = require_grid(ptr)->get_transform().cast<float>();
				std::copy(matf.data(), matf.data() + matf.size(), xform);
			});
		}

#pragma endregion Generic

#pragma region FloatGrid

		float DEEPSIGHT_CALL FloatGrid_GetValueWs(GridBase* ptr, double x, double y, double z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_ws<openvdb::FloatGrid>(Eigen::Vector3d(x, y, z));
			}, 0.0f);
		}

		float DEEPSIGHT_CALL FloatGrid_GetValueIs(GridBase* ptr, int x, int y, int z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_is<openvdb::FloatGrid>(Eigen::Vector3i(x, y, z));
			}, 0.0f);
		}

		void DEEPSIGHT_CALL FloatGrid_SetValue(GridBase* ptr, int x, int y, int z, float v)
		{
			api_guard([&] {
				require_grid(ptr)->set_value<openvdb::FloatGrid>(Eigen::Vector3i(x, y, z), v);
			});
		}

		void DEEPSIGHT_CALL FloatGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, float* values)
		{
			api_guard([&] { get_values_ws_impl<openvdb::FloatGrid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL FloatGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, float* values)
		{
			api_guard([&] { get_values_is_impl<openvdb::FloatGrid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL FloatGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const float* values)
		{
			api_guard([&] { set_values_impl<openvdb::FloatGrid>(ptr, num_coords, coords, values); });
		}

		long long DEEPSIGHT_CALL FloatGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords)
		{
			return api_guard([&] {
				return get_active_voxels_impl<openvdb::FloatGrid>(ptr, capacity, coords);
			}, -1LL);
		}

		void DEEPSIGHT_CALL FloatGrid_SetActiveState(GridBase* ptr, const int* coord, int state)
		{
			api_guard([&] { set_active_state_impl<openvdb::FloatGrid>(ptr, coord, state); });
		}

		void DEEPSIGHT_CALL FloatGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state)
		{
			api_guard([&] { set_active_states_impl<openvdb::FloatGrid>(ptr, num_coords, coord, state); });
		}

		void DEEPSIGHT_CALL FloatGrid_GetNeighbours(GridBase* ptr, const int* coord, float* values)
		{
			api_guard([&] { get_neighbours_impl<openvdb::FloatGrid>(ptr, coord, values); });
		}

		void DEEPSIGHT_CALL FloatGrid_InactivateBelow(GridBase* ptr, float threshold)
		{
			api_guard([&] { require_grid(ptr)->inactivate_below<openvdb::FloatGrid>(threshold); });
		}

#pragma endregion FloatGrid

#pragma region DoubleGrid

		double DEEPSIGHT_CALL DoubleGrid_GetValueWs(GridBase* ptr, double x, double y, double z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_ws<openvdb::DoubleGrid>(Eigen::Vector3d(x, y, z));
			}, 0.0);
		}

		double DEEPSIGHT_CALL DoubleGrid_GetValueIs(GridBase* ptr, int x, int y, int z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_is<openvdb::DoubleGrid>(Eigen::Vector3i(x, y, z));
			}, 0.0);
		}

		void DEEPSIGHT_CALL DoubleGrid_SetValue(GridBase* ptr, int x, int y, int z, double v)
		{
			api_guard([&] {
				require_grid(ptr)->set_value<openvdb::DoubleGrid>(Eigen::Vector3i(x, y, z), v);
			});
		}

		void DEEPSIGHT_CALL DoubleGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, double* values)
		{
			api_guard([&] { get_values_ws_impl<openvdb::DoubleGrid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL DoubleGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, double* values)
		{
			api_guard([&] { get_values_is_impl<openvdb::DoubleGrid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL DoubleGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const double* values)
		{
			api_guard([&] { set_values_impl<openvdb::DoubleGrid>(ptr, num_coords, coords, values); });
		}

		long long DEEPSIGHT_CALL DoubleGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords)
		{
			return api_guard([&] {
				return get_active_voxels_impl<openvdb::DoubleGrid>(ptr, capacity, coords);
			}, -1LL);
		}

		void DEEPSIGHT_CALL DoubleGrid_SetActiveState(GridBase* ptr, const int* coord, int state)
		{
			api_guard([&] { set_active_state_impl<openvdb::DoubleGrid>(ptr, coord, state); });
		}

		void DEEPSIGHT_CALL DoubleGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state)
		{
			api_guard([&] { set_active_states_impl<openvdb::DoubleGrid>(ptr, num_coords, coord, state); });
		}

		void DEEPSIGHT_CALL DoubleGrid_GetNeighbours(GridBase* ptr, const int* coord, double* values)
		{
			api_guard([&] { get_neighbours_impl<openvdb::DoubleGrid>(ptr, coord, values); });
		}

#pragma endregion DoubleGrid

#pragma region Int32Grid

		int DEEPSIGHT_CALL Int32Grid_GetValueWs(GridBase* ptr, double x, double y, double z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_ws<openvdb::Int32Grid>(Eigen::Vector3d(x, y, z));
			}, 0);
		}

		int DEEPSIGHT_CALL Int32Grid_GetValueIs(GridBase* ptr, int x, int y, int z)
		{
			return api_guard([&] {
				return require_grid(ptr)->get_value_is<openvdb::Int32Grid>(Eigen::Vector3i(x, y, z));
			}, 0);
		}

		void DEEPSIGHT_CALL Int32Grid_SetValue(GridBase* ptr, int x, int y, int z, int v)
		{
			api_guard([&] {
				require_grid(ptr)->set_value<openvdb::Int32Grid>(Eigen::Vector3i(x, y, z), v);
			});
		}

		void DEEPSIGHT_CALL Int32Grid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, int* values)
		{
			api_guard([&] { get_values_ws_impl<openvdb::Int32Grid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL Int32Grid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, int* values)
		{
			api_guard([&] { get_values_is_impl<openvdb::Int32Grid>(ptr, num_coords, coords, values); });
		}

		void DEEPSIGHT_CALL Int32Grid_SetValues(GridBase* ptr, int num_coords, const int* coords, const int* values)
		{
			api_guard([&] { set_values_impl<openvdb::Int32Grid>(ptr, num_coords, coords, values); });
		}

		long long DEEPSIGHT_CALL Int32Grid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords)
		{
			return api_guard([&] {
				return get_active_voxels_impl<openvdb::Int32Grid>(ptr, capacity, coords);
			}, -1LL);
		}

		void DEEPSIGHT_CALL Int32Grid_SetActiveState(GridBase* ptr, const int* coord, int state)
		{
			api_guard([&] { set_active_state_impl<openvdb::Int32Grid>(ptr, coord, state); });
		}

		void DEEPSIGHT_CALL Int32Grid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state)
		{
			api_guard([&] { set_active_states_impl<openvdb::Int32Grid>(ptr, num_coords, coord, state); });
		}

		void DEEPSIGHT_CALL Int32Grid_GetNeighbours(GridBase* ptr, const int* coord, int* values)
		{
			api_guard([&] { get_neighbours_impl<openvdb::Int32Grid>(ptr, coord, values); });
		}

#pragma endregion Int32Grid

#pragma region Vec3fGrid

		void DEEPSIGHT_CALL Vec3fGrid_GetValueWs(GridBase* ptr, double x, double y, double z, float* value)
		{
			api_guard([&] {
				require_buffer(value, "value");

				// USE-AFTER-FREE FIX. This used to read:
				//     float* vec = ptr->get_value_ws<Vec3fGrid>(...).asPointer();
				//     std::copy(vec, vec + 3, value);
				// get_value_ws returns an openvdb::Vec3f *by value*. The
				// temporary it returns is destroyed at the end of the
				// declaration statement, so `vec` was dangling before the
				// std::copy on the next line ever ran. It appeared to work
				// because the freed stack slot usually still held the right
				// bytes. Bind the value to a named local instead.
				const openvdb::Vec3f v =
					require_grid(ptr)->get_value_ws<openvdb::Vec3fGrid>(Eigen::Vector3d(x, y, z));

				value[0] = v.x();
				value[1] = v.y();
				value[2] = v.z();
			});
		}

		void DEEPSIGHT_CALL Vec3fGrid_GetValueIs(GridBase* ptr, int x, int y, int z, float* value)
		{
			api_guard([&] {
				require_buffer(value, "value");

				// Same use-after-free as Vec3fGrid_GetValueWs above.
				const openvdb::Vec3f v =
					require_grid(ptr)->get_value_is<openvdb::Vec3fGrid>(Eigen::Vector3i(x, y, z));

				value[0] = v.x();
				value[1] = v.y();
				value[2] = v.z();
			});
		}

		void DEEPSIGHT_CALL Vec3fGrid_SetValue(GridBase* ptr, int x, int y, int z, const float* v)
		{
			api_guard([&] {
				require_buffer(v, "v");
				require_grid(ptr)->set_value<openvdb::Vec3fGrid>(
					Eigen::Vector3i(x, y, z), openvdb::Vec3f(v[0], v[1], v[2]));
			});
		}

		void DEEPSIGHT_CALL Vec3fGrid_GetValuesWs(GridBase* ptr, int num_coords, const double* coords, float* values)
		{
			api_guard([&] {
				if (num_coords > 0) require_buffer(values, "values");

				auto res = require_grid(ptr)->get_values_ws<openvdb::Vec3fGrid>(
					to_world_coords(num_coords, coords));

				for (std::size_t i = 0; i < res.size(); ++i)
				{
					values[i * 3 + 0] = res[i].x();
					values[i * 3 + 1] = res[i].y();
					values[i * 3 + 2] = res[i].z();
				}
			});
		}

		void DEEPSIGHT_CALL Vec3fGrid_GetValuesIs(GridBase* ptr, int num_coords, const int* coords, float* values)
		{
			api_guard([&] {
				if (num_coords > 0) require_buffer(values, "values");

				auto res = require_grid(ptr)->get_values_is<openvdb::Vec3fGrid>(
					to_index_coords(num_coords, coords));

				for (std::size_t i = 0; i < res.size(); ++i)
				{
					values[i * 3 + 0] = res[i].x();
					values[i * 3 + 1] = res[i].y();
					values[i * 3 + 2] = res[i].z();
				}
			});
		}

		void DEEPSIGHT_CALL Vec3fGrid_SetValues(GridBase* ptr, int num_coords, const int* coords, const float* values)
		{
			api_guard([&] {
				if (num_coords > 0) require_buffer(values, "values");

				std::vector<openvdb::Vec3f> vals;
				vals.reserve(static_cast<std::size_t>(std::max(0, num_coords)));
				for (int i = 0; i < num_coords; ++i)
				{
					vals.emplace_back(values[i * 3 + 0], values[i * 3 + 1], values[i * 3 + 2]);
				}

				require_grid(ptr)->set_values<openvdb::Vec3fGrid>(
					to_index_coords(num_coords, coords), vals);
			});
		}

		long long DEEPSIGHT_CALL Vec3fGrid_GetActiveVoxels(GridBase* ptr, long long capacity, int* coords)
		{
			return api_guard([&] {
				return get_active_voxels_impl<openvdb::Vec3fGrid>(ptr, capacity, coords);
			}, -1LL);
		}

		void DEEPSIGHT_CALL Vec3fGrid_SetActiveState(GridBase* ptr, const int* coord, int state)
		{
			api_guard([&] { set_active_state_impl<openvdb::Vec3fGrid>(ptr, coord, state); });
		}

		void DEEPSIGHT_CALL Vec3fGrid_SetActiveStates(GridBase* ptr, int num_coords, const int* coord, const int* state)
		{
			api_guard([&] { set_active_states_impl<openvdb::Vec3fGrid>(ptr, num_coords, coord, state); });
		}

		void DEEPSIGHT_CALL Vec3fGrid_GetNeighbours(GridBase* ptr, const int* coord, float* values)
		{
			api_guard([&] {
				require_buffer(coord, "coord");
				require_buffer(values, "values");

				auto nbrs = require_grid(ptr)->get_neighbourhood<openvdb::Vec3fGrid>(
					Eigen::Vector3i(coord[0], coord[1], coord[2]));

				for (int i = 0; i < 27; ++i)
				{
					values[i * 3 + 0] = nbrs[i].x();
					values[i * 3 + 1] = nbrs[i].y();
					values[i * 3 + 2] = nbrs[i].z();
				}
			});
		}

#pragma endregion Vec3fGrid

	} // extern "C"

}
