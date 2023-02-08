#ifndef GRIDBASE_H
#define GRIDBASE_H

#define NOMINMAX
#include <windows.h>

#include <openvdb/openvdb.h>
#include <openvdb/tools/Interpolation.h>

#include <string>
#include <memory>


#include <Eigen/Geometry>


namespace DeepSight
{
	class GridBase
	{
	public:
		openvdb::SharedPtr<openvdb::GridBase> m_grid;

		GridBase();

		template<typename GridT>
		void initialize();

		void set_name(std::string name);
		std::string get_name();

		void set_transform(Eigen::Matrix4d xform);
		Eigen::Matrix4d get_transform();

		template<typename GridT, typename T>
		T get_value(double x, double y, double z);

		void clip_index(int* min, int* max);
		void clip_world(double* min, double* max);

		void prune(float tolerance=0.0f);

		int get_grid_class();
		void set_grid_class(int c);
	};

#ifdef __cplusplus
extern "C" {
#endif
	DEEPSIGHT_EXPORT GridBase* GridBase_CreateFloat();
	DEEPSIGHT_EXPORT GridBase* GridBase_CreateDouble();
	DEEPSIGHT_EXPORT GridBase* GridBase_CreateInt();

	DEEPSIGHT_EXPORT float GridBase_GetValueFloat(GridBase* ptr, double x, double y, double z);
	DEEPSIGHT_EXPORT double GridBase_GetValueDouble(GridBase* ptr, double x, double y, double z);
	DEEPSIGHT_EXPORT int GridBase_GetValueInt32(GridBase* ptr, double x, double y, double z);

	DEEPSIGHT_EXPORT void GridBase_SetName(GridBase *ptr, const char* name);
	DEEPSIGHT_EXPORT const char * GridBase_GetName(GridBase *ptr);

	DEEPSIGHT_EXPORT void GridBase_ClipIndex(GridBase* ptr, int* min, int* max);
	DEEPSIGHT_EXPORT void GridBase_ClipWorld(GridBase* ptr, double* min, double* max);
	DEEPSIGHT_EXPORT void GridBase_Prune(GridBase* ptr, float tolerance);

	DEEPSIGHT_EXPORT int GridBase_GetGridClass(GridBase* ptr);
	DEEPSIGHT_EXPORT void GridBase_SetGridClass(GridBase* ptr, int c);

#ifdef __cplusplus
	}
#endif
}
#endif
