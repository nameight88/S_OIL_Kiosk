// LoginDlg.cpp : 구현 파일입니다.
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "LoginDlg.h"


// CLoginDlg 대화 상자입니다.

IMPLEMENT_DYNAMIC(CLoginDlg, CDialog)

CLoginDlg::CLoginDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLoginDlg::IDD, pParent)
	, m_strId(_T(""))
	, m_strPass(_T(""))
{

}

CLoginDlg::~CLoginDlg()
{
}

void CLoginDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_ID_EDIT, m_strId);
	DDX_Text(pDX, IDC_PASS_EDIT, m_strPass);
}


BEGIN_MESSAGE_MAP(CLoginDlg, CDialog)
	ON_BN_CLICKED(IDC_OK_BUTTON, &CLoginDlg::OnBnClickedOkButton)
END_MESSAGE_MAP()


// CLoginDlg 메시지 처리기입니다.

void CLoginDlg::OnBnClickedOkButton()
{

	this->OnOK();
}

void CLoginDlg::OnOK()
{
	// TODO: 여기에 특수화된 코드를 추가 및/또는 기본 클래스를 호출합니다.

	UpdateData(TRUE);
	if (m_strId.IsEmpty() && m_strPass == "1")
	{
		CDialog::OnOK();
	}
	else
	{
		CDialog::OnCancel();
	}
}

void CLoginDlg::OnCancel()
{
	// TODO: 여기에 특수화된 코드를 추가 및/또는 기본 클래스를 호출합니다.

	UpdateData(TRUE);
	CDialog::OnCancel();
}
