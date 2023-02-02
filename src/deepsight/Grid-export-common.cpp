#include "Grid-export-common.h"


#define EXPORT_SCALAR_C(TypeName, Type) \
void TypeName##Grid_to_mesh(Grid<Type>* ptr, QuadMesh* mesh_ptr, float isovalue){	ptr->to_mesh(isovalue, *(mesh_ptr->vertices), *(mesh_ptr->faces));}\

#define EXPORT_COMMON_C(TypeName, Type)																			\
Grid<Type>* TypeName##Grid_create			(Type background){ return new Grid<Type>(background); }				\
Grid<Type>* TypeName##Grid_duplicate		(Grid<Type>* ptr){ return new Grid<Type>(*ptr->duplicate()); }		\
void TypeName##Grid_delete(Grid<Type>* ptr){ delete ptr; }														\
Grid<Type>* TypeName##Grid_read(const char* filename){ return new Grid<Type>(*Grid<Type>::read(filename));}		\
void TypeName##Grid_write(Grid<Type>* ptr, const char* filename, bool half_float){ ptr->write(filename, half_float); } \
\
SAFEARRAY* TypeName##Grid_get_some_grids(const char* filename) \
{ \
	std::vector<std::shared_ptr<Grid<Type>>> grid_vec = Grid<Type>::from_vdb(filename); \
	std::vector<Grid<Type>*> grid_ptr_vec(grid_vec.size()); \
	for (int i = 0; i < grid_vec.size(); ++i) \
	{ \
		grid_ptr_vec.push_back(new Grid<Type>(*grid_vec[i])); \
	} \
	\
	SAFEARRAY* psa = SafeArrayCreateVector(VT_I4, 0, grid_ptr_vec.size()); \
	void* data; \
	SafeArrayAccessData(psa, &data); \
	CopyMemory(data, grid_ptr_vec.data(), sizeof(grid_ptr_vec)); \
	SafeArrayUnaccessData(psa); \
	return psa; \
} \
 \
void TypeName##Grid_get_values_ws(Grid<Type>* ptr, int num_coords, float* coords, Type* results, int sample_type) \
{\
/* \
	//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords); \
	//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords); \
	//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map); \
	*/ \
	std::vector<Eigen::Vector3f> xyz(num_coords); \
	for (int i = 0; i < num_coords; ++i) \
	{ \
		xyz[i] = Eigen::Vector3f(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]); \
	} \
 \
	auto res = ptr->get_interpolated_values(xyz, sample_type); \
	std::copy(res.begin(), res.end(), results); \
} \
 \
void TypeName##Grid_set_values_ws(Grid<Type>* ptr, int num_coords, float* coords, Type* values) \
{ \
	/*Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords); \
	//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords); \
	//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map); */\
 \
	std::vector<Eigen::Vector3i> xyz(num_coords); \
	std::vector<Type> valuesf(num_coords); \
	/*std::copy(values[0], values[num_coords - 1], valuesf);*/ \
\
\
	for (int i = 0; i < num_coords; ++i)\
	{\
		xyz[i] = Eigen::Vector3i((int)coords[i * 3], (int)coords[i * 3 + 1], (int)coords[i * 3 + 2]);\
		valuesf[i] = values[i];\
	}\
\
	ptr->set_values(xyz, valuesf);\
}\
void TypeName##Grid_get_values(Grid<Type>* ptr, int num_coords, int* coords, Type* results) \
{\
/* \
	//Eigen::Map<Eigen::Vector3f> vector_map(coords, num_coords); \
	//std::vector<Eigen::Vector3f> xyz(vector_map.data(), vector_map.data() + num_coords); \
	//std::vector<Eigen::Vector3f> xyz(num_coords, vector_map); \
	*/ \
	std::vector<Eigen::Vector3i> xyz(num_coords); \
	for (int i = 0; i < num_coords; ++i) \
	{ \
		xyz[i] = Eigen::Vector3i(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]); \
	} \
 \
	auto res = ptr->get_values(xyz); \
	std::copy(res.begin(), res.end(), results); \
} \
 \
void TypeName##Grid_set_values(Grid<Type>* ptr, int num_coords, int* coords, Type* values) \
{ \
 \
	std::vector<Eigen::Vector3i> xyz(num_coords); \
	std::vector<Type> valuesf(num_coords); \
	/*std::copy(values[0], values[num_coords - 1], valuesf);*/ \
\
\
	for (int i = 0; i < num_coords; ++i)\
	{\
		xyz[i] = Eigen::Vector3i((int)coords[i * 3], (int)coords[i * 3 + 1], (int)coords[i * 3 + 2]);\
		valuesf[i] = values[i];\
	}\
\
	ptr->set_values(xyz, valuesf);\
}\
Type TypeName##Grid_get_value(Grid<Type>* ptr, int x, int y, int z){ return ptr->get_value(Eigen::Vector3i(x, y, z)); }\
void TypeName##Grid_set_value(Grid<Type>* ptr, int x, int y, int z, Type v){ ptr->set_value(Eigen::Vector3i(x, y, z), v); }\
void TypeName##Grid_filter(Grid<Type>* ptr, int width, int iterations, int type){ ptr->filter(width, iterations, type); }\
\
void TypeName##Grid_bounding_box(Grid<Type>* ptr, int* min, int* max)\
{\
	std::tuple<Eigen::Vector3i, Eigen::Vector3i> bb = ptr->bounding_box();\
	\
	Eigen::Vector3i bbmin = std::get<0>(bb);\
	Eigen::Vector3i bbmax = std::get<1>(bb);\
	\
	min[0] = bbmin.x();\
	min[1] = bbmin.y();\
	min[2] = bbmin.z();\
	\
	max[0] = bbmax.x();\
	max[1] = bbmax.y();\
	max[2] = bbmax.z();\
}\
\
void TypeName##Grid_set_transform(Grid<Type>* ptr, float* mat)\
{\
	Eigen::Matrix4f matf(mat);\
	ptr->set_transform(matf.cast<double>());\
}\
\
void TypeName##Grid_get_transform(Grid<Type>* ptr, float* mat)\
{\
	Eigen::Matrix4f matf = ptr->get_transform().cast<float>();\
	std::copy(matf.data(), matf.data() + matf.size(), mat);\
}\
\
Grid<Type>* TypeName##Grid_resample(Grid<Type>* ptr, float scale){	return ptr->resample(scale); }\
\
void TypeName##Grid_get_dense(Grid<Type>* ptr, int* min, int* max, Type* results)\
{\
	std::vector<Type> dense = ptr->get_dense(Eigen::Vector3i(min[0], min[1], min[2]), Eigen::Vector3i(max[0], max[1], max[2]));\
	std::copy(dense.begin(), dense.end(), results);\
}\
\
void TypeName##Grid_get_background(Grid<Type>* ptr, Type* v) \
{ \
	*v = ptr->m_grid->background(); \
} \
char* TypeName##Grid_get_name(Grid<Type>* ptr)\
{\
	ULONG ulSize = strlen(ptr->get_name().c_str()) + sizeof(char);\
	char* pszReturn = NULL;\
	\
	pszReturn = (char*)::CoTaskMemAlloc(ulSize);\
	strcpy_s(pszReturn, ulSize, ptr->get_name().c_str());\
	\
	return pszReturn;\
}\
void TypeName##Grid_set_name(Grid<Type>* ptr, const char* name){ ptr->set_name(std::string(name));}\
void TypeName##Grid_set_grid_class(Grid<Type>* ptr, int gclass) \
{\
	ptr->m_grid->setGridClass((openvdb::GridClass)gclass); \
}\
int TypeName##Grid_get_grid_class(Grid<Type>* ptr) \
{\
	return (int)ptr->m_grid->getGridClass(); \
}\
char* TypeName##Grid_get_type(Grid<Type>* ptr)\
{\
ULONG ulSize = strlen(ptr->m_grid->type().c_str()) + sizeof(char); \
char* pszReturn = NULL; \
\
pszReturn = (char*)::CoTaskMemAlloc(ulSize); \
strcpy_s(pszReturn, ulSize, ptr->get_name().c_str()); \
\
return pszReturn; \
}\
unsigned long TypeName##Grid_get_active_values_size(Grid<Type>* ptr) \
{ \
	return ptr->get_active_voxels().size() * 3; \
} \
 \
void TypeName##Grid_get_active_values(Grid<Type>* ptr, int* buffer) \
{ \
	auto active = ptr->get_active_voxels(); \
	 \
	ULONG ulSize = active.size() * sizeof(Eigen::Vector3i); \
	memcpy(buffer, active.data(), ulSize); \
} \
 \
void TypeName##Grid_erode(Grid<Type>* ptr, int iterations) \
{ \
	ptr->erode(iterations); \
} \
 \
void TypeName##Grid_get_neighbours(Grid<Type>* ptr, int* coords, Type* neighbours) \
{ \
	auto vals = ptr->get_neighbourhood(Eigen::Vector3i(coords)); \
	memcpy(neighbours, vals.data(), sizeof(Type) * 27); \
} \
 \
bool TypeName##Grid_get_active_state(Grid<Type>* ptr, int* xyz)\
{ \
	return ptr->get_active_state(Eigen::Vector3i(xyz)); \
} \
 \
void TypeName##Grid_get_active_state_many(Grid<Type>* ptr, int n, int* xyz, int* states) \
{ \
	std::vector<Eigen::Vector3i> xyz_vec(n); \
	for (size_t i = 0; i < n; ++i) \
		xyz_vec[i] = Eigen::Vector3i(xyz[i * 3]); \
	 \
	auto res = ptr->get_active_state(xyz_vec); \
	for (size_t i = 0; i < n; ++i) \
		states[i] = res[i] ? 1 : 0; \
} \
 \
void TypeName##Grid_set_active_state(Grid<Type>* ptr, int* xyz, bool state) \
{ \
	ptr->set_active_state(Eigen::Vector3i(xyz), state); \
} \
 \
void TypeName##Grid_set_active_state_many(Grid<Type>* ptr, int n, int* xyz, int* states) \
{ \
	std::vector<bool> state_vec(n); \
	std::vector<Eigen::Vector3i> xyz_vec(n); \
	for (size_t i = 0; i < n; ++i) \
	{ \
		state_vec[i] = states[i] > 0; \
		xyz_vec[i] = Eigen::Vector3i(xyz[i * 3], xyz[i * 3 + 1], xyz[i * 3 + 2]); \
	} \
	ptr->set_active_state(xyz_vec, state_vec); \
} \
void TypeName##Grid_difference(Grid<Type>* ptr0, Grid<Type>* ptr1) {	openvdb::tools::csgDifference(*(ptr0->m_grid), *(ptr1->m_grid)); }\
void TypeName##Grid_union(Grid<Type>* ptr0, Grid<Type>* ptr1) {	openvdb::tools::csgUnion(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_intersection(Grid<Type>* ptr0, Grid<Type>* ptr1) {	openvdb::tools::csgIntersection(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_max(Grid<Type>* ptr0, Grid<Type>* ptr1) {	openvdb::tools::compMax(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_min(Grid<Type>* ptr0, Grid<Type>* ptr1) {	openvdb::tools::compMin(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_sum(Grid<Type>* ptr0, Grid<Type>* ptr1) { openvdb::tools::compSum(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_diff(Grid<Type>* ptr0, Grid<Type>* ptr1) { DeepSight::compDiff(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_ifzero(Grid<Type>* ptr0, Grid<Type>* ptr1) { DeepSight::compIfZero(*(ptr0->m_grid), *(ptr1->m_grid)); } \
void TypeName##Grid_mul(Grid<Type>* ptr0, Grid<Type>* ptr1) { openvdb::tools::compMul(*(ptr0->m_grid), *(ptr1->m_grid)); } \

namespace DeepSight
{
	EXPORT_COMMON_C(Float, float)
	EXPORT_SCALAR_C(Float, float)

	Grid<float>* FloatGrid_from_mesh(int num_verts, float* verts, int num_faces, int* faces, float* transform, float isovalue, float exteriorBandWidth, float interiorBandWidth)
	{
		openvdb::Mat4d mat(transform);
		openvdb::math::Transform::Ptr xform = openvdb::math::Transform::createLinearTransform(mat);

		std::vector<openvdb::Vec4I> tris;
		for (int i = 0; i < num_faces; ++i)
			tris.push_back(openvdb::Vec4I(faces[i * 3], faces[i * 3 + 1], faces[i * 3 + 2], openvdb::util::INVALID_IDX));

		std::vector<openvdb::Vec3f> vertsf;
		for (int i = 0; i < num_verts; ++i)
			vertsf.push_back(openvdb::Vec3f(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]));

		Grid<float>::Ptr grid = from_mesh(vertsf, tris, *xform, isovalue, exteriorBandWidth, interiorBandWidth);
		return new Grid<float>(*grid->duplicate());
	}

	Grid<float>* FloatGrid_from_points(int num_points, float* points, float radius, float voxelsize)
	{
		ParticleList plist;

		for (int i = 0; i < num_points; ++i)
		{
			plist.add(openvdb::Vec3R(points[i * 3], points[i * 3 + 1], points[i * 3 + 2]), radius);
		}

		auto grid = openvdb::createLevelSet<openvdb::FloatGrid>(voxelsize);
		grid->setTransform(openvdb::math::Transform::createLinearTransform(voxelsize));
		
		openvdb::tools::ParticlesToLevelSet<openvdb::FloatGrid> pgrid(*grid);

		pgrid.setGrainSize(4);
		pgrid.rasterizeSpheres(plist);
		pgrid.finalize();

		auto dgrid = new Grid<float>();
		dgrid->m_grid = grid;
		return dgrid;
	}

	void FloatGrid_sdf_to_fog(Grid<float>* ptr, float cutoffDistance)
	{
		openvdb::tools::sdfToFogVolume(*(ptr->m_grid), cutoffDistance);
	}

	void FloatGrid_prune(Grid<float>* ptr, float tolerance)
	{
		ptr->m_grid->pruneGrid(tolerance);
	}

	//EXPORT_COMMON_C(Int, int)

	//EXPORT_COMMON_C(Double, double)
	//EXPORT_SCALAR_C(Double, double)

}


