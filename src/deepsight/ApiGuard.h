#ifndef DEEPSIGHT_API_GUARD_H
#define DEEPSIGHT_API_GUARD_H

// ---------------------------------------------------------------------------
// Exception barrier for the C ABI.
//
// Every function in this library that is declared `extern "C"` is called from
// managed code through P/Invoke. Letting a C++ exception propagate out of such
// a function is undefined behaviour: in practice the CLR cannot unwind through
// a native frame it does not own, and the host process (Rhino/Grasshopper, or
// the Python interpreter) dies instantly with no diagnostic.
//
// This matters here because almost every OpenVDB call can throw:
//   - openvdb::IoError          bad path, truncated .vdb, permission denied
//   - openvdb::TypeError        grid type mismatch
//   - openvdb::ValueError       bad arguments
//   - std::bad_alloc            a large grid is a very real OOM candidate
//
// api_guard() catches everything, stashes the message where the caller can
// retrieve it with DeepSight_GetLastError(), and returns a caller-supplied
// fallback value. All exported entry points should be wrapped in it.
//
// Usage:
//     float FloatGrid_GetValueIs(GridBase* ptr, int x, int y, int z)
//     {
//         return api_guard([&] { ...may throw...; }, 0.0f);
//     }
//
//     void FloatGrid_SetValue(GridBase* ptr, int x, int y, int z, float v)
//     {
//         api_guard([&] { ...may throw... });
//     }
// ---------------------------------------------------------------------------

#include "config.h"

#include <exception>
#include <string>
#include <type_traits>
#include <utility>

namespace DeepSight
{
	namespace detail
	{
		/// Record a failure message for the most recent API call on this thread.
		void set_last_error(const std::string& message) noexcept;

		/// Clear the failure message for this thread.
		void clear_last_error() noexcept;

		/// Read back the failure message for this thread ("" if the last call
		/// succeeded). The returned pointer is owned by the library and stays
		/// valid until the next API call on the same thread.
		const char* get_last_error() noexcept;
	}

	/// Overload for functions that return a value.
	template <typename Fn>
	auto api_guard(Fn&& fn, decltype(std::declval<Fn&>()()) fallback) noexcept
		-> decltype(std::declval<Fn&>()())
	{
		try
		{
			detail::clear_last_error();
			return fn();
		}
		catch (const std::exception& e)
		{
			detail::set_last_error(e.what());
		}
		catch (...)
		{
			detail::set_last_error("Unknown non-standard exception");
		}
		return fallback;
	}

	/// Overload for functions that return void.
	template <typename Fn>
	void api_guard(Fn&& fn) noexcept
	{
		try
		{
			detail::clear_last_error();
			fn();
		}
		catch (const std::exception& e)
		{
			detail::set_last_error(e.what());
		}
		catch (...)
		{
			detail::set_last_error("Unknown non-standard exception");
		}
	}

#ifdef __cplusplus
	extern "C" {
#endif
		/// Returns the message from the most recent failed API call on the
		/// calling thread, or an empty string if it succeeded. The buffer is
		/// owned by the library; copy it before making another call.
		DEEPSIGHT_EXPORT const char* DEEPSIGHT_CALL DeepSight_GetLastError();

		/// Non-zero if the most recent API call on this thread failed.
		DEEPSIGHT_EXPORT int DEEPSIGHT_CALL DeepSight_HasError();
#ifdef __cplusplus
	}
#endif
}

#endif // DEEPSIGHT_API_GUARD_H
