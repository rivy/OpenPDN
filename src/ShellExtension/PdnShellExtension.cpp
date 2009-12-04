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
#include "PdnShellExtension.h"
#include <shlwapi.h>
#pragma comment(lib, "shlwapi.lib")
#include <gdiplus.h>
#pragma comment(lib, "gdiplus.lib")
#include <atlenc.h>
#include "PdnGuid.h"
#include "PdnShell.h"
#include <windows.h>
#include "MemoryStream.h"

using namespace Gdiplus;

CPdnShellExtension::CPdnShellExtension()
    : m_lRefCount(1),
      m_bstrFileName(NULL)
{
    m_size.cx = -1;
    m_size.cy = -1;
    InterlockedIncrement(&g_lRefCount);
}

CPdnShellExtension::~CPdnShellExtension()
{
    InterlockedDecrement(&g_lRefCount);
}

DWORD CPdnShellExtension::AddRef()
{
    TraceEnter();
    TraceLeave();
    return (DWORD)InterlockedIncrement(&m_lRefCount);
}

DWORD CPdnShellExtension::Release()
{
    TraceEnter();
    DWORD dwRefCount = (DWORD)InterlockedDecrement(&m_lRefCount);

    if (0 == dwRefCount)
    {
        delete this;
    }

    TraceLeave();
    return dwRefCount;
}

STDMETHODIMP CPdnShellExtension::QueryInterface(REFIID iid, void **ppvObject)
{
    HRESULT hr = S_OK;
    TraceEnter();
    TraceOut("riid=%S", GuidToString(iid));

    if (NULL == ppvObject)
    {
        return E_INVALIDARG;
    }

    if (SUCCEEDED(hr))
    {
        *ppvObject = NULL;

        if (IsEqualCLSID(iid, IID_IUnknown))
        {
            *ppvObject = this;
        }
        else if (IsEqualCLSID(iid, IID_IPersistFile))
        {
            *ppvObject = (IPersistFile *)this;
        }
        else if (IsEqualCLSID(iid, IID_IExtractImage))
        {
            *ppvObject = (IExtractImage *)this;
        }
        else if (IsEqualCLSID(iid, CLSID_PdnShellExtension))
        {
            *ppvObject = this;
        }
        else
        {
            hr = E_NOINTERFACE;
        }
    }

    if (SUCCEEDED(hr))
    {
        (*(IUnknown **)ppvObject)->AddRef();
        hr = S_OK;
    }

    TraceLeaveHr(hr);
    return hr;
}


STDMETHODIMP CPdnShellExtension::GetClassID(CLSID *pClassID)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::GetCurFile(LPOLESTR *ppszFileName)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::IsDirty()
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::Load(LPCOLESTR pszFileName, 
                                      DWORD dwMode)
{
    HRESULT hr = S_OK;
    TraceEnter();

    TraceOut("filename=%S", pszFileName);
    TraceOut("mode=%u", dwMode);

    if (SUCCEEDED(hr))
    {
        if (NULL == pszFileName)
        {
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        SysFreeString(m_bstrFileName);
        m_bstrFileName = SysAllocString(pszFileName);

        if (NULL == m_bstrFileName)
        {
            hr = E_OUTOFMEMORY;
        }
        else
        {
            hr = S_OK;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::Save(LPCOLESTR pszFileName, 
                                      BOOL fRemember)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();

    TraceOut("fileName=%S", pszFileName);

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::SaveCompleted(LPCOLESTR pszFileName)
{
    HRESULT hr = E_NOTIMPL;
    TraceEnter();
    TraceLeaveHr(hr);
    return hr;
}

HRESULT DoGdiplusStartup(Status *pStatusResult, ULONG_PTR *pToken, GdiplusStartupInput *gdiplusStartupInput)
{
    // An exception may be thrown because we delay-load gdiplus.dll and
    // this is not installed on Win2K systems. Even if .NET is installed,
    // gdiplus.dll is not located in the system directory and thus is not
    // locatable by the loader.
	// This was moved into a separate function because of compiler error
	// C2712: "cannot use __try in functions that require object unwinding"

	Status status = Ok;
	HRESULT hr = S_OK;

	if (NULL == pStatusResult)
	{
		hr = E_INVALIDARG;
	}

	if (SUCCEEDED(hr))
	{
		__try
		{
			status = GdiplusStartup(pToken, gdiplusStartupInput, NULL);
		}

		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			hr = E_FAIL;
			status = Win32Error;
		}
	}

	if (SUCCEEDED(hr))
	{
		*pStatusResult = status;
	}

	return hr;
}


// ReadFile does not guarantee that it will read all the bytes that you ask for.
// It may decide to read fewer bytes for whatever reason. This function is a 
// wrapper around ReadFile that loops until all the bytes you have asked for
// are read, or there was an error, or the end of file was reached. EOF is
// considered an error condition; when you ask for N bytes with this function
// you either get all N bytes, or an error.
static HRESULT ReadFileComplete(HANDLE hFile, LPVOID lpBuffer, DWORD nNumberOfBytesToRead)
{
    HRESULT hr = S_OK;

    while (SUCCEEDED(hr) && nNumberOfBytesToRead > 0)
    {
        DWORD dwBytesRead = 0;

        BOOL bResult = ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, &dwBytesRead, NULL);

        if (!bResult)
        {
            DWORD dwError = GetLastError();
            hr = HRESULT_FROM_WIN32(dwError);
        }
        else if (bResult && 0 == dwBytesRead)
        {
            hr = HRESULT_FROM_WIN32(ERROR_HANDLE_EOF);
        }
        else
        {
            lpBuffer = (void *)((BYTE *)lpBuffer + dwBytesRead);
            nNumberOfBytesToRead -= dwBytesRead;
        }
    }
    
    return hr;
}


static SIZE ComputeThumbnailSize(int originalWidth, int originalHeight, int maxEdgeLength)
{
    SIZE thumbSize;
    ZeroMemory(&thumbSize, sizeof(thumbSize));

    if (originalWidth <= 0 || originalHeight <= 0)
    {
        thumbSize.cx = 1;
        thumbSize.cy = 1;
    }
    else if (originalWidth > originalHeight)
    {
        int longSide = min(originalWidth, maxEdgeLength);
        thumbSize.cx = longSide;
        thumbSize.cy = max(1, (originalHeight * longSide) / originalWidth);
    }
    else if (originalHeight > originalWidth)
    {
        int longSide = min(originalHeight, maxEdgeLength);
        thumbSize.cx = max(1, (originalWidth * longSide) / originalHeight);
        thumbSize.cy = longSide;
    }
    else // if (docSize.Width == docSize.Height)
    {
        int longSide = min(originalWidth, maxEdgeLength);
        thumbSize.cx = longSide;
        thumbSize.cy = longSide;
    }

    return thumbSize;
}

static HRESULT VerifyWindowsVersion(DWORD dwMajor, DWORD dwMinor, BOOL *pbResult)
{
	if (NULL == pbResult)
	{
		return E_INVALIDARG;
	}

	HRESULT hr = S_OK;
	BOOL bResult = TRUE;
	DWORD dwError = ERROR_SUCCESS;

	OSVERSIONINFOEX osviex;
	ZeroMemory(&osviex, sizeof(osviex));

	osviex.dwOSVersionInfoSize = sizeof(osviex);
	osviex.dwMajorVersion = dwMajor;
	osviex.dwMinorVersion = dwMinor;

	DWORDLONG dwlConditionMask = 0;

	int vop = VER_GREATER_EQUAL;

	VER_SET_CONDITION(dwlConditionMask, VER_MAJORVERSION, vop);
	VER_SET_CONDITION(dwlConditionMask, VER_MINORVERSION, vop);

	if (SUCCEEDED(hr))
	{
		bResult = VerifyVersionInfo(&osviex, VER_MAJORVERSION | VER_MINORVERSION, dwlConditionMask);

		if (bResult)
		{
			*pbResult = TRUE;
		}
		else
		{
			dwError = GetLastError();

			if (ERROR_OLD_WIN_VERSION == dwError)
			{
				*pbResult = FALSE;
			}
			else
			{
				hr = HRESULT_FROM_WIN32(dwError);
			}
		}
	}

	return hr;
}

STDMETHODIMP CPdnShellExtension::Extract(HBITMAP *phBmpImage)
{
    HRESULT hr = S_OK;
    DWORD dwError = ERROR_SUCCESS;
    BOOL bResult = TRUE;
    TraceEnter();

    // Open file
    HANDLE hFile = INVALID_HANDLE_VALUE;
    if (SUCCEEDED(hr))
    {
        LPCWSTR lpFileName = (LPCWSTR)m_bstrFileName;
        DWORD dwDesiredAccess = GENERIC_READ;
        DWORD dwShareMode = FILE_SHARE_READ;
        LPSECURITY_ATTRIBUTES lpSecurityAttributes = NULL;
        DWORD dwCreationDisposition = OPEN_EXISTING;
        DWORD dwFlagsAndAttributes = FILE_FLAG_SEQUENTIAL_SCAN;
        HANDLE hTemplateFile = NULL;

        hFile = CreateFileW(
            lpFileName, 
            dwDesiredAccess, 
            dwShareMode, 
            lpSecurityAttributes, 
            dwCreationDisposition, 
            dwFlagsAndAttributes, 
            hTemplateFile);

        if (INVALID_HANDLE_VALUE == hFile)
        {
            dwError = GetLastError();
            hr = HRESULT_FROM_WIN32(dwError);
            TraceOut("CreateFile failed, hr=0x%x", hr);
        }
    }

    // Read magic numbers
    BOOL bPdn3File = FALSE;
    BYTE bMagic[4];
    ZeroMemory(bMagic, sizeof(bMagic));

    if (SUCCEEDED(hr))
    {
        hr = ReadFileComplete(hFile, (LPVOID)bMagic, sizeof(bMagic));
    }

    if (SUCCEEDED(hr))
    {
        if ('P' == bMagic[0] &&
            'D' == bMagic[1] &&
            'N' == bMagic[2] &&
            '3' == bMagic[3])
        {
            bPdn3File = TRUE;
        }
    }
    else
    {
        TraceOut("ReadFile(1) failed, hr=0x%x", hr);
    }

    if (SUCCEEDED(hr) && bPdn3File)
    {
        TraceOut("we have a pdn3 file");

        TraceOut("Read + decode length");

        int iLength = -1;
        BYTE bLength[3];
        ZeroMemory(bLength, sizeof(bLength));

        if (SUCCEEDED(hr))
        {
            hr = ReadFileComplete(hFile, (LPVOID)bLength, sizeof(bLength));
        }

        if (SUCCEEDED(hr))
        {
            iLength = bLength[0] + (bLength[1] << 8) + (bLength[2] << 16);
        }
        else
        {
            TraceOut("ReadFile(2) failed, hr=0x%x", hr);
        }

        TraceOut("Allocate buffer");
        BYTE *pbHeaderBytes = NULL;

        if (SUCCEEDED(hr))
        {
            pbHeaderBytes = new BYTE[1 + iLength];

            if (NULL == pbHeaderBytes)
            {
                hr = E_OUTOFMEMORY;
                TraceOut("pbHeaderBytes alloc failed");
            }
            else
            {
                ZeroMemory(pbHeaderBytes, 1 + iLength);
            }
        }

        TraceOut("Read N bytes");
        if (SUCCEEDED(hr))
        {
            hr = ReadFileComplete(hFile, (LPVOID)pbHeaderBytes, iLength);
        }

        if (FAILED(hr))
        {
            TraceOut("ReadFile(3) failed, hr=0x%x", hr);
        }

        TraceOut("Convert to UTF8 string");
        CHAR *szHeader = (CHAR *)pbHeaderBytes;

        TraceOut("Search for \"<thumb\"");
        const CHAR *szThumbTag = "<thumb ";
        __int64 iThumbTagIndex = -1;

        if (SUCCEEDED(hr))
        {
            CHAR *szFoundHere = strstr(szHeader, szThumbTag);

            if (NULL == szFoundHere)
            {
                TraceOut("Did not find opening tag, \"%s\"", szThumbTag);
                hr = E_UNEXPECTED;
            }
            else
            {
                iThumbTagIndex = szFoundHere - szHeader;
            }
        }

        TraceOut("Search for \"png=\" or \"gif=\"");
        const char *szPngTag = "png=\"";
        const char *szGifTag = "gif=\"";
        const char *szImgTag = NULL; // the tag that we found

        __int64 iImgTagIndex = -1;
        if (SUCCEEDED(hr))
        {
            CHAR *szPngFoundHere = strstr(szHeader + iThumbTagIndex + strlen(szThumbTag), szPngTag);

            if (NULL == szPngFoundHere)
            {
                TraceOut("Did not find png tag, \"%s\"", szPngTag);

                CHAR *szGifFoundHere = strstr(szHeader + iThumbTagIndex + strlen(szThumbTag), szGifTag);

                if (NULL == szGifFoundHere)
                {
                    TraceOut("Did not find gif tag, \"%s\"", szGifTag);
                    hr = E_UNEXPECTED;
                }
                else
                {
                    szImgTag = szGifTag;
                    iImgTagIndex = szGifFoundHere - szHeader;
                }
            }
            else
            {
                szImgTag = szPngTag;
                iImgTagIndex = szPngFoundHere - szHeader;
            }
        }

        TraceOut("Search for \"");
        const char *szQuoteEnd = "\"";
        __int64 iQuoteEndIndex = -1;
        if (SUCCEEDED(hr))
        {
            CHAR *szFoundHere = strstr(szHeader + iImgTagIndex + strlen(szImgTag), szQuoteEnd);

            if (NULL == szFoundHere)
            {
                TraceOut("Did not find closing quote, \"%s\"", szQuoteEnd);
                hr = E_UNEXPECTED;
            }
            else
            {
                iQuoteEndIndex = szFoundHere - szHeader;
            }
        }

        TraceOut("Stomp out the portion of the string that is the image in base64 format");
        CHAR *szImgBase64 = NULL;
        int iImgBase64Len = -1;
        if (SUCCEEDED(hr))
        {
            szImgBase64 = szHeader + iImgTagIndex + strlen(szImgTag);
            szHeader[iQuoteEndIndex] = '\0';
            iImgBase64Len = (int)strlen(szImgBase64);
            TraceOut("iImgBase64Len=%d", iImgBase64Len);
        }

        TraceOut("Get required length of byte[] array for base64->byte[] conversion");
        int nImgBytes = -1;
        if (SUCCEEDED(hr))
        {
            nImgBytes = Base64DecodeGetRequiredLength(iImgBase64Len);
            TraceOut("nImgBytes=%d", nImgBytes);
        }

        TraceOut("Allocate %d byte buffer for base64->byte[] conversion", nImgBytes);
        BYTE *pbImgBytes = NULL;
        if (SUCCEEDED(hr))
        {
            pbImgBytes = new BYTE[nImgBytes];

            if (NULL == pbImgBytes)
            {
                hr = E_OUTOFMEMORY;
                TraceOut("pbImgBytes alloc failed");
            }
            else
            {
                ZeroMemory(pbImgBytes, nImgBytes);
            }
        }

        TraceOut("Convert from base64 to byte[]");
        int iImgLen = -1;
        if (SUCCEEDED(hr))
        {
            int nDestLen = iImgBase64Len;

            bResult = Base64Decode(szImgBase64, iImgBase64Len, pbImgBytes, &nDestLen);

            if (!bResult)
            {
                TraceOut("Base64Decode failed");
                hr = E_FAIL;
            }
            else
            {
                iImgLen = nDestLen;
            }
        }

        TraceOut("iImgLen = %d", iImgLen);
        TraceOut("Wrap a memory stream around it");
        CMemoryStream *pMemoryStream = NULL;
        if (SUCCEEDED(hr))
        {
            pMemoryStream = new CMemoryStream(pbImgBytes, iImgLen);

            if (NULL == pMemoryStream)
            {
                TraceOut("pMemoryStream alloc failed");
                hr = E_OUTOFMEMORY;
            }
        }

        TraceOut("Startup GDI+");
        ULONG_PTR pGdiToken = NULL;

        if (SUCCEEDED(hr))
        {
            GdiplusStartupInput gdiplusStartupInput;
            Status status;
            
			hr = DoGdiplusStartup(&status, &pGdiToken, &gdiplusStartupInput);

            if (status != Ok)
            {
                hr = E_FAIL;
                pGdiToken = NULL;
                TraceOut("GdiplusStartup failed");
            }
        }

        TraceOut("Load image");
        Bitmap *pBitmap = NULL;        
        if (SUCCEEDED(hr))
        {
            pBitmap = Bitmap::FromStream(pMemoryStream, FALSE);

            if (NULL == pBitmap)
            {
                hr = E_FAIL;
                TraceOut("Bitmap::FromStream returned NULL");
            }
        }

        Bitmap *pResizedBitmap = NULL;
        if (SUCCEEDED(hr))
        {
            if (m_size.cx > 0 && m_size.cy > 0)
            {
                UINT nMaxEdge = min(m_size.cx, m_size.cy);
                SIZE newSize = ComputeThumbnailSize(pBitmap->GetWidth(), pBitmap->GetHeight(), nMaxEdge);
                UINT nNewWidth = newSize.cx;
                UINT nNewHeight = newSize.cy;

                if (SUCCEEDED(hr))
                {
                    pResizedBitmap = new Bitmap(nNewWidth, nNewHeight, PixelFormat32bppARGB);

                    if (NULL == pResizedBitmap)
                    {
                        hr = E_OUTOFMEMORY;
                    }
                }

                Graphics *pG = NULL;
                if (SUCCEEDED(hr))
                {
                    pG = Graphics::FromImage(pResizedBitmap);

                    if (NULL == pG)
                    {
                        hr = E_OUTOFMEMORY;
                    }
                }

                if (SUCCEEDED(hr))
                {
					// In Windows Vista, we can have a transparent background and it works great.
					// In XP, our alpha channel gets stomped to black.
					BOOL bIsVista = FALSE;
					HRESULT hrx = VerifyWindowsVersion(6, 0, &bIsVista);

					pG->SetCompositingMode(CompositingModeSourceCopy);

					if (SUCCEEDED(hrx) && bIsVista)
					{
						pG->Clear(Color::Transparent);
					}
					else
					{
						pG->Clear(Color::White);
					}

					pG->SetCompositingMode(CompositingModeSourceOver);

					// Fit the thumbnail to the output bitmap
                    pG->SetInterpolationMode(InterpolationModeBicubic);

                    pG->SetPixelOffsetMode(PixelOffsetModeHalf);

                    pG->DrawImage(
                        pBitmap, 
                        RectF((REAL)0, (REAL)0, (REAL)pResizedBitmap->GetWidth(), (REAL)pResizedBitmap->GetHeight()),
                        (REAL)0,
                        (REAL)0,
                        (REAL)pBitmap->GetWidth(),
                        (REAL)pBitmap->GetHeight(),
                        UnitPixel);
                }

                if (NULL != pG)
                {
                    delete pG;
                    pG = NULL;
                }
            }
            else
            {
                pResizedBitmap = pBitmap->Clone(Rect(0, 0, pBitmap->GetWidth(), pBitmap->GetHeight()), pBitmap->GetPixelFormat());

                if (NULL == pResizedBitmap)
                {
                    hr = E_OUTOFMEMORY;
                }
            }
        }

        if (NULL != pBitmap)
        {
            delete pBitmap;
            pBitmap = NULL;
        }

        TraceOut("Get HBITMAP from it");
        HBITMAP hBitmap = NULL;
        if (SUCCEEDED(hr))
        {
            Status status = pResizedBitmap->GetHBITMAP(Color(0), &hBitmap);
            TraceOut("status=%d", status);

            if (Ok != status)
            {
                if (Win32Error == status)
                {
                    dwError = GetLastError();
                    hr = HRESULT_FROM_WIN32(dwError);
                    TraceOut("pResizedBitmap->GetHBITMAP failed, hr=0x%x", hr);
                }
                else
                {
                    TraceOut("pResizedBitmap->GetHBITMAP failed, not Win32Error, hr is now = E_FAIL");
                    hr = E_FAIL;
                }
            }
        }

        TraceOut("Give bitmap to the caller!");
        if (SUCCEEDED(hr))
        {
            *phBmpImage = hBitmap;
        }

        TraceOut("Cleanup");
        if (NULL != pResizedBitmap)
        {
            delete pResizedBitmap;
            pResizedBitmap = NULL;
        }

        if (NULL != pGdiToken)
        {
            GdiplusShutdown(pGdiToken);
            pGdiToken = NULL;
        }

        if (NULL != pMemoryStream)
        {
            pMemoryStream->Release();
            pMemoryStream = NULL;
        }

        if (NULL != pbHeaderBytes)
        {
            delete [] pbHeaderBytes;
            pbHeaderBytes = NULL;
        }

        if (NULL != pbImgBytes)
        {
            delete [] pbImgBytes;
            pbImgBytes = NULL;
        }
    }

    if (!bPdn3File || FAILED(hr))
    {
        // Give generic PDN icon of some sort
        hr = E_FAIL;
    }    

    // Cleanup
    if (INVALID_HANDLE_VALUE != hFile)
    {
        CloseHandle(hFile);
        hFile = INVALID_HANDLE_VALUE;
    }

    TraceLeaveHr(hr);
    return hr;
}

STDMETHODIMP CPdnShellExtension::GetLocation(LPWSTR pszPathBuffer, 
                                             DWORD cchMax, 
                                             DWORD *pdwPriority, 
                                             const SIZE *prgSize, 
                                             DWORD dwRecClrDepth, 
                                             DWORD *pdwFlags)
{
    HRESULT hr = S_OK;
    TraceEnter();

    TraceOut("pszPathBuffer=%S", pszPathBuffer);
    TraceOut("cchMax=%u", cchMax);

    if (SUCCEEDED(hr))
    {
        if (NULL == pszPathBuffer)
        {
            TraceOut("pszPathBuffer is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == pdwPriority)
        {
            TraceOut("pdwPriority is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == pdwFlags)
        {
            TraceOut("pdwFlags is NULL");
            hr = E_INVALIDARG;
        }
    }

    if (SUCCEEDED(hr))
    {
        if (NULL == prgSize)
        {
            TraceOut("prgSize is NULL");
            hr = E_INVALIDARG;
        }
    }

    TraceOut("*pdwFlags = %u", *pdwFlags);
    TraceOut("prgSize=%d x %d", prgSize->cx, prgSize->cy);

    if (SUCCEEDED(hr))
    {
        wcscpy_s(pszPathBuffer, cchMax, m_bstrFileName);

        *pdwPriority = IEIT_PRIORITY_NORMAL;

        if ((*pdwFlags & IEIFLAG_ASPECT) ||
            (*pdwFlags & IEIFLAG_ORIGSIZE))
        {
            m_size = *prgSize;
            TraceOut("m_size = %d x %d", m_size.cx, m_size.cy);
        }
        else
        {
            m_size.cx = -1;
            m_size.cy = -1;
        }

        *pdwFlags |= IEIFLAG_CACHE;

        if (*pdwFlags & IEIFLAG_ASYNC)
        {
            hr = E_PENDING;
        }
        else
        {
            hr = NOERROR;
        }
    }

    TraceLeaveHr(hr);
    return hr;
}

