// BoxDatas.h: interface for the CBoxDatas class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_BOXDATAS_H__3B81F71F_6BBF_4858_A5B0_D2789B85AAD7__INCLUDED_)
#define AFX_BOXDATAS_H__3B81F71F_6BBF_4858_A5B0_D2789B85AAD7__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CBoxDatas  
{
public:
	CBoxDatas();
	virtual ~CBoxDatas();

public:
	void fnClearDatas();
	CString m_strUserCode;
	CString m_strTranCode;
	CString m_strUserPhone;
	CString m_strTranPhone;
	CString m_strStartTime;
	CString m_strEndTime;
	CString m_strBoxPassWord;

};

#endif // !defined(AFX_BOXDATAS_H__3B81F71F_6BBF_4858_A5B0_D2789B85AAD7__INCLUDED_)
