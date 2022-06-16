#include "InfoLog.h"

namespace RawLam
{

	std::string knot::to_string()
	{
		return "Knot (" + std::to_string(index) + ")";
	}

	InfoLog::InfoLog()
	{
		pith = std::vector<Eigen::Vector2f>();
		knots = std::vector<knot>();
	}

	std::string InfoLog::to_string()
	{
		return "InfoLog (" + name + ")";
	}

}