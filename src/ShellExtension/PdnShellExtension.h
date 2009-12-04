/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <Shlobj.h>

class CPdnShellExtension 
    : IPersistFile,
      IExtractImage
{
public:
    CPdnShellExtension();
    ~CPdnShellExtension();

    // IUnknown methods
    STDMETHODIMP QueryInterface(REFIID iid, void **ppvObject);
    STDMETHODIMP_(DWORD) AddRef();
    STDMETHODIMP_(DWORD) Release();

    // IPersist methods (via IPersistFile)
    STDMETHODIMP GetClassID(CLSID *pClassID);

    // IPersistFile methods
    STDMETHODIMP GetCurFile(LPOLESTR *ppszFileName);
    STDMETHODIMP IsDirty();

    STDMETHODIMP Load(LPCOLESTR pszFileName, 
                      DWORD dwMode);

    STDMETHODIMP Save(LPCOLESTR pszFileName, 
                      BOOL fRemember);

    STDMETHODIMP SaveCompleted(LPCOLESTR pszFileName);

    // IExtractImage methods
    STDMETHODIMP Extract(HBITMAP *phBmpImage);

    STDMETHODIMP GetLocation(LPWSTR pszPathBuffer, 
                             DWORD cchMax, 
                             DWORD *pdwPriority, 
                             const SIZE *prgSize, 
                             DWORD dwRecClrDepth, 
                             DWORD *pdwFlags);

protected:
    volatile LONG m_lRefCount;

private:
    BSTR m_bstrFileName;
    SIZE m_size;
};
