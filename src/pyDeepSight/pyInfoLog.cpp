#include "pyInfoLog.h"

#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h> 

#include <iostream>

#include "../deepsight/InfoLog.h"
#include "pyutil.h"

namespace py = pybind11;


void infolog(py::module& m)
{
	py::class_<RawLam::knot, std::shared_ptr<RawLam::knot>>(m, "Knot")
		.def(py::init())
		.def_readonly("index", &RawLam::knot::index)
		.def_readonly("start", &RawLam::knot::start)
		.def_readonly("end", &RawLam::knot::end)
		.def_readonly("dead_knot_border", &RawLam::knot::dead_knot_border)
		.def_readonly("radius", &RawLam::knot::radius)
		.def_readonly("length", &RawLam::knot::length)
		.def_readonly("volume", &RawLam::knot::volume)
		.def("__repr__", &RawLam::knot::to_string);

	py::class_<RawLam::InfoLog, std::shared_ptr<RawLam::InfoLog>>(m, "InfoLog")
		.def(py::init())
		.def_readwrite("name", &RawLam::InfoLog::name)
		.def_readonly("pith", &RawLam::InfoLog::pith)
		.def_readonly("knots", &RawLam::InfoLog::knots)
		.def("__repr__", &RawLam::InfoLog::to_string);
	;
}