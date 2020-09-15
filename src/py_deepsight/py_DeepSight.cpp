#include <pybind11/pybind11.h>

#include <openvdb/openvdb.h>

namespace py = pybind11;

void grid(py::module &m);

PYBIND11_MODULE(_deepsight, m) 
{
    m.doc() = "deepsight";
	grid(m);
	
}

