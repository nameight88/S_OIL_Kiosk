// BoxDatas.cpp: implementation of the CBoxDatas class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "PLCControl.h"
#include "BoxDatas.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CBoxDatas::CBoxDatas()
{
	m_strUserCode		= _T("");
	m_strTranCode		= _T("");
	m_strUserPhone		= _T("");
	m_strTranPhone		= _T("");
	m_strStartTime		= _T("");
	m_strEndTime		= _T("");
	m_strBoxPassWord	= _T("");
}

CBoxDatas::~CBoxDatas()
{

}


void CBoxDatas::fnClearDatas()
{
	m_strUserCode.Empty();
	m_strTranCode.Empty();
	m_strUserPhone.Empty();
	m_strTranPhone.Empty();
	m_strStartTime.Empty();
	m_strEndTime.Empty();
	m_strBoxPassWord.Empty();
}
