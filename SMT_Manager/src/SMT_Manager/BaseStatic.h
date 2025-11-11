#if !defined(AFX_BASESTATIC_H__1F55F9DB_E99C_420E_825C_82BB682812A9__INCLUDED_)
#define AFX_BASESTATIC_H__1F55F9DB_E99C_420E_825C_82BB682812A9__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// BaseStatic.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CBaseStatic window

class CBaseStatic : public CStatic
{
// Construction
public:
	CBaseStatic();

// Attributes
public:
	CString m_strBGImage;
	CString m_strBGText;
	CFont   m_FontText;
	int		m_xPosition;
	int		m_yPosition;
	COLORREF m_TextColor;
	BOOL	m_bMultiLine;
	int		m_MultiLineOffset;
	UINT	m_TextAlign;

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CBaseStatic)
	//}}AFX_VIRTUAL

// Implementation
public:
	void fnSetTextMultiline(BOOL bMultiline = TRUE){m_bMultiLine = bMultiline;}
	void fnSetTextMultilineOffset(int Offset = 0){m_MultiLineOffset = Offset;}
	CString fnGetItemText(){return m_strBGText;}
	void fnDisableWindow(){EnableWindow(FALSE);Invalidate(FALSE); }
	int  fnGetItemInt();
	void fnSetStaticPosition(int xPos,int yPos);
	void fnSetStaticSize(int width, int height);
	void fnSetDrawFont(int size,int weigth,CString strFontFace);
	void fnSetDrawText(CString strText,BOOL bReDraw = FALSE);
	void fnSetBkImage(CString strImagePath , BOOL bAutoSize = TRUE);
	void fnSetDrawTextColor(COLORREF TextColor){ m_TextColor = TextColor; }
	void fnSetDrawTextAlign(UINT Align){m_TextAlign = Align;}
	virtual ~CBaseStatic();

	// Generated message map functions
protected:
	//{{AFX_MSG(CBaseStatic)
	afx_msg void OnPaint();
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_BASESTATIC_H__1F55F9DB_E99C_420E_825C_82BB682812A9__INCLUDED_)
