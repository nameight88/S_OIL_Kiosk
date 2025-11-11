// PLCControl.h: interface for the CPLCControl class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_PLCCONTROL_H__8BBA5417_24DD_46FD_8B71_056C3B803424__INCLUDED_)
#define AFX_PLCCONTROL_H__8BBA5417_24DD_46FD_8B71_056C3B803424__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "JSocket.h"

/////////////////////////////////////////////////////////////////////////////
#define MAKE_WORD(blo,bhi)				((WORD)bhi<<8) + blo
#define LO_BYTE(U16)					((BYTE)U16)
#define HI_BYTE(U16)  					((BYTE)(U16>>8))
#define LO_WORD(U32)					((WORD)U32)
#define HI_WORD(U32)  					((WORD)(U32>>16))
#define MAKE_DWORD(ll,lh,hl,hh)			(((DWORD)hh<<24)+((DWORD)hl<<16)+((DWORD)lh<<8)+((DWORD)ll<<0))

#define TIMEOUT_DEFAULT						2000	//msec, 2sec
#define PLC_DEFAULT_PORT					5000

#define PLC_CMD_STX		0x02
#define PLC_CMD_ETX		0x03
#define PLC_CMD_ACK		0x06
#define PLC_CMD_NAK		0x15
#define PLC_CMD_STAT	0x30
#define PLC_CMD_OPEN	0X31

#define MK_SUM(A)		((BYTE)(A[0]+A[1]+A[2]+A[3]))
#define MK_RESUM(A)		((BYTE)(A[0]+A[1]+A[2]+A[3]+A[4]+A[5]+A[6]+A[7]))

//////////////////////////////////////////////////////////////////
// 고성준: 16번 처리 개선인 경우 아래를 활성화함.

#define NEW_PLC_CONTROL

// 고성준: 16번 처리 개선인 경우 아래를 활성화함.
//////////////////////////////////////////////////////////////////


#ifdef NEW_PLC_CONTROL
#define MK_ADDR(A)		((BYTE)(( ((A-1)/16) << 4) | ((A-1)%16)&0x0f ))
#else
#define MK_ADDR(A)		((BYTE)(( (A/16) << 4) | ((A%16)-1)&0x0f ))
#endif


#pragma pack(push, 1)

#pragma pack(pop)
/////////////////////////////////////////////////////////////////////////////

class CPLCControl  
{
public:
	CPLCControl();
	virtual ~CPLCControl();

public:
	BOOL OpenBox(int PLCNo, int nTimeoutMilli = 2000);
	BOOL IsOpenBox(int PLCNo, int *retStatus, int nTimeoutMilli = 2000);
	BOOL InitPLC(LPCTSTR szPLCAddress , int iPLCPort = PLC_DEFAULT_PORT);
	void ExitPLC();
	int  GetSlotCount(){ return m_SlotCount;}
	BOOL GetPLCInputStatusAll(int *iStatusArray, int iArrayCount);

protected:



private:
	int			m_SlotCount;
	int			m_PLCPort;
	WORD		m_wTransactionID;
	TCHAR		m_PLCAddress[32];
	CJSocket	m_Socket;
};

typedef CMap<int,int,int,int>	BOX_PLC_MAP;

#endif // !defined(AFX_PLCCONTROL_H__8BBA5417_24DD_46FD_8B71_056C3B803424__INCLUDED_)
