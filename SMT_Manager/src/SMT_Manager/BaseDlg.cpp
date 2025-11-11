// BaseDlg.cpp : implementation file
//

#include "stdafx.h"
//#include "SMTLocker.h"
#include "BaseDlg.h"
#include <afxpriv.h>
#include "Ado.h"
//#include "MessageDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

/////////////////////////////////////////////////////////////////////////////

// 홍동성 - 출처 : 서울대학교
//////////
extern "C" _declspec(dllimport) BOOL CSSendSMS(char* strMsg,char* strSendPhone,char* strRecvPhone);

BOOL loadSMSDll(CString szPath, HINSTANCE& hInstance);
BOOL freeSMSDll(HINSTANCE hInstance);
void ConnectSMSDB(HINSTANCE&	hSMSKorea,CString	strUserID,CString	strUserID2,CString	PWD);
void CloseSMSDB(HINSTANCE&	hSMSKorea);

BOOL loadSMSDll(CString szPath, HINSTANCE& hInstance)
{
	hInstance = NULL;
	hInstance = LoadLibrary(szPath);

	if(hInstance == NULL)
	{
		MessageBox(NULL, _T("DLL 로드 실패!"), _T("에러"), MB_OK);
		return FALSE;
	}

	return TRUE;
}

BOOL freeSMSDll(HINSTANCE hInstance)
{
	if(hInstance != NULL)
	{
		if(!FreeLibrary(hInstance))
		{
			MessageBox(NULL, _T("DLL 해제 실패!"), _T("에러"), MB_OK);
			return FALSE;
		}
	}

	return TRUE;
}

void ConnectSMSDB(HINSTANCE&	hSMSKorea,CString	strUserID,CString strUserID2,CString	strPWD)
{
	// DLL 동적 로드
	if(hSMSKorea == NULL)
		loadSMSDll(_T("SMSKorea2c.dll"),hSMSKorea);

	// 다음 DLL 프로토 타입을 호출합니다.
	// BOOL Start(CString UserID, CString Password)
	if(hSMSKorea != NULL)
	{
		BOOL (*pFunc)(CString UserID, CString UserID2, CString Password);
		pFunc = (BOOL (*)(CString, CString, CString))GetProcAddress(hSMSKorea, "OpenConnection");

		if(pFunc != NULL)
			if(pFunc(strUserID, strUserID2, strPWD) == FALSE)
				AfxMessageBox(_T("DB 연결 실패!"));
	}
}

void CloseSMSDB(HINSTANCE&	hSMSKorea)
{
	if(hSMSKorea != NULL)
	{
		void (*pFunc)();
		pFunc = (void (*)())GetProcAddress(hSMSKorea, "ShutdownConnection");

		if(pFunc != NULL)
			pFunc();
	}
	else
	{
		AfxMessageBox(_T("먼저 SMSKorea2c.dll을 로드해주십시요."));
	}

	// DLL 로드 해제
	freeSMSDll(hSMSKorea);
}

#include "AtlConv.h"

BOOL CBaseDlg::fnSendSMS(CString strMsg,CString strSendPhone,CString strRecvPhone)
{
/*
	CStringA strSPhone; 
	CStringA strRPhone;
	CStringA strSMsg; 

	USES_CONVERSION;

	strSPhone = W2A(strSendPhone);
	strRPhone = W2A(strRecvPhone);
	strSMsg   = W2A(strMsg);

	char szSPhone[64];
	char szRPhone[64];
	char szSMsg[1024];

	strcpy_s(szSPhone,strSPhone);
	strcpy_s(szRPhone,strRPhone);
	strcpy_s(szSMsg,strSMsg);

	return CSSendSMS(szSMsg,szSPhone,szRPhone);
*/
	return TRUE;
}
//////////

//const CString g_strSMSID  = _T("DP1100731");
//const CString g_strSMSID2 = _T("ryu1223");
//const CString g_strSMSPW  = _T("ryu8080");

const char *g_DELIVERY_TYPE_STRING[15] = 
{
	_T("입주자의뢰"),
	_T("우체국등기"),
	_T("우체국택배"),
	_T("세탁소"),
	_T("한진택배"),
	_T("대한택배"),
	_T("현대택배"),
	_T("CJ택배"),
	_T("로젠택배"),
	_T("엘로우택배"),
	_T("패밀리택배"),
	_T("KGB택배"),
	_T("동부택배"),
	_T("하나로택배"),
	_T("기타택배"),
};

const char *g_GROUP_TYPE_STRING[15] = 
{
	_T("01"),
	_T("04"),
	_T("10"),
	_T("05"),
	_T("11"),
	_T("12"),
	_T("13"),
	_T("14"),
	_T("15"),
	_T("16"),
	_T("17"),
	_T("18"),
	_T("20"),
	_T("21"),
	_T("19"),
};

/////////////////////////////////////////////////////////////////////////////
// CBaseDlg dialog
IMPLEMENT_DYNAMIC(CBaseDlg, CDialog)

CBaseDlg::CBaseDlg(UINT nIDTemplate,CWnd* pParent /*=NULL*/)
	: CDialog( nIDTemplate, pParent) 
	, m_strBGImage(_T(""))
	, m_strSubImage1(_T(""))
	, m_strSubImage2(_T(""))
	, m_strSubImage3(_T(""))
	, m_TimerIDShowTime(0)
	, m_pGUIWorkEv(NULL)
	, m_DlgShowTimeSec(30)
	, m_SubImagePt1(0,0)
	, m_SubImagePt2(0,0)
	, m_SubImagePt3(0,0)
	, m_bNoLogoImage(FALSE)
	, m_bBoxOpenFlag(FALSE)
	, m_bCompleteFlag(FALSE)
	, m_bTransParentBK(FALSE)
{
	//{{AFX_DATA_INIT(CBaseDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	m_pGUIWorkEv = new CEvent(FALSE,TRUE);
}


void CBaseDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CBaseDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CBaseDlg, CDialog)
	//{{AFX_MSG_MAP(CBaseDlg)
	ON_WM_PAINT()
	ON_WM_ERASEBKGND()
	ON_WM_NCHITTEST()
	//}}AFX_MSG_MAP
	ON_WM_TIMER()
	ON_WM_DESTROY()
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CBaseDlg message handlers

void CBaseDlg::OnPaint() 
{
	CPaintDC dc(this); // device context for painting
	
	// TODO: Add your message handler code here
	if(m_bTransParentBK)
		fnDrawBKImage2(dc);
	else
		fnDrawBKImage(dc);
	
	// Do not call CDialog::OnPaint() for painting messages
}

void CBaseDlg::fnSetBkImage(CString strIMagePath, BOOL bAutoSize ,BOOL bShowWindow)
{
	BSTR str;
	m_strBGImage = strIMagePath;
	str=m_strBGImage.AllocSysString();
	Bitmap BGImg(str);
    SysFreeString(str);
	int iwd = BGImg.GetWidth();
	int iht = BGImg.GetHeight();
	if(bAutoSize)
	{
		//MoveWindow(0,0,iwd,iht);
#ifdef _DEBUG
		SetWindowPos(NULL,0,0,iwd,iht,SWP_NOZORDER|SWP_NOMOVE);
#else
		SetWindowPos(NULL,0,0,iwd,iht,SWP_NOZORDER|SWP_NOMOVE);
		//SetWindowPos(&wndTopMost,0,0,iwd,iht,0);
#endif
		Invalidate(FALSE);
	}
	if(bShowWindow)
		ShowWindow(SW_SHOW);
}


void CBaseDlg::fnSetSubImageNo1(CString strImagePath,int xPos,int yPos)
{
	m_strSubImage1  = strImagePath;
	m_SubImagePt1.x = xPos;
	m_SubImagePt1.y = yPos;
}

void CBaseDlg::fnSetSubImageNo2(CString strImagePath,int xPos,int yPos)
{
	m_strSubImage2  = strImagePath;
	m_SubImagePt2.x = xPos;
	m_SubImagePt2.y = yPos;
}

void CBaseDlg::fnSetSubImageNo3(CString strImagePath,int xPos,int yPos)
{
	m_strSubImage3  = strImagePath;
	m_SubImagePt3.x = xPos;
	m_SubImagePt3.y = yPos;
}

BOOL CBaseDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();

	// 홍동성 - 커서는 Release 모드에서 보이지 않는다.
#ifndef _DEBUG
	ShowCursor(FALSE);
#endif

	if(m_bNoLogoImage == FALSE)
	{
//		m_XILogoStatic.Create(NULL,WS_CHILD | WS_VISIBLE,CRect(0,0,0,0),this,IDC_STATIC_XI_LOGO);
//		m_XILogoStatic.fnSetBkImage(_T("..\\images\\customer.jpg"));
//		m_XILogoStatic.fnSetStaticPosition(113,1211);
//		m_KPLogoStatic.Create(NULL,WS_CHILD | WS_VISIBLE,CRect(0,0,0,0),this,IDC_STATIC_KP_LOGO);
//		m_KPLogoStatic.fnSetBkImage(_T("..\\images\\kphouse.jpg"));
//		m_KPLogoStatic.fnSetStaticPosition(703,1211);
	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

LRESULT CBaseDlg::OnNcHitTest(CPoint point) 
{
	LRESULT unRet = CDialog::OnNcHitTest(point);
#ifdef _DEBUG
	if( unRet == HTCLIENT )
		unRet = HTCAPTION;
#endif
	return unRet;
}

BOOL CBaseDlg::OnEraseBkgnd(CDC* pDC) 
{
	// TODO: Add your message handler code here and/or call default
	
	return TRUE;
	//return CDialog::OnEraseBkgnd(pDC);
}

void CBaseDlg::OnOK()
{
	// TODO: 여기에 특수화된 코드를 추가 및/또는 기본 클래스를 호출합니다.
	fnStartDlgAlive(FALSE);

	if( m_pGUIWorkEv )
	{
		delete m_pGUIWorkEv;
		m_pGUIWorkEv = NULL;
	}

	//CSMTLockerApp::fnPlaySound(SOUND_NONE);

	CDialog::OnOK();
}

void CBaseDlg::OnCancel()
{
	// TODO: 여기에 특수화된 코드를 추가 및/또는 기본 클래스를 호출합니다.

	fnStartDlgAlive(FALSE);

	if( m_pGUIWorkEv )
	{
		delete m_pGUIWorkEv;
		m_pGUIWorkEv = NULL;
	}

	//CSMTLockerApp::fnPlaySound(SOUND_NONE);

	CDialog::OnCancel();
}

void CBaseDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: 여기에 메시지 처리기 코드를 추가 및/또는 기본값을 호출합니다.
	if(nIDEvent == m_TimerIDShowTime)
	{
		if( m_DlgShowTime <= CTime::GetCurrentTime() )
		{
			if(GetForegroundWindow() == this)
			{
				fnStartDlgAlive(FALSE);
				PostMessage(WM_CLOSE);
			}
			else
				fnUpdateShowTime();
		}
	}

	CDialog::OnTimer(nIDEvent);
}

BOOL CBaseDlg::fnStartDlgAlive(BOOL bStart , int AliveTimeSec)
{
	if(bStart)
	{
		m_TimerIDShowTime = SetTimer(TIMER_ID_DLG_SHOW,1000,NULL);
		m_DlgShowTimeSec  = AliveTimeSec;
		fnUpdateShowTime();
	}
	else
	{
		if(m_TimerIDShowTime > 0)
		{
			KillTimer(m_TimerIDShowTime);
			m_TimerIDShowTime = 0;
		}
	}
	return 0;
}

void CBaseDlg::OnDestroy()
{
	CDialog::OnDestroy();

	// TODO: 여기에 메시지 처리기 코드를 추가합니다.
	fnStartDlgAlive(FALSE);

	if( m_pGUIWorkEv )
	{
		delete m_pGUIWorkEv;
		m_pGUIWorkEv = NULL;
	}
}

BOOL CBaseDlg::fnCheckExistSecuNumber(CString strSecuNumber)
{
	char szQuery[2048];
	CADODatabase DataBase;

	sprintf( szQuery ,_T("Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;Initial Catalog=%s;Persist Security Info=True"),
			theApp.m_strDBIPAddr, theApp.m_strDBUser, theApp.m_strDBPass, theApp.m_strDBCatalog);

	if( !DataBase.Open(	szQuery, theApp.m_strDBUser, theApp.m_strDBPass) ) 
		return FALSE;

	sprintf( szQuery , _T("select 1 from tblBoxMaster where boxPassword='%s' and deliveryType >=%d and areaCode='%s' "),
			 strSecuNumber,CONTENTS_SENDUSER,theApp.m_strLockerNo);
	CADORecordset Rset(&DataBase);
	if( !Rset.Open(szQuery) )
	{
		DataBase.Close();
		return FALSE;
	}
	if( Rset.IsBOF() )
	{
		Rset.Close();
		DataBase.Close();
		return TRUE;
	}

	Rset.Close();
	DataBase.Close();

	return FALSE;
}

BOOL CBaseDlg::fnMakeSecuNumber(CString &strSecuNumber)
{
	CString strSecNum = _T("");

	int nMaxLoop = 100;
	int nRand1    = 0;
	int nRand2    = 0;
	while(nMaxLoop > 0)
	{
		nRand1 = rand()%1000;
		nRand2 = rand()%1000;
		strSecNum.Format(_T("%03d%03d"),nRand1,nRand2);
		if(fnCheckExistSecuNumber(strSecNum))
			break;
		nMaxLoop--;
	}

	strSecuNumber = strSecNum;

	if(nMaxLoop <= 0)
		return FALSE;

	return TRUE;
}

BOOL CBaseDlg::fnMakeSecuNumber2(CString &strSecuNumber)
{
	CString strSecNum = _T("");

	int nMaxLoop = 100;
	int nRand1    = 0;
	int nRand2    = 0;
	nRand1 = rand()%1000;
	nRand2 = rand()%1000;
	strSecNum.Format(_T("%03d%03d"),nRand1,nRand2);
	strSecuNumber = strSecNum;
	return TRUE;
}

int CBaseDlg::fnDisPlayBox(CString strMsg,CString strTitle,int BoxType)
{
	int nRet = 0;
//	CDisplayMsgDlg dlg;
//	dlg.fnSetButtonStyle(BoxType);
//	dlg.fnSetDrawText(strMsg);
//	nRet = dlg.DoModal();

	return nRet;
}

LRESULT CBaseDlg::fnOnComboSelChangeMessage(WPARAM wParam, LPARAM lParam)
{
	return 0;
}

void CBaseDlg::fnDrawBKImage(CPaintDC &dc)
{


	CDC memDC; // bk DC
	CBitmap bitmap,*OldBitmap ; // bk Bitmap //조동환 *OldBitmap
	memDC.CreateCompatibleDC(&dc);//조동환

   
	CRect ClientRc;
	GetClientRect(ClientRc);

	bitmap.CreateCompatibleBitmap( &dc,ClientRc.Width(),ClientRc.Height()); //조동환
    OldBitmap = (CBitmap*)memDC.SelectObject(&bitmap); //조동환

	Graphics graphics(memDC); // bk graphics
	Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());	

	if(!m_strBGImage.IsEmpty())
	{
        BSTR str;
        str=m_strBGImage.AllocSysString();
		Bitmap myBMP(str);
		int iwd = myBMP.GetWidth();
		int iht = myBMP.GetHeight();
		graphics.DrawImage(&myBMP,rect);
        SysFreeString(str);
	}
	if(!m_strSubImage1.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage1.AllocSysString();
		Bitmap myBMP1(str);
		int iwd = myBMP1.GetWidth();
		int iht = myBMP1.GetHeight();
		graphics.DrawImage(&myBMP1,m_SubImagePt1.x,m_SubImagePt1.y,iwd,iht);
        SysFreeString(str);
	}
	if(!m_strSubImage2.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage2.AllocSysString();
		Bitmap myBMP2(str);
		int iwd = myBMP2.GetWidth();
		int iht = myBMP2.GetHeight();
		graphics.DrawImage(&myBMP2,m_SubImagePt2.x,m_SubImagePt2.y,iwd,iht);
        SysFreeString(str);
	}
	if(!m_strSubImage3.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage3.AllocSysString();
		Bitmap myBMP3(str);
		int iwd = myBMP3.GetWidth();
		int iht = myBMP3.GetHeight();
		graphics.DrawImage(&myBMP3,m_SubImagePt3.x,m_SubImagePt3.y,iwd,iht);
        SysFreeString(str);
	}
	graphics.ReleaseHDC(memDC.m_hDC);		// 임시로 막아둠		SCHPARK8	20090514

	dc.BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC
	//::TransparentBlt(dc.GetSafeHdc(), 0, 0, ClientRc.Width(), ClientRc.Height(), memDC.GetSafeHdc(), 0, 0, ClientRc.Width(), ClientRc.Height(), RGB(0,0,0));
    memDC.SelectObject(OldBitmap);//조동환
    memDC.DeleteDC();//조동환
    bitmap.DeleteObject();////조동환


}

void CBaseDlg::fnDrawBKImage2(CPaintDC &dc)
{

	CDC memDC; // bk DC
	memDC.CreateCompatibleDC(&dc);//조동환

	CRect ClientRc;
	GetClientRect(ClientRc);
	Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());

	Graphics graphics(dc); // bk graphics

	if(!m_strBGImage.IsEmpty())
	{
        BSTR str;
        str=m_strBGImage.AllocSysString();
		Bitmap myBMP(str);
		int iwd = myBMP.GetWidth();
		int iht = myBMP.GetHeight();
		graphics.DrawImage(&myBMP,rect);
        SysFreeString(str);
		
	}
	if(!m_strSubImage1.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage1.AllocSysString();

		Bitmap myBMP1(str);
		int iwd = myBMP1.GetWidth();
		int iht = myBMP1.GetHeight();
		graphics.DrawImage(&myBMP1,m_SubImagePt1.x,m_SubImagePt1.y,iwd,iht);
        SysFreeString(str);
	}
	if(!m_strSubImage2.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage2.AllocSysString();

		Bitmap myBMP2(str);
		int iwd = myBMP2.GetWidth();
		int iht = myBMP2.GetHeight();
		graphics.DrawImage(&myBMP2,m_SubImagePt2.x,m_SubImagePt2.y,iwd,iht);
        SysFreeString(str);
	}
	if(!m_strSubImage3.IsEmpty())
	{
        BSTR str;
        str=m_strSubImage3.AllocSysString();

		Bitmap myBMP3(str);
		int iwd = myBMP3.GetWidth();
		int iht = myBMP3.GetHeight();
		graphics.DrawImage(&myBMP3,m_SubImagePt3.x,m_SubImagePt3.y,iwd,iht);
        SysFreeString(str);
	}

	graphics.ReleaseHDC(dc.m_hDC);	// 임시 막아둠

	dc.BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC

    //**조동환 추가
	//::TransparentBlt(dc.GetSafeHdc(), 0, 0, ClientRc.Width(), ClientRc.Height(), memDC.GetSafeHdc(), 0, 0, ClientRc.Width(), ClientRc.Height(), RGB(0,0,0));
    memDC.DeleteDC();


}

int CBaseDlg::fnCallMessageDlg(CString strMsg, MESSAGETYPE type, int Align,BOOL NoYOffset)
{
	CMessageDlg dlg;
	dlg.fnSetNoYOffset(NoYOffset);
	dlg.fnSetMessageType(type);
	dlg.fnSetMessageText(strMsg,Align);
	return dlg.DoModal();
}

void CBaseDlg::fnPlaySound(CString strSound, int key, UINT fuSound)
{
	UINT nMode = SND_ASYNC;
	if(fuSound != SND_ASYNC)
	{
		if(fuSound == SND_SYNC)
			nMode = SND_SYNC;
		else
			nMode |= fuSound;
	}
	nMode |= SND_FILENAME;
	if(strSound.IsEmpty())
		PlaySound(NULL,NULL,nMode);
	else
		PlaySound(strSound,NULL,nMode);
}