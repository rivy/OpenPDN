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
#include "ClassFactory.h"
#include "PdnShell.h"
#include "PdnGuid.h"
#include "PdnShellExtension.h"

CClassFactory::CClassFactory(CLSID clsid)
{
    Constructor(clsid);
    return;
}

void CClassFactory::Constructor(CLSID clsid)
{
    m_clsidObject = clsid;
    m_lRefCount = 1;
    InterlockedIncrement(&g_lRefCount);
    return;
}

CClassFactory::~CClassFactory(void)
{
    Destructor();
    return;
}

void CClassFactory::Destructor(void)
{
    InterlockedDecrement (&g_lRefCount);
    return;
}

STDMETHODIMP CClassFactory::QueryInterface(REFIID riid, LPVOID *ppReturn)
{
    TraceEnter();
    TraceOut("riid=%S", GuidToString(riid));
    *ppReturn = NULL;

    if (IsEqualCLSID (riid, IID_IUnknown))
    {
        *ppReturn = this;
    }
    else if (IsEqualCLSID (riid, IID_IClassFactory))
    {
        *ppReturn = (IClassFactory *)this;
    }

    if (*ppReturn != NULL)
    {
        (*(LPUNKNOWN *)ppReturn)->AddRef();
        TraceLeaveHr(S_OK);
        return S_OK;
    }

    TraceLeaveHr(E_NOINTERFACE);
    return E_NOINTERFACE;
}

STDMETHODIMP_(DWORD) CClassFactory::AddRef()
{
    return InterlockedIncrement(&m_lRefCount);
}

STDMETHODIMP_(DWORD) CClassFactory::Release()
{
    DWORD dwNewRC = InterlockedDecrement(&m_lRefCount);

    if (0 == dwNewRC)
    {
        delete this;
        return 0;
    }
    else
    {
        return dwNewRC;
    }
}

STDMETHODIMP CClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
    void *pvResult = NULL;
    HRESULT hr = S_OK;

    TraceEnter();
    TraceOut("riid=%S", GuidToString(riid));

    if (SUCCEEDED(hr))
    {
        if (NULL == ppvObject)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (pUnkOuter != NULL)
        {
            hr = CLASS_E_NOAGGREGATION;
        }
    }

    CPdnShellExtension *pShellExtension = NULL;
    if (SUCCEEDED(hr))
    {
        pShellExtension = new CPdnShellExtension();

        if (NULL == pShellExtension)
        {
            hr = E_OUTOFMEMORY;
        }
    }

    if (SUCCEEDED(hr))
    {
        hr = pShellExtension->QueryInterface(riid, ppvObject);
        pShellExtension->Release();
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CClassFactory::LockServer(BOOL fLock)
{
    return E_NOTIMPL;
}
