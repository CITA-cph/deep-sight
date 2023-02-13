#define _USE_MATH_DEFINES
#define NOMINMAX

#define XSTR(x) STR(x)
#define STR(x) #x
#define _VERSION XSTR(_DEEPSIGHT_VERSION)
#pragma message ("pydeepsight v" _VERSION)

#include <cmath>
#include <ctime>    

#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h> 

#include "pyGrid.h"
#include "pyInfoLog.h"
#include "../deepsight/ReadWrite.h"

#include <openvdb/openvdb.h>
#include "pyutil.h"


namespace py = pybind11;

namespace pybind11
{
    namespace detail
    {
        template <> struct type_caster<openvdb::Vec3s> {
        public:
            PYBIND11_TYPE_CASTER(openvdb::Vec3s, const_name("openvdb::Vec3s"));

            bool load(handle src, bool) {
                PyObject* source = src.ptr();
                if (!PySequence_Check(source))
                    return false;

                Py_ssize_t length = PySequence_Length(source);
                if (length != openvdb::Vec3s::size)
                    return false;

                for (Py_ssize_t i = 0; i < length; ++i) {
                    PyObject* item = PySequence_GetItem(source, i);
                    if (item) {
                        PyObject* number = PyNumber_Float(item);
                        if (number) {
                            value(static_cast<int>(i)) = static_cast<openvdb::Vec3s::value_type>(PyFloat_AsDouble(number));
                        }
                        Py_XDECREF(number);
                    }
                    Py_XDECREF(item);

                    if (PyErr_Occurred())
                        return false;
                }
                return true;
            }

            static handle cast(openvdb::Vec3s src, return_value_policy, handle) {
                py::tuple tuple = py::make_tuple(src[0], src[1], src[2]);
                return tuple.release();
            }
        };
    }
}

PYBIND11_MODULE(_deepsight, m)
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

 


	m.doc() = "_deepsight";
	m.attr("VERSION") = py::cast(_VERSION);

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

