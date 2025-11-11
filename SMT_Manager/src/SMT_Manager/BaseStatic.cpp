// BaseStatic.cpp : implementation file
//

#include "stdafx.h"
#include "BaseStatic.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


/////////////////////////////////////////////////////////////////////////////
// CBaseStatic

CBaseStatic::CBaseStatic() : 
			m_strBGImage(_T("")),
			m_strBGText(_T("")),
			m_xPosition(0),
			m_yPosition(0),
			m_TextColor(RGB(43,37,39)),
			m_bMultiLine(FALSE),
			m_MultiLineOffset(0),
			m_TextAlign(DT_CENTER)
{
}

CBaseStatic::~CBaseStatic()
{
	m_FontText.DeleteObject();
}


BEGIN_MESSAGE_MAP(CBaseStatic, CStatic)
	//{{AFX_MSG_MAP(CBaseStatic)
	ON_WM_PAINT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CBaseStatic message handlers

void CBaseStatic::OnPaint() 
{
	CPaintDC dc(this); // device context for painting
	
	// TODO: Add your message handler code here

	
	CDC memDC; // bk DC
	CRect ClientRc;
//	CBitmap bitmap; // bk Bitmap //조동환
	GetClientRect(ClientRc);

	CBitmap bitmap,*OldBitmap ; ////조동환 *OldBitmap


	if(!m_strBGImage.IsEmpty())
	{
		memDC.CreateCompatibleDC(&dc);
		bitmap.CreateCompatibleBitmap( &dc,ClientRc.Width(),ClientRc.Height());

		OldBitmap = (CBitmap*)memDC.SelectObject(&bitmap); //조동환

		memDC.FillSolidRect(ClientRc,RGB(0,0,0));// .Rectangle(ClientRc);

		if(m_strBGImage != _T("None") )
		{
            BSTR str;		
			memDC.BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),&dc,0,0,SRCCOPY);
			Graphics graphics(memDC); // bk graphics
			Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());
			str=m_strBGImage.AllocSysString();
			Bitmap myBMP(str);
			graphics.DrawImage(&myBMP,rect);
			graphics.ReleaseHDC(memDC.m_hDC);
	        SysFreeString(str);

		}

		if(!m_strBGText.IsEmpty())
		{
			memDC.SetBkMode(TRANSPARENT);
			memDC.SetTextColor(m_TextColor);
			CRect rc(ClientRc);
			rc.DeflateRect(2,10);
			CFont* pOldFont = memDC.SelectObject(&m_FontText);
			if( m_bMultiLine )
			{
				rc.OffsetRect(0,m_MultiLineOffset);
				memDC.DrawText( m_strBGText, &rc, m_TextAlign);
			}
			else
			memDC.DrawText( m_strBGText, &rc, m_TextAlign|DT_VCENTER|DT_SINGLELINE );
			memDC.SelectObject(pOldFont);
            pOldFont->DeleteObject();
		}
		dc.BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),&memDC,0,0,SRCCOPY); // bitblt to ScreenDC From bk DC

        //조동환
		//
		memDC.SelectObject(OldBitmap);
        memDC.DeleteDC();
        bitmap.DeleteObject();
		//

	}
	else 
	{
		if(!m_strBGText.IsEmpty())
		{
			dc.SetBkMode(TRANSPARENT);
			dc.SetTextColor(m_TextColor);
			CRect rc(ClientRc);
			rc.DeflateRect(2,10);
			CFont* pOldFont = dc.SelectObject(&m_FontText);
			if( m_bMultiLine )
			{
				rc.OffsetRect(0,m_MultiLineOffset);
				dc.DrawText( m_strBGText, &rc, m_TextAlign );
			}
			else
				dc.DrawText( m_strBGText, &rc, m_TextAlign|DT_VCENTER|DT_SINGLELINE );
			
			dc.SelectObject(pOldFont);

			//조동환
			//
			ReleaseDC(&dc);
			pOldFont->DeleteObject();
			//
		}
	}


	// Do not call CStatic::OnPaint() for painting messages
}
void CBaseStatic::fnSetBkImage(CString strImagePath , BOOL bAutoSize)
{
	BSTR str;
	m_strBGImage = strImagePath;
	str=m_strBGImage.AllocSysString();
	Bitmap BGImg(str);
	int iwd = BGImg.GetWidth();
	int iht = BGImg.GetHeight();
    SysFreeString(str);
	if(bAutoSize)
	{
		//MoveWindow(0,0,iwd,iht);
		SetWindowPos(NULL,0,0,iwd,iht,SWP_SHOWWINDOW|SWP_NOMOVE);
		Invalidate(FALSE);
	}
}

void CBaseStatic::fnSetDrawText(CString strText,BOOL bReDraw)
{
	m_strBGText = strText;
	if(bReDraw)
		Invalidate(FALSE);
}

void CBaseStatic::fnSetDrawFont(int size, int weigth, CString strFontFace)
{
	m_FontText.CreateFont(
		size,										// nHeight
		0,											// nWidth
		0,											// nEscapement
		0,											// nOrientation
		weigth,										// nWeight
		0,											// bItalic
		0,											// bUnderline
		0,											// cStrikeOut
		DEFAULT_CHARSET,								// nCharSet
		OUT_DEFAULT_PRECIS,							// nOutPrecision
		CLIP_DEFAULT_PRECIS,						// nClipPrecision
		DEFAULT_QUALITY,							// nQuality
		DEFAULT_PITCH | FF_SWISS,					// nPitchAndFamily 
		strFontFace);								// lpszFacename
}

void CBaseStatic::fnSetStaticPosition(int xPos, int yPos)
{
	m_xPosition = xPos;
	m_yPosition = yPos;
	SetWindowPos(NULL,xPos,yPos,0,0,SWP_SHOWWINDOW|SWP_NOSIZE);
}

void CBaseStatic::fnSetStaticSize(int width, int height)
{
	SetWindowPos(NULL,0,0,width,height,SWP_SHOWWINDOW|SWP_NOMOVE);
}

int CBaseStatic::fnGetItemInt()
{
	return StrToInt(m_strBGText);
}