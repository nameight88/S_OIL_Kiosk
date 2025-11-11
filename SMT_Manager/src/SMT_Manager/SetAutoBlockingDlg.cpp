// SetAutoBlockingDlg.cpp : 구현 파일입니다.
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "SetAutoBlockingDlg.h"
#include "Ado.h"
#include "Math.h"


// CSetAutoBlockingDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CSetAutoBlockingDlg, CDialog)

CSetAutoBlockingDlg::CSetAutoBlockingDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CSetAutoBlockingDlg::IDD, pParent)
{

}

CSetAutoBlockingDlg::~CSetAutoBlockingDlg()
{
}

// DB로부터 설정 정보 가져오기
BOOL CSetAutoBlockingDlg::Form_LoadBlockListTerms(void)
{
	CADODatabase	db;
	char			szQuery[1024] = {0,};
	CString			strText;
	int				nNum;
	double			dubRate;


	if( !CSMT_ManagerApp::fnOpenLocalDatabase(db) )
	{
		AfxMessageBox("서버 DB 접속 실패.",MB_OK);
		return FALSE;
	}

	CADORecordset rs(&db);

	sprintf(szQuery,"SELECT useNumberYN,number,numberOverHour,numberRate,useOverdueHourYN,overdueHour,overdueRate,updateId,updateDate "
		" FROM tblBlackListTerms ORDER BY updateDate DESC ");
	if( !rs.Open(szQuery) ) 
	{
		db.Close();

		AfxMessageBox("서버 DB 접속 실패.",MB_OK);
		return FALSE;
	}

	if (!rs.IsEOF())
	{
		rs.GetFieldValue("useNumberYN",strText );
		if (strText=="Y")
			this->m_chkNumber.SetCheck(BST_CHECKED);
		else
			this->m_chkNumber.SetCheck(BST_UNCHECKED);

		rs.GetFieldValue("number", nNum);
		if (nNum < 1) nNum = 1;
		this->m_cmbNumber.SetCurSel(nNum-1);

		rs.GetFieldValue("numberOverHour",nNum);
		if (nNum < 24) nNum = 24;
		this->m_cmbNumberOverDay.SetCurSel(nNum/24 - 1);

		rs.GetFieldValue("numberRate",dubRate);
		strText.Format(_T("%.1lf"), dubRate);
		this->m_editNumberRate.SetWindowText(strText);

		rs.GetFieldValue("useOverdueHourYN",strText );
		if (strText=="Y")
			this->m_chkOverdue.SetCheck(BST_CHECKED);
		else
			this->m_chkOverdue.SetCheck(BST_UNCHECKED);

		rs.GetFieldValue("overdueHour",nNum);
		if (nNum < 24) nNum = 24;
		this->m_cmbOverDay.SetCurSel(nNum/24-1);

		rs.GetFieldValue("overdueRate",dubRate );
		strText.Format(_T("%.1lf"), dubRate);
		this->m_editOverdueRate.SetWindowText(strText);

	}

	rs.Close();
	db.Close();

	return TRUE;
}

// DB로 설정정보 저장하기
BOOL CSetAutoBlockingDlg::Form_StoreBlockListTerms(void)
{
	CADODatabase	db;
	CString			strQuery;
	CString			strText;

	CString			useNumberYN		= _T("N");
	int				number 			= 0;
	int				numberOverHour	= 0;
	double			numberRate		= 1;
	CString			useOverdueHourYN = _T("N");
	int				overdueHour		= 0;
	double			overdueRate		= 1;


	if (this->m_chkNumber.GetCheck() == BST_CHECKED)
		useNumberYN = _T("Y");

	number = this->m_cmbNumber.GetCurSel() + 1;
	numberOverHour = (this->m_cmbNumberOverDay.GetCurSel() + 1) * 24;

	this->m_editNumberRate.GetWindowText(strText);
	try
	{
		numberRate = ::atof(strText.GetBuffer());
		strText.ReleaseBuffer();
	}
	catch (...)
	{
		this->MessageBox(_T("횟수 관련 사용금지 처리 배율은 숫자만 입력가능합니다."));
		return FALSE;
	}

	if (this->m_chkOverdue.GetCheck() == BST_CHECKED)
		useOverdueHourYN = _T("Y");

	overdueHour = (this->m_cmbOverDay.GetCurSel() + 1) * 24;

	this->m_editOverdueRate.GetWindowText(strText);
	try
	{
		overdueRate = ::atof(strText.GetBuffer());
		strText.ReleaseBuffer();
	}
	catch (...)
	{
		this->MessageBox(_T("연체일 관련 사용금지 처리 배율은 숫자만 입력가능합니다."));
		return FALSE;
	}


	strQuery.Format("USE SMT_LOCKER UPDATE tblBlackListTerms SET useNumberYN='%s',number=%d,numberOverHour=%d,"
			"numberRate=%lf,useOverdueHourYN='%s',overdueHour=%d,overdueRate=%lf,updateId='',"
			"updateDate=GETDATE()",
			useNumberYN,
			number,
			numberOverHour,
			numberRate,
			useOverdueHourYN,
			overdueHour,
			overdueRate);


	if( !CSMT_ManagerApp::fnOpenLocalDatabase(db) )
	{
		AfxMessageBox("서버 DB 접속 실패.",MB_OK);
		return FALSE;
	}

	CADORecordset rs(&db);


	if(! db.Execute(strQuery) ) 
	{
		AfxMessageBox("서버 DB 업데이트 실패.",MB_OK);
		db.Close();
		return FALSE;
	}

	db.Close();

	return TRUE;
}


void CSetAutoBlockingDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_CHECK_NUMBER, m_chkNumber);
	DDX_Control(pDX, IDC_CHECK_OVERDUE, m_chkOverdue);

	DDX_Control(pDX, IDC_CMB_NUMBER, m_cmbNumber);
	DDX_Control(pDX, IDC_CMB_NUMBER_OVERDAY, m_cmbNumberOverDay);
	DDX_Control(pDX, IDC_CMB_OVERDAY, m_cmbOverDay);

	DDX_Control(pDX, IDC_EDIT_BLOCKING_RATE_NUMBER, m_editNumberRate);
	DDX_Control(pDX, IDC_EDIT_BLOCKING_RATE_OVERDUE, m_editOverdueRate);
}


BEGIN_MESSAGE_MAP(CSetAutoBlockingDlg, CDialog)
	ON_BN_CLICKED(IDC_CHECK_OVERDUE, &CSetAutoBlockingDlg::OnBnClickedCheckOverdue)
	ON_BN_CLICKED(IDC_CHECK_NUMBER, &CSetAutoBlockingDlg::OnBnClickedCheckNumber)
	ON_BN_CLICKED(IDOK, &CSetAutoBlockingDlg::OnBnClickedOk)
END_MESSAGE_MAP()


// CSetAutoBlockingDlg 메시지 처리기입니다.
void CSetAutoBlockingDlg::OnOK()
{
	UpdateData(TRUE);

	this->Form_StoreBlockListTerms();

	CDialog::OnOK();
}

void CSetAutoBlockingDlg::OnBnClickedCheckOverdue()
{
	BOOL bEnable = this->m_chkOverdue.GetCheck() == BST_CHECKED;
	this->m_cmbOverDay.EnableWindow(bEnable);
	this->m_editOverdueRate.EnableWindow(bEnable);
}

void CSetAutoBlockingDlg::OnBnClickedCheckNumber()
{
	BOOL bEnable = this->m_chkNumber.GetCheck() == BST_CHECKED;
	this->m_cmbNumber.EnableWindow(bEnable);
	this->m_cmbNumberOverDay.EnableWindow(bEnable);
	this->m_editNumberRate.EnableWindow(bEnable);
}

BOOL CSetAutoBlockingDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// 컨트롤 설정
	
	CString input;
	int i;
	for(i=1;i<= 90; i++)
	{
		input.Format("%d일",i);
		m_cmbOverDay.AddString(input);
		m_cmbNumberOverDay.AddString(input);
	}

	for(i=1;i<= 10; i++)
	{
		input.Format("%d회",i);
		m_cmbNumber.AddString(input);
	}

	// 설정값을 읽어옵니다.
	this->Form_LoadBlockListTerms();
	this->OnBnClickedCheckNumber();
	this->OnBnClickedCheckOverdue();


	return TRUE;  // return TRUE unless you set the focus to a control
	// 예외: OCX 속성 페이지는 FALSE를 반환해야 합니다.
}

void CSetAutoBlockingDlg::OnBnClickedOk()
{
	// TODO: 여기에 컨트롤 알림 처리기 코드를 추가합니다.
	OnOK();
}
