#pragma once


// CSetAutoBlockingDlg 대화 상자입니다.

class CSetAutoBlockingDlg : public CDialog
{
	DECLARE_DYNAMIC(CSetAutoBlockingDlg)

public:
	CSetAutoBlockingDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CSetAutoBlockingDlg();

	// DB로부터 설정 정보 가져오기
	BOOL Form_LoadBlockListTerms(void);

	// DB로 설정정보 저장하기
	BOOL Form_StoreBlockListTerms(void);

protected:

	CButton		m_chkNumber;
	CButton		m_chkOverdue;

	CComboBox	m_cmbNumber;
	CComboBox	m_cmbNumberOverDay;
	CComboBox	m_cmbOverDay;

	CEdit		m_editNumberRate;
	CEdit		m_editOverdueRate;


// 대화 상자 데이터입니다.
	enum { IDD = IDD_SET_AUTOBLOCKING_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:


protected:
	virtual void OnOK();
public:
	afx_msg void OnBnClickedCheckOverdue();
	afx_msg void OnBnClickedCheckNumber();
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedOk();
};
