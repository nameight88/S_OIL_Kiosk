#pragma once

#include "SkinDialog.h"
#include "afxwin.h"
#include "afxcmn.h"

// CProductRegistrationDlg 다이얼로그

class CProductRegistrationDlg : public CSkinDialog
{
	DECLARE_DYNAMIC(CProductRegistrationDlg)

public:
	CProductRegistrationDlg(CWnd* pParent = NULL);   // 표준 생성자입니다.
	virtual ~CProductRegistrationDlg();

// 다이얼로그 데이터입니다.
	enum { IDD = IDD_PRODUCT_REGISTRATION_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV 지원입니다.

	DECLARE_MESSAGE_MAP()

public:
	// 컨트롤 변수들
	CListCtrl m_ProductList;
	CEdit m_ProductNameEdit;
	CEdit m_ProductPriceEdit;
	CComboBox m_ProductTypeCombo;
	CButton m_AddButton;
	CButton m_UpdateButton;
	CButton m_DeleteButton;
	CButton m_ClearButton;
	CButton m_SearchButton;

	// 멤버 함수들
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedButtonAdd();
	afx_msg void OnBnClickedButtonUpdate();
	afx_msg void OnBnClickedButtonDelete();
	afx_msg void OnBnClickedButtonClear();
	afx_msg void OnBnClickedButtonSearch();
	afx_msg void OnLvnItemchangedProductList(NMHDR *pNMHDR, LRESULT *pResult);
	
	BOOL LoadProductList();
	void InitializeList();
	void ClearList();
	void ClearInputFields();
	BOOL ValidateInput();
	BOOL SaveProduct();
	BOOL UpdateProduct();
	BOOL DeleteProduct();
	void LoadSelectedItem(int nItem);
	CString GetProductTypeString(int productType);
	CString FormatCurrency(int amount);

private:
	BOOL m_bInitialized;
	int m_nSelectedItem;
	int m_nSelectedProductCode;
};