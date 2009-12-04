/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

#pragma once

#ifdef PDNSHELL_EXPORTS
#define PDNSHELL_API __declspec(dllexport)
#else
#define PDNSHELL_API __declspec(dllimport)
#endif

extern HINSTANCE g_hInstance;
extern volatile LONG g_lRefCount;

#ifdef NDEBUG
#define TraceOut if(0)
#else
extern void TraceOut(const char *szFormat, ...);
#endif

#define TraceEnter() TraceOut("enter: %s", __FUNCTION__);
#define TraceLeave() TraceOut("leave: %s", __FUNCTION__);
#define TraceLeaveHr(hr) TraceOut("leave: %s, hr=0x%x", __FUNCTION__, hr);
extern const WCHAR *GuidToString(GUID guid);
