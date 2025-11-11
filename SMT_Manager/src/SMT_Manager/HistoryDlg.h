#pragma once
#include "SkinDialog.h"
#include "SkinButton.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "SortListCtrl.h"
#include "afxdtctl.h"

// CHistoryDlg 대화 상자입니다.

class CHistoryDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CHistoryDlg)

public:
	CHistoryDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CHistoryDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_HISTORY_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:
	CSkinButton m_Button1;
	afx_msg void OnBnClickedButton1();
	virtual BOOL OnInitDialog();
	CSortListCtrl m_ListCon1;
	afx_msg void OnPaint();
    void Font_Set(int size,int idc_id);
	void fnListConInit();
    void fnControlFontSet();
    void fnDropListSet();
	CString m_Search;
	CComboBox m_SearchCombo;
	CDateTimeCtrl m_dtpStartCon;
	CDateTimeCtrl m_dtpEndCon;
	CTime m_dtpStartCTime;
	CTime m_dtpEndCTime;
	CSkinButton m_Save;
	afx_msg void OnBnClickedButton2();
	CComboBox m_address;
	
	CTime m_tmLastPaint;
};
