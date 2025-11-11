// PLCMatrixDatas.h: interface for the CPLCMatrixDatas class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_PLCMATRIXDATAS_H__B9357FFC_670D_4BB2_8704_363DD6AAA21D__INCLUDED_)
#define AFX_PLCMATRIXDATAS_H__B9357FFC_670D_4BB2_8704_363DD6AAA21D__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

//#define PLC_INI_FILE _T("./PLC.ini")

class CPLCMatrixDatas  
{
public:
	CPLCMatrixDatas();
	virtual ~CPLCMatrixDatas();

public:
	BOOL InitMatrix();
	int  GetPlcNumber(int boxNo);
	void ReadPlcData(CString VAR_NAME, CString &Data);
	void SavePlcData(CString VAR_NAME, CString Data);

	inline int GetSlotCount(){ return m_SlotCount; }

public:
	int  m_SlotCount;

};


#endif // !defined(AFX_PLCMATRIXDATAS_H__B9357FFC_670D_4BB2_8704_363DD6AAA21D__INCLUDED_)
