#define WIN32_LEAN_AND_MEAN
// dllmain.cpp : Defines the entry point for the DLL application.
#include "windows.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#define XSTR(x) STR(x)
#define STR(x) #x
#define _VERSION XSTR(_DEEPSIGHT_VERSION)

#pragma message ("deepsight v" _VERSION)

namespace DeepSight
{
    const char* VERSION = _VERSION;

    const char* get_version()
    {
        return VERSION;
    }
}

