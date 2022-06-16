#include "pyGrid.cpp"
#include "pyutil.h"

template void grid<float>(py::module& m, std::string class_name);
template void grid<int>(py::module& m, std::string class_name);
template void grid<double>(py::module& m, std::string class_name);

//template void grid<bool>(py::module& m, std::string class_name);
template void grid<openvdb::Vec3f>(py::module& m, std::string class_name);
//template void grid<openvdb::Vec3d>(py::module& m, std::string class_name);
