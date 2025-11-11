#pragma once

#include "SkinDialog.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "afxdtctl.h"

// CPaymentHistoryDlg 다이얼로그

class CPaymentHistoryDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CPaymentHistoryDlg)

public:
	CPaymentHistoryDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CPaymentHistoryDlg();

// 다이얼로그 데이터입니다.
	enum { IDD = IDD_PAYMENT_HISTORY_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()

public:
	// 컨트롤 변수들
	CListCtrl m_PaymentList;
	CDateTimeCtrl m_StartDate;
	CDateTimeCtrl m_EndDate;
	CComboBox m_LocationCombo;
	CComboBox m_PayTypeCombo;
	CEdit m_SearchEdit;
	CButton m_SearchButton;
	CButton m_SaveButton;

	// 멤버 함수들
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedButton1();	// 검색 버튼
	afx_msg void OnBnClickedButton2();	// 저장 버튼
	afx_msg void OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult);
	
	BOOL LoadPaymentHistory();
	void InitializeList();
	void ClearList();
	CString FormatCurrency(long amount);
	CString GetPayTypeString(int payType);

private:
	BOOL m_bInitialized;
};
