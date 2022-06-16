#ifndef INFOLOG_EXPORT_H
#define INFOLOG_EXPORT_H

#define _USE_MATH_DEFINES
#include <cmath>
#include <Eigen/Core>
#include <vector>

#include "InfoLog.h"


namespace RawLam
{
#ifdef __cplusplus
extern "C" {
#endif

	RAWLAM_EXPORT void InfoLog_Load(const char* filepath, int& n_pith, float*& pith, int& n_knots, float*& knots);
	RAWLAM_EXPORT void InfoLog_free(float*& ptr);

#ifdef __cplusplus
}
#endif
}
#endif