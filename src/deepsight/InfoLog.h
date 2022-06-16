#ifndef INFOLOG_H
#define INFOLOG_H

#define _USE_MATH_DEFINES
#include <cmath>
#include <Eigen/Core>
#include <vector>


namespace RawLam
{
	struct knot
	{
		float radius;
		float length;
		float volume;
		int index;
		int dead_knot_border;
		Eigen::Vector3f start;
		Eigen::Vector3f end;

		std::string to_string();
	};

	class InfoLog
	{
	public:

		using Ptr = std::shared_ptr<InfoLog>;

		InfoLog();

		std::vector<Eigen::Vector2f> pith;
		std::vector<knot> knots;

		std::string name;
		std::string to_string();

	};
}



#endif