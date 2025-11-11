// SMT_ManagerDlg.h : 헤더 파일
//

#pragma once


#include "SkinDialog.h"
#include "afxwin.h"
#include "afxcmn.h"

#include "SkinButton.h"
#include "TabCtrlEx.h"
#include "ControlDlg.h"
#include "AccountDlg.h"
#include "HistoryDlg.h"
#include "SMSHistoryDlg.h"
// 결제내역 조회 다이얼로그 헤더 추가
#include "PaymentHistoryDlg.h"
// 상품 등록 다이얼로그 헤더 추가
#include "ProductRegistrationDlg.h"

// CSMT_ManagerDlg 대화 상자
class CSMT_ManagerDlg : public CSkinDialog
{
// 생성입니다.
public:
	CSMT_ManagerDlg(CWnd* pParent = NULL);	// 표준 생성자입니다.

// 대화 상자 데이터입니다.
	enum { IDD = IDD_SMT_MANAGER_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV 지원입니다.


	CImageList *m_pImageList;
// 구현입니다.
protected:
	HICON m_hIcon;
	void RelocationControls();
	// 생성된 메시지 맵 함수
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:

	CSkinButton m_Ok;
	CSkinButton m_Cancel;

    CControlDlg* m_ControlDlg;
    CAccountDlg* m_AccountDlg;
    CHistoryDlg* m_HistoryDlg;
    CSMSHistoryDlg* m_SMSHistoryDlg;

	////////////////////
	// 블랙리스트 관리 제거
	
	// 결제내역 조회 다이얼로그 포인터 추가
	CPaymentHistoryDlg* m_PaymentHistoryDlg;
	
	// 상품 등록 다이얼로그 포인터 추가
	CProductRegistrationDlg* m_ProductRegistrationDlg;


    HWND hTabDlg;

	CTabCtrlEx m_tab1;
	BOOL m_bInitialized;

	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBnClickedButton1();
	afx_msg void OnBnClickedButton2();
	afx_msg void OnTcnSelchangeTab1(NMHDR *pNMHDR, LRESULT *pResult);

    void WindowsVisibleCheck(int sel);
    BOOL GetDBData();

	virtual BOOL PreTranslateMessage(MSG* pMsg);
};
