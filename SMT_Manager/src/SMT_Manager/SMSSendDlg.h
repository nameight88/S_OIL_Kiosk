#pragma once
#include "afxcmn.h"
#include "afxwin.h"
#include "SkinDialog.h"
#include "SkinButton.h"
#include "SortListCtrl.h"
#include "afxdtctl.h"

// CSMSSendDlg 대화 상자입니다.

class CSMSSendDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CSMSSendDlg)

public:
	CSMSSendDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CSMSSendDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_SMS_SEND_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
	CSortListCtrl m_listSend;
	CString m_strMsg;
public:
	afx_msg void OnBnClickedBtnSmsSend();
	afx_msg void OnPaint();
	virtual BOOL OnInitDialog();
    void Font_Set(int size,int idc_id);
	void fnListConInit();
    void fnControlFontSet();
    void fnDropListSet();
	afx_msg void OnCbnSelchangeComboLocation();
	// 제어부 위치설정
	CComboBox m_cmbAddress;
	// 현재 LOCK_INFO.m_Selidx
	int		m_nCurrentSelidx;
	CString m_strAddress;
	afx_msg void OnBnClickedBtnSmsResend();
	// 발송 버튼
	CButton m_btnSend;
	// 재발송 버튼
	CButton m_btnResend;
};
