// SMT_ManagerDlg.cpp : 구현 파일
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "SMT_ManagerDlg.h"
#include "LoginDlg.h"
#include "Ado.h"
#include "LoginDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// 응용 프로그램 정보에 사용되는 CAboutDlg 대화 상자입니다.

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

// 구현입니다.
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()


// CSMT_ManagerDlg 대화 상자



#define TAB_HEADER_HEIGHT		24

CSMT_ManagerDlg::CSMT_ManagerDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CSMT_ManagerDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_bInitialized = FALSE;

	m_pImageList = NULL;

	m_ControlDlg = new CControlDlg(this);
	m_AccountDlg = new CAccountDlg(this);
	m_HistoryDlg = new CHistoryDlg(this);
	m_SMSHistoryDlg = new CSMSHistoryDlg(this);
	// 블랙리스트 다이얼로그 제거
	// m_BlackListDlg = new CBlackListDlg(this);
	
	// 결제내역 조회 다이얼로그 객체 생성
	m_PaymentHistoryDlg = new CPaymentHistoryDlg(this);
	
	// 상품 등록 다이얼로그 객체 생성
	m_ProductRegistrationDlg = new CProductRegistrationDlg(this);

}

void CSMT_ManagerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_TAB1, m_tab1);
	DDX_Control(pDX, IDC_BUTTON1, m_Ok);
	DDX_Control(pDX, IDC_BUTTON2, m_Cancel);
}

BEGIN_MESSAGE_MAP(CSMT_ManagerDlg, CSkinDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_BUTTON1, &CSMT_ManagerDlg::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &CSMT_ManagerDlg::OnBnClickedButton2)
	ON_NOTIFY(TCN_SELCHANGE, IDC_TAB1, &CSMT_ManagerDlg::OnTcnSelchangeTab1)
END_MESSAGE_MAP()


// CSMT_ManagerDlg 메시지 처리기

BOOL CSMT_ManagerDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();
//RETURNLOGIN:
//	CLoginDlg dlg;
//	if(dlg.DoModal()==IDOK){
//		if((theApp.m_strManagerId == dlg.m_strId) &&(theApp.m_strManagerPass == dlg.m_strPass)){
//
//		}else{
//			if(MessageBox("정확한 아이디와 비밀번호를 입력해 주십시오","로그인실패",MB_OKCANCEL) ==IDOK){
//                goto RETURNLOGIN;
//			}else{
//                exit(0);
//			}
///		}
//	}else{
//		exit(0);
//	}

	// 시스템 메뉴에 "정보..." 메뉴 항목을 추가합니다.

	// IDM_ABOUTBOX는 시스템 명령 범위에 있어야 합니다.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// 이 대화 상자의 아이콘을 설정합니다. 응용 프로그램의 주 창이 대화 상자가 아닐 경우에는
	//  프레임워크가 이 작업을 자동으로 수행합니다.
	SetIcon(m_hIcon, TRUE);			// 큰 아이콘을 설정합니다.
	SetIcon(m_hIcon, FALSE);		// 작은 아이콘을 설정합니다.

	if( !m_bInitialized )
	{
		m_bInitialized = TRUE;

		// Ok Button
		m_Ok.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE), Global.GetRGB(IDX_RGB_MASK));
		m_Ok.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
        
		// Cancel Button
		//m_Cancel.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE), Global.GetRGB(IDX_RGB_MASK));
		//m_Cancel.SetIcon(Global.GetIcon(IDX_ICON_CANCEL, ICON16), NULL, 5);
  
		// Image List
		m_pImageList = new CImageList();		
		if(!m_pImageList->Create( 16, 16, ILC_COLOR32, 0, 1))
		{
			delete m_pImageList;
			m_pImageList = NULL;
		}
		else 
		{
			m_pImageList->Add(Global.GetIcon(IDX_ICON_HOME,ICON16));
			m_pImageList->Add(Global.GetIcon(IDX_ICON_FAVORITES,ICON16));
			m_pImageList->Add(Global.GetIcon(IDX_ICON_INTERNET,ICON16));
			m_pImageList->Add(Global.GetIcon(IDX_ICON_SETTING,ICON16));
			m_pImageList->Add(Global.GetIcon(IDX_ICON_SMS,ICON16));
			m_pImageList->Add(Global.GetIcon(IDX_ICON_BLACKLIST,ICON16));
		}

		// TAB 1
		m_tab1.SetImageList(m_pImageList);
		m_tab1.ModifyStyle(TCS_BOTTOM|TCS_MULTILINE|TCS_VERTICAL|TCS_BUTTONS, TCS_OWNERDRAWFIXED|TCS_FIXEDWIDTH);
		m_tab1.InsertItem( 0, _T("박스 상태현황 및 제어"), 0 );

#ifdef _DEBUG
		m_tab1.InsertItem( 1, _T("결제내역 조회"), 2 );
		m_tab1.InsertItem( 2, _T("상품 등록"), 3 );

#else
		m_tab1.InsertItem( 1, _T("결제내역 조회"), 2 );
		m_tab1.InsertItem( 2, _T("상품 등록"), 3 );
#endif

		m_tab1.SetColor(RGB(51,51,51), RGB(206,206,206), Global.GetRGB(IDX_RGB_BACKGROUND), Global.GetRGB(IDX_RGB_BACKGROUND));
		m_tab1.SetItemSize(CSize(160,TAB_HEADER_HEIGHT));

		CRect rc;
		this->GetWindowRect(rc);
		::SetWindowPos(::GetDlgItem(this->m_hWnd,IDC_TAB1),NULL,10, 10, rc.right-rc.left-38  ,rc.bottom-rc.top-100, SWP_NOZORDER | SWP_SHOWWINDOW);

		m_tab1.Set3dBorder(TRUE);
////////////////////////////////////////////////
	if( !CSMT_ManagerApp::fnCheckDBConnection() )
	{
		MessageBox("데이터베이스 연결오류","연결오류",MB_OK);
		PostMessage(WM_CLOSE);
		return 0;
	}

#ifdef _REMOTE_DB_USED_
	if( !CSMT_ManagerApp::fnCheckCenterDBConnection() ) 
	{
		MessageBox("중앙데이터베이스(포스텍) 연결오류","연결오류",MB_OK);
		PostMessage(WM_CLOSE);
		return 0;
	}

#endif
	GetDBData();
	////////////////////////////////////////////////

	//	RelocationControls();
	m_ControlDlg->Create(IDD_CONTROL_DIALOG,GetDlgItem(IDC_TAB1));
	m_ControlDlg->ShowWindow(SW_SHOW);
	
	m_AccountDlg->Create(IDD_ACCOUNT_DIALOG,GetDlgItem(IDC_TAB1));
	m_AccountDlg->ShowWindow(SW_HIDE);
	
	m_HistoryDlg->Create(IDD_HISTORY_DIALOG,GetDlgItem(IDC_TAB1));
	m_HistoryDlg->ShowWindow(SW_HIDE);

	m_SMSHistoryDlg->Create(IDD_SMSHISTORY_DIALOG,GetDlgItem(IDC_TAB1));
	m_SMSHistoryDlg->ShowWindow(SW_HIDE);

	// 결제내역 조회 다이얼로그 생성
	m_PaymentHistoryDlg->Create(IDD_PAYMENT_HISTORY_DIALOG,GetDlgItem(IDC_TAB1));
	m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
	
	// 상품 등록 다이얼로그 생성
	m_ProductRegistrationDlg->Create(IDD_PRODUCT_REGISTRATION_DIALOG,GetDlgItem(IDC_TAB1));
	m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
	
	// 다이얼로그 위치 조정 - 탭 내부에 맞춤
	CRect tabRect;
	m_tab1.GetClientRect(&tabRect);
	m_tab1.AdjustRect(FALSE, &tabRect);  // 탭 헤더 영역 제외
	
	m_ControlDlg->MoveWindow(&tabRect, FALSE);
	m_PaymentHistoryDlg->MoveWindow(&tabRect, FALSE);
	m_ProductRegistrationDlg->MoveWindow(&tabRect, FALSE);


	}

	return TRUE;  // 포커스를 컨트롤에 설정하지 않으면 TRUE를 반환합니다.
}
BOOL CSMT_ManagerDlg::GetDBData()
{
	CADODatabase Database;
	CString strQuery   =_T("");
	BOOL bRet		= TRUE;
    int i=0;
	if(!CSMT_ManagerApp::fnOpenLocalDatabase(Database))
	{
		MessageBox("데이터베이스 연결오류","연결오류",MB_OK);
      return FALSE;
	}
	else
	{
		CADORecordset Rset(&Database);

		////////////////////////////////////////////////
		// 고성준 : 디비에서 알아오는 잘못된 코드 삭제(정렬이 엉망이됨) 라커번호를 ini파일에서 알아오도록 수정함

		//strQuery.Format(_T("select distinct areaCode from tblBoxMaster"));
		//if( !Rset.Open((LPCTSTR)strQuery) ){
		//	MessageBox("데이터베이스 쿼리실패","쿼리오류",MB_OK);
		//	Database.Close();
		//	return FALSE;
		//}

		//if(Rset.IsBOF())
		//{
		//	Rset.Close();
		//	Database.Close();
		//	return FALSE;

		//}
		//else
		//{
		//	while(!Rset.IsEOF() )
		//	{
		//		
		//		if( !Rset.GetFieldValue(_T("areaCode"),LOCK_INFO.m_LockerId[i]) ){bRet = FALSE; break;}
		//		
		//		TRACE("%s",LOCK_INFO.m_LockerId[i]);
		//		i++;
		//		Rset.MoveNext();
		//	}
		//}

		//LOCK_INFO.m_Locker_Sum=i;
		//i=0;

		theApp.fnLoadInIt();
		for (int n = 0; n < 20; n++)
		{
			LOCK_INFO.m_LockerId[n] = theApp.m_strAreaCode[n];
			if (LOCK_INFO.m_LockerId[n].GetLength() > 0)
			{
				LOCK_INFO.m_Locker_Sum = n + 1;
			}
		}


		for(i=0;i<LOCK_INFO.m_Locker_Sum;i++)
		{
		    strQuery.Format(_T("select min(boxNo) as boxmin, count(boxNo) as boxsum from tblBoxMaster where areaCode='%s'"),LOCK_INFO.m_LockerId[i]);
            if( Rset.Open((LPCTSTR)strQuery) )
			{
				Rset.GetFieldValue(_T("boxsum"),LOCK_INFO.m_BoxSum[i]);				
				TRACE("boxsum %d",LOCK_INFO.m_BoxSum[i]);
				Rset.GetFieldValue(_T("boxmin"),LOCK_INFO.m_BoxStartNo[i]);				
				TRACE("boxmin %d",LOCK_INFO.m_BoxStartNo[i]);
				Rset.Close();
			}
			
		}

		// 고성준 : 디비에서 알아오는 잘못된 코드 삭제 라커번호를 ini파일에서 알아오도록 수정함
		////////////////////////////////////////////////


		Database.Close();
	   TRACE("%d",LOCK_INFO.m_Locker_Sum);

		return TRUE;
	}
	return bRet;


}
void CSMT_ManagerDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}


void CSMT_ManagerDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // 그리기를 위한 디바이스 컨텍스트

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// 클라이언트 사각형에서 아이콘을 가운데에 맞춥니다.
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// 아이콘을 그립니다.
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CSkinDialog::OnPaint();
	}
}

// 사용자가 최소화된 창을 끄는 동안에 커서가 표시되도록 시스템에서
//  이 함수를 호출합니다.
HCURSOR CSMT_ManagerDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void CSMT_ManagerDlg::OnSize(UINT nType, int cx, int cy)
{
	CSkinDialog::OnSize(nType, cx, cy);
	if( m_bInitialized )
		RelocationControls();
	// TODO: 여기에 메시지 처리기 코드를 추가합니다.
}
void CSMT_ManagerDlg::RelocationControls()
{
	CRect rc;
	GetClientRect(&rc);

	//int x=0, y=0;

	//SIZE size = Global.GetBitmapSize(IDX_BMP_BTN_BASE);

	//x = rc.Width() - 20 - size.cx;
	//y = rc.Height() - 10 - size.cy;
	//m_Cancel.MoveWindow(x, y, size.cx, size.cy, TRUE);
	//
	//x -= size.cx + 10;
	//m_Ok.MoveWindow(x, y, size.cx, size.cy, TRUE);
	//m_Ok.Invalidate();
}
void CSMT_ManagerDlg::OnBnClickedButton1()
{
	    OnCancel();
}

void CSMT_ManagerDlg::OnBnClickedButton2()
{
        OnCancel();
}

void CSMT_ManagerDlg::OnTcnSelchangeTab1(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	int n_Sel;
	switch (pNMHDR->code)
	{
		case TCN_SELCHANGE:
			n_Sel=TabCtrl_GetCurSel(m_tab1);
			switch(n_Sel)
			{
				case 0:
                    WindowsVisibleCheck(n_Sel);
				    break;
				case 1:
                    WindowsVisibleCheck(n_Sel);
					break;
				case 2:
                    WindowsVisibleCheck(n_Sel);
					break;
				case 3:
                    WindowsVisibleCheck(n_Sel);
					break;
				case 4:
                    WindowsVisibleCheck(n_Sel);
					break;
				default:
                    WindowsVisibleCheck(n_Sel);
					break;
			
			}
			break;
		
	}
	
	*pResult = 0;
}

void CSMT_ManagerDlg::WindowsVisibleCheck(int Sel)
{
#ifdef _DEBUG
	switch(Sel)
	{
		case 0:  // 박스 상태현황 및 제어
				m_ControlDlg->ShowWindow(SW_SHOW);
				m_ControlDlg->SetFocus();

				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
		case 1:  // 결제내역 조회
				m_PaymentHistoryDlg->ShowWindow(SW_SHOW);
				m_PaymentHistoryDlg->SetFocus();

				m_ControlDlg->ShowWindow(SW_HIDE);
				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
		case 2:  // 상품 등록
				m_ProductRegistrationDlg->ShowWindow(SW_SHOW);
				m_ProductRegistrationDlg->SetFocus();

				m_ControlDlg->ShowWindow(SW_HIDE);
				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
			break;
		default:
				m_ControlDlg->ShowWindow(SW_SHOW);
				m_ControlDlg->SetFocus();

				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
	}
#else
	switch(Sel)
	{
		case 0:  // 박스 상태현황 및 제어
				m_ControlDlg->ShowWindow(SW_SHOW);
				m_ControlDlg->SetFocus();

				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
		case 1:  // 결제내역 조회
				m_PaymentHistoryDlg->ShowWindow(SW_SHOW);
				m_PaymentHistoryDlg->SetFocus();

				m_ControlDlg->ShowWindow(SW_HIDE);
				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
		case 2:  // 상품 등록
				m_ProductRegistrationDlg->ShowWindow(SW_SHOW);
				m_ProductRegistrationDlg->SetFocus();

				m_ControlDlg->ShowWindow(SW_HIDE);
				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
			break;
		default:
				m_ControlDlg->ShowWindow(SW_SHOW);
				m_ControlDlg->SetFocus();

				m_AccountDlg->ShowWindow(SW_HIDE);
				m_HistoryDlg->ShowWindow(SW_HIDE);
				m_SMSHistoryDlg->ShowWindow(SW_HIDE);
				m_PaymentHistoryDlg->ShowWindow(SW_HIDE);
				m_ProductRegistrationDlg->ShowWindow(SW_HIDE);
			break;
	}
#endif
	
}
BOOL CSMT_ManagerDlg::PreTranslateMessage(MSG* pMsg)
{
	BOOL retval = CSkinDialog::PreTranslateMessage(pMsg);

	// TODO: 여기에 특수화된 코드를 추가 및/또는 기본 클래스를 호출합니다.
	if (pMsg->message == WM_KEYDOWN)
	{
		if (pMsg->wParam == 120)
		{
			if (this->m_ControlDlg != NULL &&
				this->m_ControlDlg->IsWindowVisible())
			{
				this->m_ControlDlg->fnDoF9Key();
			}
		}
	}

	return retval;
}
