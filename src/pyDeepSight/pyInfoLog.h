#ifndef PYINFOLOG_H
#define PYINFOLOG_H

#define _USE_MATH_DEFINES
#define NOMINMAX
#include <cmath>
#include <ctime>    

#include <pybind11/pybind11.h>

#include "../deepsight/InfoLog.h"

extern void infolog(pybind11::module& m);


#endif
