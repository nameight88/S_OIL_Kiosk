// SMT_Manager.cpp : 응용 프로그램에 대한 클래스 동작을 정의합니다.
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "SMT_ManagerDlg.h"
#include "Ado.h"
#include "UtilFile.h"
#include "SMTPacket.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#endif
const CString g_strSMSID  = _T("DP1100731");
const CString g_strSMSID2 = _T("ryu1223");
const CString g_strSMSPW  = _T("ryu8080");


typedef BOOL (*PSMSCONNECT)(CString UserID, CString UserID2, CString Password);
typedef void (*PSMSDOWN)();
typedef int  (*PSMSSEND)(CString strSendNo, CString strRecvNo, CString strMSG, CString strReserveDT);

/////////////////////////////////////////////////////////////////////////
BOX_PLC_MAP		 CSMT_ManagerApp::m_BoxMapTable;
CPLCControl		 CSMT_ManagerApp::m_PLCControl;
CPLCControl	     CSMT_ManagerApp::m_PLCControl_2;
CPLCMatrixDatas	 CSMT_ManagerApp::m_PLCMatrixDatas;
CBoxDatas		 CSMT_ManagerApp::m_BoxDatas;
BOOL			 CSMT_ManagerApp::m_bSendSMS;
/////////////////////////////////////////////////////////////////////////

// CSMT_ManagerApp

BEGIN_MESSAGE_MAP(CSMT_ManagerApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

// CSMT_ManagerApp 생성


CSMT_ManagerApp::CSMT_ManagerApp()
{
	m_strDBIPAddr	= _T("");
	m_strDBUser		= _T("");
	m_strDBPass		= _T("");
	m_strDBCatalog	= _T("");

	m_strCenterDBIPAddr = _T("");
	m_strCenterDBUser = _T("");
	m_strCenterDBPass = _T("");
	m_strCenterDBCatalog = _T("");

	m_nBoxCount=0;
	m_nBoxRow=0;
	m_MasterBoxidx=0;
	m_BoxRate=0;

    m_strManagerPhone = _T("");

	m_bSendSMS = FALSE;

}


// 유일한 CSMT_ManagerApp 개체입니다.

CSMT_ManagerApp theApp;
CGlobal Global;

// CSMT_ManagerApp 초기화

BOOL CSMT_ManagerApp::InitInstance()
{
#ifdef __GDI_PLUS__	
	GdiplusStartupInput gdiplusStartupInput;
	GdiplusStartup(&m_gdiplusToken, &gdiplusStartupInput, NULL);
#endif
	// InitCommonControlsEx()를 사용하지 않으면 창을 만들 수 없습니다.

	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);

	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinApp::InitInstance();

	AfxEnableControlContainer();

	SetRegistryKey(_T("키핑하우스_SMT_Manager"));

	Global.LoadBitmaps();
	Global.LoadIcons();

	Global.SetRGB(IDX_RGB_MASK, RGB(255,0,255));
	Global.SetRGB(IDX_RGB_BACKGROUND, RGB(241,241,241));

	Global.SetRGB(IDX_RGB_FONT, RGB(51,51,51));
	Global.SetRGB(IDX_RGB_FONT_CAPTION, RGB(0,0,0));
	Global.SetRGB(IDX_RGB_FONT_CAPTION_INACTIVE, RGB(178,178,178));
    fnLoadInIt();

	CSMT_ManagerDlg dlg;
	m_pMainWnd = &dlg;

	Global.SetFont(IDX_FONT_SMALL, _T("Arial"), 8);
	Global.SetFont(IDX_FONT_MEDIUM, _T("Arial"), 9);
	Global.SetFont(IDX_FONT_LARGE, _T("Arial"), 11, TRUE);

	INT_PTR nResponse = dlg.DoModal();
	if (nResponse == IDOK)
	{
	}
	else if (nResponse == IDCANCEL)
	{
	}
	return FALSE;
}

BOOL CSMT_ManagerApp::fnLoadInIt()
{
	CString strIniPath = ".\\SMT_Manager.INI";
	char str[MAX_PATH] = {0};

	/////////////////////////////////////////////////
	// 고성준 : ini 읽어오기 개선 및 추가
	char szFind[MAX_PATH];

	int n;

	for (n = 0; n < 20; n++)
	{
		sprintf(szFind, "PLC_IP%d", n+1);
		if( GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath) > 0)
			m_strPLCIPAddr[n] = str;

		sprintf(szFind, "PLC_2_START%d", n+1);
		m_PLC_2_START[n] = GetPrivateProfileInt("Config",szFind,0,strIniPath);

		sprintf(szFind, "PLC_2_IP%d", n+1);
		GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath);
		m_strPLCIPAddr_2[n] = str;
	}
	
	for (n = 0; n < 20; n++)
	{
		sprintf(szFind, "DB_IP%d", n+1);
		if( GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath) > 0)
			this->m_strDBIPClient[n] = str;
	}

	for (n = 0; n < 20; n++)
	{
		sprintf(szFind, "ADDRESS%d", n+1);
		if( GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath) > 0)
			m_strAddress[n] = str;
	}

	for (n = 0; n < 20; n++)
	{
		sprintf(szFind, "AREA_CODE%d", n+1);
		if( GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath) > 0)
			m_strAreaCode[n] = str;
	}

	for (n = 0; n < 20; n++)
	{
		sprintf(szFind, "MasterBoxindex%d", n+1);
		if( GetPrivateProfileString("Config", szFind,  "", str, MAX_PATH, strIniPath) > 0)
			m_strMasterBoxindex[n] = str;
	}

	// 고성준 : ini 읽어오기 개선 및 추가
	/////////////////////////////////////////////////


	////////////////////////////
	// 고성준: SMS 전송 정보 저장
	m_bDoSMSHistory = FALSE;
	if( GetPrivateProfileString("Config", "SMSHistory",  "", str, MAX_PATH, strIniPath) > 0)
	{
		if (str[0] == 'Y')
			m_bDoSMSHistory = TRUE;
	}
	// 고성준: SMS 전송 정보 저장
	////////////////////////////
	
	///////////////////////////////
	// 고성준: SMS 서버 연동(아이디 길이가 2보다 작거나 같으면 구버전 동작 dll이용)
	if( GetPrivateProfileString("Config", "SMSUser",  "ryu1223", str, MAX_PATH, strIniPath) > 0)
		m_strSMSUserID = str;
	// 고성준: SMS 서버 연동(아이디 길이가 2보다 작거나 같으면 구버전 동작 dll이용)
	///////////////////////////////


	if( GetPrivateProfileString("Config", "DB_IP",  "", str, MAX_PATH, strIniPath) > 0)
		m_strDBIPAddr = str;

	if( GetPrivateProfileString("Config", "Catalog",  "SMT_Locker", str, MAX_PATH, strIniPath) > 0)
		m_strDBCatalog = str;

	//if( GetPrivateProfileString("Config", "US",  "sa", str, MAX_PATH, strIniPath) > 0)
	if( GetPrivateProfileString("Config", "US",  "SMTUser", str, MAX_PATH, strIniPath) > 0)
		m_strDBUser = str;

	//if( GetPrivateProfileString("Config", "PW",  "lotecs@kr9240", str, MAX_PATH, strIniPath) > 0)
	if( GetPrivateProfileString("Config", "PW",  "SMTUserPass", str, MAX_PATH, strIniPath) > 0)
		m_strDBPass = str;

	if( GetPrivateProfileString("Config", "C_DB_IP",  "", str, MAX_PATH, strIniPath) > 0)
		m_strCenterDBIPAddr = str;

	if( GetPrivateProfileString("Config", "C_Catalog",  "SMT_Locker", str, MAX_PATH, strIniPath) > 0)
		m_strCenterDBCatalog = str;

	if( GetPrivateProfileString("Config", "C_US",  "SMTUser", str, MAX_PATH, strIniPath) > 0)
		m_strCenterDBUser = str;

	if( GetPrivateProfileString("Config", "C_PW",  "SMTUserPass", str, MAX_PATH, strIniPath) > 0)
		m_strCenterDBPass = str;

/////////////////////////////////SMT_Manager_SuperUserPass 셋팅
	if( GetPrivateProfileString("Config", "ManagerId",  "admin", str, MAX_PATH, strIniPath) > 0)
		m_strManagerId = str;
	if( GetPrivateProfileString("Config", "ManagerPass",  "keeping@", str, MAX_PATH, strIniPath) > 0)
		m_strManagerPass = str;

/////////////////////////////////
	m_nBoxCount    =     GetPrivateProfileInt("Config","BoxCount",0,strIniPath);
	m_nBoxRow      =     GetPrivateProfileInt("Config","BoxRow",0,strIniPath);
	m_MasterBoxidx =     GetPrivateProfileInt("Config","MasterBoxindex",0,strIniPath);
	m_BoxRate      =     GetPrivateProfileInt("Config","BoxRate",0,strIniPath);

	if( GetPrivateProfileString("Config", "ManagerPhone",  "", str, MAX_PATH, strIniPath) > 0)
		m_strManagerPhone = str;


	//_T("./PLC.ini")//PLC.INI파일 경로설정
    int i=0,j=0;
	char plcFile[60];
	for(i=0;i<20;i++){
		j=i+1;
		sprintf(plcFile,"./PLC%d.ini",j);
		m_strPLCiniFile[i]=plcFile;
	}

	if( GetPrivateProfileString("Config", "EnableSMS",  "", str, MAX_PATH, strIniPath) > 0)
	{
		if (str[0] == 'Y')
			m_bSendSMS = TRUE;
	}

	return TRUE;

}

BOOL CSMT_ManagerApp::fnInitBoxAndPLCMatrix()
{
	if( !m_PLCMatrixDatas.InitMatrix() )
		return FALSE;

	int iCount = m_PLCMatrixDatas.GetSlotCount();
	int iPLCNo = 0;
	CString strBoxNo;
	for(int i=0;i<iCount;i++)
	{
		iPLCNo = m_PLCMatrixDatas.GetPlcNumber(i+1);
		m_BoxMapTable.SetAt(i+1,iPLCNo);
		strBoxNo.Format("%d",i+1);
	}

	return TRUE;
}

BOOL CSMT_ManagerApp::fnInitPLCControl()
{
    if(!fnInitBoxAndPLCMatrix()) return FALSE;
	  
	if( !m_PLCControl.InitPLC(theApp.m_strPLCIPAddr[LOCK_INFO.m_Selidx]) ) return FALSE;

	if (theApp.m_strPLCIPAddr_2[LOCK_INFO.m_Selidx] != "")
	{
		if( !m_PLCControl_2.InitPLC(theApp.m_strPLCIPAddr_2[LOCK_INFO.m_Selidx]) )
		{
             return FALSE;
		}
	}

	return TRUE;
}

///////////////////////
// 고성준 : 로컬 디비 쿼리 실행 연산자 추가
BOOL CSMT_ManagerApp::fnExcuteClientDB(CString strIP, CString strQuery)
{
	CADODatabase	dbClient;
	CString			strConnect;

	if (strIP.GetLength() <= 0)
		return TRUE;


	strConnect.Format(_T("Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;"
						"Initial Catalog=SMT_LOCKER;Persist Security Info=True"), 
						strIP, theApp.m_strDBUser, theApp.m_strDBPass);
	if(! dbClient.Open( strConnect , theApp.m_strDBUser, theApp.m_strDBPass))
	{
		return FALSE;
	}

	if (! dbClient.Execute(strQuery))
	{
		return FALSE;
	}

	return TRUE;

}

BOOL CSMT_ManagerApp::fnCheckDBConnection()
{
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	int  iBoxNo = 0;

	sprintf(szQuery,"Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;Initial Catalog=%s;Persist Security Info=True",
				theApp.m_strDBIPAddr,
				theApp.m_strDBUser,
				theApp.m_strDBPass,
				theApp.m_strDBCatalog);

	if(! ADODataBaseLocal.Open( szQuery , 
								theApp.m_strDBUser, 
								theApp.m_strDBPass) )
	{
		return FALSE;
	}

	ADODataBaseLocal.Close();
	
	return TRUE;
}

BOOL CSMT_ManagerApp::fnCheckCenterDBConnection()
{
	CADODatabase ADODataBaseLocal;
	char szQuery[1024] = {0,};
	int  iBoxNo = 0;

	sprintf(szQuery,"Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;Initial Catalog=%s;Persist Security Info=True",
				theApp.m_strCenterDBIPAddr,
				theApp.m_strCenterDBUser,
				theApp.m_strCenterDBPass,
				theApp.m_strCenterDBCatalog);

	if(! ADODataBaseLocal.Open( szQuery , 
								theApp.m_strCenterDBUser, 
								theApp.m_strCenterDBPass) )
	{
		return FALSE;
	}

	ADODataBaseLocal.Close();
	
	return TRUE;
}

BOOL CSMT_ManagerApp::fnOpenLocalDatabase(CADODatabase &Database)
{
	char szQuery[1024] = {0,};

	sprintf(szQuery,"Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;Initial Catalog=%s;Persist Security Info=True",
				theApp.m_strDBIPAddr,theApp.m_strDBUser,
				theApp.m_strDBPass,theApp.m_strDBCatalog);

	if(! Database.Open( szQuery , theApp.m_strDBUser, 
			theApp.m_strDBPass) )
	{
		return FALSE;
	}

	return TRUE;
}

BOOL CSMT_ManagerApp::fnOpenCenterDatabase(CADODatabase &Database)
{
	char szQuery[1024] = {0,};

	sprintf(szQuery,"Provider=SQLOLEDB.1;Data Source=%s;User ID=%s;Password=%s;Initial Catalog=%s;Persist Security Info=True",
				theApp.m_strCenterDBIPAddr,theApp.m_strCenterDBUser,
				theApp.m_strCenterDBPass,theApp.m_strCenterDBCatalog);

	if(! Database.Open( szQuery , theApp.m_strCenterDBUser, 
			theApp.m_strCenterDBPass) )
	{
		return FALSE;
	}

	return TRUE;
}

CTime CSMT_ManagerApp::strDateTimeToTime(CString &strTime)
{
	CTime curtime = CTime::GetCurrentTime();
	CTime time = curtime; //  기본값을 현재 시간으로 변경
	
	CString strTmp;
	strTmp = strTime;
	strTmp.Remove(' ');
	strTmp.Remove('-');
	strTmp.Remove(':');
	
	if(strTime.IsEmpty() || strTmp.GetLength() < 14)
	{
	    return curtime;
	}
	
	int iYear	= StrToInt(strTmp.Mid(0,4));
	int iMonth	= StrToInt(strTmp.Mid(4,2));
	int iDay	= StrToInt(strTmp.Mid(6,2));
	int iHour	= StrToInt(strTmp.Mid(8,2));
	int iMin	= StrToInt(strTmp.Mid(10,2));
	int iSec	= StrToInt(strTmp.Mid(12,2));

	// ?유효성 검사 추가
	if (iYear < 1970 || iYear > 3000 ||
		iMonth < 1 || iMonth > 12 ||
		iDay < 1 || iDay > 31 ||
		iHour < 0 || iHour > 23 ||
		iMin < 0 || iMin > 59 ||
		iSec < 0 || iSec > 59)
	{
		return curtime;
	}

	try
	{
		time = CTime(iYear,iMonth,iDay,iHour,iMin,iSec);
	}
	catch (...)
	{
		//  예외 발생 시 현재 시간 반환
		time = curtime;
	}

	return time;
}

BOOL CSMT_ManagerApp::fnDoBoxHistory(int iHistype, int boxNo, CADODatabase *pDatabase)
{
	CADODatabase *pADODataBaseLocal = NULL;
	char szQuery[1024] = {0,};

	if(pDatabase == NULL)
	{
		pADODataBaseLocal = new CADODatabase;
		if(pADODataBaseLocal == NULL)
			return FALSE;
		if( !CSMT_ManagerApp::fnOpenLocalDatabase(*pADODataBaseLocal) )
		{
			delete pADODataBaseLocal;
			return FALSE;
		}
	}

	sprintf(szQuery,"INSERT INTO tblBoxHistory (eventType,areaCode,boxNo,serviceType,boxSizeType,useState,"
					"userCode,userName,userPhone,dong,addressNum,transCode,transPhone,barcode,deliveryType,"
					"boxPassword,payCode,payAmount,useTimeType,startTime,endTime,createDate) "
					"select %d,areaCode, boxNo, serviceType, boxSizeType, useState, userCode,"
					"userName, userPhone, dong, addressNum,transCode,transPhone,"
					"barCode, deliveryType, boxPassword, payCode, payAmount, useTimeType,"
					"startTime, endTime,productCode ,GetDate() From tblBoxMaster "
					"where boxNo=%d and areaCode='%s' ",
					iHistype,boxNo,LOCK_INFO.m_LockerId[LOCK_INFO.m_Selidx]);
	
	////////////////////////////
	// 고성준 : 로컬 디비 업데이트 추가

	CUtilFile::LogData("BoxHistory", szQuery);
	if ( !CSMT_ManagerApp::fnExcuteClientDB(theApp.m_strDBIPClient[LOCK_INFO.m_Selidx], szQuery))
	{
		AfxMessageBox("로컬 DB 업데이트(History) 실패.",MB_OK);
		return FALSE;
	}


	if(pDatabase)
	{
		if( !pDatabase->Execute(szQuery) ) 
		{
			AfxMessageBox("서버 DB 업데이트(History) 실패.",MB_OK);
			return FALSE;
		}
	}
	else
	{
		if( !pADODataBaseLocal->Execute(szQuery) ) 
		{
			AfxMessageBox("서버 DB 업데이트(History) 실패.",MB_OK);
			pADODataBaseLocal->Close();
			delete pADODataBaseLocal;
			return FALSE;
		}
        pADODataBaseLocal->Close();
		delete pADODataBaseLocal;
	}

#ifdef _REMOTE_DB_USED_

	    CADODatabase *pADODataBaseCenter = NULL;
		pADODataBaseCenter = new CADODatabase;
		if(pADODataBaseCenter == NULL)
			return FALSE;
		if( !CSMT_ManagerApp::fnOpenCenterDatabase(*pADODataBaseCenter) )
		{
			delete pADODataBaseCenter;
			return FALSE;
		}

		if( !pADODataBaseCenter->Execute(szQuery) ) 
		{
			pADODataBaseCenter->Close();
		    delete pADODataBaseCenter;
			return FALSE;
		}
	    pADODataBaseCenter->Close();
		delete pADODataBaseCenter;

#endif


	return TRUE;
}

///////////////////////////////
// 고성준: SMS 서버 연동
BOOL CSMT_ManagerApp::fnSendSMS(CString strAreaCode, CString strAreaName, int nBoxNo, CString strSendCompany, CString strSendPhone, CString strRecvPhone, CString strMsg)
{
	BOOL		retval = FALSE;
	CSMTPacket	packetSend;
	CSMTPacket	packetResult;
	CJSocket	socket;

	// sms 사용자 아이디를 이용하여 구버전으로 동작하도록 한다.
	if (theApp.m_strSMSUserID.GetLength() <= 2)
	{
		return fnSendSMS(strAreaCode, strMsg, strSendPhone, strRecvPhone);
	}

	packetSend.Create(theApp.m_strSMSUserID, strAreaCode, strAreaName, nBoxNo, strSendCompany, strSendPhone, strRecvPhone, strMsg);
#ifdef _LOCAL_SMS_SERVER
	if (socket.Connect("127.0.0.1", 8810))
#else
	if (socket.Connect("112.217.194.250", 8810))
#endif // _LOCAL_SMS_SERVER
	{
		if (socket.Send(packetSend.GetFullBytePoint(), packetSend.GetFullByteSize()) == packetSend.GetFullByteSize())
		{	
			BYTE	buf[1024];
			int		nBufSize = 1024;
			int		nRecv = socket.Read(buf, nBufSize);
			int		len = nRecv;

			packetResult.Create(buf, len);
			while (nRecv >= 0 && !packetResult.IsPacket())
			{
				nRecv = socket.Read(buf+len, nBufSize-len);
				if (nRecv > 0)
					len += nRecv;
				packetResult.Create(buf, len);

				if (nRecv > 1000)
					nRecv = -1;
			}

			if (packetResult.IsPacket())
			{
				if (packetResult.IsACK())
				{
					retval = TRUE;

					////////////////////////////
					// 고성준: SMS 전송 정보 저장
					CSMT_ManagerApp::fnDoSMSHistory(CSMT_ManagerApp::fnFindAreaCode(strAreaCode), strSendPhone, strRecvPhone, strMsg);
					// 고성준: SMS 전송 정보 저장
					////////////////////////////
				}
				else
				{
					CUtilFile::LogData("SMS", "[ERROR] 실패값 리턴 (%s, %d번, recv:%s)", strAreaName, nBoxNo, strRecvPhone);
				}
			}
			else
			{
				CUtilFile::LogData("SMS", "[ERROR] 결과 받기 실패 (%s, %d번, recv:%s)", strAreaName, nBoxNo, strRecvPhone);
			}
		}
		else
		{
			CUtilFile::LogData("SMS", "[ERROR] 보내기 실패 (%s, %d번, recv:%s)", strAreaName, nBoxNo, strRecvPhone);
		}

		socket.Shutdown();
		socket.Close();
	}
	else
	{
		CUtilFile::LogData("SMS", "[ERROR] 연결 실패 (%s, %d번, recv:%s)", strAreaName, nBoxNo, strRecvPhone);
	}


	return retval;
}

// 고성준: SMS 서버 연동
///////////////////////////////

BOOL CSMT_ManagerApp::fnSendSMS(CString strAddress, CString Msg, CString SendPhone,CString RecvPhone)
{
	HINSTANCE hInstance = NULL;
	//CString	strReserveDT = _T("1900-01-01 00:00");	//예약없을 때
	CString	strReserveDT = ("1900-01-01 00:00");
	CString strMsg		 = Msg;
	CString strSendPhone = SendPhone;
	CString strRecvPhone = RecvPhone;

	if(strMsg.IsEmpty() || strRecvPhone.IsEmpty())
		return FALSE;

	if( strSendPhone.IsEmpty() )
		strSendPhone = "0";

	hInstance = LoadLibrary(_T("SMSKorea2c.dll"));
	if(hInstance == NULL)
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS DLL 로드 실패! <%s,%s,%s>",strSendPhone, strRecvPhone, strMsg);
		return FALSE;
	}

	PSMSCONNECT pFuncConnect = (PSMSCONNECT)GetProcAddress(hInstance, "OpenConnection");

	if(pFuncConnect == NULL)
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS OpenConnection 로드 실패! <%s,%s,%s>",strSendPhone, strRecvPhone, strMsg);
		FreeLibrary(hInstance);
		return FALSE;
	}
	PSMSSEND pFuncSend = (PSMSSEND)GetProcAddress(hInstance, "SendMessage1");

	if(pFuncSend == NULL)
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS SendMessage1 로드 실패 <%s,%s,%s>",strSendPhone, strRecvPhone, strMsg);
		FreeLibrary(hInstance);
		return FALSE;
	}

	PSMSDOWN pFundown = (PSMSDOWN)GetProcAddress(hInstance, "ShutdownConnection");
	if(pFundown == NULL)
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS ShutdownConnection 로드 실패! <%s,%s,%s>",strSendPhone, strRecvPhone, strMsg);
		FreeLibrary(hInstance);
		return FALSE;
	}

	if(pFuncConnect(g_strSMSID, g_strSMSID2, g_strSMSPW) == FALSE)
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS Connect 실패! <%s,%s,%s>",strSendPhone, strRecvPhone, strMsg);
		FreeLibrary(hInstance);
		return FALSE;
	}

	// 홍동성
	//char * cTest = TEXT("01089979152");
	//int ret = pFuncSend(cTest, cTest, cTest, cTest);
	//int ret = 100;

	// Debug에서 중단되지만 실행은 된다.
	char * cSendPhone = (char *) strSendPhone.GetBuffer(strSendPhone.GetLength());
	char * cRecvPhone = (char *) strRecvPhone.GetBuffer(strRecvPhone.GetLength());
	char * cMsg = (char *) strMsg.GetBuffer(strMsg.GetLength());
	char * crReserveDT = (char *) strReserveDT.GetBuffer(strReserveDT.GetLength());
	int ret = pFuncSend(cSendPhone,	cRecvPhone, cMsg, crReserveDT);

	// 기존
	//int ret = pFuncSend(strSendPhone, strRecvPhone, strMsg, strReserveDT);	
	if( ret != 100 )
	{
		CSMT_ManagerApp::fnLogData("SMS", "SMS 전송 실패! <%s,%s,%s> <code=%d>",strSendPhone, strRecvPhone, strMsg,ret);
		pFundown();
		FreeLibrary(hInstance);
		return FALSE;
	}

	CSMT_ManagerApp::fnLogData("SMS", "SMS 전송 성공! <%s,%s,%s> <code=%d>",strSendPhone, strRecvPhone, strMsg,ret);

	pFundown();
	FreeLibrary(hInstance);

	////////////////////////////
	// 고성준: SMS 전송 정보 저장
	CSMT_ManagerApp::fnDoSMSHistory(CSMT_ManagerApp::fnFindAreaCode(strAddress), strSendPhone, strRecvPhone, strMsg);
	// 고성준: SMS 전송 정보 저장
	////////////////////////////

	return TRUE;
}


////////////////////////////
// 고성준: SMS 전송 정보 저장
BOOL CSMT_ManagerApp::fnDoSMSHistory(CString strLockerNo, CString strSendPhone, CString strRecvPhone, CString strMsg)
{
	if (!theApp.m_bDoSMSHistory)
		return TRUE;

	CADODatabase db;
	CString strQuery;
	CTime tmNow = CTime::GetTickCount();


	if( !CSMT_ManagerApp::fnOpenLocalDatabase(db) )
	{
		CSMT_ManagerApp::fnLogData("SMT_Manager", "SMS ErrorStmt <open db false >");
		return FALSE;
	}

	strQuery.Format("INSERT INTO tblSMSHistory (processName,areaCode,sendPhone,recvPhone,sendMessage,sendDate,createDate) "
					"VALUES('%s','%s','%s','%s','%s','%s',GETDATE())",
					"SMTLocker",strLockerNo, strSendPhone,strRecvPhone,strMsg,tmNow.Format("%Y-%m-%d %H:%M:%S"));

	if( db.Execute(strQuery) ) 
	{
		db.Close();
		CSMT_ManagerApp::fnLogData("SMT_Manager", "DB_Wrtie_SMS <%s>", strQuery);

		return TRUE;
	}
	else
	{
		CSMT_ManagerApp::fnLogData("SMT_Manager", "SMS ErrorStmt <%s>", strQuery);
		db.Close();

		return FALSE;
	}

}


// 고성준: SMS 전송 정보 저장
////////////////////////////

// 라커 번호(코드)에 해당하는 주소명(도서관A열 등...)을 알아옵니다.
CString CSMT_ManagerApp::fnFindAddress(CString areaCode)
{
	CString retval = "";

	int nIndex = 0;
	for (nIndex = 0; nIndex < LOCK_INFO.m_Locker_Sum; nIndex++)
	{
		if (LOCK_INFO.m_LockerId[nIndex] == areaCode)
		{
			retval = theApp.m_strAddress[nIndex];
			break;
		}
	}
	if (retval.IsEmpty())
	{
		CSMT_ManagerApp::fnLogData("SMT_Manager", "FindAddress <%s> not found!", areaCode);
	}

	return retval;
}

// 주소명(도서관A열 등...)에 해당하는 라커 번호(코드)를 알아옵니다.
CString CSMT_ManagerApp::fnFindAreaCode(CString strAddress)
{
	CString retval = "";

	int nIndex = 0;
	for (nIndex = 0; nIndex < LOCK_INFO.m_Locker_Sum; nIndex++)
	{
		if (theApp.m_strAddress[nIndex] == strAddress)
		{
			retval = LOCK_INFO.m_LockerId[nIndex];
			break;
		}
	}

	if (retval.IsEmpty())
	{
		CSMT_ManagerApp::fnLogData("SMT_Manager", "FindAddress <%s> not found!", strAddress);
	}

	return retval;
}

void CSMT_ManagerApp::fnLogData(CString strPart,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	strFileName.Format(_T("../Log/%s_%02d.txt"), strPart, time.GetDay());
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer(strLog.GetLength()) , strLog.GetLength()*2 );
#else
	file.Write(strLog.GetBuffer(strLog.GetLength()) , strLog.GetLength() );
#endif
	file.Close();
}
