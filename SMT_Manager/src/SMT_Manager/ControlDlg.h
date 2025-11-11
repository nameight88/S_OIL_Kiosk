#pragma once
#include "SkinDialog.h"
#include "SkinButton.h"
#include "afxwin.h"
#include "afxcmn.h"
#include "SortListCtrl.h"
#include "PLCControl.h"
#include "PLCMatrixDatas.h"
#include "BoxDatas.h"
#include "afxdtctl.h"

// CControlDlg 대화 상자입니다.


class CControlDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CControlDlg)

public:
	CControlDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CControlDlg();

// 대화 상자 데이터입니다.
	enum { IDD = IDD_CONTROL_DIALOG };

	//CSkinButton m_Button1;

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL OnInitDialog();
    void fnFontSet(int size,int id);

///////////////////////////////////////
	BOOL fnCreateLockerButtons();
	BOOL fnRefreshLockers();
	void fnDeleteLockerButtons();
	void fnAutoCalcLockerButtonsPos_Size();
    void fnDropListSet();
    void SetCurData();
    void fnControlFontSet();
    void fnDbDateDisplay();
	void fnAllControlInit();
	void fnListConInit();
    void fnBoxTotalStatus();
 //   BOOL fnRefreshPLCStatus();
    BOOL fnDoUserBoxClear(int iTypeMsg);
	void fnDoF9Key();


    int         m_BackGroundWidth;
	int         m_BackGroundHeight;
    int         m_BoxBackBgStartx;
    int         m_BoxBackBgStarty;
	int         m_BoxStartPosx;
    int         m_BoxStartPosy;
	int         m_BoxAutoWidth;
	int         m_BoxAutoHeight;
	int		 product_Code;

	CBaseButton *m_pButtonLocker;
	BOOL		m_bLockerStatus[MAX_LOCKER_BOX_CNT];
	CBaseButton m_LockerTitle;
	CBaseStatic m_totalStatic;


	// 현재 LOCK_INFO.m_Selidx
	int		m_nCurrentSelidx;

	CString m_strAddress;
	CString	m_strBoxNo;
	int		m_nUseState;

	CString m_strReceivePhone;

	CComboBox m_cmbAddress;
	CComboBox m_cmbUsed;

	CTime m_tmLastPaint;

	CSkinButton m_AllBoxOpen;
	CSkinButton m_BoxOpen;
	CSkinButton m_dbUpdate;

	LRESULT OnDlgInitMessage(WPARAM wParam,LPARAM lParam);
	afx_msg void OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnDestroy();

	afx_msg void OnCbnSelchangeComboLocation();
	afx_msg void OnBnClickedButtonTitle();
	afx_msg void OnPaint();
	afx_msg void OnButtonsClicked(UINT CtrlID);

	afx_msg void OnCbnSelchangeComboSearch();
	afx_msg void OnBnClickedButtonSearch();
	afx_msg void OnBnClickedButtonOpen();
	afx_msg void OnBnClickedButtonReturn();
	afx_msg void OnBnClickedButtonUpdate();
	afx_msg void OnBnClickedButtonCamera();
	afx_msg void OnBnClickedButtonSms();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg void OnBnClickedButtonAllopen();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	// 바코드값
	CString m_strBarcode;
	
	// 상품 정보 표시용
	CEdit m_ProductInfoEdit;
	void fnLoadProductInfo(int productCode);
	void fnClearProductInfo();
};
