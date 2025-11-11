// PLCMatrixDatas.cpp: implementation of the CPLCMatrixDatas class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "SMT_Manager.h"
#include "PLCMatrixDatas.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CPLCMatrixDatas::CPLCMatrixDatas()
{
	m_SlotCount = 0;
}

CPLCMatrixDatas::~CPLCMatrixDatas()
{

}

void CPLCMatrixDatas::SavePlcData(CString VAR_NAME, CString Data) //사용하지 않음
{
	//WritePrivateProfileString(_T("PLC"), VAR_NAME, Data, PLC_INI_FILE);
	WritePrivateProfileString(_T("PLC"), VAR_NAME, Data, theApp.m_strPLCiniFile[LOCK_INFO.m_Selidx]);

}

void CPLCMatrixDatas::ReadPlcData(CString VAR_NAME, CString &Data)
{
	TCHAR tmp[80] = {0};
	//GetPrivateProfileString(_T("PLC"), VAR_NAME, NULL, tmp, 80, PLC_INI_FILE);
	GetPrivateProfileString(_T("PLC"), VAR_NAME, NULL, tmp, 80, theApp.m_strPLCiniFile[LOCK_INFO.m_Selidx]);
	Data = tmp;
}

int CPLCMatrixDatas::GetPlcNumber(int boxNo)
{
	CString tmp  = _T("");
	CString str  = _T("");
	tmp.Format(_T("SLOT_%03d"), boxNo);
	ReadPlcData(tmp, str); 
	return  (StrToInt(str));
}

BOOL CPLCMatrixDatas::InitMatrix()
{
	CFileStatus Status;
	int icount = 0;
	//if(CFile::GetStatus(PLC_INI_FILE, Status) != TRUE)
	if(CFile::GetStatus(theApp.m_strPLCiniFile[LOCK_INFO.m_Selidx], Status) != TRUE)
	{
		TRACE("PLC.ini file not found");
		return FALSE;
	}
	//icount = GetPrivateProfileInt(_T("PLC"),_T("SLOT_CNT"),0,PLC_INI_FILE);
	icount = GetPrivateProfileInt(_T("PLC"),_T("SLOT_CNT"),0,_T(theApp.m_strPLCiniFile[LOCK_INFO.m_Selidx]));
//	icount = LOCK_INFO.m_BoxSum[LOCK_INFO.m_Selidx];

	if(icount == 0)
		return FALSE;

	m_SlotCount = icount;

	return TRUE;
}
