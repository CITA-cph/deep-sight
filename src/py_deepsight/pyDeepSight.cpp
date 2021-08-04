#include <pybind11/pybind11.h>

#include <openvdb/openvdb.h>
#include "pyGrid.h"

namespace py = pybind11;

//void window(py::module &m);


PYBIND11_MODULE(_deepsight, m) 
{
    m.doc() = "deepsight";
	//grid<openvdb::FloatGrid>(m);
	grid<openvdb::FloatGrid>(m);
	//window(m);

}

