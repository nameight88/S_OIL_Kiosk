// PLCControl.cpp: implementation of the CPLCControl class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "PLCControl.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CPLCControl::CPLCControl()
{
	m_SlotCount	 = 0;
	m_PLCPort	 = PLC_DEFAULT_PORT;
	m_wTransactionID = 0;
	ZeroMemory(m_PLCAddress,sizeof(m_PLCAddress));
}

CPLCControl::~CPLCControl()
{
	ExitPLC();
}

BOOL CPLCControl::InitPLC(LPCTSTR szPLCAddress, int iPLCPort)
{
	this->ExitPLC();

	//iPLCPort = 502;
	WORD InputPLCCount  = 0;
	WORD OutputPLCCount = 0;
	_tcscpy(m_PLCAddress , szPLCAddress);
	m_PLCPort	 = iPLCPort;

	return TRUE;
}

void CPLCControl::ExitPLC()
{
	if(m_Socket.IsConnected())
	{
		m_Socket.Shutdown();
		m_Socket.Close();
	}
}

BOOL CPLCControl::IsOpenBox(int PLCNo, int *retStatus, int nTimeoutMilli /*= 2000*/)
{
	if(PLCNo <= 0)
		return FALSE;

	if( !m_Socket.IsConnected() )
	{
		if(!m_Socket.Connect(m_PLCAddress,m_PLCPort) )
			return FALSE;
	}
	m_Socket.SetTimeout(nTimeoutMilli);
	
	char cStr[10] = {0,};
	char cSum = 0;
	WORD cRes = 0;
	cStr[0] = PLC_CMD_STX;
	cStr[1] = MK_ADDR(PLCNo);
	cStr[2] = PLC_CMD_STAT;
	cStr[3] = PLC_CMD_ETX;
	cStr[4] = MK_SUM(cStr);
	if (m_Socket.Send(cStr, 5) != 5)
	{
		TRACE("Modbus_ReadInputRegisterData Error 1-1\n");
		return FALSE;
	}

	if( !m_Socket.Select(TRUE) )
	{
		TRACE("Modbus_ReadInputRegisterData Error 1-1\n");
		return FALSE;
	}

	ZeroMemory(cStr,10);

	if( !m_Socket.ReadExact(cStr,9))
	{
		TRACE("Modbus_ReadInputRegisterData Error 1-2\n");
		return FALSE;
	}


	cSum = MK_RESUM(cStr);
	if(cSum != cStr[8])
	{
		TRACE("Modbus_ReadInputRegisterData Error 2-1\n");
		return FALSE;
	}

	cRes |= cStr[3];
	cRes |= (cStr[4]<<8);
	
//#ifdef NEW_PLC_CONTROL
//	if(  ( 0x01 << (((PLCNo-1)%16)&0x0f) ) & cRes )
//		*retStatus = TRUE;
//	else
//		*retStatus = FALSE;
//#else
//	if(  ( 0x01 << (((PLCNo%16)-1)&0x0f) ) & cRes )
//		*retStatus = TRUE;
//	else
//		*retStatus = FALSE;
//#endif
	if(  ( 0x01 << (((PLCNo%16)-1)&0x0f) ) & cRes )
		*retStatus = FALSE;
	else
		*retStatus = TRUE;

	return TRUE;
}

BOOL CPLCControl::OpenBox(int PLCNo, int nTimeoutMilli /*= 2000*/)
{
	BOOL bRet = TRUE;

	if( !m_Socket.IsConnected() )
	{
		if(!m_Socket.Connect(m_PLCAddress,m_PLCPort) )
			return FALSE;
	}
	m_Socket.SetTimeout(nTimeoutMilli);
	
	char cStr[5] = {0,};
	cStr[0] = PLC_CMD_STX;
	cStr[1] = MK_ADDR(PLCNo);
	cStr[2] = PLC_CMD_OPEN;
	cStr[3] = PLC_CMD_ETX;
	cStr[4] = MK_SUM(cStr);

	bRet = m_Socket.Send(cStr, 5) == 5;


	return bRet;
}

BOOL CPLCControl::GetPLCInputStatusAll(int *iStatusArray, int iArrayCount)
{
	char cStr[10] = {0,};
	char cSum = 0;
	WORD cRes = 0;
	int  iArrayIdx = 0;
	cStr[0] = PLC_CMD_STX;
	cStr[2] = PLC_CMD_STAT;
	cStr[3] = PLC_CMD_ETX;
	for(int i=0;i<8 && iArrayIdx < iArrayCount;i++)
	{
		cStr[1] = MK_ADDR(i);
		cStr[4] = MK_SUM(cStr);
		if (m_Socket.Send(cStr, 5) != 5)
		{
			TRACE("Modbus_ReadInputRegisterData Error 1-0\n");
			return FALSE;
		}
		
		if( !m_Socket.Select(TRUE) )
		{
			TRACE("Modbus_ReadInputRegisterData Error 1-1\n");
			return FALSE;
		}

		ZeroMemory(cStr,10);
		
		if(!m_Socket.ReadExact(cStr,9))
		{
			TRACE("Modbus_ReadInputRegisterData Error 1-2\n");
			return FALSE;
		}
		
		cSum = MK_RESUM(cStr);
		if(cSum != cStr[8])
		{
			TRACE("Modbus_ReadInputRegisterData Error 2-1\n");
			return FALSE;
		}

		cRes |= cStr[3];
		cRes |= (cStr[4]<<8);
		for(int j=0;j<16;j++)
		{
			iStatusArray[iArrayIdx] = cRes&(0x01<<j) ? 1 : 0;
			iArrayIdx++;
		}
	}
	return TRUE;
}