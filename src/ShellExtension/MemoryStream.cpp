/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

#pragma strict_gs_check(on)
#include "MemoryStream.h"
#include "PdnShell.h"
#include <limits.h>

CMemoryStream::CMemoryStream(BYTE *pbBuffer, int nSize)
    : m_pbBuffer(pbBuffer),
      m_nSize(nSize),
      m_nPos(0)
{
    TraceEnter();
    TraceOut("m_nSize=%d", m_nSize);
    m_lRefCount = 1;

    SYSTEMTIME st;
    GetSystemTime(&st);
    SystemTimeToFileTime(&st, &m_ftCreation);
    SystemTimeToFileTime(&st, &m_ftModified);
    SystemTimeToFileTime(&st, &m_ftAccessed);
    TraceLeave();
}

CMemoryStream::~CMemoryStream()
{
}

// IUnknown methods
STDMETHODIMP CMemoryStream::QueryInterface(REFIID iid, void **ppvObject)
{
    TraceEnter();

    TraceOut("%S", GuidToString(iid));

    if (NULL == ppvObject)
    {
        return E_INVALIDARG;
    }

    if (IsEqualCLSID(iid, IID_IStream))
    {
        AddRef();
        *ppvObject = this;
        return S_OK;
    }
    else
    {
        return E_NOINTERFACE;
    }
}

STDMETHODIMP_(DWORD) CMemoryStream::AddRef()
{
    TraceEnter();
    return InterlockedIncrement(&m_lRefCount);
}

STDMETHODIMP_(DWORD) CMemoryStream::Release()
{
    TraceEnter();
    DWORD dwNewRefCount = InterlockedDecrement(&m_lRefCount);

    if (0 == dwNewRefCount)
    {
        delete this;
    }

    return dwNewRefCount;
}

// IStream members
STDMETHODIMP CMemoryStream::Clone(IStream **ppstm)
{
    TraceEnter();
    if (NULL == ppstm)
    {
        return E_INVALIDARG;
    }

    CMemoryStream *pMemoryStream = new CMemoryStream(m_pbBuffer, m_nSize);

    if (NULL == pMemoryStream)
    {
        return E_INVALIDARG;
    }
    
    LARGE_INTEGER dlibMove;
    dlibMove.QuadPart = m_nPos;
    DWORD dwOrigin = STREAM_SEEK_SET;
    ULARGE_INTEGER dlibNewPosition;

    HRESULT hr = pMemoryStream->Seek(dlibMove, dwOrigin, &dlibNewPosition);

    if (FAILED(hr))
    {
        pMemoryStream->Release();
        return hr;
    }
    else
    {
        *ppstm = (IStream *)pMemoryStream;
        return S_OK;
    }
}

STDMETHODIMP CMemoryStream::Commit(DWORD grfCommitFlags)
{
    HRESULT hr = S_OK;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CMemoryStream::CopyTo(IStream *pstm,
                                   ULARGE_INTEGER cb,
                                   ULARGE_INTEGER *pcbRead,
                                   ULARGE_INTEGER *pcbWritten)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CMemoryStream::LockRegion(ULARGE_INTEGER libOffset,
                                       ULARGE_INTEGER cb,
                                       DWORD dwLockType)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CMemoryStream::Read(void *pv,
                                 ULONG cb,
                                 ULONG *pcbRead)
{
    TraceEnter();

    if (NULL == pv)
    {
        TraceLeaveHr(STG_E_INVALIDPOINTER);
        return STG_E_INVALIDPOINTER;
    }

    TraceOut("pv=%p, cb=%u, pcbRead=%p, m_nPos=%d", pv, cb, pcbRead, m_nPos);

    BYTE *pbEnd = m_pbBuffer + m_nSize;
    BYTE *pbReqStart = m_pbBuffer + m_nPos;
    BYTE *pbReqEnd = pbReqStart + cb;
    ULONG nBytesToGet = (ULONG)(min(pbEnd - pbReqStart, pbReqEnd - pbReqStart));

    memcpy(pv, pbReqStart, nBytesToGet);
   
    if (NULL != pcbRead)
    {
        *pcbRead = nBytesToGet;
    }

    m_nPos += nBytesToGet;

    TraceOut("m_nPos=%d", m_nPos);

    SYSTEMTIME st;
    GetSystemTime(&st);
    SystemTimeToFileTime(&st, &m_ftAccessed);

    TraceLeaveHr(S_OK);
    return S_OK;
}

STDMETHODIMP CMemoryStream::Write(const void *pv,
                                  ULONG cb,
                                  ULONG *pcbWritten)
{
    TraceEnter();

    if (NULL == pv)
    {
        return STG_E_INVALIDPOINTER;
    }

    TraceOut("pv=%p, cb=%u, pcbWritten=%p", pv, cb, pcbWritten);

    BYTE *pbEnd = m_pbBuffer + m_nSize;
    BYTE *pbReqStart = m_pbBuffer + m_nPos;
    BYTE *pbReqEnd = pbReqStart + cb;
    ULONG nBytesToSet = (ULONG)(min(pbEnd, pbReqEnd) - pbReqStart);

    memcpy(pbReqStart, pv, nBytesToSet);
   
    if (NULL != pcbWritten)
    {
        *pcbWritten = nBytesToSet;
    }

    m_nPos += nBytesToSet;

    TraceOut("m_nPos=%d", m_nPos);

    SYSTEMTIME st;
    GetSystemTime(&st);
    SystemTimeToFileTime(&st, &m_ftModified);

    if (nBytesToSet < cb)
    {
        TraceLeaveHr(STG_E_MEDIUMFULL);
        return STG_E_MEDIUMFULL;
    }
    else
    {
        TraceLeaveHr(S_OK);
        return S_OK;
    }
}
    
STDMETHODIMP CMemoryStream::Revert()
{
    HRESULT hr = S_OK;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CMemoryStream::Seek(LARGE_INTEGER dlibMove,
                                 DWORD dwOrigin,
                                 ULARGE_INTEGER *plibNewPosition)
{
    HRESULT hr = S_OK;
    TraceEnter();

    TraceOut("dlibMove=%Ld, dwOrigin=%d", dlibMove.QuadPart, dwOrigin);

    if (dlibMove.QuadPart > INT_MAX || dlibMove.QuadPart < INT_MIN)
    {
        hr = STG_E_INVALIDFUNCTION;
    }
    else
    {
        int nMove = (int)dlibMove.QuadPart;
        TraceOut("nMove = %d", nMove);
        int nNewPos = 0;

        switch (dwOrigin)
        {
            case STREAM_SEEK_SET:
                if (nMove >= 0 && nMove < m_nSize)
                {
                    m_nPos = nMove;

                    if (NULL != plibNewPosition)
                    {
                        plibNewPosition->QuadPart = (__int64)m_nPos;
                    }
                }
                else
                {
                    hr = STG_E_INVALIDFUNCTION;
                }

                break;

            case STREAM_SEEK_CUR:
                nNewPos = m_nPos + nMove;

                if (nNewPos >= 0 && nNewPos < m_nSize)
                {
                    m_nPos = nNewPos;

                    if (NULL != plibNewPosition)
                    {
                        plibNewPosition->QuadPart = (__int64)m_nPos;
                    }
                }
                else
                {
                    hr = STG_E_INVALIDFUNCTION;
                }

                break;

            case STREAM_SEEK_END:
                nNewPos = m_nSize - 1 + nMove;

                if (nNewPos >= 0 && nNewPos < m_nSize)
                {
                    m_nPos = nNewPos;

                    if (NULL != plibNewPosition)
                    {
                        plibNewPosition->QuadPart = (__int64)m_nPos;
                    }
                }
                else
                {
                    hr = STG_E_INVALIDFUNCTION;
                }

                break;                

            default:
                hr = STG_E_INVALIDFUNCTION;
                break;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CMemoryStream::SetSize(ULARGE_INTEGER libNewSize)
{
    return E_NOTIMPL;
}

STDMETHODIMP CMemoryStream::Stat(STATSTG *pstatstg,
                                 DWORD grfStatFlag)
{
    TraceEnter();

    TraceOut("grfStatFlag=%u", grfStatFlag);

    if (NULL == pstatstg)
    {
        return STG_E_INVALIDPOINTER;
    }

    ZeroMemory(pstatstg, sizeof(*pstatstg));

    if (grfStatFlag != STATFLAG_NONAME)
    {
        WCHAR wszName[64];
        wsprintfW(wszName, L"%p", m_pbBuffer);
		size_t nSize = 1 + wcslen(wszName);
        pstatstg->pwcsName = (LPOLESTR)CoTaskMemAlloc(nSize);
        wcscpy_s(pstatstg->pwcsName, nSize, wszName);
    }

    pstatstg->cbSize.QuadPart = (ULONGLONG)m_nSize;
    pstatstg->type = STGTY_STREAM;
    pstatstg->ctime = m_ftCreation;
    pstatstg->mtime = m_ftModified;
    pstatstg->atime = m_ftAccessed;
    pstatstg->grfMode = STGM_READ;
    pstatstg->grfLocksSupported = 0;
    pstatstg->clsid = CLSID_NULL;
    pstatstg->grfStateBits = 0;
    pstatstg->reserved = 0;

    return S_OK;
}

STDMETHODIMP CMemoryStream::UnlockRegion(ULARGE_INTEGER libOffset,
                                         ULARGE_INTEGER cb,
                                         DWORD dwLockType)
{
    return E_NOTIMPL;
}                                        
