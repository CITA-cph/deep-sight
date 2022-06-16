#ifndef PYGRID_H
#define PYGRID_H

#include <pybind11/pybind11.h>

template<typename GridType>
extern void grid(pybind11::module &m);

#endif