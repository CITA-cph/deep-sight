#include "InfoLog-export.h"
#include "InfoLog.h"
#include "ReadWrite.h"

namespace RawLam
{

	void InfoLog_Load(const char* filepath, int& n_pith, float*& pith, int& n_knots, float*& knots)
	{
		RawLam::InfoLog::Ptr ilog = DeepSight::load_infolog(filepath, false);
		n_pith = ilog->pith.size();
		n_knots = ilog->knots.size();

		unsigned int knot_size = 11;

		pith = new float[n_pith * 2];
		knots = new float[n_knots * knot_size];

		size_t ii = 0;
		for (size_t i = 0; i < ilog->pith.size(); ++i)
		{
			ii = i * 2;
			Eigen::Vector2f v = ilog->pith[i];

			pith[ii] = v.x();
			pith[ii + 1] = v.y();
		}

		for (size_t i = 0; i < ilog->knots.size(); ++i)
		{
			ii = i * knot_size;
			knot k = ilog->knots[i];

			knots[ii + 0] = (float)k.index;
			knots[ii + 1] = k.start.x();
			knots[ii + 2] = k.start.y();
			knots[ii + 3] = k.start.z();

			knots[ii + 4] = k.end.x();
			knots[ii + 5] = k.end.y();
			knots[ii + 6] = k.end.z();

			knots[ii + 7] = k.dead_knot_border;
			knots[ii + 8] = k.radius;
			knots[ii + 9] = k.length;
			knots[ii + 10] = k.volume;
		}

	}

	void InfoLog_free(float*& ptr)
	{
		delete[] ptr;
		ptr = NULL;
	}
}