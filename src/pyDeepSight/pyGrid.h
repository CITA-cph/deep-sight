#ifndef PYGRID_H
#define PYGRID_H

#define _USE_MATH_DEFINES
#define NOMINMAX
#include <cmath>
#include <ctime>    

#include <pybind11/pybind11.h>

#include "../deepsight/Grid.h"

template<typename GridType>
extern void grid(pybind11::module& m, std::string class_name);


#endif