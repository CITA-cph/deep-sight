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

		property float Radius
		{
			float get() {
				return radius;
			}
		}

		property float Length
		{
			float get() {
				return length;
			}
		}

		property float Volume
		{
			float get() {
				return volume;
			}
		}

		property int Index
		{
			int get() {
				return index;
			}
		}

		property int DeadKnotBorder
		{
			int get() {
				return dead_knot_border;
			}
		}

		property System::Tuple<float, float, float>^ Start
		{
			System::Tuple<float, float, float>^ get()
			{
				return start;
			}
		}

		property System::Tuple<float, float, float>^ End
		{
			System::Tuple<float, float, float>^ get()
			{
				return end;
			}
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

