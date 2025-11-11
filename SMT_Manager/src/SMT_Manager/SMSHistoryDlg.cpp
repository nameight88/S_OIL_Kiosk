// SMSHistoryDlg.cpp : 구현 파일입니다.
//
#pragma once
#include "stdafx.h"
#include "SMT_Manager.h"
#include "SMSHistoryDlg.h"
#include "SkinDialog.h"
#include "Ado.h"
// CSMSHistoryDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CSMSHistoryDlg, CDialog)
//CGlobal Global1;
CSMSHistoryDlg::CSMSHistoryDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CSMSHistoryDlg::IDD, pParent)
	, m_Search(_T(""))
	, m_dtpStartCTime(0)
	, m_dtpEndCTime(0)
{

}

CSMSHistoryDlg::~CSMSHistoryDlg()
{
}

void CSMSHistoryDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BUTTON1, m_Button1);
	DDX_Control(pDX, IDC_LIST1, m_ListCon1);
	DDX_Text(pDX, IDC_EDIT1, m_Search);
	DDX_Control(pDX, IDC_COMBO6, m_SearchCombo);
	DDX_Control(pDX, IDC_DATETIMEPICKER1, m_dtpStartCon);
	DDX_Control(pDX, IDC_DATETIMEPICKER3, m_dtpEndCon);
	DDX_DateTimeCtrl(pDX, IDC_DATETIMEPICKER1, m_dtpStartCTime);
	DDX_DateTimeCtrl(pDX, IDC_DATETIMEPICKER3, m_dtpEndCTime);
	DDX_Control(pDX, IDC_BUTTON2, m_Save);
	DDX_Control(pDX, IDC_COMBO_LOCATION, m_address);
	DDX_Control(pDX, IDC_COMBO_PROCESSNAME, m_cmbProcessName);
}


BEGIN_MESSAGE_MAP(CSMSHistoryDlg, CSkinDialog)
	ON_BN_CLICKED(IDC_BUTTON1, &CSMSHistoryDlg::OnBnClickedButton1)
	ON_WM_PAINT()
	ON_BN_CLICKED(IDC_BUTTON2, &CSMSHistoryDlg::OnBnClickedButton2)
END_MESSAGE_MAP()


// CSMSHistoryDlg 메시지 처리기입니다.

void CSMSHistoryDlg::OnBnClickedButton1() //Search 버튼
{
	int i=0,j=0;
	CString createDate;
	CString processName;
	CString areaCode;
	CString sendPhone;
	CString recvPhone;
	CString sendMessage;
	CString sendDate;
	CString strAddress;


    UpdateData(TRUE);
	//if(m_Search==""){MessageBox("검색내용을 입력해 주세요.","입력오류",MB_OK);return;}
	m_ListCon1.DeleteAllItems();


	// 프로세스 이름 조건 추가
	CString strProcessName;
	CString strSqlProcessName;
	m_cmbProcessName.GetWindowText(strProcessName);
	if (m_cmbProcessName.GetCurSel() > 0)
	{
		m_cmbProcessName.GetLBText(m_cmbProcessName.GetCurSel(), strProcessName);
		if (processName == "관리자")
		{
			processName="SMT_Manager";
		}
		strSqlProcessName.Format("processName='%s' and ", strProcessName);
	}
	else
	{
		strSqlProcessName = " ";
	}

	
	// 라커주소 조건 추가
	if (LOCK_INFO.m_Locker_Sum <= 0)
		return;

	CString strSqlLocations = "";
	for (int nIndex = 0; nIndex < LOCK_INFO.m_Locker_Sum - 1; nIndex++)
	{
		strSqlLocations += "areaCode='";
		strSqlLocations += LOCK_INFO.m_LockerId[nIndex];
		strSqlLocations += "' or ";
	}
	strSqlLocations += "areaCode='";
	strSqlLocations += LOCK_INFO.m_LockerId[LOCK_INFO.m_Locker_Sum - 1];
	strSqlLocations += "' ";



	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return ;
	CADORecordset rs(&ADODataBaseLocal);

	if(m_address.GetCurSel()==0){  //address check;
		areaCode="-1";
	}else{
		areaCode=LOCK_INFO.m_LockerId[m_address.GetCurSel()-1];
	}

	if(areaCode=="-1")
	{
		if(m_SearchCombo.GetCurSel()==0)
		{
			sprintf(szQuery,
				"Select createDate,processName,areaCode,sendPhone,recvPhone,sendMessage,sendDate From tblSMSHistory where "
				"%s recvPhone like '%%%s%%' and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc",
				strSqlProcessName,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);

		}
		else
		{
			sprintf(szQuery,
				"Select createDate,processName,areaCode,sendPhone,recvPhone,sendMessage,sendDate From tblSMSHistory where "
				"%s sendPhone like '%%%s%%' and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc",
				strSqlProcessName,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);

		}
	}
	else
	{
		if(m_SearchCombo.GetCurSel()==0)
		{
			sprintf(szQuery,
				"Select createDate,processName,areaCode,sendPhone,recvPhone,sendMessage,sendDate From tblSMSHistory where "
				"%s areaCode='%s' and recvPhone like '%%%s%%' and createDate >= '%s' and createDate <= '%s' order by createDate desc",
				strSqlProcessName,areaCode,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

		}
		else if(m_SearchCombo.GetCurSel()==1)
		{
			sprintf(szQuery,
				"Select createDate,processName,areaCode,sendPhone,recvPhone,sendMessage,sendDate From tblSMSHistory where "
				"%s areaCode='%s' and sendPhone like '%%%s%%' and createDate >= '%s' and createDate <= '%s' order by createDate desc",
				strSqlProcessName,areaCode,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

		}
	}


	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("createDate",createDate );
		rs.GetFieldValue("processName",processName );
		rs.GetFieldValue("areaCode",areaCode );
		rs.GetFieldValue("sendPhone",sendPhone );
		rs.GetFieldValue("recvPhone",recvPhone );
		rs.GetFieldValue("sendMessage",sendMessage );
		rs.GetFieldValue("sendDate",sendDate );

		if (processName == "SMT_Manager")
		{
			processName="관리자";
		}

		for(i=0;i< LOCK_INFO.m_Locker_Sum;i++)
		{
			if(LOCK_INFO.m_LockerId[i] == areaCode)
			{
				break;
			}
		}
		if (i < LOCK_INFO.m_Locker_Sum)
		{
			m_ListCon1.AddItem(createDate,processName,theApp.m_strAddress[i],recvPhone,sendPhone,sendDate,sendMessage);
		}
		else
		{
			m_ListCon1.AddItem(createDate,processName,"없음",recvPhone,sendPhone,sendDate,sendMessage);
		}
		

		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
	return ;

}

BOOL CSMSHistoryDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();
	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	m_Button1.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_Button1.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
	m_Save.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_Save.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	CRect rc;
	GetParent()->GetWindowRect(rc);
	::SetWindowPos(this->m_hWnd,HWND_BOTTOM,10, 30, rc.right-rc.left-20  ,rc.bottom-rc.top-40, SWP_NOZORDER | SWP_SHOWWINDOW);

    fnListConInit();
	fnControlFontSet();
	fnDropListSet();
    ::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO6), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO_LOCATION), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);

	UpdateData(FALSE);
	return TRUE;  
}
void CSMSHistoryDlg::fnDropListSet()
{
    int i=0;
	char *aa[]={"RecvPhone","SendPhone"};
	char *bb[]={"전체","SMTLocker","KHBatch","관리자"};

	for(i=0; i< 2; i++)
	{
		m_SearchCombo.AddString(aa[i]);
	}

	for(i=0; i< 4; i++)
	{
		m_cmbProcessName.AddString(bb[i]);
	}

	for(i=0;i<LOCK_INFO.m_Locker_Sum; i++)
	{
		if(i==0){m_address.AddString("전체");}
		m_address.AddString(theApp.m_strAddress[i]);
	}

	m_cmbProcessName.SetCurSel(0);
	m_address.SetCurSel(0);
	m_SearchCombo.SetCurSel(0);

	CTime m_cur= CTime::GetCurrentTime();
	m_dtpStartCon.SetFormat("yyyy-MM-dd HH:mm:ss");
	m_dtpEndCon.SetFormat("yyyy-MM-dd HH:mm:ss");
	m_dtpStartCTime=m_cur;
	m_dtpEndCTime=m_cur;
    UpdateData(FALSE);
}

void CSMSHistoryDlg::fnControlFontSet()
{
    Font_Set(13,IDC_EDIT1);
    Font_Set(13,IDC_COMBO6);
    Font_Set(13,IDC_COMBO_LOCATION);
    Font_Set(12,IDC_LIST1);
    Font_Set(15,IDC_DATETIMEPICKER1);
    Font_Set(15,IDC_DATETIMEPICKER3);
	
}
void CSMSHistoryDlg::Font_Set(int size,int idc_id)
{
	    HFONT hFontEdit;
    	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
		::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}
void CSMSHistoryDlg::fnListConInit()
{
	m_ListCon1.SetExtendedStyle( LVS_EX_FULLROWSELECT );
	m_ListCon1.SetHeadings( _T("CreatTime,120;processName,100;Location,120;Recv Phone,93;Send Phone,93;SendTime,120;Message,283;") );
	m_ListCon1.LoadColumnInfo();

}

void CSMSHistoryDlg::OnPaint()
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


void CSMSHistoryDlg::OnBnClickedButton2()
{
	int iCnt = m_ListCon1.GetItemCount();
	if (iCnt <= 0) 
	{
		AfxMessageBox("저장할 데이터가 없습니다.");
		return ;

	}

	//현재의 작업경로를 얻어와 저장 한다. 
	char path[MAX_PATH] = {0}; 
	GetCurrentDirectory(MAX_PATH, path); 


	CFileDialog dlg(FALSE);
	dlg.m_ofn.lpstrFilter = "*.csv";
	if (dlg.DoModal() == IDOK)
	{
		// 가장 마지막에 저장해 두었던 작업경로로 다시금 세팅한다.
		SetCurrentDirectory(path); 

		CFile file;
		CFileException e;
		CString FileName = dlg.GetPathName();

		if (FileName.GetLength() <= 4 || FileName.Right(4) != ".csv")
			FileName += ".csv";


		file.Open(FileName, CFile::modeCreate | CFile::modeWrite, &e);
		CString buffer = _T("");

		for( int icol = 0 ; icol < m_ListCon1.GetHeaderCtrl()->GetItemCount() ; icol ++ )
		{

			LVCOLUMN    col;
			col.mask = LVCF_TEXT;
			col.cchTextMax = 512;
			char szCol[512];
			memset( szCol, 0, 512 );
			col.pszText = szCol;
			m_ListCon1.GetColumn( icol, &col );

			buffer += col.pszText; 
			if( icol == m_ListCon1.GetHeaderCtrl()->GetItemCount()-1 ) buffer += "\n";
			else     
				buffer += ",";
		}

		file.Write((LPCSTR)buffer, buffer.GetLength());
		for(int i=0; i<m_ListCon1.GetItemCount();i++)
		{
			for(int j=0;j<m_ListCon1.GetHeaderCtrl()->GetItemCount(); j++){
				buffer = m_ListCon1.GetItemText(i,j);
				buffer.Remove(',');
				file.Write((LPCSTR)buffer, buffer.GetLength());
				if(j==m_ListCon1.GetHeaderCtrl()->GetItemCount()-1){
					buffer = "\n";
					file.Write((LPCSTR)buffer, buffer.GetLength());
				}else{
					buffer = ",";
					file.Write((LPCSTR)buffer, buffer.GetLength());
				}
			}
		}
		file.Close();
		AfxMessageBox("데이터가 저장되었습니다.");
	}
}
