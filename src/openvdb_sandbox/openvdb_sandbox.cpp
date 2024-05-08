#include "openvdb/openvdb.h"

#include <openvdb/Platform.h>
#include <cmath>
#include <ctime>    

#include "openvdb/openvdb.h"
#include <openvdb/tools/Interpolation.h>
#include <openvdb/tools/Filter.h>
#include <openvdb/tools/VolumeToMesh.h>
#include <openvdb/tools/GridTransformer.h>
#include <openvdb/tools/GridOperators.h>
#include <openvdb/tools/Composite.h>
#include <openvdb/tools/Composite.h>

#include "openvdb/Types.h"

#include <Eigen/Core>
#pragma warning(disable:4146)

std::string log_path = R"(C:\Users\tsvi\OneDrive - Det Kongelige Akademi\03_Projects\2019_RawLam\Data\Microtec\20220301.142735.DK.feb.log01_char\20220301.142735.DK.feb.log01_char.vdb)";
double minimum_threshold = 0.01;

// Get current date/time, format is YYYY-MM-DD.HH:mm:ss
const std::string currentDateTime() {
    time_t     now = time(0);
    struct tm  tstruct;
    char       buf[80];
    localtime_s(&tstruct, &now);
    // Visit http://en.cppreference.com/w/cpp/chrono/c/strftime
    // for more information about date/time format
    strftime(buf, sizeof(buf), "%Y-%m-%d-%H_%M_%S", &tstruct);

    return buf;
}
/*
template <typename GridType>
void get_lamella_grid(double dx, double dy, double dz, double voxel_size, openvdb::math::Mat4d xform, std::string name="test")
{
    //using GridType = typename openvdb::FloatGrid;
    using ValueT = typename GridType::ValueType;

    openvdb::initialize();

    // Open log VDB
    openvdb::io::File log_file(log_path);
    log_file.open();
    typename GridType::Ptr log = openvdb::gridPtrCast<GridType>(log_file.readGrid("density"));
    log_file.close();

    // Create lamella grid
    typename GridType::Ptr lamella =
    //  GridType::create(0.0);
    // Set grid transform
    openvdb::math::Transform::Ptr linearTransform =
        openvdb::math::Transform::createLinearTransform(voxel_size); // voxel size
    linearTransform->postTranslate(openvdb::Vec3d(-dx / 2, -dy / 2, 0));
    linearTransform->postMult(xform);
    lamella->setTransform(linearTransform);

    typename GridType::Accessor lam_access = lamella->getAccessor();
    typename GridType::ConstAccessor src_access = log->getConstAccessor();

    int vx = (int)ceil(dx / voxel_size), vy = (int)ceil(dy / voxel_size), vz = (int)ceil(dz / voxel_size);
    int hvx = vx / 2, hvy = vy / 2, hvz = vz / 2;

    // Instantiate the GridSampler template on the accessor type and on
    // a box sampler for accelerated trilinear interpolation.
    openvdb::tools::GridSampler<GridType::ConstAccessor, openvdb::tools::BoxSampler>
        logSampler(src_access, log->transform());

    openvdb::tools::GridSampler<GridType::Accessor, openvdb::tools::BoxSampler>
        lamSampler(lam_access, lamella->transform());

    openvdb::Coord ijk;
    int& i = ijk[0], & j = ijk[1], & k = ijk[2];

    for (i = 0; i < vx; ++i)
        for (j = 0; j < vy; ++j)
            for (k = 0; k < vz; ++k)
            {
                openvdb::Vec3d wPt = linearTransform->indexToWorld(ijk);
                ValueT val = ValueT(logSampler.wsSample(wPt));
                if (val > minimum_threshold)
                    lam_access.setValue(ijk, val);
            }

    lamella->tree().prune();
    lamella->pruneGrid(voxel_size);

    lamella->setName("density");
    std::string dt = currentDateTime();

    std::cout << dt << std::endl;

    openvdb::io::File output("C:/tmp/" + name + "_" + dt + ".vdb");

    // Add the grid pointer to a container.
    openvdb::GridPtrVec grids;
    grids.push_back(lamella);

    // Write out the contents of the container.
    output.write(grids);
    output.close();
}
*/

typedef void (*AllocateOutput)(void* output, int num_verts, int num_tris, int num_quads);
typedef void (*AddVert)(void* output, const float co[3]);
typedef void (*AddTri)(void* output, const unsigned int tri[3]);
typedef void (*AddQuad)(void* output, const unsigned int quad[4]);

void m_mesh_volume(
    int num_coords,
    int* coords,
    float *values,
    float isovalue,
    AllocateOutput alloc_output,
    AddVert add_vert,
    AddTri add_tri,
    AddQuad add_quad
)
{
    openvdb::FloatGrid::Ptr grid0 = openvdb::FloatGrid::create(0.0f);
    openvdb::FloatGrid::Accessor acc = grid0->getAccessor();

    for (int i = 0; i < num_coords; ++i)
    {
        openvdb::Coord coord(coords[i * 3], coords[i * 3 + 1], coords[i * 3 + 2]);
        acc.setValue(coord, values[i]);
    }

    openvdb::tools::VolumeToMesh mesher(isovalue);
    mesher(*grid0);

    openvdb::Coord ijk;
    std::vector<Eigen::Vector3f> vertsVector;
    std::vector<Eigen::Vector4i> quadsVector;
    std::vector<Eigen::Vector3i> trisVector;


    // Copy primitives
    openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();
    int nTris = 0, nQuads = 0;

    for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i)
    {
        nTris += polygonPoolList[i].numTriangles();
        nQuads += polygonPoolList[i].numQuads();
    }

    alloc_output(nullptr, mesher.pointListSize(), nTris, nQuads);

    for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
    {
        const openvdb::Vec3s& vert = mesher.pointList()[i];
        add_vert(nullptr, vert.asPointer());

        vertsVector.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
    }

    for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i)
    {
        const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
        for (size_t j = 0, I = polygons.numQuads(); j < I; ++j)
        {
            const openvdb::Vec4I& quad = polygons.quad(j);
            add_quad(nullptr, quad.asPointer());
            quadsVector.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
        }

        for (size_t j = 0, I = polygons.numTriangles(); j < I; ++j)
        {
            const openvdb::Vec3I& tri = polygons.triangle(j);
            add_tri(nullptr, tri.asPointer());
            trisVector.push_back(Eigen::Vector3i((int)tri.x(), (int)tri.y(), (int)tri.z()));
        }
    }

    std::cout << "num verts: " << vertsVector.size() << std::endl;
    std::cout << "num tris:  " << trisVector.size() << std::endl;
    std::cout << "num quads: " << quadsVector.size() << std::endl;

}

int main()
{
    std::cout << "RawLam - OpenVDB sandbox" << std::endl;
    std::cout << "Tom Svilans, May 2022" << std::endl;

    // Metric units
    openvdb::math::Mat4d transform = openvdb::math::Mat4d::identity();
    //transform.preScale(openvdb::math::Vec3d(2, 3, 2));
    transform.postRotate(openvdb::math::Z_AXIS, M_PI / 10.0);
    transform.postTranslate(openvdb::math::Vec3d(0, 0, 1.5));

    transform = openvdb::math::Mat4d(
        openvdb::Vec4d(1, 0, 0, 0),
        openvdb::Vec4d(0, 1, 0, 0.0),
        openvdb::Vec4d(0, 0, 1, 0),
        openvdb::Vec4d(0, 0, 0, 1)
        );


    //get_lamella_grid<openvdb::FloatGrid>(0.5, 0.02, 5.0, 0.003, transform.transpose(), "board");

    transform = openvdb::math::Mat4d(
        openvdb::Vec4d(1, 0, 0, 0.08),
        openvdb::Vec4d(0, 1, 0, 0.025),
        openvdb::Vec4d(0, 0, 1, 1.0),
        openvdb::Vec4d(0, 0, 0, 1)
    );

    //get_lamella_grid<openvdb::FloatGrid>(0.14, 0.02, 2.1, 0.003, transform.transpose(), "lamella");

    transform = openvdb::math::Mat4d(
        openvdb::Vec4d(1, 0, 0, -0.07),
        openvdb::Vec4d(0, 1, 0, 0.025),
        openvdb::Vec4d(0, 0, 1, 0.3),
        openvdb::Vec4d(0, 0, 0, 1)
    );

    //get_lamella_grid<openvdb::FloatGrid>(0.14, 0.02, 2.1, 0.003, transform.transpose(), "lamella");
    openvdb::FloatGrid::Ptr grid0 = openvdb::FloatGrid::create(0.0f);
    openvdb::FloatGrid::Ptr grid1 = openvdb::FloatGrid::create(0.0f);

    auto acc = grid0->getAccessor();
    acc.setValue(openvdb::math::Coord(20, 20, 10), 1.0f);
    acc.setValue(openvdb::math::Coord(20, 21, 10), 1.0f);
    acc.setValue(openvdb::math::Coord(21, 21, 10), 1.0f);
    acc.setValue(openvdb::math::Coord(22, 21, 10), 1.0f);

    openvdb::tools::VolumeToMesh mesher(0.5);
    mesher(*grid0);

    openvdb::Coord ijk;
    std::vector<Eigen::Vector3f> verts; 
    std::vector<Eigen::Vector4i> quads; 
    std::vector<Eigen::Vector3i> tris;

    for (size_t i = 0, N = mesher.pointListSize(); i < N; ++i)
    {
        const openvdb::Vec3s& vert = mesher.pointList()[i];
        verts.push_back(Eigen::Vector3f((float)vert.x(), (float)vert.y(), (float)vert.z()));
    }

    // Copy primitives
    openvdb::tools::PolygonPoolList& polygonPoolList = mesher.polygonPoolList();

    for (size_t i = 0, N = mesher.polygonPoolListSize(); i < N; ++i)
    {
        const openvdb::tools::PolygonPool& polygons = polygonPoolList[i];
        for (size_t j = 0, I = polygons.numQuads(); j < I; ++j)
        {
            const openvdb::Vec4I& quad = polygons.quad(j);
            quads.push_back(Eigen::Vector4i((int)quad.x(), (int)quad.y(), (int)quad.z(), (int)quad.w()));
        }

        for (size_t j = 0, I = polygons.numTriangles(); j < I; ++j)
        {
            const openvdb::Vec3I& tri = polygons.triangle(j);
            tris.push_back(Eigen::Vector3i((int)tri.x(), (int)tri.y(), (int)tri.z()));
        }
    }

    std::cout << "Num verts: " << verts.size() << std::endl;
    std::cout << "Num tris:  " << tris.size() << std::endl;
    std::cout << "Num quads: " << quads.size() << std::endl;

    // openvdb::tools::compMin(grid0, grid1);

    std::cout << transform << std::endl;

    std::cin.get();
}

#ifdef __cplusplus
extern "C" {
#endif

    _declspec (dllexport) void mesh_volume(
        int num_coords,
        int* coords,
        float* values,
        float isovalue,
        AllocateOutput alloc_output, 
        AddVert add_vert, 
        AddTri add_tri, 
        AddQuad add_quad
    )
    {
        m_mesh_volume(
            num_coords,
            coords,
            values,
            isovalue,
            alloc_output,
            add_vert,
            add_tri,
            add_quad
        );
    }
#ifdef __cplusplus
}
#endif



