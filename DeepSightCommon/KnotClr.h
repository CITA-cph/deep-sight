#pragma once

#include "InfoLog.h"


namespace DeepSightCommon
{
	public ref class Knot
	{

	public:
		Knot(RawLam::knot knot)
		{
			radius = knot.radius;
			length = knot.length;
			volume = knot.volume;
			index = knot.index;
			dead_knot_border = knot.dead_knot_border;

			start = gcnew System::Tuple<float, float, float>(knot.start.x(), knot.start.y(), knot.start.z());
			end = gcnew System::Tuple<float, float, float>(knot.end.x(), knot.end.y(), knot.end.z());
		}
		
	private:
		float radius;
		float length;
		float volume;
		int index;
		int dead_knot_border;
		System::Tuple<float, float, float>^ start;
		System::Tuple<float, float, float>^ end;
	};
}

