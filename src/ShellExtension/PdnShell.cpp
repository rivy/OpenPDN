/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

#pragma strict_gs_check(on)
#pragma warning (disable: 4530)
#include <windows.h>
#include <comcat.h>
#include "PdnShell.h"
#include "ClassFactory.h"

#pragma data_seg(".text")
#define INITGUID
#include <initguid.h>
#include <shlguid.h>
#include "PdnGuid.h"
#pragma data_seg()

#include <iostream>
using namespace std;

HINSTANCE g_hInstance;
volatile LONG g_lRefCount;

#ifndef NDEBUG
void TraceOut(const char *szFormat, ...)
{
    va_list marker;
    va_start(marker, szFormat);

    char buffer[2048];
    _vsnprintf_s(buffer, sizeof(buffer), (sizeof(buffer) / sizeof(buffer[0])) - 1, szFormat, marker);

    OutputDebugStringA(buffer);

	FILE *flog = NULL;
	errno_t err = fopen_s(&flog, "C:\\log.txt", "a");

	if (0 == err && NULL != flog)
	{
		fprintf(flog, "%s\n", buffer);
		fclose(flog);
	}
}
#endif

// Only used when calling TraceOut
const WCHAR *GuidToString(GUID guid)
{
    static WCHAR szGuid[128];
    StringFromGUID2(guid, szGuid, 128);
    return szGuid;
}

extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReasonForCall, LPVOID lpvReserved)
{
    TraceEnter();
    TraceOut("hInstance=%p, dwReasonForCall=%u, lpvReserved=%p", hInstance, dwReasonForCall, lpvReserved);

    switch (dwReasonForCall)
    {
        case DLL_PROCESS_ATTACH:
            g_hInstance = hInstance;
            break;

        case DLL_THREAD_ATTACH:
            break;

        case DLL_THREAD_DETACH:
            break;

        case DLL_PROCESS_DETACH:
            g_hInstance = NULL;
            break;
    }

    TraceLeave();
    return TRUE;
}

STDAPI DllCanUnloadNow()
{
    HRESULT hr;

    TraceEnter();

    if (0 == g_lRefCount)
    {
        hr = S_OK;
    }
    else
    {
        hr = S_FALSE;
    }

    TraceLeaveHr(hr);
    return hr;
}

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID *ppv)
{
    TraceEnter();
    CClassFactory *pFactory = NULL;
    HRESULT hr = S_OK;

    TraceOut("rclsid=%s, riid=%s, ppv=%p", GuidToString(rclsid), GuidToString(riid), ppv);

    if (SUCCEEDED(hr))
    {
        if (NULL == ppv)
        {
            hr = E_INVALIDARG;
        }
    }

    // TODO: Why aren't we checking riid at all?

    if (SUCCEEDED(hr))
    {
        TraceOut("Calling IsEqualCLSID(rclsid, CLSID_PdnShellExtension");
        if (!IsEqualCLSID(rclsid, CLSID_PdnShellExtension))
        {
            hr = CLASS_E_CLASSNOTAVAILABLE;
        }
    }

    if (SUCCEEDED(hr))
    {
        TraceOut("Creating CClassFactory instance");
        pFactory = new CClassFactory(rclsid);

        if (NULL == pFactory)
        {
            hr = E_OUTOFMEMORY;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = pFactory->QueryInterface(riid, ppv);
        pFactory->Release();
    }

    TraceLeaveHr(hr);
    return hr;
}


