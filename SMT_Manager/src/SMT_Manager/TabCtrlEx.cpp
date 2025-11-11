// TabCtrlEx.cpp : implementation file
//

#include "stdafx.h"
#include "SMT_Manager.h"
#include "TabCtrlEx.h"
#include "MemDC.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

enum { PADDING = 3, EDGE = 20};

/////////////////////////////////////////////////////////////////////////////
// CTabCtrlEx

Color ToColor(COLORREF cf)
{
	BYTE r,g,b;
	r = (BYTE)(0x000000FF & (cf));		
	g = (BYTE)(0x000000FF & (cf>>8));
	b = (BYTE)(0x000000FF & (cf>>16));

	return( Color((BYTE)255, r, g, b) );
}

CTabCtrlEx::CTabCtrlEx()
{
	m_crBackIn = RGB(255,255,255);
	m_crBackOut = RGB(255,255,255);
	m_crText = RGB(0,0,0);
	m_crBorder= RGB(128,128,128);

	m_b3dBorder=FALSE;
}

CTabCtrlEx::~CTabCtrlEx()
{
}


BEGIN_MESSAGE_MAP(CTabCtrlEx, CTabCtrl)
	//{{AFX_MSG_MAP(CTabCtrlEx)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
	ON_WM_ERASEBKGND()
	ON_WM_PAINT()
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTabCtrlEx message handlers

void CTabCtrlEx::SetColor(COLORREF crText, COLORREF crBorder, COLORREF crBackIn, COLORREF crBackOut, BOOL bRedraw/*=TRUE*/)
{
	m_crText = crText;
	m_crBorder= crBorder;
	m_crBackIn = crBackIn;
	m_crBackOut = crBackOut;

	if( bRedraw )
		Invalidate();
}

void CTabCtrlEx::DrawItem(LPDRAWITEMSTRUCT lpdis)
{
	TC_ITEM tci;
	CDC* pDC = CDC::FromHandle(lpdis->hDC);
	HIMAGELIST hilTabs = (HIMAGELIST)TabCtrl_GetImageList(GetSafeHwnd());

	BOOL bSelected = (lpdis->itemID == (UINT)GetCurSel());

	CRect rItem(lpdis->rcItem);

	if( bSelected )
		rItem.bottom += 1;

	// 텍스트 획득
	rItem.left += PADDING;
	rItem.top += PADDING + (bSelected ? 1 : 0);

	pDC->SetBkMode(TRANSPARENT);

	CString sTemp;
	tci.mask        = TCIF_TEXT | TCIF_IMAGE;
	tci.pszText     = sTemp.GetBuffer(100);
	tci.cchTextMax  = 99;
	GetItem(lpdis->itemID, &tci);
	sTemp.ReleaseBuffer();

	// 아이콘 그리기
	if( hilTabs )
		ImageList_Draw(hilTabs, tci.iImage, *pDC, rItem.left, rItem.top, ILD_TRANSPARENT);

	// 텍스트 그리기
	CFont *pFont = Global.GetFont(IDX_FONT_SMALL);
	if( !pFont )
		return;	

	LOGFONT lf;
	pFont->GetLogFont(&lf);
	if( bSelected )
		lf.lfWeight = FW_BOLD;

	if( bSelected && (GetStyle() & TCS_BUTTONS) )
		pDC->SetTextColor(RGB(255,255,255));
	else
		pDC->SetTextColor(m_crText);

	CFont *poldFont, newFont;
	newFont.CreateFontIndirect(&lf);
	poldFont = pDC->SelectObject( &newFont );

	DWORD dwStyle = GetStyle();

	// 세로 탭
	if( dwStyle&TCS_VERTICAL )
	{
		if( hilTabs )
			rItem.top += 16 + PADDING;

		TextOutVertical( pDC, rItem, (CStringW)sTemp );
	}
	// 가로 탭
	else
	{
		if( hilTabs )
			rItem.left += 16 + PADDING;

		rItem.right -= PADDING;
		Global.FormatText(sTemp, pDC, rItem.Width());
		pDC->DrawText(sTemp, rItem, DT_NOPREFIX | DT_CENTER);
	}	
	
	pDC->SelectObject(poldFont);
}

void CTabCtrlEx::TextOutVertical( CDC* pDC, CRect rect, CStringW sText )
{
	CSize szText = pDC->GetTextExtent( CString("A") ) ;
	for(int i=0; i< sText.GetLength(); i++)
	{
		rect.bottom = rect.top+szText.cy;

		WCHAR ch = sText.GetAt(i);
		::DrawTextW(pDC->m_hDC, &ch, 1, &rect, DT_SINGLELINE|DT_VCENTER|DT_CENTER);

        rect.top += szText.cy;
	}
}

void CTabCtrlEx::DrawItemBorder(LPDRAWITEMSTRUCT lpdis)
{
	BOOL bSelected = (lpdis->itemID == (UINT)GetCurSel());

	CRect rcItem(lpdis->rcItem);
	CDC* pDC = CDC::FromHandle(lpdis->hDC);

	DWORD dwStyle = GetStyle();

	// 버튼 스타일일 경우
	if( dwStyle & TCS_BUTTONS )
	{
		Graphics graphics(pDC->m_hDC);

		Rect rc(rcItem.left, rcItem.top, rcItem.Width(), rcItem.Height());

		// 세로 탭
		if( dwStyle&TCS_VERTICAL )
			rc.Width += 2;
		// 가로 탭
		else
			rc.Height += 2;

		Pen penBorder( ToColor(m_crBorder) );
		Pen penHighlight( Color(249,249,249) );
		Pen pen3D1( Color(239,239,239) );
		Pen pen3D2( Color(205,205,205) );
		Color crGradient1(233,233,233);
		Color crGradient2(208,208,208);

		if( bSelected )
		{
			penHighlight.SetColor( Color(115,210,246) );
			pen3D1.SetColor( Color(59,189,231) );
			pen3D2.SetColor( Color(27,134,170) );
			crGradient1.SetValue( Color::MakeARGB(255,33,184,226) );
			crGradient2.SetValue( Color::MakeARGB(255,30,142,179) );
		}

		// 테두리 그리기
		graphics.DrawRectangle(&penBorder, rc);

		// Highlight 효과 주기
		rc.Inflate(-1,-1);
		graphics.DrawLine(&penHighlight, rc.GetLeft(), rc.GetTop(), rc.GetRight(), rc.GetTop());

		// 3D 효과 주기
		rc.Y+=1;
		rc.Height-=1;
		graphics.DrawLine(&pen3D1, rc.GetLeft(), rc.GetBottom(), rc.GetLeft(), rc.GetTop());
		graphics.DrawLine(&pen3D1, rc.GetLeft(), rc.GetTop(), rc.GetRight(), rc.GetTop());
		graphics.DrawLine(&pen3D2, rc.GetRight(), rc.GetTop(), rc.GetRight(), rc.GetBottom());
		graphics.DrawLine(&pen3D2, rc.GetRight(), rc.GetBottom(), rc.GetLeft()+1, rc.GetBottom());

		// GradientBrush 로 채우기
		LinearGradientBrush linearBrush(rc, crGradient1, crGradient2, LinearGradientModeVertical);
		graphics.FillRectangle(&linearBrush, rc.X+1, rc.Y+1, rc.Width-1, rc.Height-1);
	}
	// 버튼 스타일이 아닐 경우
	else
	{
		CPen *poldPen, pen;
		if( bSelected )
			pen.CreatePen(PS_SOLID, 1, Global.Darker(m_crBorder, 0.5f) );
		else
			pen.CreatePen(PS_SOLID, 1, m_crBorder);

		poldPen = pDC->SelectObject(&pen);

		// 세로 탭
		if( dwStyle&TCS_VERTICAL )
		{
			if(bSelected)
				pDC->FillSolidRect(CRect(rcItem.left+1, rcItem.top+1, rcItem.right+1, rcItem.bottom), m_crBackIn);
			else
				pDC->FillSolidRect(CRect(rcItem.left+1, rcItem.top+1, rcItem.right, rcItem.bottom), m_crBackIn);
		
			pDC->MoveTo(rcItem.right, rcItem.top);
			pDC->LineTo(rcItem.left+2, rcItem.top);
			pDC->LineTo(rcItem.left, rcItem.top+2);
			pDC->LineTo(rcItem.left, rcItem.bottom-2);
			pDC->LineTo(rcItem.left+2, rcItem.bottom);
			pDC->LineTo(rcItem.right+1, rcItem.bottom);
		}
		// 가로 탭
		else
		{
			if(bSelected)
				pDC->FillSolidRect(CRect(rcItem.left+1, rcItem.top+1, rcItem.right, rcItem.bottom+1), m_crBackIn);
			else
				pDC->FillSolidRect(CRect(rcItem.left+1, rcItem.top+1, rcItem.right, rcItem.bottom), m_crBackIn);
			
			pDC->MoveTo(rcItem.left, rcItem.bottom);
			pDC->LineTo(rcItem.left, rcItem.top+2);
			pDC->LineTo(rcItem.left+2, rcItem.top);
			pDC->LineTo(rcItem.right-2, rcItem.top);
			pDC->LineTo(rcItem.right, rcItem.top+2);
			pDC->LineTo(rcItem.right, rcItem.bottom+1);
		}

		pDC->SelectObject(poldPen);
		pen.DeleteObject();
	}
}

void CTabCtrlEx::DrawMainBorder(LPDRAWITEMSTRUCT lpdis)
{
	if( GetStyle() & TCS_BUTTONS )
		return; 

	CRect rc(lpdis->rcItem);
	CDC* pDC = CDC::FromHandle(lpdis->hDC);
	pDC->FillSolidRect(&rc, m_crBackIn);

	COLORREF crHighlight = Global.Lighter(m_crBorder, 1.4f);
	COLORREF crShadow = Global.Darker(m_crBorder, 0.5f);

	if( m_b3dBorder )
	{
		pDC->Draw3dRect(rc, m_crBorder, crShadow);

		rc.DeflateRect(1,1);
		pDC->Draw3dRect(rc, crHighlight, m_crBorder);

		rc.DeflateRect(1,1);
		pDC->Draw3dRect(rc, m_crBorder, m_crBorder);

		rc.DeflateRect(1,1);
		pDC->Draw3dRect(rc, m_crBorder, crHighlight);

		rc.DeflateRect(1,1);
		pDC->Draw3dRect(rc, crShadow, m_crBorder);
	}
	else
	{
		pDC->Draw3dRect(rc, m_crBorder, m_crBorder);
	}
}

void CTabCtrlEx::PreSubclassWindow() 
{
	ModifyStyle(0, TCS_OWNERDRAWFIXED/*, SWP_FRAMECHANGED*/);

	CTabCtrl::PreSubclassWindow();
}

BOOL CTabCtrlEx::OnEraseBkgnd(CDC* pDC) 
{
	return TRUE;
//	return CTabCtrl::OnEraseBkgnd(pDC);
}

void CTabCtrlEx::OnPaint() 
{
	CPaintDC dc(this); // device context for painting

	MemDC MemDC(&dc);

	DWORD dwStyle = GetStyle();

	// 배경 그리기
	CRect rc;
	GetWindowRect(&rc);
	ScreenToClient(&rc);
	MemDC.FillSolidRect(rc, m_crBackOut);

	// DC 획득
	DRAWITEMSTRUCT dis;
	dis.CtlType = ODT_TAB;
	dis.CtlID = GetDlgCtrlID();
	dis.hwndItem = GetSafeHwnd();
	dis.hDC = MemDC.GetSafeHdc();
	dis.itemAction = ODA_DRAWENTIRE;

	// 전체 보더 그리기
	CRect rClient, rPage;
	GetClientRect(&dis.rcItem);
	rPage = dis.rcItem;
	AdjustRect(FALSE, rPage);	
	
	// 세로탭
	if( dwStyle&TCS_VERTICAL )	
		dis.rcItem.left = rPage.left - 6;
	// 가로탭
	else						
		dis.rcItem.top = rPage.top - 6;

	DrawMainBorder(&dis);

	int nCnt = GetItemCount();
	int nSel = GetCurSel();

	if( !nCnt )
		return;

	// 선택안된 아이템들의 보더와 텍스트 그리기
	for( int i=nCnt-1; i>=0; i-- )
	{
		if( i != nSel )
		{
			dis.itemID = i;
			dis.itemState = 0;

			VERIFY(GetItemRect(i, &dis.rcItem));

			// 세로탭
			if( dwStyle&TCS_VERTICAL )
			{
				if( dis.rcItem.top < 0 )
				{
					continue;
				}
				else if( dis.rcItem.top<10 )
				{
					dis.rcItem.top = 10;
				}
				dis.rcItem.right -= 5;
			}
			// 가로탭
			else
			{
				if( dis.rcItem.left < 0 )
				{
					continue;
				}
				else if( dis.rcItem.left<10 )
				{
					dis.rcItem.left = 10;
				}
				dis.rcItem.bottom -= 5;
			}

			DrawItemBorder(&dis);
			DrawItem(&dis);			
		}		
	}

	// 선택된 아이템의 보더와 텍스트 그리기
	dis.itemID = nSel;
	dis.itemState = ODS_SELECTED;

	VERIFY(GetItemRect(nSel, &dis.rcItem));

	// 세로탭
	if( dwStyle&TCS_VERTICAL )
	{
		if( dis.rcItem.top < 0 )
		{
			return;
		}
		else if( dis.rcItem.top<10 )
		{
			dis.rcItem.top = 10;
		}

		if( !m_b3dBorder )
			dis.rcItem.right -= 4;

		if( !(dwStyle&TCS_BUTTONS) )
			dis.rcItem.left -= 2;
	}
	// 가로탭
	else
	{
		if( dis.rcItem.left < 0 )
		{
			return;
		}
		else if( dis.rcItem.left<10 )
		{
			dis.rcItem.left = 10;
		}

		if( !m_b3dBorder )
			dis.rcItem.bottom -= 4;

		if( !(dwStyle&TCS_BUTTONS) )
			dis.rcItem.top -= 2;
	}

	DrawItemBorder(&dis);
	DrawItem(&dis);
}

void CTabCtrlEx::SetItemImage(int nItem, int nImage)
{
	TC_ITEM tci;
	tci.mask = TCIF_IMAGE;
	GetItem(nItem, &tci);

	if( tci.iImage == nImage ) 
		return;

	tci.iImage = nImage;
	SetItem(nItem, &tci);
}