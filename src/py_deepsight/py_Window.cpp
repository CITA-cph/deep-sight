#include <pybind11/pybind11.h>
#include <pybind11/eigen.h>
#include <pybind11/stl.h>
#include <pybind11/numpy.h>

#include <iostream>

#include "core/Window.h"

namespace py = pybind11;

void window(py::module &m)
{
	m.def("test_open_gl", &DeepSight::TestOpenGLWindow, "Test GLFW window.");
}
