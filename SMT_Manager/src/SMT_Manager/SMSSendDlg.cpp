// SMSSendDlg.cpp : 구현 파일입니다.
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "SMSSendDlg.h"
#include "Ado.h"


// CSMSSendDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CSMSSendDlg, CDialog)

CSMSSendDlg::CSMSSendDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CSMSSendDlg::IDD, pParent)
{
	m_nCurrentSelidx = -1;
}

CSMSSendDlg::~CSMSSendDlg()
{
}

void CSMSSendDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_SMS_SEND_LIST, m_listSend);
	DDX_Text(pDX, IDC_EDIT_SMS_MSG, m_strMsg);
	DDX_Control(pDX, IDC_COMBO_LOCATION, m_cmbAddress);
	DDX_Control(pDX, IDC_BTN_SMS_SEND, m_btnSend);
	DDX_Control(pDX, IDC_BTN_SMS_RESEND, m_btnResend);
}


BEGIN_MESSAGE_MAP(CSMSSendDlg, CDialog)
	ON_BN_CLICKED(IDC_BTN_SMS_SEND, &CSMSSendDlg::OnBnClickedBtnSmsSend)
	ON_WM_PAINT()
	ON_CBN_SELCHANGE(IDC_COMBO_LOCATION, &CSMSSendDlg::OnCbnSelchangeComboLocation)
	ON_BN_CLICKED(IDC_BTN_SMS_RESEND, &CSMSSendDlg::OnBnClickedBtnSmsResend)
END_MESSAGE_MAP()



// CSMSSendDlg 메시지 처리기입니다.

void CSMSSendDlg::OnBnClickedBtnSmsSend()
{
	CString areaCode;
	int boxNo;
	CString userPhone;

	UpdateData(TRUE);

	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	if(m_strMsg.GetLength() <= 0)
	{
		MessageBox("발송내용을 입력해 주세요.","입력오류",MB_OK);
		return;
	}
	m_listSend.DeleteAllItems();

	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return ;
	CADORecordset rs(&ADODataBaseLocal);

	sprintf (szQuery, "select areaCode, boxNo, userPhone from tblBoxMaster where useState=1 and userPhone <> '' and areaCode='%s'",
			LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]);
	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("areaCode",areaCode );
		rs.GetFieldValue("boxNo", boxNo);
		rs.GetFieldValue("userPhone", userPhone);
		CString strBoxNo;

		strBoxNo.Format("%d", boxNo);

		if(CSMT_ManagerApp::fnSendSMS(areaCode, areaCode, boxNo, "스마트큐브", theApp.m_strManagerPhone, userPhone, m_strMsg) )
		{
			m_listSend.AddItem(strBoxNo, userPhone, "전송 성공!");
		}
		else
		{
			m_listSend.AddItem(strBoxNo, userPhone, "전송 실패!");
		}
		m_listSend.RedrawWindow();
		Sleep(500);
		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
}

void CSMSSendDlg::OnPaint()
{
	CPaintDC dc(this); // device context for painting
	CRect rc;
	GetClientRect(&rc);
	dc.FillSolidRect(rc, Global.GetRGB(IDX_RGB_BACKGROUND));

	CDC memDC; // bk DC
	memDC.CreateCompatibleDC(&dc);//조동환

	//Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());

	Graphics graphics(dc); // bk graphics
    BSTR str;
	CString m_bg;
    m_bg="..\\skin\\locker_bg1.png";
    str=m_bg.AllocSysString();

	Bitmap myBMP(str);
	//int iwd = myBMP.GetWidth();
	//int iht = myBMP.GetHeight();
	Rect rect(0,0,rc.Width(),rc.Height()-150);
	graphics.DrawImage(&myBMP,rect);
		
	graphics.ReleaseHDC(dc.m_hDC);	// 임시 막아둠

	dc.BitBlt(0,0,rc.Width(),rc.Height(),&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC

    memDC.DeleteDC();

}

BOOL CSMSSendDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();
	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	CRect rc;
	GetParent()->GetWindowRect(rc);
	::SetWindowPos(this->m_hWnd,HWND_BOTTOM,10, 30, rc.right-rc.left-20  ,rc.bottom-rc.top-40, SWP_NOZORDER | SWP_SHOWWINDOW);

	if (CSMT_ManagerApp::m_bSendSMS)
	{
		m_btnSend.EnableWindow(TRUE);
		m_btnResend.EnableWindow(TRUE);
	}

    fnListConInit();
	fnControlFontSet();
	fnDropListSet();

	UpdateData(FALSE);
	return TRUE;  
}
void CSMSSendDlg::fnDropListSet()
{
	int i;

	for(i=0;i<LOCK_INFO.m_Locker_Sum; i++)
	{
		m_cmbAddress.AddString(theApp.m_strAddress[i]);
	}
	m_cmbAddress.SetCurSel(0);
	LOCK_INFO.m_Selidx =0;
	this->m_nCurrentSelidx = LOCK_INFO.m_Selidx;
}

void CSMSSendDlg::fnControlFontSet()
{
	Font_Set(13,IDC_COMBO_LOCATION);
    Font_Set(13,IDC_EDIT_SMS_MSG);
    Font_Set(12,IDC_SMS_SEND_LIST);
}

void CSMSSendDlg::Font_Set(int size,int idc_id)
{
	    HFONT hFontEdit;
    	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
		::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}
void CSMSSendDlg::fnListConInit()
{
	m_listSend.SetExtendedStyle( LVS_EX_FULLROWSELECT );
	m_listSend.SetHeadings( _T("Box No,100;Recv Phone,200;status,240;") );
	m_listSend.LoadColumnInfo();

}

void CSMSSendDlg::OnCbnSelchangeComboLocation()
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	CString strText;

  	LOCK_INFO.m_Selidx = m_cmbAddress.GetCurSel();
	this->m_nCurrentSelidx = LOCK_INFO.m_Selidx;
	
	m_cmbAddress.GetLBText(LOCK_INFO.m_Selidx, strText);
	if (this->m_strAddress != strText)
		this->m_strAddress = strText;
}

void CSMSSendDlg::OnBnClickedBtnSmsResend()
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	CString areaCode = LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx];
	int boxNo = 0;
	CString userPhone;
	CString result;

	UpdateData(TRUE);

	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	if(m_strMsg.GetLength() <= 0)
	{
		MessageBox("발송내용을 입력해 주세요.","입력오류",MB_OK);
		return;
	}

	int i = 0;
	int nCnt = m_listSend.GetItemCount();
	while(i < nCnt)
	{
		result = m_listSend.GetItemText(i, 2);
		if (result.Find("실패") < 0)
		{
			i++;
			continue;
		}
		CString strBoxNo = m_listSend.GetItemText(i, 0);
		boxNo = _ttoi(strBoxNo);
		userPhone = m_listSend.GetItemText(i, 1);

		if(CSMT_ManagerApp::fnSendSMS(areaCode, areaCode, boxNo, "스마트큐브", theApp.m_strManagerPhone, userPhone, m_strMsg) )
		{
			m_listSend.SetItemText(i, 2, "재전송 성공!");
		}
		else
		{
			m_listSend.SetItemText(i, 2, "재전송 실패!");
		}
		m_listSend.RedrawWindow();
		Sleep(100);
		i++;
	}
}
