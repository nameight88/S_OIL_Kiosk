#pragma once

// 컴퓨터에서 Microsoft Visual C++를 사용하여 생성한 IDispatch 래퍼 클래스입니다.

// 참고: 이 파일의 내용을 수정하지 마십시오. Microsoft Visual C++에서
//  이 클래스를 다시 생성할 때 수정한 내용을 덮어씁니다.

/////////////////////////////////////////////////////////////////////////////
// CWebguardctrl1 래퍼 클래스입니다.

class CWebguardctrl1 : public CWnd
{
protected:
	DECLARE_DYNCREATE(CWebguardctrl1)
public:
	CLSID const& GetClsid()
	{
		static CLSID const clsid
			= { 0x88EDEE4B, 0xD10, 0x4461, { 0xBB, 0x3F, 0xC2, 0x64, 0xDD, 0x8A, 0x81, 0x4F } };
		return clsid;
	}
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle,
						const RECT& rect, CWnd* pParentWnd, UINT nID, 
						CCreateContext* pContext = NULL)
	{ 
		return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID); 
	}

    BOOL Create(LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, 
				UINT nID, CFile* pPersist = NULL, BOOL bStorage = FALSE,
				BSTR bstrLicKey = NULL)
	{ 
		return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID,
		pPersist, bStorage, bstrLicKey); 
	}

// 특성입니다.
public:


// 작업입니다.
public:

// _DWebGuard

// Functions
//


// Properties
//



};
