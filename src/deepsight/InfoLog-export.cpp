#include "InfoLog-export.h"
#include "InfoLog.h"
#include "io.h"

namespace RawLam
{

	void InfoLog_Load(const char* filepath, int& n_pith, float*& pith, int& n_knots, float*& knots)
	{
		RawLam::InfoLog::Ptr ilog = DeepSight::load_infolog(filepath, false);
		n_pith = ilog->pith.size();
		n_knots = ilog->knots.size();


		pith = new float[n_pith * 2];
		knots = new float[n_knots * 6];

		//std::copy(std::begin(ilog->pith), std::end(ilog->pith), pith);

		for (int i = 0; i < ilog->pith.size(); ++i)
		{
			Eigen::Vector2f v = ilog->pith[i];

			pith[i * 2] = v.x();
			pith[i * 2 + 1] = v.y();
			//*pith[i * 3 + 2] = v.z();
		}

		for (int i = 0; i < ilog->knots.size(); ++i)
		{
			knot k = ilog->knots[i];

			knots[i * 6] = k.start.x();
			knots[i * 6 + 1] = k.start.y();
			knots[i * 6 + 2] = k.start.z();

			knots[i * 6 + 3] = k.end.x();
			knots[i * 6 + 4] = k.end.y();
			knots[i * 6 + 5] = k.end.z();
		}

	}

	void InfoLog_free(float*& ptr)
	{
		delete[] ptr;
		ptr = NULL;
	}
}