#define _USE_MATH_DEFINES
#define NOMINMAX
#include <cmath>
#include <ctime>    

#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h> 

#include "pyGrid.h"
#include "pyInfoLog.h"
#include "../deepsight/io.h"

#include <openvdb/openvdb.h>
#include "pyutil.h"


namespace py = pybind11;



PYBIND11_MODULE(pyrawlam, m)
{
	//py::class_<openvdb::Vec3f>(m, "Vec3f", py::buffer_protocol())
	//	.def_buffer([](openvdb::Vec3f& v) -> py::buffer_info {
	//	return py::buffer_info(
	//		v.asPointer(),                               /* Pointer to buffer */
	//		sizeof(float),                          /* Size of one scalar */
	//		py::format_descriptor<float>::format(), /* Python struct-style format descriptor */
	//		1,                                      /* Number of dimensions */
	//	{ v.asPointer() },                 /* Buffer dimensions */
	//	{ sizeof(float) }
	//	);
	//});

	m.doc() = "pydeepsight";
	grid<double>(m, "DoubleGrid");
	grid<float>(m, "FloatGrid");
	grid<int>(m, "IntGrid");
	grid<openvdb::Vec3f>(m, "VectorGrid");
	infolog(m);

	m.def("load_scalar_tiff", &DeepSight::load_scalar_tiff, "Loads a multipage TIFF with scalar data. Returns FloatGrid.");
	m.def("load_vector_tiff", &DeepSight::load_vector_tiff, "Loads a multipage TIFF with scalar data. Returns VectorGrid.");
	m.def("load_infolog", &DeepSight::load_infolog, "Loads a InfoLog TIFF. Returns nothing at the moment.");

	//grid<bool>(m, "BoolGrid");

#ifdef VERSION_INFO
	m.attr("__version__") = VERSION_INFO;
#else
	m.attr("__version__") = "dev";
#endif
}

