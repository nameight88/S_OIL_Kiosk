// SMT_Manager.h : PROJECT_NAME 응용 프로그램에 대한 주 헤더 파일입니다.
//

#pragma once

#ifndef __AFXWIN_H__
	#error "PCH에 대해 이 파일을 포함하기 전에 'stdafx.h'를 포함합니다."
#endif
class CADODatabase;
#include "resource.h"		// 주 기호입니다.
#include "PLCControl.h"
#include "PLCMatrixDatas.h"
#include "BoxDatas.h"

// CSMT_ManagerApp:
// 이 클래스의 구현에 대해서는 SMT_Manager.cpp을 참조하십시오.
//

class CSMT_ManagerApp : public CWinApp
{
public:
	CSMT_ManagerApp();

/////////////////////////////////////////
	static BOX_PLC_MAP		 m_BoxMapTable;
	static CPLCControl		 m_PLCControl;
	static CPLCControl		 m_PLCControl_2; 

	static CPLCMatrixDatas	 m_PLCMatrixDatas;
	static CBoxDatas		 m_BoxDatas;

	static BOOL				 m_bSendSMS;

    static BOOL fnInitPLCControl();
    static BOOL fnInitBoxAndPLCMatrix();
/////////////////////////////////////////

	#ifdef __GDI_PLUS__	
	ULONG_PTR m_gdiplusToken;
	#endif

// 재정의입니다.
	public:
	virtual BOOL InitInstance();

// 구현입니다.

public:
	static BOOL fnCheckDBConnection();
	static BOOL fnCheckCenterDBConnection();
	static BOOL fnOpenLocalDatabase(CADODatabase &Database);
	static BOOL fnOpenCenterDatabase(CADODatabase &Database);
    static CTime strDateTimeToTime(CString &strTime);  
    static BOOL fnDoBoxHistory(int iHistype, int boxNo, CADODatabase *pDatabase);

	///////////////////////////////
	// 고성준: SMS 서버 연동
	static BOOL fnSendSMS(CString strAreaCode, CString strAreaName, int nBoxNo, CString strSendCompany, CString strSendPhone, CString strRecvPhone, CString strMsg);
	CString m_strSMSUserID;
	// 고성준: SMS 서버 연동
	///////////////////////////////
	static BOOL fnSendSMS(CString strAddress, CString Msg, CString SendPhone,CString RecvPhone);

	////////////////////////////
	// 고성준: SMS 전송 정보 저장
	static BOOL fnDoSMSHistory(CString strLockerNo, CString strSendPhone, CString strRecvPhone, CString strMsg);

	BOOL m_bDoSMSHistory;
	// 고성준: SMS 전송 정보 저장
	////////////////////////////

	// 라커 번호(코드)에 해당하는 주소명(도서관A열 등...)을 알아옵니다.
	static CString fnFindAddress(CString areaCode);

	// 주소명(도서관A열 등...)에 해당하는 라커 번호(코드)를 알아옵니다.
	static CString fnFindAreaCode(CString strAddress);

    static void fnLogData(CString strPart,CString strFmt, ...);
    BOOL fnLoadInIt();


public:

	CString m_strCenterDBIPAddr;
	CString m_strCenterDBUser;
	CString m_strCenterDBPass;
	CString m_strCenterDBCatalog;

	///////////////////////
	// 고성준 : 로컬 디비 쿼리 실행 연산자 추가
	static BOOL fnExcuteClientDB(CString strIP, CString strQuery);

	///////////////////////
	// 고성준 : 로컬 디비 아이피 추가
	CString m_strDBIPClient[20];

	CString m_strDBIPAddr;
	CString m_strDBUser;
	CString m_strDBPass;
	CString m_strDBCatalog;
	CString m_strPLCIPAddr[20];

	int		m_PLC_2_START[20];
	CString m_strPLCIPAddr_2[20]; 

	CString m_strAddress[20];
	CString m_strPLCiniFile[20];

	///////////////////////
	// 고성준 : 라커 번호를 ini 파일에서 알아오도록 수정
	CString m_strAreaCode[20];

	// 김준희 : 키오스크 줄 번호를 ini 파일에서 알아오도록 수정
	CString m_strMasterBoxindex[20];


	//SMT_Manager_ SuperUserPass
	CString m_strManagerId;
	CString m_strManagerPass;
	//

	int		m_nBoxCount;
	int		m_nBoxRow;
	int     m_MasterBoxidx;
	int     m_BoxRate;
    CString m_strManagerPhone;
	DECLARE_MESSAGE_MAP()
};

extern CSMT_ManagerApp theApp;
extern CGlobal Global;