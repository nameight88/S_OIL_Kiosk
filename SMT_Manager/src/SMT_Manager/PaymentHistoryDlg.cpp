#include "stdafx.h"
#include "SMT_Manager.h"
#include "PaymentHistoryDlg.h"
#include "ADO.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

IMPLEMENT_DYNAMIC(CPaymentHistoryDlg, CSkinDialog)

CPaymentHistoryDlg::CPaymentHistoryDlg(CWnd* pParent /*=NULL*/)
	: CSkinDialog(CPaymentHistoryDlg::IDD, pParent)
{
	m_bInitialized = FALSE;
}

CPaymentHistoryDlg::~CPaymentHistoryDlg()
{
}

void CPaymentHistoryDlg::DoDataExchange(CDataExchange* pDX)
{
	CSkinDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_LIST1, m_PaymentList);
	DDX_Control(pDX, IDC_DATETIMEPICKER1, m_StartDate);
	DDX_Control(pDX, IDC_DATETIMEPICKER3, m_EndDate);
	DDX_Control(pDX, IDC_COMBO8, m_LocationCombo);
	DDX_Control(pDX, IDC_COMBO1, m_PayTypeCombo);
	DDX_Control(pDX, IDC_EDIT1, m_SearchEdit);
	DDX_Control(pDX, IDC_BUTTON1, m_SearchButton);
	DDX_Control(pDX, IDC_BUTTON2, m_SaveButton);
}

BEGIN_MESSAGE_MAP(CPaymentHistoryDlg, CSkinDialog)
	ON_BN_CLICKED(IDC_BUTTON1, &CPaymentHistoryDlg::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &CPaymentHistoryDlg::OnBnClickedButton2)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST1, &CPaymentHistoryDlg::OnLvnItemchangedList1)
END_MESSAGE_MAP()

BOOL CPaymentHistoryDlg::OnInitDialog()
{
	CSkinDialog::OnInitDialog();

	if (!m_bInitialized)
	{
		m_bInitialized = TRUE;
		
		// 리스트 컨트롤 초기화
		InitializeList();
		
		// 날짜 선택기 초기화 (기본값: 최근 30일)
		COleDateTime endDate = COleDateTime::GetCurrentTime();
		COleDateTime startDate = endDate - COleDateTimeSpan(30, 0, 0, 0);
		
		m_StartDate.SetTime(startDate);
		m_EndDate.SetTime(endDate);
		
		// 지역 코드 콤보박스 초기화
		m_LocationCombo.AddString(_T("전체"));
		for (int i = 0; i < LOCK_INFO.m_Locker_Sum; i++)
		{
			if (!LOCK_INFO.m_LockerId[i].IsEmpty())
			{
				m_LocationCombo.AddString(LOCK_INFO.m_LockerId[i]);
			}
		}
		m_LocationCombo.SetCurSel(0);
		
		// 결제 유형 콤보박스 초기화
		m_PayTypeCombo.AddString(_T("전체"));
		m_PayTypeCombo.SetCurSel(0);

		
		// 초기 데이터 로드
		LoadPaymentHistory();
	}

	return TRUE;
}

void CPaymentHistoryDlg::InitializeList()
{
	// 리스트 컨트롤 스타일 설정
	DWORD dwStyle = m_PaymentList.GetExtendedStyle();
	dwStyle |= LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES;
	m_PaymentList.SetExtendedStyle(dwStyle);
	
	// 컬럼 추가
	m_PaymentList.InsertColumn(0, _T("지역코드"), LVCFMT_CENTER, 80);
	m_PaymentList.InsertColumn(1, _T("박스번호"), LVCFMT_CENTER, 70);
	m_PaymentList.InsertColumn(2, _T("사용자코드"), LVCFMT_LEFT, 120);
	m_PaymentList.InsertColumn(3, _T("결제유형"), LVCFMT_CENTER, 80);
	m_PaymentList.InsertColumn(4, _T("결제금액"), LVCFMT_RIGHT, 90);
	m_PaymentList.InsertColumn(5, _T("결제전화번호"), LVCFMT_CENTER, 120);
	m_PaymentList.InsertColumn(6, _T("확인키"), LVCFMT_CENTER, 80);
	m_PaymentList.InsertColumn(7, _T("카드번호"), LVCFMT_LEFT, 120);
	m_PaymentList.InsertColumn(8, _T("결제시간"), LVCFMT_CENTER, 150);
}

void CPaymentHistoryDlg::OnBnClickedButton1()
{
	UpdateData(TRUE);
	
	try
	{
		if (!LoadPaymentHistory())
		{
			AfxMessageBox(_T("결제내역 조회에 실패했습니다."));
		}
	}
	catch (...)
	{
		AfxMessageBox(_T("검색 중 오류가 발생했습니다."));
	}
	
	UpdateData(FALSE);
}

void CPaymentHistoryDlg::OnBnClickedButton2()
{
	// 저장 기능 구현 (Excel 저장 등)
	AfxMessageBox(_T("저장 기능은 추후 구현 예정입니다."));
}

BOOL CPaymentHistoryDlg::LoadPaymentHistory()
{
	CADODatabase db;
	
	if (!CSMT_ManagerApp::fnOpenLocalDatabase(db))
	{
		AfxMessageBox(_T("로컬 DB 연결 실패."));
		return FALSE;
	}
	
	// 검색 조건 가져오기
	COleDateTime startDate, endDate;
	m_StartDate.GetTime(startDate);
	m_EndDate.GetTime(endDate);
	
	CString locationFilter;
	int locationSel = m_LocationCombo.GetCurSel();
	if (locationSel > 0)
	{
		m_LocationCombo.GetLBText(locationSel, locationFilter);
	}
	
	int payType = m_PayTypeCombo.GetCurSel();
	
	CString searchText;
	m_SearchEdit.GetWindowText(searchText);
	
	// SQL 쿼리 작성
	CString strQuery;
	strQuery.Format(_T("SELECT areaCode, boxNo, userCode, payType, payAmount, ")
					_T("payPhone, confirmKey, cardNumber, payTime ")
					_T("FROM tblPayment ")
					_T("WHERE payTime BETWEEN '%s' AND '%s'"),
					startDate.Format(_T("%Y-%m-%d 00:00:00")),
					endDate.Format(_T("%Y-%m-%d 23:59:59")));
	
	// 지역 코드 필터 추가
	if (!locationFilter.IsEmpty())
	{
		strQuery += _T(" AND areaCode = '") + locationFilter + _T("'");
	}
	
	// 결제 유형 필터 추가
	if (payType > 0)
	{
		strQuery += _T(" AND payType = ") + CString(_T("0123456789")[payType]);
	}
	
	
	strQuery += _T(" ORDER BY payTime DESC");
	
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
		
		long totalAmount = 0;
		int nIndex = 0;
		
		while (!rs.IsEOF())
		{
			CString areaCode, boxNo, userCode, payType, payAmount, payPhone, confirmKey, cardNumber, payTime;
			
			rs.GetFieldValue(_T("areaCode"), areaCode);
			rs.GetFieldValue(_T("boxNo"), boxNo);
			rs.GetFieldValue(_T("userCode"), userCode);
			rs.GetFieldValue(_T("payType"), payType);
			rs.GetFieldValue(_T("payAmount"), payAmount);
			rs.GetFieldValue(_T("payPhone"), payPhone);
			rs.GetFieldValue(_T("confirmKey"), confirmKey);
			rs.GetFieldValue(_T("cardNumber"), cardNumber);
			rs.GetFieldValue(_T("payTime"), payTime);
			
			// 리스트에 항목 추가
			int nItem = m_PaymentList.InsertItem(nIndex, areaCode);
			m_PaymentList.SetItemText(nItem, 1, boxNo);
			m_PaymentList.SetItemText(nItem, 2, userCode);
			m_PaymentList.SetItemText(nItem, 3, GetPayTypeString(_ttoi(payType)));
			m_PaymentList.SetItemText(nItem, 4, FormatCurrency(_ttol(payAmount)));
			m_PaymentList.SetItemText(nItem, 5, payPhone);
			m_PaymentList.SetItemText(nItem, 6, confirmKey);
			m_PaymentList.SetItemText(nItem, 7, cardNumber);
			m_PaymentList.SetItemText(nItem, 8, payTime);
			
			totalAmount += _ttol(payAmount);
			nIndex++;
			rs.MoveNext();
		}
		
		// 총 금액 표시
		CString totalAmountStr;
		//totalAmountStr.Format(_T("총 %d건, 총 금액: %s"), nIndex, FormatCurrency(totalAmount));
		GetDlgItem(IDC_STATIC_AMOUNT)->SetWindowText(totalAmountStr);
		
		rs.Close();
	}
	catch (_com_error& e)
	{
		CString strError;
		strError.Format(_T("결제내역 조회 중 오류: %s"), (LPCTSTR)e.Description());
		AfxMessageBox(strError);
		rs.Close();
		db.Close();
		return FALSE;
	}
	
	db.Close();
	return TRUE;
}

void CPaymentHistoryDlg::OnLvnItemchangedList1(NMHDR *pNMHDR, LRESULT *pResult)
{
	LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);
	
	if (pNMLV->uChanged == LVIF_STATE && pNMLV->uNewState == (LVIS_SELECTED | LVIS_FOCUSED))
	{
		// 선택된 항목에 대한 추가 처리
		int nSelectedItem = pNMLV->iItem;
		if (nSelectedItem >= 0)
		{
			// 필요시 상세 정보 표시
		}
	}
	
	*pResult = 0;
}

void CPaymentHistoryDlg::ClearList()
{
	m_PaymentList.DeleteAllItems();
}

CString CPaymentHistoryDlg::FormatCurrency(long amount)
{
	CString strAmount;
	strAmount.Format(_T("%ld"), amount);
	
	// 천 단위 콤마 추가
	int len = strAmount.GetLength();
	for (int i = len - 3; i > 0; i -= 3)
	{
		strAmount.Insert(i, _T(","));
	}
	
	return strAmount + _T("원");
}

CString CPaymentHistoryDlg::GetPayTypeString(int payType)
{
	switch (payType)
	{
	case 1:
		return _T("카드결제");
	case 2:
		return _T("현금결제");
	default:
		return _T("기타");
	}
}