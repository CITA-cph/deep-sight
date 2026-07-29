#pragma once
//#define UNICODE
//#define _UNICODE

#define _CRT_SECURE_NO_WARNINGS
#define THREADED

// ---------------------------------------------------------------------------
// Export macro
//
// The build defines DEEPSIGHT_EXPORT=__declspec(dllexport) via the project
// preprocessor settings. That works when building the DLL, but any consumer
// that includes these headers without setting the macro fails to compile with
// a confusing "missing type specifier" error. Provide a sensible fallback so
// the headers are self-contained: importers get __declspec(dllimport) unless
// they explicitly opt in to building the library.
// ---------------------------------------------------------------------------
#ifndef DEEPSIGHT_EXPORT
#  if defined(_WIN32)
#    ifdef DEEPSIGHT_BUILD
#      define DEEPSIGHT_EXPORT __declspec(dllexport)
#    else
#      define DEEPSIGHT_EXPORT __declspec(dllimport)
#    endif
#  else
#    define DEEPSIGHT_EXPORT __attribute__((visibility("default")))
#  endif
#endif

// Every function exported through the C ABI must use the same calling
// convention the C# DllImport declarations specify (CallingConvention.Cdecl).
// On x64 Windows there is only one convention so this is a no-op today, but it
// makes the contract explicit and keeps a future x86 build from silently
// corrupting the stack.
#if defined(_WIN32)
#  define DEEPSIGHT_CALL __cdecl
#else
#  define DEEPSIGHT_CALL
#endif
