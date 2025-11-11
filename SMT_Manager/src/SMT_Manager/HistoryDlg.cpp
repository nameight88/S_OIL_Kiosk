// HistoryDlg.cpp : 구현 파일입니다.
//
#pragma once
#include "stdafx.h"
#include "SMT_Manager.h"
#include "HistoryDlg.h"
#include "SkinDialog.h"
#include "Ado.h"
// CHistoryDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CHistoryDlg, CDialog)
//CGlobal Global1;
CHistoryDlg::CHistoryDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CHistoryDlg::IDD, pParent)
	, m_Search(_T(""))
	, m_dtpStartCTime(0)
	, m_dtpEndCTime(0)
{

}

CHistoryDlg::~CHistoryDlg()
{
}

void CHistoryDlg::DoDataExchange(CDataExchange* pDX)
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
	DDX_Control(pDX, IDC_COMBO8, m_address);
}


BEGIN_MESSAGE_MAP(CHistoryDlg, CSkinDialog)
	ON_BN_CLICKED(IDC_BUTTON1, &CHistoryDlg::OnBnClickedButton1)
	ON_WM_PAINT()
	ON_BN_CLICKED(IDC_BUTTON2, &CHistoryDlg::OnBnClickedButton2)
END_MESSAGE_MAP()


// CHistoryDlg 메시지 처리기입니다.

void CHistoryDlg::OnBnClickedButton1() //Search 버튼
{
	int i=0,j=0;
    CString areaid;
	CString m_Type;
	CString m_Dtype;
	CString m_event;
	CString m_Selstr="";
	
	CString m_neventType="";
	CString m_ndeliveryType="";
	CString m_nlocation="";
	CString m_nboxNo="";
	CString m_nreceivePhone="";
	CString m_nsendPhone="";
	CString m_nuseType="";
	CString m_npass="";
	CString m_nstartTime="";
	CString m_nendTime="";
	CString m_nCreateTime="";
	CString m_nUserId="";

    UpdateData(TRUE);
	//if(m_Search==""){MessageBox("검색내용을 입력해 주세요.","입력오류",MB_OK);return;}
	m_ListCon1.DeleteAllItems();


	
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
		areaid="-1";
	}else{
		areaid=LOCK_INFO.m_LockerId[m_address.GetCurSel()-1];
	}

	if(areaid=="-1")
	{
		if(m_SearchCombo.GetCurSel()==0)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"(userPhone like '%%%s%%' or transPhone like '%%%s%%') and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc"
				,m_Search,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);

		}
		else if (m_SearchCombo.GetCurSel() == 1)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"boxNo=%s and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc"
				, m_Search, m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"), m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
		}
		/*else if(m_SearchCombo.GetCurSel()==1)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"userCode like '%%%s%%' and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc"
				,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);

		}
		else
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"boxNo=%s and  createDate >= '%s' and createDate <= '%s' and (%s) order by createDate desc"
				,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
		}*/
	}
	else
	{
		if(m_SearchCombo.GetCurSel()==0)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"areaCode='%s' and (userPhone like '%%%s%%' or transPhone like '%%%s%%') and  createDate >= '%s' and createDate <= '%s' order by createDate desc"
				,areaid,m_Search,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

		}
		else if (m_SearchCombo.GetCurSel() == 1)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"areaCode='%s' and boxNo=%s and  createDate >= '%s' and createDate <= '%s' order by createDate desc"
				, areaid, m_Search, m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"), m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
		}
		/*else if(m_SearchCombo.GetCurSel()==1)
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"areaCode='%s' and userCode like '%%%s%%' and  createDate >= '%s' and createDate <= '%s' order by createDate desc"
				,areaid,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

		}
		else
		{
			sprintf(szQuery,
				"Select eventType,createDate,areaCode,boxNo,deliveryType,userPhone,transPhone,startTime,endTime,userCode From tblBoxHistory where "
				"areaCode='%s' and boxNo=%s and  createDate >= '%s' and createDate <= '%s' order by createDate desc"
				,areaid,m_Search,m_dtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_dtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
		}*/
	}


	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("eventType",m_neventType );
		rs.GetFieldValue("areaCode",m_nlocation );
		rs.GetFieldValue("boxNo",m_nboxNo );
		rs.GetFieldValue("userPhone",m_nreceivePhone );
		rs.GetFieldValue("transPhone",m_nsendPhone );
		//rs.GetFieldValue("useType",m_nuseType );
		rs.GetFieldValue("deliveryType",m_ndeliveryType );
		rs.GetFieldValue("startTime",m_nstartTime );
		rs.GetFieldValue("endTime",m_nendTime );
		rs.GetFieldValue("userCode",m_nUserId );
		rs.GetFieldValue("createDate",m_nCreateTime );

		j=StrToInt(m_ndeliveryType);
		switch(j)
		{
			case 1:
		        m_Type="사물함";
				break;
			case 5:
		        m_Type="택배발송";
				break;
			case 9:
		        m_Type="물품전달";
				break;
			case 0:
		        m_Type="없음";
				break;
			default:
		        m_Type="택배배달";
				break;
		}

		j=StrToInt(m_neventType);
		switch(j){
			case 0:
		        m_event="사물함신규 ";
				break;
			case 1:
		        m_event="사물함열기";
				break;
			case 2:
		        m_event="사물함연장";
				break;
			case 3:
		        m_event="사물함반납";
				break;
			case 4:
		        m_event="사물함연체반납";
				break;
			case 5:
		        m_event="택배배달";
				break;
			case 6:
		        m_event="택배배달실패";
				break;
			case 7:
		        m_event="택배찾기";
				break;
			case 8:
		        m_event="택배/물품회수하기";
				break;
			case 9:
		        m_event="물품전달";
				break;
			case 10:
		        m_event="택배발송";
				break;
			case 11:
		        m_event="택배수거";
				break;
			case 12:
		        m_event="물품찾기";
				break;
			case 13:
		        m_event="관리자반납";
				break;
			case 14:
		        m_event="관리자디비수정";
				break;
			case 15:
		        m_event="관리자열기";
				break;
			case LOCKER_DELIVERY_MAN_MODIFY:
		        m_event="기사번호수정";
				break;
			case LOCKER_SYSTEM_AUTO_RETURN:
		        m_event="시스템자동반납";
				break;
			default:
		        m_event="없음";
				break;
		}

		j=StrToInt(m_ndeliveryType);
		switch(j){
			case 10:
		        m_Dtype="우체국";
				break;
			case 11:
		        m_Dtype="한진택배";
				break;
			case 12:
		        m_Dtype="대한통운";
				break;
			case 13:
		        m_Dtype="현대택배";
				break;
			case 14:
		        m_Dtype="CJ택배";
				break;
			case 15:
		        m_Dtype="로젠택배";
				break;
			case 16:
		        m_Dtype="옐로우캡";
				break;
			case 17:
		        m_Dtype="훼미리";
				break;
			case 18:
		        m_Dtype="KGB";
				break;
			case 19:
		        m_Dtype="동부택배";
				break;
			case 20:
		        m_Dtype="하나로";
				break;
			case 21:
		        m_Dtype="기타";
				break;
			default:
		        m_Dtype="해당없음";
				break;
		}

		for(i=0;i< LOCK_INFO.m_Locker_Sum;i++)
		{
			if(LOCK_INFO.m_LockerId[i] == m_nlocation){

				break;
			}
		}

		(void)m_ListCon1.AddItem( m_event,m_nCreateTime,theApp.m_strAddress[i],m_nboxNo,m_Type,m_Dtype,m_nUserId,m_nreceivePhone,m_nsendPhone,m_nstartTime,m_nendTime);

		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
	return ;

}

BOOL CHistoryDlg::OnInitDialog()
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
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO8), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);

	UpdateData(FALSE);
	return TRUE;  
}
void CHistoryDlg::fnDropListSet()
{
    int i=0;
	//char *aa[]={"UserPhone","UserId","BoxNo"};
	char *aa[]={"UserPhone","BoxNo"};

	for(i=0; i< 2; i++)
	{
		m_SearchCombo.AddString(aa[i]);
	}

	for(i=0;i<LOCK_INFO.m_Locker_Sum; i++)
	{
		if(i==0){m_address.AddString("전체");}
		m_address.AddString(theApp.m_strAddress[i]);
	}
	m_address.SetCurSel(0);
	m_SearchCombo.SetCurSel(0);

	CTime m_cur= CTime::GetCurrentTime();
	m_dtpStartCon.SetFormat("yyyy-MM-dd HH:mm:ss");
	m_dtpEndCon.SetFormat("yyyy-MM-dd HH:mm:ss");
	m_dtpStartCTime=m_cur;
	m_dtpEndCTime=m_cur;
    UpdateData(FALSE);
}

void CHistoryDlg::fnControlFontSet()
{
    Font_Set(13,IDC_EDIT1);
    Font_Set(13,IDC_COMBO6);
    Font_Set(13,IDC_COMBO8);
    Font_Set(12,IDC_LIST1);
    Font_Set(15,IDC_DATETIMEPICKER1);
    Font_Set(15,IDC_DATETIMEPICKER3);
	
}
void CHistoryDlg::Font_Set(int size,int idc_id)
{
	HFONT hFontEdit;
	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
	::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}
void CHistoryDlg::fnListConInit()
{
	(void)m_ListCon1.SetExtendedStyle( LVS_EX_FULLROWSELECT );
	//m_ListCon1.SetHeadings( _T("EventType,72;Create Time,120;Location,180;No,30;UseType,65;Company,67;UserId,65;ReceiveUser Phone,123;SendUser Phone,107;StartTime,120;EndTime,120;") );
	m_ListCon1.SetHeadings( _T("EventType,72;Create Time,120;Location,180;No,30;UseType,65;Company,67;UserId,65;User Phone,123;SendUser Phone,107;StartTime,120;EndTime,120;") );
	m_ListCon1.LoadColumnInfo();

}

void CHistoryDlg::OnPaint()
{
	CTime tmNow = CTime::GetTickCount() - CTimeSpan(0,0,0,1);
	if (m_tmLastPaint > tmNow)
		return;
	
	m_tmLastPaint = CTime::GetTickCount();


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


void CHistoryDlg::OnBnClickedButton2()
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
