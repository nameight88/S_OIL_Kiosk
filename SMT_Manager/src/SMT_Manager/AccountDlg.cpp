// AccountDlg.cpp : 구현 파일입니다.
//
#pragma once
#include "stdafx.h"
#include "SMT_Manager.h"
#include "AccountDlg.h"
#include "SkinDialog.h"

// CAccountDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CAccountDlg, CDialog)
//CGlobal Global1;
CAccountDlg::CAccountDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CAccountDlg::IDD, pParent)
	, m_DtpStartCTime(0)
	, m_DtpEndCTime(0)
{

}

CAccountDlg::~CAccountDlg()
{
}

void CAccountDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BUTTON1, m_Button1);
	DDX_Control(pDX, IDC_LIST1, m_ListCon1);
	DDX_Control(pDX, IDC_DATETIMEPICKER1, m_DtpStartcon);
	DDX_Control(pDX, IDC_DATETIMEPICKER3, m_DtpEndCon);
	DDX_DateTimeCtrl(pDX, IDC_DATETIMEPICKER1, m_DtpStartCTime);
	DDX_DateTimeCtrl(pDX, IDC_DATETIMEPICKER3, m_DtpEndCTime);
	DDX_Control(pDX, IDC_COMBO8, m_address);
	DDX_Control(pDX, IDC_COMBO1, m_PayType);
	DDX_Control(pDX, IDC_COMBO5, m_UseType);
	DDX_Control(pDX, IDC_COMBO6, m_DPayType);
	DDX_Control(pDX, IDC_BUTTON2, m_Save);
}


BEGIN_MESSAGE_MAP(CAccountDlg, CSkinDialog)
	ON_BN_CLICKED(IDC_BUTTON1, &CAccountDlg::OnBnClickedButton1)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST1, &CAccountDlg::OnLvnItemchangedList1)
	ON_WM_PAINT()
	ON_CBN_SELCHANGE(IDC_COMBO6, &CAccountDlg::OnCbnSelchangeCombo6)
	ON_CBN_SELCHANGE(IDC_COMBO5, &CAccountDlg::OnCbnSelchangeCombo5)
	ON_BN_CLICKED(IDC_BUTTON2, &CAccountDlg::OnBnClickedButton2)
END_MESSAGE_MAP()


// CAccountDlg 메시지 처리기입니다.

void CAccountDlg::fnCreateQuery(char *szQuery)
{	
    CString areaid;
	int usetype;
	int paytype;
	int dpaytype;
	UpdateData(FALSE);

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


	if(m_address.GetCurSel()==0){  //address
		areaid="-1";
	}else{
		areaid=LOCK_INFO.m_LockerId[m_address.GetCurSel()-1];
	}

	switch(m_PayType.GetCurSel()){
		case 0:
			paytype=-1;
			break;
		case 1:
			paytype=3;
			break;
		case 2:
			paytype=5;
			break;
		case 3:
			paytype=2;
			break;
		case 4:
			paytype=1;//현금
			break;
		case 5:
			paytype=4;
			break;
	}
	switch(m_UseType.GetCurSel()){
		case 0:
			usetype=-1;
			break;
		//case 1:
		//	usetype=1;
		//	break;
		//case 2:
		//	usetype=10;// deliverytype 백배배달 10~~21
		//	break;
		//case 3:
		//	usetype=5;
		//	break;
		//case 4:
		//	usetype=25;
		//	break;

	}
	switch(m_DPayType.GetCurSel()){
		case 0:
			dpaytype=-1;
			break;
		case 1:
			dpaytype=1;
			break;
		case 2:
			dpaytype=2;
			break;
    }
	if(areaid=="-1"){
		if(paytype == -1){
			if(usetype ==-1){
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);

				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}
			}else{
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where deliveryType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where deliveryType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}
			}
		}else{
			if(usetype ==-1){
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where payType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,paytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where payType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,paytype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}
			}else{
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where payType=%d and deliveryType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,paytype,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where payType=%d and deliveryType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' and (%s) order by payTime desc"
								,paytype,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"), strSqlLocations);
				}
			}
		}
	}else{
		if(paytype == -1){
			if(usetype ==-1){
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

				}
			}else{
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and deliveryType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and deliveryType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}
			}
		}else{
			if(usetype ==-1){
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and payType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,paytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and payType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,paytype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}
			}else{
				if(dpaytype ==-1){
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and payType=%d and deliveryType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,paytype,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}else{
   				sprintf(szQuery,"Select areaCode,boxNo,payType,payAmount,payPhone,payTime From tblPayment "
								"where areaCode ='%s' and payType=%d and deliveryType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' order by payTime desc"
								,areaid,paytype,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
				}
			}
		}
	}
	//if(areaid=="-1"){
	//	if(paytype == -1){
	//		if(usetype ==-1){
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where payTime >= '%s' and payTime <= '%s' "
	//							,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}else{
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where useType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where useType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}
	//	}else{
	//		if(usetype ==-1){
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where payType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,paytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where payType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,paytype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}else{
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where payType=%d and useType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,paytype,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where payType=%d and useType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,paytype,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}
	//	}
	//}else{
	//	if(paytype == -1){
	//		if(usetype ==-1){
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));

	//			}
	//		}else{
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and useType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and useType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}
	//	}else{
	//		if(usetype ==-1){
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and payType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,paytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and payType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,paytype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}else{
	//			if(dpaytype ==-1){
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and payType=%d and useType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,paytype,usetype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}else{
 //  				sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment "
	//							"where areaCode ='%s' and payType=%d and useType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
	//							,areaid,paytype,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));
	//			}
	//		}
	//	}
	//}


   	//sprintf(szQuery,"Select areaCode,boxNo,payType,useType,district,deliveryPayType,payAmount,payPhone,payTime From tblPayment  where "
	   //             "where areaCode ='%s' and payType=%d and useType=%d and deliveryPayType=%d and payTime >= '%s' and payTime <= '%s' "
				//	,areaid,paytype,usetype,dpaytype,m_DtpStartCTime.Format("%Y-%m-%d %H:%M:%S.000"),m_DtpEndCTime.Format("%Y-%m-%d %H:%M:%S.000"));




}
void CAccountDlg::OnBnClickedButton1()
{
	int i=0,j=0;
	m_TotalAmount=0;
	CString m_TypeUse="";
	CString m_TypePay="";
	CString m_TypeDPay="";
	CString m_TypeDistrict="";
	CString m_Selstr="";
	CString m_BoxNo="";
	CString m_PayAmount="";
	CString m_nlocation="";
	int m_nboxNo=0;
	int m_npayType=0;
	int m_nuseType=0;
	int m_ndistrict=0;
	int m_ndtype=0;
	int m_npayAmount=0;
	CString m_npayPhone="";
	CString m_npayTime="";

	m_ListCon1.DeleteAllItems();
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	if( !CSMT_ManagerApp::fnOpenLocalDatabase(ADODataBaseLocal) )
		return ;
	CADORecordset rs(&ADODataBaseLocal);
    UpdateData(TRUE);
    fnCreateQuery(szQuery);  //쿼리 작업
	if( !rs.Open(szQuery) )
	{
		ADODataBaseLocal.Close();
		return ;
	}

	while(!rs.IsEOF())
	{
		rs.GetFieldValue("areaCode",m_nlocation );
		rs.GetFieldValue("boxNo",m_nboxNo );
		rs.GetFieldValue("payType",m_npayType );
		//rs.GetFieldValue("deliveryType",m_nuseType );
		//rs.GetFieldValue("district",m_ndistrict );
		//rs.GetFieldValue("deliveryPayType",m_ndtype );
		rs.GetFieldValue("PayAmount",m_npayAmount );
		rs.GetFieldValue("PayPhone",m_npayPhone );
		rs.GetFieldValue("PayTime",m_npayTime );

		switch(m_nuseType){
			case 1:
		        m_TypeUse="사물함";
				break;
			case 5:
		        m_TypeUse="택배발송";
				break;
			case 9:
		        m_TypeUse="물품전달";
				break;
			case 11: case 12: case 13: case 14 : case 15: case 16: case 17: case 18: case 19: case 20: case 21: 
		        m_TypeUse="택배배달";
				break;

			default:
                m_TypeUse="없음";
				break;
		}

		switch(m_ndistrict){
			case 1:
		        m_TypeDistrict="일반지역";
				break;
			case 2:
		        m_TypeDistrict="제주도";
				break;
			case 3:
		        m_TypeDistrict="기타도서";
				break;
			default:
				m_TypeDistrict="없음";
				break;
		}

		switch(m_npayType){
			case 0:
		        m_TypePay="없음";
				break;
			case 1:
		        m_TypePay="현금";
				break;
			case 2:
		        m_TypePay="핸드폰";
				break;
			case 3:
		        m_TypePay="신용카드";
				break;
			case 4:
		        m_TypePay="T-머니";
				break;
			case 5:
		        m_TypePay="K-CASH";
				break;
			default:
		        m_TypePay="없음";
				break;
		}

		switch(m_ndtype){
			case 1:
		        m_TypeDPay="선불";
				break;
			case 2:
		        m_TypeDPay="착불";
				break;
			default:
		        m_TypeDPay="없음";
				break;
		}

		for(i=0;i< LOCK_INFO.m_Locker_Sum;i++)
		{
			if(LOCK_INFO.m_LockerId[i] == m_nlocation){

				break;
			}
		}
		m_TotalAmount = m_TotalAmount+m_npayAmount;
        m_BoxNo=IntToStr(m_nboxNo);
		//m_PayAmount=IntToStr(m_npayAmount);
		m_PayAmount=_ToCurrencyString(m_npayAmount);
		//CString aa=_ToCurrencyString(m_PayAmount);
		(void)m_ListCon1.AddItem( theApp.m_strAddress[i],m_BoxNo,m_TypeUse,m_TypePay,m_PayAmount,m_TypeDistrict,m_TypeDPay,m_npayPhone,m_npayTime);

		rs.MoveNext();
	}
    fnAmountTotal();
	rs.Close();
	ADODataBaseLocal.Close();
}
CString CAccountDlg::_ToCurrencyString(int dwNumber)
{
	CString sNumber;
	sNumber.Format("%d",dwNumber);

	CString sCurrency=_T("");
	for(int i = sNumber.GetLength(), nCount=1;i>0; (i--,nCount++)){
		sCurrency= sNumber.GetAt(i-1) + sCurrency;
		if(nCount%3 ==0)
			sCurrency= ","+sCurrency;
	}
	sCurrency.TrimLeft(",");
	return sCurrency;
}

BOOL CAccountDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();

	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	m_Button1.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_Button1.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_Save.SetBitmaps(Global.GetBitmap(IDX_BMP_BTN_BASE_MIDDLE), Global.GetRGB(IDX_RGB_MASK));
	m_Save.SetIcon(Global.GetIcon(IDX_ICON_OK, ICON16), NULL, 5);

	m_totalStatic.SubclassDlgItem(IDC_STATIC_AMOUNT,this);
	m_totalStatic.fnSetStaticPosition(350,550);
	m_totalStatic.fnSetDrawFont(20, FW_BOLD ,_T("굴림"));
	m_totalStatic.fnSetDrawTextColor(RGB(0,0,0));
	m_totalStatic.fnSetDrawTextAlign(DT_CENTER);
	m_totalStatic.fnSetDrawText(_T(""));

	CRect rc;
	GetParent()->GetWindowRect(rc);
	::SetWindowPos(this->m_hWnd,HWND_BOTTOM,10, 30, rc.right-rc.left-20  ,rc.bottom-rc.top-40, SWP_NOZORDER | SWP_SHOWWINDOW);


	m_DtpStartcon.SetFormat("yyyy-MM-dd HH:mm:ss");
	m_DtpEndCon.SetFormat("yyyy-MM-dd HH:mm:ss");
    fnListConInit();
    fnControlFontSet();
	fnDropListSet();
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO1), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO5), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO6), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);
	::SendMessage( ::GetDlgItem(this->m_hWnd,IDC_COMBO8), CB_SETITEMHEIGHT, (WPARAM) -1, (LPARAM) 18);

	::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_COMBO6),FALSE);

	return TRUE;  // return TRUE unless you set the focus to a control
	// 예외: OCX 속성 페이지는 FALSE를 반환해야 합니다.
}

void CAccountDlg::fnAmountTotal()
{
	CString n_Totalsum;
	CRect rc; 
	n_Totalsum.Format("[ Total : %s원 ]",_ToCurrencyString(m_TotalAmount));
	m_totalStatic.fnSetDrawText(n_Totalsum,TRUE);
	m_totalStatic.GetWindowRect(rc);
	ScreenToClient(rc);
	InvalidateRect(rc,FALSE);
}
void CAccountDlg::fnDropListSet()
{
	CTime curTime= CTime::GetCurrentTime();
    int i=0;
	//char *aa[]={"전체","신용카드","K-CASH","핸드폰","현금","TMoney"};
	char *aa[]={"신용카드"};
	//char *bb[]={"전체","사물함","물품전달","택배발송","택배배달"};
	char *bb[]={"전체"};

	char *cc[]={"전체","선불","착불"};
	for(i=0;i<LOCK_INFO.m_Locker_Sum; i++)
	{
		if(i==0){m_address.AddString("전체");}
		m_address.AddString(theApp.m_strAddress[i]);
	}

	//LOCK_INFO.m_Selidx =0;
	//LOCK_INFO.m_kioskidx =((LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx]/theApp.m_nBoxRow)/2)+1;

	for(i=0; i< 1; i++)
	{
		m_PayType.AddString(aa[i]);
	}

	for(i=0; i< 1; i++)
	{
		m_UseType.AddString(bb[i]);
	}

	for(i=0; i< 3; i++)
	{
		m_DPayType.AddString(cc[i]);
	}

	m_address.SetCurSel(0);
	m_PayType.SetCurSel(0);
	m_address.SetCurSel(0);
	m_UseType.SetCurSel(0);
	m_DPayType.SetCurSel(0);

	m_PayType.EnableWindow(FALSE);
	m_UseType.EnableWindow(FALSE);
	m_DPayType.EnableWindow(FALSE);

	m_DtpStartCTime=curTime;
    m_DtpEndCTime=curTime;
    UpdateData(FALSE);
}

void CAccountDlg::fnListConInit()
{
	(void)m_ListCon1.SetExtendedStyle( LVS_EX_FULLROWSELECT );
	m_ListCon1.SetHeadings( _T("Location,180;No,30;UseType,75;PayType,75;PayAmount,90;District,65;DType,65;PayPhone,150;PayTime,150") );
	m_ListCon1.LoadColumnInfo();

}

void CAccountDlg::fnControlFontSet()
{
    Font_Set(13,IDC_COMBO1);
    Font_Set(13,IDC_COMBO8);
    Font_Set(13,IDC_COMBO5);
    Font_Set(13,IDC_COMBO6);
    Font_Set(12,IDC_LIST1);
    Font_Set(15,IDC_DATETIMEPICKER1);
    Font_Set(15,IDC_DATETIMEPICKER3);
	
}
void CAccountDlg::Font_Set(int size,int idc_id)
{
	    HFONT hFontEdit;
    	hFontEdit=CreateFont(size,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
		::SendMessage(::GetDlgItem(this->m_hWnd,idc_id),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
}
//void CAccountDlg::List_Font_Set()
//{
//	    HFONT hFontEdit;
//    	hFontEdit=CreateFont(14,0,0,0,500,0,0,0,HANGEUL_CHARSET,3,2,1,VARIABLE_PITCH | FF_ROMAN,"굴림");
//		::SendMessage(::GetDlgItem(this->m_hWnd,IDC_LIST1),WM_SETFONT,(WPARAM)hFontEdit, MAKELPARAM(FALSE,0));
//}
void CAccountDlg::OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	*pResult = 0;
}

void CAccountDlg::OnPaint()
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

	dc.BitBlt(0,0,rc.Width(),rc.Height()-150,&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC

    memDC.DeleteDC();

}

void CAccountDlg::OnCbnSelchangeCombo6()
{
}

void CAccountDlg::OnCbnSelchangeCombo5()
{
	if(m_UseType.GetCurSel()==3){
		::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_COMBO6),FALSE);
	}else{
		m_DPayType.SetCurSel(0);
		::EnableWindow(::GetDlgItem(this->m_hWnd,IDC_COMBO6),FALSE);
	}
	
}

void CAccountDlg::OnBnClickedButton2() //save 버튼
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
		char tmp[100];
		sprintf(tmp,"\n,,,Total,%d",m_TotalAmount);
		buffer=tmp;

		file.Write((LPCSTR)buffer, buffer.GetLength());

		file.Close();
		AfxMessageBox("데이터가 저장되었습니다.");
	}
}
