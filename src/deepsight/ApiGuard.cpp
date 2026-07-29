#include "ApiGuard.h"

namespace DeepSight
{
	namespace detail
	{
		namespace
		{
			// Thread-local so that concurrent Grasshopper solves (or a threaded
			// Python caller) cannot clobber each other's error state.
			// Function-local static avoids the static-initialisation-order
			// problem that a namespace-scope thread_local std::string would
			// have inside a DLL.
			std::string& error_slot() noexcept
			{
				static thread_local std::string slot;
				return slot;
			}
		}

		void set_last_error(const std::string& message) noexcept
		{
			try
			{
				error_slot() = message;
			}
			catch (...)
			{
				// If we cannot even store the message (bad_alloc while handling
				// bad_alloc), swallow it. Reporting nothing is strictly better
				// than terminating.
			}
		}

		void clear_last_error() noexcept
		{
			try
			{
				error_slot().clear();
			}
			catch (...)
			{
			}
		}

		const char* get_last_error() noexcept
		{
			try
			{
				return error_slot().c_str();
			}
			catch (...)
			{
				return "";
			}
		}
	}

	extern "C"
	{
		const char* DEEPSIGHT_CALL DeepSight_GetLastError()
		{
			return detail::get_last_error();
		}

		int DEEPSIGHT_CALL DeepSight_HasError()
		{
			const char* msg = detail::get_last_error();
			return (msg != nullptr && msg[0] != '\0') ? 1 : 0;
		}
	}
}
