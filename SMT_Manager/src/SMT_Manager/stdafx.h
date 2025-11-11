// stdafx.h : 자주 사용하지만 자주 변경되지는 않는
// 표준 시스템 포함 파일 및 프로젝트 관련 포함 파일이 
// 들어 있는 포함 파일입니다.

#pragma once

#define _CRT_SECURE_NO_WARNINGS

#ifndef _SECURE_ATL
#define _SECURE_ATL 1
#endif

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN            // 거의 사용되지 않는 내용은 Windows 헤더에서 제외합니다.
#endif

#include "targetver.h"

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // 일부 CString 생성자는 명시적으로 선언됩니다.

// MFC의 공통 부분과 무시 가능한 경고 메시지에 대한 숨기기를 해제합니다.
#define _AFX_ALL_WARNINGS

#include <afxwin.h>         // MFC 핵심 및 표준 구성 요소입니다.
#include <afxext.h>         // MFC 확장입니다.
#include <afxmt.h>

#include <afxdisp.h>        // MFC 자동화 클래스입니다.
#include <shlwapi.h>

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdtctl.h>           // Internet Explorer 4 공용 컨트롤에 대한 MFC 지원입니다.
#endif
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>                     // Windows 공용 컨트롤에 대한 MFC 지원입니다.
#endif // _AFX_NO_AFXCMN_SUPPORT
#include "BaseStatic.h"

#ifndef __GDI_PLUS__
#define __GDI_PLUS__
#pragma comment(lib, "gdiplus.lib")
#include <gdiplus.h>
using namespace Gdiplus;
#endif

#include "Global.h"
#include "BaseButton.h"
#include "ImageProto.h"
#include "SystemProto.h"
//#define _REMOTE_DB_USED_
#define UM_DLG_INIT_MSG		WM_USER+100
#define KIOSKSUM   20


typedef struct{
	CString m_LockerId[KIOSKSUM];//라커별 아이디 저장
	int m_Locker_Sum;       //라커개수
	int m_BoxSum[KIOSKSUM];//위치별 박스 개수저장
	int m_BoxStartNo[KIOSKSUM];
	int m_Selidx;          //combo 선택 idx
	int m_BoxNo;       //선택된 boxNo
	int m_kioskidx;        //kioskidx 위치
	int m_usedSum;         //위치별 사용중인 box개수
}LOCKER_DATA;




extern LOCKER_DATA LOCK_INFO;

#ifdef _UNICODE
#if defined _M_IX86
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='x86' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_IA64
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='ia64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#elif defined _M_X64
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='amd64' publicKeyToken='6595b64144ccf1df' language='*'\"")
#else
#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")
#endif
#endif


