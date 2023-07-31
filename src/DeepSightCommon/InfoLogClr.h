#pragma once

#include "InfoLog.h"
#include "KnotClr.h"
#pragma managed(push, off)
#include "ReadWrite.h"
#pragma managed(pop)

#include <msclr\marshal_cppstd.h>

namespace DeepSightCommon
{
	public ref class InfoLog
	{
	public:

		static InfoLog^ Read(System::String^ file_path, System::Boolean verbose)
		{
			std::string file_path_native = msclr::interop::marshal_as<std::string>(file_path);

			//RawLam::InfoLog::Ptr ptr = DeepSight::load_infolog(file_path_native, verbose);
			RawLam::InfoLog::Ptr loaded = DeepSight::load_infolog(file_path_native, verbose);
			RawLam::InfoLog* ptr = new RawLam::InfoLog(std::move(*loaded));

			/*
			RawLam::InfoLog* ptr = new RawLam::InfoLog(
				std::move(
					*DeepSight::load_infolog(
						file_path_native,
						verbose)));
			*/

			return gcnew InfoLog(ptr);
		}

		static InfoLog^ Read(System::String^ file_path)
		{
			return Read(file_path, false);
		}

		InfoLog(RawLam::InfoLog* ptr)
		{
			m_infolog = ptr;

			m_knots = gcnew array< Knot^ >(m_infolog->knots.size());
			for (size_t i = 0; i < m_infolog->knots.size(); ++i)
			{
				m_knots[i] = gcnew Knot(m_infolog->knots[i]);
			}

			m_pith = gcnew array< System::Tuple<float, float>^ >(m_infolog->pith.size());

			for (size_t i = 0; i < m_infolog->pith.size(); ++i)
			{
				m_pith[i] = gcnew System::Tuple<float, float>(m_infolog->pith[i].x(), m_infolog->pith[i].y());
			}

			m_borders = gcnew array<array<System::Tuple<float, float>^>^>(m_infolog->border.size());
			for (size_t i = 0; i < m_infolog->border.size(); ++i)
			{
				m_borders[i] = gcnew array<System::Tuple<float, float>^>(m_infolog->border[i].size());

				for (size_t j = 0; j < m_infolog->border[i].size(); ++j)
				{
					m_borders[i][j] = gcnew System::Tuple<float, float>(m_infolog->border[i][j].x(), m_infolog->border[i][j].y());
				}
			}

			m_sapwood = gcnew array<array<System::Tuple<float, float>^>^>(m_infolog->sapwood.size());
			for (size_t i = 0; i < m_infolog->sapwood.size(); ++i)
			{
				m_sapwood[i] = gcnew array<System::Tuple<float, float>^>(m_infolog->sapwood[i].size());

				for (size_t j = 0; j < m_infolog->sapwood[i].size(); ++j)
				{
					m_sapwood[i][j] = gcnew System::Tuple<float, float>(m_infolog->sapwood[i][j].x(), m_infolog->sapwood[i][j].y());
				}
			}
		}

		~InfoLog()
		{
			if (m_infolog != nullptr)
				delete m_infolog;
		}

		property System::String^ Name
		{
			System::String^ get() {
				return gcnew System::String(m_infolog->name.c_str());
			}
			void set(System::String^ value)
			{
				m_infolog->name = msclr::interop::marshal_as<std::string>(value);
			}
		}

		property array<System::Tuple<float, float>^>^ Pith
		{
			array<System::Tuple<float, float>^>^ get()
			{
				return m_pith;
			}
		}

		property array<Knot^>^ Knots
		{
			array<Knot^>^ get()
			{
				return m_knots;
			}
		}

		property array<array<System::Tuple<float, float>^>^>^ Borders
		{
			array<array<System::Tuple<float, float>^>^>^ get()
			{
				return m_borders;
			}
		}

		property array<array<System::Tuple<float, float>^>^>^ Sapwood
		{
			array<array<System::Tuple<float, float>^>^>^ get()
			{
				return m_sapwood;
			}
		}

	private:

		//RawLam::InfoLog* m_infolog;
		RawLam::InfoLog* m_infolog;
		array<Knot^>^ m_knots;
		array<System::Tuple<float, float>^>^ m_pith;
		array<array<System::Tuple<float, float>^>^>^ m_borders;
		array<array<System::Tuple<float, float>^>^>^ m_sapwood;
	};
}