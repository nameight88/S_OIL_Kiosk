#pragma once
#include "SkinDialog.h"
#include "SkinButton.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "SortListCtrl.h"
#include "afxdtctl.h"
#include "Ado.h"
// CAccountDlg 대화 상자입니다.

class CAccountDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CAccountDlg)

public:
	CAccountDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CAccountDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_ACCOUNT_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:
	CSkinButton m_Button1;
	afx_msg void OnBnClickedButton1();
	virtual BOOL OnInitDialog();
    //void List_Font_Set();
    void fnListConInit();
    void fnControlFontSet();
    void Font_Set(int size,int idc_id);
    void fnDropListSet();
    void fnCreateQuery(char *szQuery);
    void fnAmountTotal();
    CString _ToCurrencyString(int dwNumber);
    afx_msg void OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult);
	CSortListCtrl m_ListCon1;
	afx_msg void OnPaint();
	CDateTimeCtrl m_DtpStartcon;
	CDateTimeCtrl m_DtpEndCon;
	CTime m_DtpStartCTime;
	CTime m_DtpEndCTime;
	CComboBox m_address;
	CComboBox m_PayType;
	CComboBox m_UseType;
	CComboBox m_DPayType;
//////////////////////////////////////////////
	int m_TotalAmount;
	CTime m_tmLastPaint;

	CBaseStatic m_totalStatic;
	afx_msg void OnCbnSelchangeCombo6();
	afx_msg void OnCbnSelchangeCombo5();
	CSkinButton m_Save;
	afx_msg void OnBnClickedButton2();
};
