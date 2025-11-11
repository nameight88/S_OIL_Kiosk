// LongTermUseDlg.cpp : 구현 파일입니다.
//
#pragma once
#include "stdafx.h"
#include "SMT_Manager.h"
#include "LongTermUseDlg.h"
#include "SkinDialog.h"
#include "Ado.h"
#include "PLCControl.h"
#include "PLCMatrixDatas.h"
#include "BoxDatas.h"
// CLongTermUseDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CLongTermUseDlg, CDialog)
//CGlobal Global1;
CLongTermUseDlg::CLongTermUseDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CLongTermUseDlg::IDD, pParent)
	, m_nCurrentSelidx(-1)
	, m_OverUseDay(_T(""))
	, m_strUseTypeSearch(_T(""))
	, m_strAddress(_T(""))
	, m_strBoxNo(_T(""))
	, m_strUseType(_T(""))
	, m_strReceivePhone(_T(""))
	, m_strSendPhone(_T(""))
	, m_strStartTime(_T(""))
	, m_strEndTime(_T(""))
{

}

CLongTermUseDlg::~CLongTermUseDlg()
{
}

void CLongTermUseDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BUTTON_SMS_ALL, m_SMSTotalSend);

	/////////////////////
	// 고성준 : 저장 버튼 추가
	DDX_Control(pDX, IDC_BTN_STORE, m_btnStore);

	DDX_Control(pDX, IDC_LIST1, m_ListCon1);
	DDX_Control(pDX, IDC_BUTTON_SEARCH, m_Search);
	DDX_Control(pDX, IDC_BUTTON_SMS_ONE, m_SMSSend);
	DDX_Control(pDX, IDC_BUTTON_OPEN, m_btnOpen);
	DDX_Control(pDX, IDC_BUTTON_RETURN, m_btnReturn);
	DDX_Control(pDX, IDC_COMBO1, m_LongUseDay);
	DDX_CBString(pDX, IDC_COMBO1, m_OverUseDay);
	DDX_Control(pDX, IDC_COMBO5, m_UseTypeCombo);
	DDX_CBString(pDX, IDC_COMBO5, m_strUseTypeSearch);
	DDX_Text(pDX, IDC_EDIT1, m_strAddress);
	DDX_Text(pDX, IDC_EDIT2, m_strBoxNo);
	
	/////////////////////////////
	// 고성준 : 사용자명, 정보 추가
	DDX_Text(pDX, IDC_EDIT_USERNAME, m_strUserName);
	DDX_Text(pDX, IDC_EDIT_USERINFO, m_strUserInfo);


	DDX_Text(pDX, IDC_EDIT9, m_strUseType);
	DDX_Text(pDX, IDC_EDIT3, m_strReceivePhone);
	DDX_Text(pDX, IDC_EDIT4, m_strSendPhone);
	DDX_Text(pDX, IDC_EDIT5, m_strStartTime);
	DDX_Text(pDX, IDC_EDIT6, m_strEndTime);
}


BEGIN_MESSAGE_MAP(CLongTermUseDlg, CSkinDialog)
	ON_WM_PAINT()
	ON_BN_CLICKED(IDC_BUTTON_SEARCH, &CLongTermUseDlg::OnBnClickedButtonSearch)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST1, &CLongTermUseDlg::OnLvnItemchangedList1)
	ON_BN_CLICKED(IDC_BUTTON_SMS_ONE, &CLongTermUseDlg::OnBnClickedButtonSmsOne)
	ON_BN_CLICKED(IDC_BUTTON_SMS_ALL, &CLongTermUseDlg::OnBnClickedButtonSmsAll)
	ON_BN_CLICKED(IDC_BTN_STORE, &CLongTermUseDlg::OnBnClickedBtnStore)
	ON_BN_CLICKED(IDC_BUTTON_OPEN, &CLongTermUseDlg::OnBnClickedButtonOpen)
	ON_BN_CLICKED(IDC_BUTTON_RETURN, &CLongTermUseDlg::OnBnClickedButtonReturn)
	ON_WM_SHOWWINDOW()
END_MESSAGE_MAP()


// CLongTermUseDlg 메시지 처리기입니다.

BOOL CLongTermUseDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();


	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	m_SMSTotalSend.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_BIG), Global.GetRGB(IDX_RGB_MASK));
	m_SMSTotalSend.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	/////////////////////
	// 고성준 : 저장 버튼 추가
	m_btnStore.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_BIG), Global.GetRGB(IDX_RGB_MASK));
	m_btnStore.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_btnOpen.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_btnOpen.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_btnReturn.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_btnReturn.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_Search.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_Search.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_SMSSend.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_SMSSend.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	CRect rc;
	GetParent()->GetWindowRect(rc);
	::SetWindowPos(this->m_hWnd,HWND_BOTTOM,10, 30, rc.right-rc.left-20  ,rc.bottom-rc.top-40, SWP_NOZORDER | SWP_SHOWWINDOW);

	fnListConInit();
    fnControlFontSet();
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO1), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO5), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);

	CString input;
	int i;
	for(i=0;i<= 90; i++)
	{
		input.Format("%d",i);
		m_LongUseDay.AddString(input);
	}

	char *cc[]={"전체","사물함","물품전달","택배발송","택배배달"};
	//char *cc[]={"전체","사물함"};
	for(i=0; i< 2; i++)
	{
		m_UseTypeCombo.AddString(cc[i]);
	}
	m_UseTypeCombo.SetCurSel(0);
	m_LongUseDay.SetCurSel(0);

	return TRUE;  // return TRUE unless you set the focus to a control

}

void CLongTermUseDlg::fnListConInit()
{
	(void)m_ListCon1.SetExtendedStyle( LVS_EX_FULLROWSELECT );
	m_ListCon1.SetHeadings( _T("Location,110;No,35;UseType,70;Name,55;Information,127;UserId,85;User Phone,85;Send Phone,85;Start Time,125;End Time,125") );
	m_ListCon1.LoadColumnInfo();

}

void CLongTermUseDlg::fnControlFontSet()
{
    fnFontSet(13,IDC_COMBO1);
    fnFontSet(13,IDC_COMBO5);
    fnFontSet(15,IDC_EDIT1);
    fnFontSet(15,IDC_EDIT2);
    fnFontSet(15,IDC_EDIT3);
    fnFontSet(15,IDC_EDIT4);
    fnFontSet(15,IDC_EDIT5);
    fnFontSet(15,IDC_EDIT6);
    fnFontSet(15,IDC_EDIT_USERNAME);
    fnFontSet(15,IDC_EDIT_USERINFO);

    fnFontSet(12,IDC_LIST1);

}

void CLongTermUseDlg::fnFontSet(int size,int idc_id)
{
	HFONT hFontEdit;
	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
	::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}

void CLongTermUseDlg::OnPaint()
{
	CTime tmNow = CTime::GetCurrentTime();
	CTimeSpan span(0, 0, 0, 1); // 1초
	if (m_tmLastPaint.GetTime() > 0 && (tmNow - m_tmLastPaint).GetTotalSeconds() < 1)
		return;
	
	m_tmLastPaint = CTime::GetCurrentTime();


	CPaintDC dc(this); // device context for painting
	CRect rc;
	GetClientRect(&rc);
	dc.FillSolidRect(rc, Global.GetRGB(IDX_RGB_BACKGROUND));

	CDC memDC; // bk DC
	memDC.CreateCompatibleDC(&dc);//장치환경

	//Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());

	Graphics graphics(dc); // bk graphics
    BSTR str;
	CString m_bg;
    m_bg="..\\skin\\locker_bg1.png";
    str=m_bg.AllocSysString();

	Bitmap myBMP(str);
	//int iwd = myBMP.GetWidth();
	//int iht = myBMP.GetHeight();
	Rect rect(0,0,rc.Width(),rc.Height()-190);

	graphics.DrawImage(&myBMP,rect);
		
	graphics.ReleaseHDC(dc.m_hDC);	// 임시 리소스들

	dc.BitBlt(0,0,rc.Width(),rc.Height(),&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC

    memDC.DeleteDC();

}

void CLongTermUseDlg::OnBnClickedButtonSearch() //search button
{
	int i=0,j=0;
	CString Type;
	CString Selstr="";
	
	CString areaCode="";
	CString boxNo="";
	CString receivePhone="";
	CString sendPhone="";
	CString deliveryType="";
	CString startTime="";
	CString endTime="";
	CString userCode="";
	CString boxPassword="";

	
	/////////////////////////////
	// 고성준 : 사용자명, 정보 추가
	CString strUserName;
	CString strUserInfo;



    UpdateData(TRUE);
	CTime cur= CTime::GetCurrentTime();
    cur-=CTimeSpan(_ttoi(m_OverUseDay),0,0,0);
 //   UpdateData(TRUE);
	//if(Search==""){MessageBox("검색내용을 입력해 주세요.","입력오류",MB_OK);return;}
	m_ListCon1.DeleteAllItems();
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return ;
	CADORecordset rs(&ADODataBaseLocal);

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


	//char *cc[]={"전체","사물함","물품전달","택배발송","택배배달"};
	if(m_UseTypeCombo.GetCurSel()==0){
		sprintf(szQuery,
			"Select areaCode,boxNo,userName,userPhone,deliveryType,transPhone,barCode,boxPassword,startTime,endTime,userCode From tblBoxMaster where "
			" useState=1 and endTime < '%s' and (%s)"
			,cur.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
	}
	if(m_UseTypeCombo.GetCurSel()==1){
		sprintf(szQuery,
			"Select areaCode,boxNo,userName,userPhone,deliveryType,transPhone,barCode,boxPassword,startTime,endTime,userCode From tblBoxMaster where "
			" useState=1 and deliveryType=1 and endTime < '%s' and (%s)"
			,cur.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
	}
	if(m_UseTypeCombo.GetCurSel()==2){
		sprintf(szQuery,
			"Select areaCode,boxNo,userName,userPhone,deliveryType,transPhone,barCode,boxPassword,startTime,endTime,userCode From tblBoxMaster where "
			" useState=1 and deliveryType=9 and endTime < '%s' and (%s)"
			,cur.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
	}
	if(m_UseTypeCombo.GetCurSel()==3){
		sprintf(szQuery,
			"Select areaCode,boxNo,userName,userPhone,deliveryType,transPhone,barCode,boxPassword,startTime,endTime,userCode From tblBoxMaster where "
			" useState=1 and deliveryType=5 and endTime < '%s' and (%s)"
			,cur.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
	}
	if(m_UseTypeCombo.GetCurSel()==4){
		sprintf(szQuery,
			"Select areaCode,boxNo,userName,userPhone,deliveryType,transPhone,barCode,boxPassword,startTime,endTime,userCode From tblBoxMaster where "
			" useState=1 and deliveryType>9 and endTime < '%s' and (%s)"
			,cur.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
	}

	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("areaCode",areaCode );
		rs.GetFieldValue("boxNo",boxNo );
		rs.GetFieldValue("userPhone",receivePhone );
		rs.GetFieldValue("transPhone",sendPhone );
		rs.GetFieldValue("deliveryType",deliveryType );
		rs.GetFieldValue("startTime",startTime );
		rs.GetFieldValue("endTime",endTime );
		rs.GetFieldValue("userCode",userCode );
		rs.GetFieldValue("boxPassword",boxPassword );


		/////////////////////////////
		// 고성준 : 사용자명, 정보 추가
		rs.GetFieldValue("userName",strUserName );
		rs.GetFieldValue("barCode",strUserInfo );

		try
		{
			if (strUserInfo.GetLength() > 2)
			{
				int nYear = ::StrToInt(strUserInfo.Mid(0,2));
				CString strText = strUserInfo.Mid(2, strUserInfo.GetLength()-2);
				strUserInfo.Format(_T("%s %d년"), strText, nYear);
			}
			else
			{
				strUserInfo = boxPassword;
			}
		}
		catch(...)
		{
		}


		j=StrToInt(deliveryType);
		switch(j){
			case 1:
		        Type="사물함";
				break;
			case 5:
		        Type="택배발송";
				break;
			case 9:
		        Type="물품전달";
				break;
			case 10:case 11:case 12:case 13:case 14:case 15:case 16:case 17:case 18:case 19:case 20:case 21:
		        Type="택배배달";
				break;
			default:
		        Type="없음";
				break;
		}


		for(i=0;i< LOCK_INFO.m_Locker_Sum;i++)
		{
			if(LOCK_INFO.m_LockerId[i] == areaCode){

				break;
			}
		}

		/////////////////////////////
		// 고성준 : 사용자명, 정보 추가 
		(void)m_ListCon1.AddItem( theApp.m_strAddress[i],boxNo,Type,strUserName,strUserInfo,userCode,receivePhone,sendPhone,startTime,endTime);

		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
	return ;
}

void CLongTermUseDlg::OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	int selidx=0;
	char m_temp[255];

	switch (pNMHDR->code) {
		case LVN_ITEMCHANGED:
			if (pNMLV->uChanged == LVIF_STATE && pNMLV->uNewState == (LVIS_SELECTED | LVIS_FOCUSED)) {
				//selidx=ListView_GetNextItem(::GetDlgItem(this->m_hWnd,IDC_LIST1),-1,LVNI_ALL | LVNI_SELECTED);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,0,m_temp,255);
				m_strAddress.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,1,m_temp,255);
				m_strBoxNo.Format("%s",m_temp);

				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,2,m_temp,255);
				m_strUseType.Format("%s",m_temp);

				/////////////////////////////
				// 고성준 : 사용자명, 정보 추가
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,3,m_temp,255);
				m_strUserName.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,4,m_temp,255);
				m_strUserInfo.Format("%s",m_temp);

				//ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,5,m_temp,255);
				//m_strAddress.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,6,m_temp,255);
				m_strReceivePhone.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,7,m_temp,255);
				m_strSendPhone.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,8,m_temp,255);
				m_strStartTime.Format("%s",m_temp);
				ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),pNMLV->iItem,9,m_temp,255);
				m_strEndTime.Format("%s",m_temp);
				UpdateData(FALSE);
			}
		break;
	}

	for (int nIndex = 0; nIndex < LOCK_INFO.m_Locker_Sum; nIndex++)
	{
		if (theApp.m_strAddress[nIndex] == this->m_strAddress)
		{
			this->m_nCurrentSelidx = nIndex;
		}
	}


	*pResult = 0;
}

void CLongTermUseDlg::OnBnClickedButtonSmsOne()//개별전송
{
	CString strMSG	 = _T("");
	CString strText	 = _T("");
    UpdateData(TRUE);
	int m_Total;
    m_Total=ListView_GetItemCount(::GetDlgItem(this->m_hWnd,IDC_LIST1));
	if(m_Total==0){
		MessageBox("SMS 전송 할 검색된 데이터가 없습니다.","SMS 전송 데이터 오류",MB_OK);
        return ;
	}
	if(m_strUseType=="사물함"){
	strMSG.Format("%s %s번함 연체료가 계속 부과되고 있으니 반납 처리 해 주십시오.",
		m_strAddress,m_strBoxNo,m_strEndTime );
	}else if(m_strUseType=="물품전달"){
	strMSG.Format("%s %s번함 연체료가 계속 부과되고 있으니 물품을 찾아 주십시오.",
		m_strAddress,m_strBoxNo,m_strEndTime );
	}else if(m_strUseType=="택배발송"){
	strMSG.Format("[택배수거요청][%s]라커%s번함[택배의뢰시간:%s]",
		m_strAddress,m_strBoxNo,m_strEndTime );
	}else if(m_strUseType=="택배배달"){
	strMSG.Format("%s %s번함 인증번호[%s] 택배를 찾아 주십시오.[%s]",
		m_strAddress,m_strBoxNo,m_strUserInfo,m_strEndTime );
	}
	strText.Format("%s에위치한 사물함 %s번을 사용하고 있는\n\n%s분께 연체사용 메시지를 보내시겠습니까?",m_strAddress,m_strBoxNo,m_strReceivePhone);
	TRACE("%s",theApp.m_strManagerPhone);
	if(MessageBox(strText,"SMS 전송확인",MB_OKCANCEL)==IDOK)
	{

		if(m_strReceivePhone.GetLength() <= 0)
		{
			this->MessageBox("전송 중지!\r\n수신자 핸드폰 번호와 비밀번호를 기입해 주십시오.","SMS 전송",MB_OK);
			return;
		}

		if( !CSMT_ManagerApp::fnSendSMS(m_strAddress,m_strAddress, StrToInt(m_strBoxNo), "KHTech", theApp.m_strManagerPhone,m_strReceivePhone, strMSG) )
		{
			strText="SMS를 보내는데 실패 하였습니다. 관리자에게 문의바랍니다.";
			MessageBox(strText,"SMS 전송실패",MB_OK);
			return;

		}else{
			strText="SMS를 정상적으로 보냈습니다.";
			MessageBox(strText,"SMS 전송완료",MB_OK);
			return;

		}
	}

}


void CLongTermUseDlg::OnBnClickedButtonSmsAll()//전체전송
{

	CString strMSG	 = _T("");
	CString strText	 = _T("");
	int m_Total,i;
    m_Total=ListView_GetItemCount(::GetDlgItem(this->m_hWnd,IDC_LIST1));
	if(m_Total==0){
		MessageBox("SMS 전송 할 검색된 데이터가 없습니다.","SMS 전송 데이터 오류",MB_OK);
        return ;
	}else{
		strText.Format("%d건의 SMS전송을 하시겠습니까?",m_Total);
		if(MessageBox(strText,"SMS 전송확인",MB_OKCANCEL)==IDCANCEL){
			return;
		}
	}


	int nEmpty = 0;
	int nFalse = 0;
	for(i=0;i< m_Total; i++)
	{
		fnGetListConData(i);

		if(m_strUseType=="사물함"){
			strMSG.Format("%s %s번함 연체료가 계속 부과되고 있으니 반납 처리 해 주십시오.",
				m_strAddress,m_strBoxNo,m_strEndTime );
		}else if(m_strUseType=="물품전달"){
			strMSG.Format("%s %s번함 연체료가 계속 부과되고 있으니 물품을 찾아 주십시오.",
				m_strAddress,m_strBoxNo,m_strEndTime );
		}else if(m_strUseType=="택배발송"){
			strMSG.Format("[택배수거요청][%s]라커%s번함[택배의뢰시간:%s]",
				m_strAddress,m_strBoxNo,m_strEndTime );
		}else if(m_strUseType=="택배배달"){
			strMSG.Format("%s %s번함 연체료가 계속 부과되고 있으니 택배를 찾아 주십시오.",
				m_strAddress,m_strBoxNo,m_strEndTime );
		}

		if (m_strReceivePhone.GetLength() > 0)
		{
			if( !CSMT_ManagerApp::fnSendSMS(m_strAddress,m_strAddress, StrToInt(m_strBoxNo), "KHTech", theApp.m_strManagerPhone,m_strReceivePhone, strMSG) )
			{
				strText.Format("%s 번호로\r\nSMS를 보내는데 실패 하였습니다.\r\n관리자에게 문의바랍니다.", m_strReceivePhone);
				MessageBox(strText,"SMS 전송실패",MB_OK);
				nFalse++;
			}
		}
		else
		{
			nEmpty++;
		}

		Sleep(500);
	}

	if (nEmpty > 0)
	{
		strText.Format("수신자(사용자) 핸드폰 번호가 없는\r\n%d 개의 SMS 전송을 취소했습니다.", nEmpty);
		MessageBox(strText,"SMS 전송 취소",MB_OK);
	}

	strText.Format("%d/%d 건의 SMS전송을 완료 했습니다.", m_Total - nEmpty - nFalse, m_Total);
	MessageBox(strText,"SMS 전송완료",MB_OK);
		
}

void CLongTermUseDlg::fnGetListConData(int m_idx)
{
	char m_temp[255];

	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,0,m_temp,255);
	m_strAddress.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,1,m_temp,255);
	m_strBoxNo.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,2,m_temp,255);
	m_strUseType.Format("%s",m_temp);

	/////////////////////////////
	// 고성준 : 사용자명, 정보 추가
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,3,m_temp,255);
	m_strUserName.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,4,m_temp,255);
	m_strUserInfo.Format("%s",m_temp);

	//ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,5,m_temp,255);
	//m_strAddress.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,6,m_temp,255);
	m_strReceivePhone.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,7,m_temp,255);
	m_strSendPhone.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,8,m_temp,255);
	m_strStartTime.Format("%s",m_temp);
	ListView_GetItemText(::GetDlgItem(this->m_hWnd,IDC_LIST1),m_idx,9,m_temp,255);
	m_strEndTime.Format("%s",m_temp);

	//UpdateData(FALSE);
}
void CLongTermUseDlg::OnBnClickedBtnStore()
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

void CLongTermUseDlg::OnBnClickedButtonOpen()
{
	CString strText;
	CADODatabase ADODataBaseLocal;

	if (this->m_nCurrentSelidx < 0)
	{
		MessageBox("먼저 사용자를 선택하십시오.");
		return;
	}

	if (LOCK_INFO.m_Selidx != this->m_nCurrentSelidx)
	{
		LOCK_INFO.m_Selidx = this->m_nCurrentSelidx;

		if( !theApp.fnInitPLCControl())
		{
			MessageBox("PLC 연결 실패. 장비 상태 점검후 관리자에게 연락 바랍니다.");
		}
	}



	int iPLCNo = 0;
	int iBoxNo = atoi(this->m_strBoxNo.GetBuffer());
	this->m_strBoxNo.ReleaseBuffer();
	int iRet   = 0;
	int nIndex;
	
	// GetTickCount()를 GetCurrentTime()으로 변경
	CTime tmStart = CTime::GetCurrentTime();

	if(iBoxNo < LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx])
	{
		MessageBox("먼저 사용자를 선택하십시오.");
		return;
	}

	strText.Format("%s에 위치한\n\n보관함 %d번을 엽니다.\n\n다시한번 확인해 주십시오.\n\n진행을 하시겠습니까?",theApp.m_strAddress[LOCK_INFO.m_Selidx],iBoxNo);
	if( MessageBox(strText,"오픈 확인",MB_OKCANCEL) == IDCANCEL)
	{
		return;
	}

	if( theApp.m_BoxMapTable.Lookup(iBoxNo,iPLCNo) )
	{
		if(theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] != 0)
		{
			if (theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] <= iBoxNo) // PLC 2
			{
				if (theApp.m_strPLCIPAddr_2[LOCK_INFO.m_Selidx].GetLength() <= 0)
				{
					strText.Format("[%s]는 현재\r\n%d번 보관함까지만\r\n사용하실 수 있습니다.", 
						theApp.m_strAddress[LOCK_INFO.m_Selidx], theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] - 1);
					MessageBox(strText);
					return;
				}
				else
				{
					for(nIndex=0;nIndex<3;nIndex++)
					{
						theApp.m_PLCControl_2.OpenBox(iPLCNo);
						Sleep(750);		

						// GetTickCount()를 GetCurrentTime()으로 변경
						CTime tmNow = CTime::GetCurrentTime();
						CTimeSpan ts = tmNow - tmStart;
						if (ts.GetTotalSeconds() > nIndex*5+5)
						{
							nIndex=3;
							break;
						}

						if (!theApp.m_PLCControl_2.IsOpenBox(iPLCNo,&iRet) || !iRet)
							continue;
						else
							break;
					}
					theApp.m_PLCControl_2.ExitPLC();
				}
			}
			else
			{
				for(nIndex=0;nIndex<3;nIndex++)
				{
					theApp.m_PLCControl.OpenBox(iPLCNo);
					Sleep(750);		

					// GetTickCount()를 GetCurrentTime()으로 변경
					CTime tmNow = CTime::GetCurrentTime();
					CTimeSpan ts = tmNow - tmStart;
					if (ts.GetTotalSeconds() > nIndex*5+5)
					{
						nIndex=3;
						break;
					}

					if (!theApp.m_PLCControl.IsOpenBox(iPLCNo,&iRet) || !iRet)
						continue;
					else
						break;
				}
				theApp.m_PLCControl.ExitPLC();
			}
		}
		else
		{
			for(nIndex=0;nIndex<3;nIndex++)
			{
				theApp.m_PLCControl.OpenBox(iPLCNo);
				Sleep(750);		

				// GetTickCount()를 GetCurrentTime()으로 변경
				CTime tmNow = CTime::GetCurrentTime();
				CTimeSpan ts = tmNow - tmStart;
				if (ts.GetTotalSeconds() > nIndex*5+5)
				{
					nIndex=3;
					break;
				}

				if (!theApp.m_PLCControl.IsOpenBox(iPLCNo,&iRet) || !iRet)
					continue;
				else
					break;
			}
			theApp.m_PLCControl.ExitPLC();

		}

		if(nIndex>=3)
		{
			MessageBox("보관함 오픈 실패");
			return;
		}

		if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
			return;

		if(!CSMT_ManagerApp::fnDoBoxHistory(LOCKER_MANAGER_OPEN,iBoxNo,&ADODataBaseLocal) )
		{		
		}

		MessageBox("보관함 오픈 성공");
	}
	else
	{
		MessageBox("보관함 오픈 실패");		
	}

}

void CLongTermUseDlg::OnBnClickedButtonReturn()
{
	CString strText;

	if (this->m_nCurrentSelidx < 0)
	{
		MessageBox("먼저 사용자를 선택하십시오.");
		return;
	}

	int iBoxNo = atoi(this->m_strBoxNo.GetBuffer());
	this->m_strBoxNo.ReleaseBuffer();
	if(iBoxNo < LOCK_INFO.m_BoxStartNo[this->m_nCurrentSelidx])
	{
		MessageBox("먼저 사물함을 선택하십시오.");
	}
	else
	{
		strText.Format("%s에 위치한\n\n사물함 %d번이 반납처리 됩니다.\n\n다시한번 확인해 주십시요.\n\n계속진행 하시겠습니까?",theApp.m_strAddress[this->m_nCurrentSelidx],iBoxNo);

		if( MessageBox(strText,"반납 확인",MB_OKCANCEL) == IDCANCEL)
			return;

		if( fnDoUserBoxClear(LOCKER_MANAGER_RETURN, iBoxNo) )
		{
			MessageBox("반납처리 성공");
		}
		else
			MessageBox("반납처리 실패");	
	}
}

BOOL CLongTermUseDlg::fnDoUserBoxClear(int iTypeMsg, int nBoxNo)
{
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	int iCount = 0;
	CTime time = CTime::GetCurrentTime();

	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return FALSE;

	if( !CSMT_ManagerApp::fnDoBoxHistory(LOCKER_MANAGER_RETURN,nBoxNo,&ADODataBaseLocal) )
	{		
	}


	CADORecordset rs(&ADODataBaseLocal);

	sprintf(szQuery,"Update tblBoxMaster set useState=2, userCode='', userName='', userPhone='', "
		"dong='', addressNum='', transCode='', transPhone='', barCode='', "
		//"accCheck='', "
		"boxPassword='', paycode='', payAmount=0, useTimeType=0, startTime='%s',endTime='%s',productCode=0 "
		"where boxNo=%d and areaCode='%s' ",
		time.Format("%Y-%m-%d %H:%M:%S.000"),
		time.Format("%Y-%m-%d %H:%M:%S.000"),
		nBoxNo,LOCK_INFO.m_LockerId[this->m_nCurrentSelidx]);

	////////////////////////////
	// 고성준 : 로컬 디비 업데이트 추가
	if ( !CSMT_ManagerApp::fnExcuteClientDB(theApp.m_strDBIPClient[this->m_nCurrentSelidx], szQuery))
	{
		return FALSE;
	}

	if( !ADODataBaseLocal.Execute(szQuery) ) 
	{
		return FALSE;
	}

#ifdef _REMOTE_DB_USED_

	CADODatabase CenterDatabase;
	if( !CSMT_ManagerApp::fnOpenCenterDatabase(CenterDatabase) )
		return FALSE;

	if( !CenterDatabase.Execute(szQuery) ) 
	{
		CenterDatabase.Close();
		return FALSE;
	}
	CenterDatabase.Close();

#endif

	return TRUE;

}


void CLongTermUseDlg::OnShowWindow(BOOL bShow, UINT nStatus)
{
	CSkinDialog::OnShowWindow(bShow, nStatus);

	// TODO: 여기에 메시지 처리기 코드를 추가합니다.
	if (bShow)
	{
		if (this->m_nCurrentSelidx >= 0)
		{
			LOCK_INFO.m_Selidx = this->m_nCurrentSelidx;
		}

		if( !theApp.fnInitPLCControl())
		{
			MessageBox("PLC 연결 실패. 문을 열지 마시고 관리자에게 연락 바랍니다.");
		}
	}
}
