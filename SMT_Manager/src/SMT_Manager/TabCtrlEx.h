#if !defined(AFX_TabCtrlEx_H__1E6E4FE9_BE01_4DA1_AFA9_A98527A3769B__INCLUDED_)
#define AFX_TabCtrlEx_H__1E6E4FE9_BE01_4DA1_AFA9_A98527A3769B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// TabCtrlEx.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CTabCtrlEx window

class CTabCtrlEx : public CTabCtrl
{
// Construction
public:
	CTabCtrlEx();

	// 전경/배경/보더 색 지정
	void SetColor(COLORREF crText, COLORREF crBorder, COLORREF crBackIn, COLORREF crBackOut, BOOL bRedraw=TRUE);
	// 보더 스타일 지정
	void Set3dBorder(BOOL b3dBorder) { m_b3dBorder = b3dBorder; }
	// 탭 아이템 이미지 설정
	void SetItemImage(int nItem, int nImage);

protected:
	COLORREF m_crBackIn;		// 탭 안쪽 배경색
	COLORREF m_crBackOut;		// 탭 바깥쪽 배경색
	COLORREF m_crText;			// 텍스트 색
	COLORREF m_crBorder;		// 보더 색

	BOOL	m_b3dBorder;		// 보더 스타일 : TRUE=3D, FALSE=플랫

protected:
// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTabCtrlEx)
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CTabCtrlEx();

	// Generated message map functions
protected:
	//{{AFX_MSG(CTabCtrlEx)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	afx_msg void OnPaint();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

protected:
	// 세로 스타일일 경우 텍스트를 한자씩 세로로 출력
	virtual void TextOutVertical(CDC* pDC, CRect rect, CStringW sText);
	// 메인 보더를 그림
	virtual void DrawMainBorder(LPDRAWITEMSTRUCT lpDrawItemStruct);
	// 해당 탭 아이템의 텍스트를 출력
	virtual void DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct);
	// 해당 탭 아이템의 보더를 그림
	virtual void DrawItemBorder(LPDRAWITEMSTRUCT lpDrawItemStruct);
	virtual void PreSubclassWindow();
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TabCtrlEx_H__1E6E4FE9_BE01_4DA1_AFA9_A98527A3769B__INCLUDED_)
