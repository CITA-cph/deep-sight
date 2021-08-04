// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: MPL-2.0

#ifndef OPENVDB_PYUTIL_HAS_BEEN_INCLUDED
#define OPENVDB_PYUTIL_HAS_BEEN_INCLUDED

#include "openvdb/openvdb.h"
#include "openvdb/points/PointDataGrid.h"
#include <mutex>
#include <sstream>
#include <string>
#include <utility> // for std::pair


namespace pyutil {


////////////////////////////////////////


template<class GridType>
struct GridTraitsBase
{
    /// @brief Return the name of the Python class that wraps this grid type
    /// (e.g., "FloatGrid" for openvdb::FloatGrid).
    ///
    /// @note This name is not the same as GridType::type().
    /// The latter returns a name like "Tree_float_5_4_3".
    static const char* name();

    /// Return the name of this grid type's value type ("bool", "float", "vec3s", etc.).
    static const char* valueTypeName()
    {
        return openvdb::typeNameAsString<typename GridType::ValueType>();
    }

    /// @brief Return a description of this grid type.
    ///
    /// @note This name is generated at runtime for each call to descr().
    static const std::string descr()
    {
        return std::string("OpenVDB grid with voxels of type ") + valueTypeName();
    }
}; // struct GridTraitsBase


template<class GridType>
struct GridTraits: public GridTraitsBase<GridType>
{
};

/// Map a grid type to a traits class that derives from GridTraitsBase
/// and that defines a name() method.
#define GRID_TRAITS(_typ, _name) \
    template<> struct GridTraits<_typ>: public GridTraitsBase<_typ> { \
        static const char* name() { return _name; } \
    }

GRID_TRAITS(openvdb::FloatGrid, "FloatGrid");
GRID_TRAITS(openvdb::Vec3SGrid, "Vec3SGrid");
GRID_TRAITS(openvdb::BoolGrid, "BoolGrid");
#ifdef PY_OPENVDB_WRAP_ALL_GRID_TYPES
GRID_TRAITS(openvdb::DoubleGrid, "DoubleGrid");
GRID_TRAITS(openvdb::Int32Grid, "Int32Grid");
GRID_TRAITS(openvdb::Int64Grid, "Int64Grid");
GRID_TRAITS(openvdb::Vec3IGrid, "Vec3IGrid");
GRID_TRAITS(openvdb::Vec3DGrid, "Vec3DGrid");
GRID_TRAITS(openvdb::points::PointDataGrid, "PointDataGrid");
#endif

#undef GRID_TRAITS

}


#endif // OPENVDB_PYUTIL_HAS_BEEN_INCLUDED
