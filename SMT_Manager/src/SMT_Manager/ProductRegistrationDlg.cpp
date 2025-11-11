#include "stdafx.h"
#include "SMT_Manager.h"
#include "ProductRegistrationDlg.h"
#include "ADO.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

IMPLEMENT_DYNAMIC(CProductRegistrationDlg, CSkinDialog)

CProductRegistrationDlg::CProductRegistrationDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CProductRegistrationDlg::IDD, pParent)
{
	m_bInitialized = FALSE;
	m_nSelectedItem = -1;
	m_nSelectedProductCode = 0;
}

CProductRegistrationDlg::~CProductRegistrationDlg()
{
}

void CProductRegistrationDlg::DoDataExchange(CDataExchange* pDX)
{
	CSkinDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST1, m_ProductList);
	DDX_Control(pDX, IDC_EDIT1, m_ProductNameEdit);
	DDX_Control(pDX, IDC_EDIT2, m_ProductPriceEdit);
	DDX_Control(pDX, IDC_COMBO1, m_ProductTypeCombo);
	DDX_Control(pDX, IDC_BUTTON1, m_AddButton);
	DDX_Control(pDX, IDC_BUTTON2, m_UpdateButton);
	DDX_Control(pDX, IDC_BUTTON3, m_DeleteButton);
	DDX_Control(pDX, IDC_BUTTON4, m_ClearButton);
	DDX_Control(pDX, IDC_BUTTON_SEARCH, m_SearchButton);
}

BEGIN_MESSAGE_MAP(CProductRegistrationDlg, CSkinDialog)
	ON_BN_CLICKED(IDC_BUTTON1, &CProductRegistrationDlg::OnBnClickedButtonAdd)
	ON_BN_CLICKED(IDC_BUTTON2, &CProductRegistrationDlg::OnBnClickedButtonUpdate)
	ON_BN_CLICKED(IDC_BUTTON3, &CProductRegistrationDlg::OnBnClickedButtonDelete)
	ON_BN_CLICKED(IDC_BUTTON4, &CProductRegistrationDlg::OnBnClickedButtonClear)
	ON_BN_CLICKED(IDC_BUTTON_SEARCH, &CProductRegistrationDlg::OnBnClickedButtonSearch)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST1, &CProductRegistrationDlg::OnLvnItemchangedProductList)
END_MESSAGE_MAP()

BOOL CProductRegistrationDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();

	if (!m_bInitialized)
	{
		m_bInitialized = TRUE;
		
		// 리스트 컨트롤 초기화
		InitializeList();
		
		// 상품 유형 콤보박스 초기화
		m_ProductTypeCombo.AddString(_T("요소수"));
		m_ProductTypeCombo.AddString(_T("차량보조제"));
		m_ProductTypeCombo.SetCurSel(0);
		
		// 버튼 텍스트 설정
		m_AddButton.SetWindowText(_T("추가"));
		m_UpdateButton.SetWindowText(_T("수정"));
		m_DeleteButton.SetWindowText(_T("삭제"));
		m_ClearButton.SetWindowText(_T("초기화"));
		m_SearchButton.SetWindowText(_T("조회"));
		
		// 초기 버튼 상태 설정
		m_UpdateButton.EnableWindow(FALSE);
		m_DeleteButton.EnableWindow(FALSE);
		
		// 초기 데이터 로드
		LoadProductList();
	}

	return TRUE;
}

void CProductRegistrationDlg::InitializeList()
{
	// 리스트 컨트롤 스타일 설정
	DWORD dwStyle = m_ProductList.GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES;
	m_ProductList.SetExtendedStyle(dwStyle);
	
	// 컬럼 추가
	m_ProductList.InsertColumn(0, _T("상품코드"), LVCFMT_CENTER, 80);
	m_ProductList.InsertColumn(1, _T("상품유형"), LVCFMT_CENTER, 120);
	m_ProductList.InsertColumn(2, _T("상품명"), LVCFMT_LEFT, 200);
	m_ProductList.InsertColumn(3, _T("가격"), LVCFMT_RIGHT, 100);
}

BOOL CProductRegistrationDlg::LoadProductList()
{
	CADODatabase db;
	
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(db))
	{
		AfxMessageBox(_T("로컬 DB 연결 실패."));
		return FALSE;
	}
	
	// SQL 쿼리 작성
	CString strQuery = _T("SELECT productCode, productType, productName, productPrice ")
					   _T("FROM tblProduct ORDER BY productType, productName");
	
	CADORecordset rs(&db);
	
	try
	{
		if (!rs.Open(strQuery))
		{
			db.Close();
			return FALSE;
		}
		
		// 리스트 초기화
		ClearList();
		
		while (!rs.IsEOF())
		{
			CString productCode, productType, productName, productPrice;
			
			rs.GetFieldValue(_T("productCode"), productCode);
			rs.GetFieldValue(_T("productType"), productType);
			rs.GetFieldValue(_T("productName"), productName);
			rs.GetFieldValue(_T("productPrice"), productPrice);
			
			// 리스트에 항목 추가
			int nItem = m_ProductList.InsertItem(m_ProductList.GetItemCount(), productCode);
			m_ProductList.SetItemText(nItem, 1, GetProductTypeString(_ttoi(productType)));
			m_ProductList.SetItemText(nItem, 2, productName);
			m_ProductList.SetItemText(nItem, 3, FormatCurrency(_ttoi(productPrice)));
			
			// productCode를 ItemData로 저장
			m_ProductList.SetItemData(nItem, _ttol(productCode));
			
			rs.MoveNext();
		}
		
		rs.Close();
	}
	catch (_com_error& e)
	{
		CString strError;
		strError.Format(_T("상품 목록 조회 중 오류: %s"), (LPCTSTR)e.Description());
		AfxMessageBox(strError);
		rs.Close();
		db.Close();
		return FALSE;
	}
	
	db.Close();
	return TRUE;
}

void CProductRegistrationDlg::OnBnClickedButtonAdd()
{
	if (!ValidateInput())
		return;
		
	if (SaveProduct())
	{
		AfxMessageBox(_T("상품이 성공적으로 등록되었습니다."));
		LoadProductList();
		ClearInputFields();
	}
	else
	{
		AfxMessageBox(_T("상품 등록에 실패했습니다."));
	}
}

void CProductRegistrationDlg::OnBnClickedButtonUpdate()
{
	if (m_nSelectedItem < 0)
	{
		AfxMessageBox(_T("수정할 상품을 선택하세요."));
		return;
	}
	
	if (!ValidateInput())
		return;
		
	if (UpdateProduct())
	{
		AfxMessageBox(_T("상품이 성공적으로 수정되었습니다."));
		LoadProductList();
		ClearInputFields();
	}
	else
	{
		AfxMessageBox(_T("상품 수정에 실패했습니다."));
	}
}

void CProductRegistrationDlg::OnBnClickedButtonDelete()
{
	if (m_nSelectedItem < 0)
	{
		AfxMessageBox(_T("삭제할 상품을 선택하세요."));
		return;
	}
	
	if (AfxMessageBox(_T("선택한 상품을 삭제하시겠습니까?"), MB_YESNO | MB_ICONQUESTION) == IDYES)
	{
		if (DeleteProduct())
		{
			AfxMessageBox(_T("상품이 성공적으로 삭제되었습니다."));
			LoadProductList();
			ClearInputFields();
		}
		else
		{
			AfxMessageBox(_T("상품 삭제에 실패했습니다."));
		}
	}
}

void CProductRegistrationDlg::OnBnClickedButtonClear()
{
	ClearInputFields();
}

void CProductRegistrationDlg::OnBnClickedButtonSearch()
{
	LoadProductList();
}

void CProductRegistrationDlg::OnLvnItemchangedProductList(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	
	if (pNMLV->uChanged == LVIF_STATE && pNMLV->uNewState == (LVIS_SELECTED | LVIS_FOCUSED))
	{
		m_nSelectedItem = pNMLV->iItem;
		if (m_nSelectedItem >= 0)
		{
			LoadSelectedItem(m_nSelectedItem);
			m_UpdateButton.EnableWindow(TRUE);
			m_DeleteButton.EnableWindow(TRUE);
		}
	}
	
	*pResult = 0;
}

BOOL CProductRegistrationDlg::ValidateInput()
{
	CString productName, productPrice;
	
	m_ProductNameEdit.GetWindowText(productName);
	m_ProductPriceEdit.GetWindowText(productPrice);
	
	if (productName.IsEmpty())
	{
		AfxMessageBox(_T("상품명을 입력하세요."));
		m_ProductNameEdit.SetFocus();
		return FALSE;
	}
	
	if (productPrice.IsEmpty())
	{
		AfxMessageBox(_T("가격을 입력하세요."));
		m_ProductPriceEdit.SetFocus();
		return FALSE;
	}
	
	// 가격이 숫자인지 확인
	if (_ttoi(productPrice) <= 0)
	{
		AfxMessageBox(_T("올바른 가격을 입력하세요."));
		m_ProductPriceEdit.SetFocus();
		return FALSE;
	}
	
	return TRUE;
}

BOOL CProductRegistrationDlg::SaveProduct()
{
	CADODatabase db;
	
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(db))
	{
		AfxMessageBox(_T("로컬 DB 연결 실패."));
		return FALSE;
	}
	
	CString productName, productPrice;
	int productType;
	
	m_ProductNameEdit.GetWindowText(productName);
	m_ProductPriceEdit.GetWindowText(productPrice);
	productType = m_ProductTypeCombo.GetCurSel() + 1; // 1: 요소수, 2: 차량보조제
	
	CString strQuery;
	strQuery.Format(_T("INSERT INTO tblProduct (productType, productName, productPrice) ")
					_T("VALUES (%d, '%s', %s)"),
					productType, productName, productPrice);
	
	BOOL bResult = db.Execute(strQuery);
	db.Close();
	
	return bResult;
}

BOOL CProductRegistrationDlg::UpdateProduct()
{
	CADODatabase db;
	
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(db))
	{
		AfxMessageBox(_T("로컬 DB 연결 실패."));
		return FALSE;
	}
	
	CString productName, productPrice;
	int productType;
	
	m_ProductNameEdit.GetWindowText(productName);
	m_ProductPriceEdit.GetWindowText(productPrice);
	productType = m_ProductTypeCombo.GetCurSel() + 1; // 1: 요소수, 2: 차량보조제
	
	int productCode = (int)m_ProductList.GetItemData(m_nSelectedItem);
	
	CString strQuery;
	strQuery.Format(_T("UPDATE tblProduct SET productType=%d, productName='%s', productPrice=%s ")
					_T("WHERE productCode=%d"),
					productType, productName, productPrice, productCode);
	
	BOOL bResult = db.Execute(strQuery);
	db.Close();
	
	return bResult;
}

BOOL CProductRegistrationDlg::DeleteProduct()
{
	CADODatabase db;
	
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(db))
	{
		AfxMessageBox(_T("로컬 DB 연결 실패."));
		return FALSE;
	}
	
	int productCode = (int)m_ProductList.GetItemData(m_nSelectedItem);
	
	CString strQuery;
	strQuery.Format(_T("DELETE FROM tblProduct WHERE productCode=%d"), productCode);
	
	BOOL bResult = db.Execute(strQuery);
	db.Close();
	
	return bResult;
}

void CProductRegistrationDlg::LoadSelectedItem(int nItem)
{
	if (nItem < 0 || nItem >= m_ProductList.GetItemCount())
		return;
	
	CString productCode = m_ProductList.GetItemText(nItem, 0);
	CString productType = m_ProductList.GetItemText(nItem, 1);
	CString productName = m_ProductList.GetItemText(nItem, 2);
	CString productPrice = m_ProductList.GetItemText(nItem, 3);
	
	// 가격에서 "원"과 콤마 제거
	productPrice.Replace(_T("원"), _T(""));
	productPrice.Replace(_T(","), _T(""));
	
	m_ProductNameEdit.SetWindowText(productName);
	m_ProductPriceEdit.SetWindowText(productPrice);
	
	// 상품 유형 선택
	if (productType == _T("요소수 계열"))
	{
		m_ProductTypeCombo.SetCurSel(0);
	}
	else if (productType == _T("차량보조제 계열"))
	{
		m_ProductTypeCombo.SetCurSel(1);
	}
	
	m_nSelectedProductCode = _ttoi(productCode);
}

void CProductRegistrationDlg::ClearList()
{
	m_ProductList.DeleteAllItems();
}

void CProductRegistrationDlg::ClearInputFields()
{
	m_ProductNameEdit.SetWindowText(_T(""));
	m_ProductPriceEdit.SetWindowText(_T(""));
	m_ProductTypeCombo.SetCurSel(0);
	
	m_nSelectedItem = -1;
	m_nSelectedProductCode = 0;
	
	m_UpdateButton.EnableWindow(FALSE);
	m_DeleteButton.EnableWindow(FALSE);
}

CString CProductRegistrationDlg::GetProductTypeString(int productType)
{
	switch (productType)
	{
	case 1:
		return _T("요소수 계열");
	case 2:
		return _T("차량보조제 계열");
	default:
		return _T("기타");
	}
}

CString CProductRegistrationDlg::FormatCurrency(int amount)
{
	CString strAmount;
	strAmount.Format(_T("%d"), amount);
	
	// 천 단위 콤마 추가
	int len = strAmount.GetLength();
	for (int i = len - 3; i > 0; i -= 3)
	{
		strAmount.Insert(i, _T(","));
	}
	
	return strAmount + _T("원");
}