#if !defined(AFX_BASEBUTTON_H__ED7F9E3D_1684_4FA0_8C8E_649707CCA951__INCLUDED_)
#define AFX_BASEBUTTON_H__ED7F9E3D_1684_4FA0_8C8E_649707CCA951__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// BaseButton.h : header file
//

typedef enum _tagBUTTON_STATUS
{
	BUTTON_NORMAL = 1,
	BUTTON_OWNUSE,
	BUTTON_DISABLE,
	BUTTON_SENDUSER,
	BUTTON_SENDDELIVERY,
	BUTTON_LOCKER,
	BUTTON_DELIVERY
} BUTTON_STATUS;

/////////////////////////////////////////////////////////////////////////////
// CBaseButton window

class CBaseButton : public CButton
{
// Construction
public:
	CBaseButton();
	virtual ~CBaseButton();

// Attributes
public:
	CString m_strDownImage;
	CString m_strUpImage;
	CString m_strDisImage;
	CString m_strOwnUseImage;

	BUTTON_STATUS m_ButtonStatus;
	int		m_xPosition;
	int		m_yPosition;
	int     m_buttonWidth;
	int     m_buttonHeight;


	CString m_strBGImage;
	CString m_strBGText;
	CFont   m_FontText;

	BOOL	m_bNoDrawBkImage;


// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CBaseButton)
	public:
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	//}}AFX_VIRTUAL

// Implementation
public:
	int  fnGetItemInt();
	void fnSetNoDrawImageBK(BOOL bNoDraw = TRUE);
	void fnSetButtonPosition(int xPos,int yPos);
	void fnInitialButtonStatus(BUTTON_STATUS ButtonStatus);
	void fnSetButtonImages(	CString strDownImg,CString strUpImg,
							CString strDisableImg,CString strOwnUseImg=_T(""),
							BOOL bAutoSize = TRUE);
	void fnSetButtonBkImage(CString strBkImg){m_strBGImage = strBkImg;}
	void fnSetButtonStatus(BUTTON_STATUS ButtonStatus);
	void fnSetDrawFont(int size, int weigth, CString strFontFace);
	void fnSetDrawText(CString strText);
    void fnSetButtonSize(int b_width, int b_height);
	// Generated message map functions
protected:
	//{{AFX_MSG(CBaseButton)
	afx_msg HBRUSH CtlColor(CDC* pDC, UINT nCtlColor);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_BASEBUTTON_H__ED7F9E3D_1684_4FA0_8C8E_649707CCA951__INCLUDED_)
