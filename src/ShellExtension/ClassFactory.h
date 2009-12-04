/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <windows.h>

class CClassFactory 
    : public IClassFactory
{
public:
    CClassFactory (CLSID clsid);
    ~CClassFactory ();

    // IUnknown methods
    STDMETHODIMP QueryInterface (REFIID riid, void **ppvObject);
    STDMETHODIMP_(DWORD) AddRef ();
    STDMETHODIMP_(DWORD) Release ();

    // IClassFactory methods
    STDMETHODIMP CreateInstance (IUnknown *pUnkOuter, REFIID riid, void **ppvObject);
    STDMETHODIMP LockServer (BOOL fLock);

    void Constructor (CLSID ClassID);
    void Destructor (void);

protected:
    LONG m_lRefCount;

private:
    CLSID m_clsidObject;
};

