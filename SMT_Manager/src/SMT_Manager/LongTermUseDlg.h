#pragma once
#include "SkinDialog.h"
#include "SkinButton.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "SortListCtrl.h"

// CLongTermUseDlg 대화 상자입니다.

class CLongTermUseDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CLongTermUseDlg)

public:
	CLongTermUseDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CLongTermUseDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_LONGTERMUSE_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
    void fnListConInit();
	CSortListCtrl m_ListCon1;
    void fnControlFontSet();
	void fnFontSet(int size,int idc_id);
    afx_msg void OnPaint();
	CSkinButton m_Search;

	CSkinButton m_SMSTotalSend;

	CSkinButton m_btnOpen;
	CSkinButton m_btnReturn;

	CSkinButton m_btnStore;
	CSkinButton m_SMSSend;
	CComboBox m_LongUseDay;
	afx_msg void OnBnClickedButtonSearch();
	CString m_OverUseDay;
	CComboBox m_UseTypeCombo;
	CString m_strUseTypeSearch;


	// 현재 LOCK_INFO.m_Selidx
	int		m_nCurrentSelidx;

	CString m_strAddress;
	CString m_strBoxNo;

	/////////////////////////////
	// 고성준 : 사용자명, 정보 추가
	CString m_strUserName;
	CString m_strUserInfo;

	CString m_strUseType;
	CString m_strReceivePhone;
	CString m_strSendPhone;
	CString m_strStartTime;
	CString m_strEndTime;
	CTime m_tmLastPaint;

	afx_msg void OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnBnClickedButtonSmsOne();
	afx_msg void OnBnClickedButtonSmsAll();
	void fnGetListConData(int m_idx);
	afx_msg void OnBnClickedBtnStore();
	afx_msg void OnBnClickedButtonOpen();
	afx_msg void OnBnClickedButtonReturn();
	BOOL fnDoUserBoxClear(int iTypeMsg, int nBoxNo);
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
};
