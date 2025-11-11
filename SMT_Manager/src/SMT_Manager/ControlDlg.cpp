// ControlDlg.cpp : 구현 파일입니다.
//
#pragma once
#include "stdafx.h"
#include "SMT_Manager.h"
#include "ControlDlg.h"
#include "Ado.h"
#include "LoginDlg.h"

//#define MAX_COMPANY_COUNT				12
//#define MAX_DELIVERY_CONTENT			18	


// CControlDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CControlDlg, CSkinDialog)
//CGlobal Global1;

CControlDlg::CControlDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CControlDlg::IDD, pParent)
	, m_nCurrentSelidx(-1)
	, m_strBoxNo(_T(""))
	, m_strReceivePhone(_T(""))
	//, m_strSearch(_T(""))
	, m_strAddress(_T(""))
	, m_strBarcode(_T(""))
{
	/////////////////////////////////////
	// 고성준 : 메모리 관련 버그 수정
	m_pButtonLocker = NULL;
	m_tmLastPaint = CTime::GetCurrentTime();
}

CControlDlg::~CControlDlg()
{
}

void CControlDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	
	DDX_Control(pDX, IDC_COMBO_LOCATION, m_cmbAddress);
	DDX_Text(pDX, IDC_EDIT_BOXNO, m_strBoxNo);
	// IDC_EDIT_RECEIVEPHONE - 리소스에서 제거되었으므로 주석 처리
	// DDX_Text(pDX, IDC_EDIT_RECEIVEPHONE, m_strReceivePhone);
	
	DDX_Control(pDX, IDC_COMBO_USE, m_cmbUsed);
	DDX_Control(pDX, IDC_BUTTON_ALLOPEN, m_AllBoxOpen);
	DDX_Control(pDX, IDC_BUTTON_OPEN, m_BoxOpen);
	DDX_Control(pDX, IDC_BUTTON_UPDATE, m_dbUpdate);
	DDX_Text(pDX, IDC_EDIT_BARCODE, m_strBarcode);
	DDX_Control(pDX, IDC_EDIT_PRODUCT_INFO, m_ProductInfoEdit);
}


BEGIN_MESSAGE_MAP(CControlDlg, CSkinDialog)
	// IDC_LIST1이 삭제되었으므로 주석 처리
	//ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST1, &CControlDlg::OnLvnItemchangedList1)

	ON_MESSAGE(UM_DLG_INIT_MSG,OnDlgInitMessage)

	ON_WM_DESTROY()
	ON_CBN_SELCHANGE(IDC_COMBO_LOCATION, &CControlDlg::OnCbnSelchangeComboLocation)
	ON_BN_CLICKED(IDC_BUTTON_TITLE, &CControlDlg::OnBnClickedButtonTitle)
	ON_WM_PAINT()
	ON_CONTROL_RANGE(BN_CLICKED, IDC_LOCKER_CTRL_BASE, IDC_LOCKER_CTRL_BASE+MAX_LOCKER_BOX_CNT, OnButtonsClicked )
	ON_BN_CLICKED(IDC_BUTTON_OPEN, &CControlDlg::OnBnClickedButtonOpen)
	ON_BN_CLICKED(IDC_BUTTON_UPDATE, &CControlDlg::OnBnClickedButtonUpdate)
	ON_WM_SHOWWINDOW()
	ON_BN_CLICKED(IDC_BUTTON_ALLOPEN, &CControlDlg::OnBnClickedButtonAllopen)
END_MESSAGE_MAP()


// CControlDlg 메시지 처리기입니다.

BOOL CControlDlg::OnInitDialog()
{
	CDialog::OnInitDialog();
    m_BoxBackBgStartx=43;         //박스를 자동으로 그리기위해 사용
	m_BoxBackBgStarty=130;        //다이얼로그가 전체화면에서의 Y좌표가 229에서 시작됨 

	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	m_AllBoxOpen.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_LONG), Global.GetRGB(IDX_RGB_MASK));
	m_AllBoxOpen.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_BoxOpen.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_BoxOpen.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
	//m_BoxReturn.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	//m_BoxReturn.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
	m_dbUpdate.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_dbUpdate.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
	//m_SearchBtn.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_LONG), Global.GetRGB(IDX_RGB_MASK));
	//m_SearchBtn.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);
	//m_camera.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	//m_camera.SetIcon(Global.GetIcon(IDX_ICON_CAMERA, ICON24), NULL, 5);
	//m_Sms.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	//m_Sms.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_totalStatic.SubclassDlgItem(IDC_STATIC_TOTAL,this);
	m_totalStatic.fnSetStaticPosition(432,140);
	m_totalStatic.fnSetDrawFont(15, FW_BOLD ,_T("굴림"));
	m_totalStatic.fnSetDrawTextColor(RGB(0,0,0));
	m_totalStatic.fnSetDrawTextAlign(DT_CENTER);
	m_totalStatic.fnSetDrawText(_T(""));
   
	CRect rc;
	GetParent()->GetWindowRect(rc);
	::SetWindowPos(this->m_hWnd,HWND_BOTTOM,10, 30, rc.right-rc.left-20  ,rc.bottom-rc.top-40, SWP_NOZORDER | SWP_SHOWWINDOW);

    fnListConInit();
	fnDropListSet();
	fnAutoCalcLockerButtonsPos_Size();
	fnControlFontSet();
	fnCreateLockerButtons();

	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO_LOCATION), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO_USE), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);

	CTime m_cur=CTime::GetCurrentTime();
    UpdateData(FALSE);

	PostMessage(UM_DLG_INIT_MSG);


/////////////////////////////////////////////////////////////////////////////////////////////////
	return TRUE;  // return TRUE unless you set the focus to a control
	// 예외: OCX 속성 페이지는 FALSE를 반환해야 합니다.
}

void CControlDlg::fnListConInit()
{

}
void CControlDlg::OnButtonsClicked(UINT CtrlID)
{
	if(CtrlID >= IDC_LOCKER_CTRL_BASE)
	{
		static int iPreBoxNo = -1;
		iPreBoxNo = LOCK_INFO.m_BoxNo;

		LOCK_INFO.m_BoxNo=CtrlID-IDC_LOCKER_CTRL_BASE;
		CADODatabase ADODataBaseLocal;
		char szQuery[1024] = {0,};
		if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
			return ;
		CADORecordset rs(&ADODataBaseLocal);

		sprintf(szQuery,
			"Select boxNo,useState,userPhone,transCode,transPhone,boxPassword,deliveryType,startTime,endTime,userCode,barCode,productCode From tblBoxMaster where areaCode='%s' and boxNo=%d ",
				LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx],LOCK_INFO.m_BoxNo);

		if( !rs.Open(szQuery) )
		{
			ADODataBaseLocal.Close();
			return ;
		}

		CString startTime;
		CString endTime;
		CString productCodeStr;
		
		while(!rs.IsEOF())
		{
			rs.GetFieldValue("boxNo",m_strBoxNo );
			rs.GetFieldValue("useState",m_nUseState );
			rs.GetFieldValue("userPhone",m_strReceivePhone );
			rs.GetFieldValue("startTime",startTime );
			rs.GetFieldValue("endTime",endTime );
			rs.GetFieldValue("barCode",m_strBarcode );
			rs.GetFieldValue("productCode",productCodeStr );
			
			product_Code = _ttoi(productCodeStr);
			rs.MoveNext();
		}

		rs.Close();
		ADODataBaseLocal.Close();
		
		// 상품 정보 로드
		if (product_Code > 0)
		{
			fnLoadProductInfo(product_Code);
		}
		else
		{
			fnClearProductInfo();
		}
		
		/*fnRefreshLockers();*/
		if (0 < iPreBoxNo)
		{
			if (m_bLockerStatus[iPreBoxNo - 1] == 1) {
				m_pButtonLocker[iPreBoxNo - 1].fnSetButtonStatus(BUTTON_DISABLE);
			}
			else {
				m_pButtonLocker[iPreBoxNo - 1].fnSetButtonStatus(BUTTON_NORMAL);
			}
		}
		m_pButtonLocker[LOCK_INFO.m_BoxNo - 1].fnSetButtonStatus(BUTTON_OWNUSE);
	    fnDbDateDisplay();
		return ;
	}


}
void CControlDlg::fnDbDateDisplay()
{
	int state=0;
	//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_SENDPHONE), TRUE);
	//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_USERID), TRUE);
	//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_START), TRUE);
	//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_END), TRUE);

	if(m_nUseState==2){
		m_cmbUsed.SetCurSel(1);
        //m_cmbUseType.SetCurSel(-1);
		m_strReceivePhone="";
		//m_strSendPhone="";
		//m_strPassWord="";
		//m_strUserCode="";
		m_strBarcode="";


	}
	else
	{
		m_cmbUsed.SetCurSel(0);
		//switch(m_nDeliveryType){
		//	case 1:
		//		// 사물함
		//	    m_cmbUseType.SetCurSel(0);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_SENDPHONE), FALSE);
		//	
		//		break;
		//	case 5:
		//		// 택배발송
		//	    m_cmbUseType.SetCurSel(2);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_START), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_END), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_USERID), FALSE);
		//		break;
		//	case 9:
		//		// 물품전달
		//	    m_cmbUseType.SetCurSel(1);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_START), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_END), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_USERID), FALSE);
		//		break;
		//	default:
		//		// 택배배달
		//	    m_cmbUseType.SetCurSel(3);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_START), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_DATETIMEPICKER_END), FALSE);
		//		//::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_EDIT_USERID), FALSE);
		//		break;
		//}
	}
    UpdateData(FALSE);

}
void CControlDlg::fnDropListSet()
{
    int i=0;
	//char *aa[]={"UserPhone","UserId","Password"};
	char *aa[]={"UserPhone","Password"};
	char *bb[]={"품절","판매중"};
	char *cc[]={"사물함","물품전달","택배발송","택배배달"};
	for(i=0;i<LOCK_INFO.m_Locker_Sum; i++)
	{
		m_cmbAddress.AddString(theApp.m_strAddress[i]);
	}
	m_cmbAddress.SetCurSel(0);
	LOCK_INFO.m_Selidx =0;
	this->m_nCurrentSelidx = LOCK_INFO.m_Selidx;
	if (theApp.m_MasterBoxidx > 0) {
		LOCK_INFO.m_kioskidx = theApp.m_MasterBoxidx;
	}
	else if (theApp.m_MasterBoxidx = -1) {
		LOCK_INFO.m_kioskidx = _ttoi(theApp.m_strMasterBoxindex[LOCK_INFO.m_Selidx]);
	}
	else {
		LOCK_INFO.m_kioskidx = ((LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx] / theApp.m_nBoxRow) / 2) + 1;
	}

	/*for(i=0; i< 2; i++)
	{
		m_cmbSearch.AddString(aa[i]);
	}
	m_cmbSearch.SetCurSel(0);*/

	for(i=0; i< 2; i++)
	{
		m_cmbUsed.AddString(bb[i]);
	}

/*	for(i=0; i< 0; i++)
	{
		m_cmbUseType.AddString(cc[i]);
	}

	m_cmbUseType.EnableWindow(FALSE*/
}

void CControlDlg::fnControlFontSet()
{
    fnFontSet(13,IDC_COMBO_LOCATION);
    fnFontSet(13,IDC_COMBO_USE);
    fnFontSet(15,IDC_EDIT_BOXNO);
    // IDC_EDIT_RECEIVEPHONE - 리소스에서 제거되었으므로 주석 처리
    // fnFontSet(15,IDC_EDIT_RECEIVEPHONE);
    // IDC_EDIT_SEARCH가 삭제되었으므로 주석 처리
    //fnFontSet(15,IDC_EDIT_SEARCH);
    // IDC_LIST1이 삭제되었으므로 주석 처리
    //fnFontSet(12,IDC_LIST1);
	fnFontSet(15,IDC_EDIT_BARCODE);
	fnFontSet(14,IDC_EDIT_PRODUCT_INFO);
}

void CControlDlg::fnFontSet(int size,int idc_id)
{
	    HFONT hFontEdit;
    	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
		::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}


void CControlDlg::OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult)// 리스트 컨트롤 선택
{
	// IDC_LIST1이 삭제되었으므로 전체 함수 주석 처리
	/*
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	int selidx=0;
	CString aa;
	switch (pNMHDR->code) {
		case LVN_ITEMCHANGED:
			if (pNMLV->uChanged == LVIF_STATE && pNMLV->uNewState == (LVIS_SELECTED | LVIS_FOCUSED)) {
				selidx=ListView_GetNextItem(::GetDlgItem(this->m_hWnd,IDC_LIST1),-1,LVNI_ALL | LVNI_SELECTED);
				aa.Format("%d",selidx);
				//::SetWindowText(::GetDlgItem(this->m_hWnd,IDC_EDIT_BOXNO),aa);
			}
		break;
		}
	*/

	*pResult = 0;
}

BOOL CControlDlg::fnCreateLockerButtons()
{

	m_pButtonLocker = new CBaseButton[MAX_LOCKER_BOX_CNT];
	if(m_pButtonLocker == NULL)
		return FALSE;

	int boxgab = 1;
	int boxmvx = 0;
	int boxmvy = 0;
	CRect rc;
	CString strText = _T("");
	BOOL masterBoxOnce=TRUE;
    int temp_xWidth=m_BoxAutoWidth+(m_BoxAutoWidth/2)+1;
	int temp_xHeight=m_BoxAutoHeight+(m_BoxAutoHeight*2/3)+1;
	int temp_xStart,temp_yStart;

	if(LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010012"){
		m_BoxStartPosx=m_BoxStartPosx-((m_BoxAutoWidth/6)*15-5);
	}
	if(LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010013"){
		m_BoxStartPosx=m_BoxStartPosx-((m_BoxAutoWidth/6)*10-5);
	}

	for(int i=0;i< LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx] ;i++)
	{
		int masterBoxIndex = _ttoi(theApp.m_strMasterBoxindex[LOCK_INFO.m_Selidx]);
		//if((((i / theApp.m_nBoxRow)+1) == LOCK_INFO.m_kioskidx) && (masterBoxOnce==TRUE)){  //KIOSK 디스플레이
		if((i == masterBoxIndex - 1) && (masterBoxOnce == TRUE)) {  //KIOSK 디스플레이

			boxmvx = m_BoxStartPosx+(m_BoxAutoWidth*(i/theApp.m_nBoxRow)+boxgab*(i/theApp.m_nBoxRow-1));
			boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*(i%theApp.m_nBoxRow)+boxgab*(i%theApp.m_nBoxRow-1));
			
			//rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth+(m_BoxAutoWidth/2),boxmvy+((m_BoxAutoHeight*theApp.m_nBoxRow)+(theApp.m_nBoxRow-1)));
			rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth,boxmvy+m_BoxAutoHeight);
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].Create(NULL,WS_CHILD|WS_VISIBLE,rc,this,IDC_LOCKER_CTRL_BASE);
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetNoDrawImageBK();
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].SetButtonStyle(BS_OWNERDRAW | m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].GetButtonStyle(),FALSE);
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetDrawFont(20, FW_BOLD ,"휴먼모음T");
			strText.Format(_T(""));
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetDrawText(strText);
			//m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetButtonSize(m_BoxAutoWidth+(m_BoxAutoWidth/2),((m_BoxAutoHeight * theApp.m_nBoxRow)+(theApp.m_nBoxRow-1)));
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetButtonSize(m_BoxAutoWidth, m_BoxAutoHeight);
			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetButtonImages(IMG_KIOSK,IMG_KIOSK,
													 IMG_KIOSK,IMG_KIOSK,FALSE);
            masterBoxOnce=FALSE;
            i--;

			boxmvx = m_BoxStartPosx + (m_BoxAutoWidth * (i / theApp.m_nBoxRow) + boxgab * (i / theApp.m_nBoxRow - 1));
			boxmvy = m_BoxStartPosy + (m_BoxAutoHeight * (i % theApp.m_nBoxRow) + boxgab * (i % theApp.m_nBoxRow - 1));

		}else{  //일반함 디스플레이
			if(masterBoxOnce==FALSE){
				if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010012") && (i >= 70)){
                    boxmvx=temp_xStart+m_BoxAutoWidth+(temp_xWidth*((i-70)/3))+1;
					boxmvy=temp_yStart+((temp_xHeight+1)*((i-70)%3));
				}else if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010013") && (i >= 60)){
                    boxmvx=temp_xStart+m_BoxAutoWidth+(temp_xWidth*((i-60)/3))+1;
					boxmvy=temp_yStart+((temp_xHeight+1)*((i-60)%3));
				}else{
					boxmvx = m_BoxStartPosx + (m_BoxAutoWidth * ((i + 1) / theApp.m_nBoxRow) + boxgab * ((i + 1) / theApp.m_nBoxRow - 1));
					boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*((i + 1) %theApp.m_nBoxRow)+boxgab*((i + 1) %theApp.m_nBoxRow-1));
                    temp_xStart=boxmvx;
					temp_yStart=m_BoxStartPosy-1;
				}
                
			}else{
				boxmvx = m_BoxStartPosx+(m_BoxAutoWidth*(i/theApp.m_nBoxRow)+boxgab*(i/theApp.m_nBoxRow-1));
				boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*(i%theApp.m_nBoxRow)+boxgab*(i%theApp.m_nBoxRow-1));
			}

			if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010012") && (i >= 70)){
				rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth+(m_BoxAutoWidth/2),boxmvy+temp_xHeight);
			}else if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010013") && (i >= 60)){
				rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth+(m_BoxAutoWidth/2),boxmvy+temp_xHeight);
			}else{
				rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth,boxmvy+m_BoxAutoHeight);
			}
			m_pButtonLocker[i].Create(NULL,WS_CHILD|WS_VISIBLE,rc,this,IDC_LOCKER_CTRL_BASE+i+LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx]);
			m_pButtonLocker[i].fnSetNoDrawImageBK();
			m_pButtonLocker[i].SetButtonStyle(BS_OWNERDRAW | m_pButtonLocker[i].GetButtonStyle(),FALSE);
			m_pButtonLocker[i].fnSetDrawFont(20, FW_BOLD ,"휴먼모음T");
			strText.Format(_T("%d"),i+LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx]);
			m_pButtonLocker[i].fnSetDrawText(strText);

			if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010012") && (i >= 70)){
				m_pButtonLocker[i].fnSetButtonSize((m_BoxAutoWidth/2)+m_BoxAutoWidth,temp_xHeight);
			}else if((LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010013") && (i >= 60)){
				m_pButtonLocker[i].fnSetButtonSize((m_BoxAutoWidth/2)+m_BoxAutoWidth,temp_xHeight);
			}else{
				m_pButtonLocker[i].fnSetButtonSize(m_BoxAutoWidth,m_BoxAutoHeight);
			}

			m_pButtonLocker[i].fnSetButtonImages(IMG_LOCKER_SEL_S_RBOX,IMG_LOCKER_SEL_S_BBOX, IMG_LOCKER_SEL_S_GBOX,IMG_LOCKER_SEL_S_RBOX,FALSE);
		}

	}

	return TRUE;
}

//BOOL CControlDlg::fnCreateLockerButtons()
//{
//	m_pButtonLocker = new CBaseButton[MAX_LOCKER_BOX_CNT];
//	if(m_pButtonLocker == NULL)
//		return FALSE;
//
//	int boxgab = 1;
//	int boxmvx = 0;
//	int boxmvy = 0;
//	CRect rc;
//	CString strText = _T("");
//	BOOL masterBoxOnce=TRUE;
//
//
//	for(int i=0;i< LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx] ;i++)
//	{
//
//		if((((i / theApp.m_nBoxRow)+1) == LOCK_INFO.m_kioskidx) && (masterBoxOnce==TRUE)){
//
//			boxmvx = m_BoxStartPosx+(m_BoxAutoWidth*(i/theApp.m_nBoxRow)+boxgab*(i/theApp.m_nBoxRow-1));
//			boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*(i%theApp.m_nBoxRow)+boxgab*(i%theApp.m_nBoxRow-1));
//
//			rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth+(m_BoxAutoWidth/2),boxmvy+((m_BoxAutoHeight*theApp.m_nBoxRow)+(theApp.m_nBoxRow-1)));
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].Create(NULL,WS_CHILD|WS_VISIBLE,rc,this,IDC_LOCKER_CTRL_BASE+MAX_LOCKER_BOX_CNT);
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetNoDrawImageBK();
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].SetButtonStyle(BS_OWNERDRAW | m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].GetButtonStyle(),FALSE);
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetDrawFont(20, FW_BOLD ,"휴먼모음T");
//			strText.Format(_T(""));
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetDrawText(strText);
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetButtonSize(m_BoxAutoWidth+(m_BoxAutoWidth/2),((m_BoxAutoHeight * theApp.m_nBoxRow)+(theApp.m_nBoxRow-1)));
//			m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].fnSetButtonImages(IMG_KIOSK,IMG_KIOSK,
//													 IMG_KIOSK,IMG_KIOSK,FALSE);
//            masterBoxOnce=FALSE;
//            i--;
//		}else{
//			if(masterBoxOnce==FALSE){
//				boxmvx = m_BoxStartPosx+(m_BoxAutoWidth/2)+(m_BoxAutoWidth*((i+theApp.m_nBoxRow)/theApp.m_nBoxRow)+boxgab*(i/theApp.m_nBoxRow-1))+1;
//				boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*(i%theApp.m_nBoxRow)+boxgab*(i%theApp.m_nBoxRow-1));
//			}else{
//				boxmvx = m_BoxStartPosx+(m_BoxAutoWidth*(i/theApp.m_nBoxRow)+boxgab*(i/theApp.m_nBoxRow-1));
//				boxmvy = m_BoxStartPosy+(m_BoxAutoHeight*(i%theApp.m_nBoxRow)+boxgab*(i%theApp.m_nBoxRow-1));
//			}
//			
//			rc.SetRect(boxmvx,boxmvy,boxmvx+m_BoxAutoWidth,boxmvy+m_BoxAutoHeight);
//			m_pButtonLocker[i].Create(NULL,WS_CHILD|WS_VISIBLE,rc,this,IDC_LOCKER_CTRL_BASE+i);
//			m_pButtonLocker[i].fnSetNoDrawImageBK();
//			m_pButtonLocker[i].SetButtonStyle(BS_OWNERDRAW | m_pButtonLocker[i].GetButtonStyle(),FALSE);
//			m_pButtonLocker[i].fnSetDrawFont(20, FW_BOLD ,"휴먼모음T");
//			strText.Format(_T("%d"),i+1);
//			m_pButtonLocker[i].fnSetDrawText(strText);
//			m_pButtonLocker[i].fnSetButtonSize(m_BoxAutoWidth,m_BoxAutoHeight);
//			m_pButtonLocker[i].fnSetButtonImages(IMG_LOCKER_SEL_S_RBOX,IMG_LOCKER_SEL_S_BBOX,
//													 IMG_LOCKER_SEL_S_GBOX,IMG_LOCKER_SEL_S_RBOX,FALSE);
//		}
//
//	}
//
//	return TRUE;
//}

void CControlDlg::fnAutoCalcLockerButtonsPos_Size()
{
	int m_Col;

	m_BackGroundWidth = 966;    //box가 들어가는 백그라운드 이미지 사이즈 Width
	m_BackGroundHeight = 419;   //box가 들어가는 백그라운드 이미지 사이즈 Height
	//m_BoxBackBgStartx         //m_BoxBackBgStartx,m_BoxBackBgStarty 는 버튼 생성시 값을 입력시킴
	//m_BoxBackBgStarty
	if(LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]=="05410010012"){
       theApp.m_BoxRate=-10;
	}
	m_BoxAutoHeight=m_BackGroundHeight/(theApp.m_nBoxRow+1)-7+theApp.m_BoxRate;    // 마지막 숫자를 변경해서 최종적으로 박스를 그린다.

	m_BoxAutoWidth=(m_BoxAutoHeight*3)/4;   //박스 가로세로 비율 3/4
	m_Col=((LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]%theApp.m_nBoxRow)==0) ? (LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]/theApp.m_nBoxRow) : (LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]/theApp.m_nBoxRow)+1;

	m_BoxStartPosx=((m_BackGroundWidth+m_BoxBackBgStartx)-(m_BackGroundWidth/2)) - ((m_Col*m_BoxAutoWidth)+(m_BoxAutoWidth+(m_BoxAutoWidth/2)))/2-7;   //(m_BoxAutoWidth+(m_BoxAutoWidth/2)) 라커넓이 적용,
	m_BoxStartPosy=((m_BackGroundHeight+m_BoxBackBgStarty)-(m_BackGroundHeight/2)) - (theApp.m_nBoxRow*m_BoxAutoHeight)/2;                                       //7은 포인트 맞추기 위해 임의로 적용함


}

BOOL CControlDlg::fnRefreshLockers()
{
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	int  iBoxNo = 0;

	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return FALSE;

	ZeroMemory(m_bLockerStatus,sizeof(m_bLockerStatus));
	CADORecordset rs(&ADODataBaseLocal);

    sprintf(szQuery,
		"Select boxNo From tblBoxMaster where useState = 1 and areaCode='%s' order by boxNo asc ",
			LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx] );

	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return FALSE;
	} 

	int nIndex = 0;
	while(!rs.IsEOF())
	{
		rs.GetFieldValue("boxNo",iBoxNo );
		TRACE("boxNo=%d\n",iBoxNo);

		if(iBoxNo >= LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx] &&
		   iBoxNo <= LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx] + LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx] - 1)
			m_bLockerStatus[iBoxNo-LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx]] = 1;

		nIndex++;
		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
    LOCK_INFO.m_usedSum=0;
	for(int i=0;i<LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx];i++)
	{
		if(m_bLockerStatus[i] == 1){
			m_pButtonLocker[i].fnSetButtonStatus(BUTTON_DISABLE);	
            LOCK_INFO.m_usedSum++;
		}else{
			m_pButtonLocker[i].fnSetButtonStatus(BUTTON_NORMAL);
		}
	}
	fnBoxTotalStatus();

	return TRUE;
}

void CControlDlg::fnBoxTotalStatus()
{
	CString n_usedSum;
	CRect rc; 
	n_usedSum.Format("[ %d/%d함 사용중 ]",LOCK_INFO.m_usedSum,LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]);
	m_totalStatic.fnSetDrawText(n_usedSum,TRUE);
	m_totalStatic.GetWindowRect(rc);
	ScreenToClient(rc);
	InvalidateRect(rc,FALSE);
}

void CControlDlg::fnDeleteLockerButtons()
{
	/////////////////////////////////////
	// 고성준 : 메모리 관련 버그 수정
	if (m_pButtonLocker == NULL)
		return;

	for(int i=0;i<LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx];i++)
	{
		if( m_pButtonLocker[i].GetSafeHwnd() )
			m_pButtonLocker[i].DestroyWindow();
	}
	if( m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].GetSafeHwnd() )
		m_pButtonLocker[MAX_LOCKER_BOX_CNT-1].DestroyWindow();

	if(m_pButtonLocker)
		delete []m_pButtonLocker;

	m_pButtonLocker = NULL;
}


LRESULT CControlDlg::OnDlgInitMessage(WPARAM wParam,LPARAM lParam)
{
	fnRefreshLockers();

	if (this->m_nCurrentSelidx >= 0)
	{
		LOCK_INFO.m_Selidx = this->m_nCurrentSelidx;

		if (this->m_strAddress.IsEmpty())
		{
			CString strText;
			m_cmbAddress.GetLBText(LOCK_INFO.m_Selidx, strText);
			if (this->m_strAddress != strText)
				this->m_strAddress = strText;
		}
	}

	if( !theApp.fnInitPLCControl())
	{
		MessageBox("PLC 연결 실패. 문을 열지 마시고 관리자에게 연락 바랍니다.");
	}

	//fnRefreshPLCStatus();

	return 0;
}

//BOOL CPLCControlDlg::fnRefreshPLCStatus()
//{
//	int  iStatusArray[512] = {0};
//	BOOL bRet = FALSE;
//	int  iMaxLoop   = 0;
//	int  iMaxPLC    = 0;
//	int  iPLCNo		= 0;
//
//	if( !m_PLCControl.GetPLCInputStatusAll(iStatusArray,512) )
//		return FALSE;
//
//	iMaxPLC  = m_PLCMatrixDatas.GetSlotCount();
//	iMaxLoop = m_BoxMapTable.GetCount();
//	for(int i=1;i<=iMaxLoop ;i++)
//	{
//		if( m_BoxMapTable.Lookup(i,iPLCNo) )
//		{
//			if(iStatusArray[iPLCNo-1])
//				m_PLCListCtrl.SetItemText(i-1,1,"열림");
//			else
//				m_PLCListCtrl.SetItemText(i-1,1,"닫힘");
//		}
//	}
//
//	return TRUE;
//}


void CControlDlg::OnDestroy()
{
	CSkinDialog::OnDestroy();

	fnDeleteLockerButtons();
}


void CControlDlg::OnCbnSelchangeComboLocation()// location combo click
{

	this->fnDeleteLockerButtons();

	CString strText;

  	LOCK_INFO.m_Selidx = m_cmbAddress.GetCurSel();
	this->m_nCurrentSelidx = LOCK_INFO.m_Selidx;
	
	m_cmbAddress.GetLBText(LOCK_INFO.m_Selidx, strText);
	if (this->m_strAddress != strText)
		this->m_strAddress = strText;
	
	LOCK_INFO.m_kioskidx =((LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]/theApp.m_nBoxRow)/2)+1;
    fnAllControlInit();
	fnAutoCalcLockerButtonsPos_Size();
	fnCreateLockerButtons();
	PostMessage(UM_DLG_INIT_MSG);
	m_cmbUsed.SetCurSel(-1);
    //m_cmbUseType.SetCurSel(-1);

	if( !theApp.fnInitPLCControl())
	{
		MessageBox("PLC 파일 로딩 실패 문을 열지 마시고 관리자에게 문의 하십시요");
	}


}
void CControlDlg::fnAllControlInit()
{
	m_strBoxNo="";
	m_strReceivePhone="";
	//m_strSendPhone="";
	//m_strPassWord="";
	//m_strUserCode="";
	m_strBarcode="";
	LOCK_INFO.m_BoxNo=0;
	product_Code=0;
	fnClearProductInfo();
	UpdateData(FALSE);
}



void CControlDlg::OnBnClickedButtonTitle()
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
}

void CControlDlg::OnPaint()
{
	CTime tmNow = CTime::GetCurrentTime(); // ? 수정
	CTimeSpan span(0, 0, 0, 1);
	if (m_tmLastPaint.GetTime() > 0 && (tmNow - m_tmLastPaint).GetTotalSeconds() < 1)
		return;
	
	m_tmLastPaint = CTime::GetCurrentTime(); // ? 수정

	CPaintDC dc(this); // device context for painting

	CRect rc;
	GetClientRect(&rc);
	dc.FillSolidRect(rc, Global.GetRGB(IDX_RGB_BACKGROUND));

	// ? 이미지 로드 예외 처리 추가
	try
	{
		Graphics graphics(dc.m_hDC); // bk graphics
		
		CString m_bg = "..\\skin\\locker_bg.png";
		BSTR str = m_bg.AllocSysString();

		Bitmap myBMP(str);
		::SysFreeString(str); // ? BSTR 메모리 해제

		// ? 이미지 로드 성공 확인
		if (myBMP.GetLastStatus() == Gdiplus::Ok)
		{
			int iwd = myBMP.GetWidth();
			int iht = myBMP.GetHeight();
			Rect rect(0, 0, iwd, iht);

			graphics.DrawImage(&myBMP, rect);
		}
		else
		{
			// ? 이미지 로드 실패 시 배경색만 표시
			TRACE("Failed to load locker_bg.png, Status: %d\n", myBMP.GetLastStatus());
		}
	}
	catch (...)
	{
		// ? 예외 발생 시 무시하고 기본 배경만 표시
		TRACE("Exception in OnPaint while loading image\n");
	}
}

void CControlDlg::OnCbnSelchangeComboSearch()
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
}

void CControlDlg::OnBnClickedButtonSearch()  //db search
{
	// IDC_LIST1과 m_listctrlSearch가 삭제되었으므로 전체 함수 주석 처리
	/*
	int i=0,j=0;
	CString strBoxUseType;
	CString strCompany;
	
	CString location="";
	CString boxNo="";
	CString useState="";
	CString receivePhone="";
	CString deliveryType="";
	CString sendPhone="";
	CString useType="";
	CString pass="";
	CString startTime="";
	CString endTime="";
	CString UserId="";
	CString barcode="";

    UpdateData(TRUE);
	m_listctrlSearch.DeleteAllItems();
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return ;
	CADORecordset rs(&ADODataBaseLocal);

	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("areaCode",location );
		rs.GetFieldValue("boxNo",boxNo );
		rs.GetFieldValue("userPhone",receivePhone );
		rs.GetFieldValue("transPhone",sendPhone );
		rs.GetFieldValue("deliveryType",deliveryType );
		rs.GetFieldValue("boxPassword",pass );
		rs.GetFieldValue("startTime",startTime );
		rs.GetFieldValue("endTime",endTime );
		rs.GetFieldValue("userCode",UserId );
		rs.GetFieldValue("barcode",barcode );

		j=StrToInt(deliveryType);
		switch(j)
		{
			case 1:
		        strBoxUseType="사물함";
				break;
			case 10: case 11: case 12: case 13: case 14:case 15: case 16: case 17: case 18: case 19: case 20: case 21:
                strBoxUseType="택배배달";
                break;
			case 5:
		        strBoxUseType="택배발송";
				break;
			case 9:
		        strBoxUseType="물품전달";
				break;
			default:
		        strBoxUseType="없음";
				break;
		}

		j=StrToInt(deliveryType);
		switch(j){
			case 10:
		        strCompany="우체국";
				break;
			case 11:
		        strCompany="한진택배";
				break;
			case 12:
		        strCompany="대한통운";
				break;
			case 13:
		        strCompany="현대택배";
				break;
			case 14:
		        strCompany="CJ택배";
				break;
			case 15:
		        strCompany="로젠택배";
				break;
			case 16:
		        strCompany="옐로우캡";
				break;
			case 17:
		        strCompany="훼미리";
				break;
			case 18:
		        strCompany="KGB";
				break;
			case 19:
		        strCompany="동부택배";
				break;
			case 20:
		        strCompany="하나로";
				break;
			case 21:
		        strCompany="기타";
				break;
			default:
		        strCompany="해당없음";
				break;
		}

		for(i=0;i< LOCK_INFO.m_Locker_Sum;i++)
		{
			if(LOCK_INFO.m_LockerId[i] == location)
			{

				break;
			}
		}

		if (i< LOCK_INFO.m_Locker_Sum)
			(void)m_listctrlSearch.AddItem( theApp.m_strAddress[i],boxNo,strBoxUseType,strCompany,UserId,barcode,receivePhone,sendPhone,pass,startTime,endTime);

		rs.MoveNext();
	}

	rs.Close();
	ADODataBaseLocal.Close();
	*/
	return ;
}

void CControlDlg::OnBnClickedButtonOpen() //box open
{
	CString strText;
	CADODatabase ADODataBaseLocal;

	strText.Format("%s에 위치한\n\n사물함 %d번이 열립니다.\n\n다시한번 확인해 주십시요.\n\n계속진행 하시겠습니까?",theApp.m_strAddress[LOCK_INFO.m_Selidx],LOCK_INFO.m_BoxNo);
	if( MessageBox(strText,"열기 확인",MB_OKCANCEL) == IDCANCEL)
	{
		return;
	}

	if(LOCK_INFO.m_Selidx < 0 || LOCK_INFO.m_BoxNo < LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx])
	{
		MessageBox("먼저 사물함을 선택하십시오.");
		return;
	}

	int iPLCNo = 0;
	int iBoxNo = LOCK_INFO.m_BoxNo;
	int iRet   = 0;
	int nIndex;
	CTime tmStart = CTime::GetCurrentTime(); // ? 수정

	if( theApp.m_BoxMapTable.Lookup(iBoxNo,iPLCNo) )
	{
		if(theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] != 0)
		{
			if (theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] <= iBoxNo) // PLC 2
			{
				if (theApp.m_strPLCIPAddr_2[LOCK_INFO.m_Selidx].GetLength() <= 0)
				{
					strText.Format("[%s]의 경우     \r\n\r\n%d번 사물함까지만을     \r\n\r\n원격으로 열 수 있습니다.     ", 
									theApp.m_strAddress[LOCK_INFO.m_Selidx], theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] - 1);
					MessageBox(strText);
					return;
				}
				else
				{
					for(nIndex=0;nIndex<5;nIndex++)
					{
						theApp.m_PLCControl_2.OpenBox(iPLCNo);
						Sleep(750);		

						CTime tmNow = CTime::GetCurrentTime(); // ? 수정
						CTimeSpan ts = tmNow - tmStart;
						if (ts.GetTotalSeconds() > nIndex*5+5)
						{
							nIndex=5;
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
				for(nIndex=0;nIndex<5;nIndex++)
				{
					theApp.m_PLCControl.OpenBox(iPLCNo);
					Sleep(750);		

					CTime tmNow = CTime::GetCurrentTime(); // ? 수정
					CTimeSpan ts = tmNow - tmStart;
					if (ts.GetTotalSeconds() > nIndex*5+5)
						{
							nIndex=5;
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
			for(nIndex=0;nIndex<5;nIndex++)
			{
				theApp.m_PLCControl.OpenBox(iPLCNo);
				Sleep(750);		

				CTime tmNow = CTime::GetCurrentTime(); // ? 수정
				CTimeSpan ts = tmNow - tmStart;
				if (ts.GetTotalSeconds() > nIndex*5+5)
				{
					nIndex=5;
					break;
				}

				if (!theApp.m_PLCControl.IsOpenBox(iPLCNo,&iRet) || !iRet)
					continue;
				else
					break;
			}
			theApp.m_PLCControl.ExitPLC();

		}

		if(nIndex>=5)
		{
			MessageBox("사물함 열기 실패");
			return;
		}

		if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
			return;

		if(!CSMT_ManagerApp::fnDoBoxHistory(LOCKER_MANAGER_OPEN,iBoxNo,&ADODataBaseLocal) )
		{		
		}

		MessageBox("사물함 열기 성공");
	}
	else
	{
		MessageBox("사물함 열기 실패");		
	}

}

void CControlDlg::OnBnClickedButtonReturn()  //box return
{
	CString strText;
	strText.Format("%s에 위치한\n\n사물함 %d번이 반납처리 됩니다.\n\n다시한번 확인해 주십시요.\n\n계속진행 하시겠습니까?",theApp.m_strAddress[LOCK_INFO.m_Selidx],LOCK_INFO.m_BoxNo);

	if( MessageBox(strText,"반납 확인",MB_OKCANCEL) == IDCANCEL)
		return;

	if( fnDoUserBoxClear(LOCKER_MANAGER_RETURN) )
	{
		MessageBox("반납처리 성공");
		int nBoxNo = LOCK_INFO.m_BoxNo;
		OnCbnSelchangeComboLocation();
		this->OnButtonsClicked(nBoxNo + IDC_LOCKER_CTRL_BASE);
	}
	else
		MessageBox("반납처리 실패");	
}

BOOL CControlDlg::fnDoUserBoxClear(int iTypeMsg)
{
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	int iCount = 0;
	CTime time = CTime::GetCurrentTime();

	if(LOCK_INFO.m_BoxNo < LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx])
	{
		MessageBox("먼저 사물함을 선택하십시오.");
		return FALSE;
	}

	int iBoxNo = LOCK_INFO.m_BoxNo;

	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return FALSE;

	if( !CSMT_ManagerApp::fnDoBoxHistory(LOCKER_MANAGER_RETURN,iBoxNo,&ADODataBaseLocal) )
	{		
	}


	CADORecordset rs(&ADODataBaseLocal);

	sprintf(szQuery,"Update tblBoxMaster set useState=2, userCode='', userName='', userPhone='', "
					"dong='', addressNum='', transCode='', transPhone='', barCode='',productCode=0 "
					//"accCheck='', "
			        "boxPassword='', paycode='', payAmount=0, useTimeType=0, startTime='%s',endTime='%s' "
					"where boxNo=%d and areaCode='%s' ",
					time.Format("%Y-%m-%d %H:%M:%S.000"),
					time.Format("%Y-%m-%d %H:%M:%S.000"),
					iBoxNo,LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]);

	////////////////////////////
	// 고성준 : 로컬 디비 업데이트 추가
	if ( !CSMT_ManagerApp::fnExcuteClientDB(theApp.m_strDBIPClient[LOCK_INFO.m_Selidx], szQuery))
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

void CControlDlg::fnDoF9Key()
{
#ifndef _DEBUG
	if (this->m_AllBoxOpen.IsWindowVisible())
	{
		this->m_AllBoxOpen.ShowWindow(SW_HIDE);
	}
	else
	{
		CLoginDlg dlg;

		if (dlg.DoModal() == IDOK)
		{
			this->m_AllBoxOpen.ShowWindow(SW_SHOW);
		}
	}
#endif
}

void CControlDlg::OnBnClickedButtonUpdate() //db update
{

	int nState;
	int nType = 0;  // 초기화 추가
	
	CString useState;
	CString useType;

	GetDlgItemText(IDC_COMBO_USE,useState);
	if(useState=="미사용"){
       nState=2;
	}else{
       nState=1;
	}

    UpdateData(TRUE);

	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	CTime time = CTime::GetCurrentTime();
    CString strText;
	strText.Format("%s에 위치한\n\n사물함 %d번이 DB 업데이트 됩니다.\n\n다시한번 확인해 주십시요.\n\n계속진행 하시겠습니까?",theApp.m_strAddress[LOCK_INFO.m_Selidx],LOCK_INFO.m_BoxNo);

	if( MessageBox(strText,"DB 업데이트 확인",MB_OKCANCEL) == IDCANCEL)
		return;

	if(LOCK_INFO.m_BoxNo < LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx])
	{
		MessageBox("먼저 사물함을 선택하십시오.");
		return ;
	}
	int iBoxNo = LOCK_INFO.m_BoxNo;

	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
	{
		MessageBox("데이터베이스 연결 실패 하였습니다.","디비 연결 실패");
		return;
	}


	// 수정된 쿼리 (존재하지 않는 필드 제거)
	sprintf(szQuery,"Update tblBoxMaster set useState=%d, userPhone='%s', barCode='%s' "
					"where boxNo=%d and areaCode='%s' ",
					nState, m_strReceivePhone, m_strBarcode,
					iBoxNo, LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]);


	////////////////////////////
	// 고성준 : 로컬 디비 업데이트 추가
	if ( !CSMT_ManagerApp::fnExcuteClientDB(theApp.m_strDBIPClient[LOCK_INFO.m_Selidx], szQuery))
	{
		MessageBox("로컬 DB 업데이트 실패.");
		return ;
	}

	if( !ADODataBaseLocal.Execute(szQuery) ) 
	{
		MessageBox("서버 DB 업데이트 실패.");
		return ;
	}

#ifdef _REMOTE_DB_USED_
	CADODatabase CenterDatabase;
	if( !CSMT_ManagerApp::fnOpenCenterDatabase(CenterDatabase) ){
		MessageBox("중앙(포스텍)데이터베이스 연결 실패 하였습니다.","디비 연결 실패");
		return;
	}

	if( !CenterDatabase.Execute(szQuery) ) 
	{
		CenterDatabase.Close();
		return ;
	}
	CenterDatabase.Close();
#endif

	if( !CSMT_ManagerApp::fnDoBoxHistory(LOCKER_MANAGER_DBUPDATE,iBoxNo,&ADODataBaseLocal) )
	{		
	}

	int nBoxNo = LOCK_INFO.m_BoxNo;
	OnCbnSelchangeComboLocation();
	this->OnButtonsClicked(nBoxNo + IDC_LOCKER_CTRL_BASE);

	MessageBox("DB 업데이트 성공");
}


void CControlDlg::OnBnClickedButtonCamera()
{
	CString strUrl="www.showdvr.com/webguard.htm";
	ShellExecute(NULL, NULL, strUrl, NULL, NULL, SW_SHOWNORMAL);
}

void CControlDlg::OnBnClickedButtonSms()
{
	CString strMSG	 = _T("");
	CString strText	 = _T("");
    UpdateData(TRUE);

	strText.Format("항상 정보수정 후 SMS보내실 경우 \n\n디비"
			"업데이트 후 SMS를 보내시기 바랍니다.\n\n"
             "핸드폰번호:%s 사용자에게 SMS를 전송 하시겠습니까?", m_strReceivePhone);
	if(this->MessageBox(strText,"SMS 전송확인",MB_OKCANCEL)==IDOK)
	{
		// SMS 전송 로직
	}
}

void CControlDlg::OnShowWindow(BOOL bShow, UINT nStatus)
{
	CSkinDialog::OnShowWindow(bShow, nStatus);

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

void CControlDlg::OnBnClickedButtonAllopen()
{
	CString strText;
	int nFalse = 0;

	if (LOCK_INFO.m_Selidx < 0 || LOCK_INFO.m_Selidx >= LOCK_INFO.m_Locker_Sum)
	{
		MessageBox("문열기 전체 테스트를 할 라커를 선택해주세요.");
		return;
	}

	strText.Format("%s 라커의 모든 문을 차례로 열기 테스트 하시겠습니까?", theApp.m_strAddress[LOCK_INFO.m_Selidx]);
	if (MessageBox(strText,"테스트",MB_OKCANCEL) == IDOK)
	{
		try
		{
			strText = "열기 실패 리스트: ";
			int nLength = strText.GetLength();

			for (int nIndex = 0; nIndex < LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]; nIndex++)
			{
				int iBoxNo = nIndex + LOCK_INFO.m_BoxStartNo[LOCK_INFO.m_Selidx];
				int iPLCNo = 0;
				int iRet   = 0;
				CTime tmStart = CTime::GetCurrentTime();

				if( theApp.m_BoxMapTable.Lookup(iBoxNo,iPLCNo) )
				{
					if(theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] != 0){
						TRACE("%d dd",theApp.m_PLC_2_START[LOCK_INFO.m_Selidx]);

						if (theApp.m_PLC_2_START[LOCK_INFO.m_Selidx] <= iBoxNo)
						{
							if (theApp.m_strPLCIPAddr_2[LOCK_INFO.m_Selidx].GetLength() <= 0)
							{
								if (nLength > 30)
								{
									strText += "\r\n            ";
									nLength = 0;
								}
								strText += ::IntToStr(iBoxNo);
								strText += " ";
								nFalse++;
							}
							else
							{
								theApp.m_PLCControl_2.OpenBox(iPLCNo);
								Sleep(750);		

								CTime tmNow = CTime::GetCurrentTime();
								CTimeSpan ts = tmNow - tmStart;
								if (ts.GetTotalSeconds() > nIndex*5+5)
								{
									strText = "네트워크 문제로 동작을 중지합니다.";
									break;
								}

								if (!theApp.m_PLCControl_2.IsOpenBox(iPLCNo,&iRet) || !iRet)
								{
									if (nLength > 30)
									{
										strText += "\r\n            ";
										nLength = 0;
									}
									strText += ::IntToStr(iBoxNo);
									strText += " ";
									nFalse++;
								}
							}
						}
						else
						{
							theApp.m_PLCControl.OpenBox(iPLCNo);
							Sleep(750);		

							CTime tmNow = CTime::GetCurrentTime();
							CTimeSpan ts = tmNow - tmStart;
							if (ts.GetTotalSeconds() > nIndex*5+5)
							{
								strText = "네트워크 문제로 동작을 중지합니다.";
								break;
							}

							if (!theApp.m_PLCControl.IsOpenBox(iPLCNo,&iRet) || !iRet)
							{
								if (nLength > 30)
								{
									strText += "\r\n            ";
									nLength = 0;
								}
							 strText += ::IntToStr(iBoxNo);
							 strText += " ";
							 nFalse++;
							}
						}
					}
					else
					{
						theApp.m_PLCControl.OpenBox(iPLCNo);
						Sleep(750);		

						CTime tmNow = CTime::GetCurrentTime();
						CTimeSpan ts = tmNow - tmStart;
						if (ts.GetTotalSeconds() > nIndex*5+5)
						{
							strText = "네트워크 문제로 동작을 중지합니다.";
							break;
						}

						if (!theApp.m_PLCControl.IsOpenBox(iPLCNo,&iRet) || !iRet)
						{
							if (nLength > 30)
							{
								strText += "\r\n          ";
								nLength = 0;
							}
						 strText += ::IntToStr(iBoxNo);
						 strText += " ";
						 nFalse++;
						}
					}
				}
			}
		}
		catch(...)
		{
		}

		theApp.m_PLCControl.ExitPLC();
		theApp.m_PLCControl_2.ExitPLC();

		if (nFalse > 0)
		{
			MessageBox(strText);
		}
	}
}

BOOL CControlDlg::PreTranslateMessage(MSG* pMsg)
{
	BOOL retval = CSkinDialog::PreTranslateMessage(pMsg);

	if (pMsg->message == WM_KEYDOWN)
	{
		if (pMsg->wParam = 120)
		{
		}
	}

	return retval;
}

// 상품 정보 로드 함수
void CControlDlg::fnLoadProductInfo(int productCode)
{
	CADODatabase ADODataBaseLocal;
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal))
	{
		fnClearProductInfo();
		return;
	}

	CADORecordset rs(&ADODataBaseLocal);
	char szQuery[512] = {0,};
	
	sprintf(szQuery,
		"SELECT productName, productPrice, productType FROM tblProduct WHERE productCode=%d",
		productCode);

	if (!rs.Open(szQuery))
	{
		ADODataBaseLocal.Close();
		fnClearProductInfo();
		return;
	}

	CString productName;
	CString productPrice;
	CString productType;
	CString displayText;

	if (!rs.IsEOF())
	{
		rs.GetFieldValue("productName", productName);
		rs.GetFieldValue("productPrice", productPrice);
		rs.GetFieldValue("productType", productType);

		int nType = _ttoi(productType);
		CString strType;
		
		switch (nType)
		{
		case 1:
			strType = "요소수";
			break;
		case 2:
			strType = "차량보조제";
			break;
		default:
			strType = "기타";
			break;
		}

		// 가격 포맷팅 (천단위 콤마)
		int price = _ttoi(productPrice);
		
		// 천 단위 콤마 추가
		CString priceStr;
		priceStr.Format("%d", price);
		int len = priceStr.GetLength();
		CString result;
		int count = 0;
		
		for (int i = len - 1; i >= 0; i--)
		{
			if (count > 0 && count % 3 == 0)
			{
				result = priceStr[i] + CString(",") + result;
			}
			else
			{
				result = priceStr[i] + result;
			}
			count++;
		}
		result += "원";

		displayText.Format("[상품] %s (%s) - %s", productName, strType, result);
	}
	else
	{
		displayText = "[상품 정보 없음]";
	}

	rs.Close();
	ADODataBaseLocal.Close();

	m_ProductInfoEdit.SetWindowText(displayText);
}

// 상품 정보 초기화 함수
void CControlDlg::fnClearProductInfo()
{
	m_ProductInfoEdit.SetWindowText("");
}
