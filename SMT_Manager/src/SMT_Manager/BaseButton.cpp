// BaseButton.cpp : implementation file
//

#include "stdafx.h"
#include "BaseButton.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


/////////////////////////////////////////////////////////////////////////////
// CBaseButton

CBaseButton::CBaseButton() :
	m_strDownImage(_T("")),
	m_strUpImage(_T("")),
	m_strDisImage(_T("")),
	m_strOwnUseImage(_T("")),
	m_strBGImage(_T("")),
	m_strBGText(_T("")),
	m_ButtonStatus(BUTTON_NORMAL),
	m_xPosition(0),
	m_yPosition(0),
	m_buttonWidth(0),
	m_buttonHeight(0),

	m_bNoDrawBkImage(FALSE)
{
}

CBaseButton::~CBaseButton()
{
}


BEGIN_MESSAGE_MAP(CBaseButton, CButton)
	//{{AFX_MSG_MAP(CBaseButton)
	ON_WM_CTLCOLOR_REFLECT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CBaseButton message handlers

void CBaseButton::fnSetButtonImages(CString strDownImg, 
									CString strUpImg, 
									CString strDisableImg,
									CString strOwnUseImg,
									BOOL bAutoSize)
{
	m_strDownImage	 = strDownImg;
	m_strUpImage	 = strUpImg;
	m_strDisImage	 = strDisableImg;
	m_strOwnUseImage = strOwnUseImg;
	if(bAutoSize)
	{
        BSTR str;
        str=m_strUpImage.AllocSysString();
		Bitmap BGImg(str);
		int iwd = BGImg.GetWidth();
		int iht = BGImg.GetHeight();
        SysFreeString(str);
		SetWindowPos(NULL,0,0,iwd,iht,SWP_SHOWWINDOW|SWP_NOMOVE|SWP_NOZORDER);
	}else{
		SetWindowPos(NULL,0,0,m_buttonWidth,m_buttonHeight,SWP_SHOWWINDOW|SWP_NOMOVE|SWP_NOZORDER);
	}

}

void CBaseButton::fnInitialButtonStatus(BUTTON_STATUS ButtonStatus)
{
	m_ButtonStatus = ButtonStatus;
}

void CBaseButton::DrawItem(LPDRAWITEMSTRUCT lpDrawItemStruct) 
{
	// TODO: Add your code to draw the specified item
	
	CDC * pDC = CDC::FromHandle(lpDrawItemStruct->hDC);
	CRect ClientRc = lpDrawItemStruct->rcItem;
	CDC memDC;
	BOOL    bDrawImg = FALSE;
	CString strBGImage;

	CBitmap bitmap,*OldBitmap ; //** bk Bitmap//조동환 수정

	memDC.CreateCompatibleDC(pDC);
	GetClientRect(ClientRc);

	bitmap.CreateCompatibleBitmap( pDC,ClientRc.Width(),ClientRc.Height());
	OldBitmap = (CBitmap*)memDC.SelectObject(&bitmap);

	Rect rect(ClientRc.left,ClientRc.top,ClientRc.Width(),ClientRc.Height());	
	
	BOOL bSelected = ( (lpDrawItemStruct->itemState & ODS_SELECTED) == ODS_SELECTED);
	BOOL bDisabled = ( (lpDrawItemStruct->itemState & ODS_DISABLED) == ODS_DISABLED);
	
	if(bDisabled)
	{
		if(m_ButtonStatus == BUTTON_NORMAL && !m_strUpImage.IsEmpty())
		{
			strBGImage = m_strUpImage;
			bDrawImg = TRUE;
		}
		else if(m_ButtonStatus == BUTTON_OWNUSE && !m_strOwnUseImage.IsEmpty())
		{
			strBGImage = m_strOwnUseImage;
			bDrawImg = TRUE;
		}
		else if(!m_strDisImage.IsEmpty())
		{
			strBGImage = m_strDisImage;
			bDrawImg = TRUE;
		}
	}
	else
	{
		if(bSelected )
		{
			if(m_ButtonStatus == BUTTON_NORMAL && !m_strUpImage.IsEmpty())
			{
				strBGImage = m_strDownImage;
				bDrawImg = TRUE;
			}
			else if(m_ButtonStatus == BUTTON_OWNUSE && !m_strOwnUseImage.IsEmpty())
			{
				strBGImage = m_strOwnUseImage;
				bDrawImg = TRUE;
			}
			else if(!m_strDisImage.IsEmpty())
			{
				//strBGImage = m_strDisImage;임의로 수정 disable일때 선택되게 하려고
				strBGImage = m_strDownImage;
				bDrawImg = TRUE;
			}
		}
		else
		{
			if(m_ButtonStatus == BUTTON_NORMAL && !m_strUpImage.IsEmpty())
			{
				strBGImage = m_strUpImage;
				bDrawImg = TRUE;
			}
			else if(m_ButtonStatus == BUTTON_OWNUSE && !m_strOwnUseImage.IsEmpty())
			{
				strBGImage = m_strOwnUseImage;
				bDrawImg = TRUE;
			}
			else if(!m_strDisImage.IsEmpty())
			{
				strBGImage = m_strDisImage;
				bDrawImg = TRUE;
			}
		}
	}
	if(bDrawImg)
	{
        BSTR str;
		CString strParentBG;
		memDC.BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),pDC,0,0,SRCCOPY);
		Graphics graphics(memDC); // bk graphics
        str=strBGImage.AllocSysString();
		Bitmap myBMP(str);
		SysFreeString(str);

		if(!m_bNoDrawBkImage)
		{
			BSTR str;
			CRect wrect;
			GetWindowRect(wrect);
			GetParent()->ScreenToClient(wrect);
			CString strParentBG;
			strParentBG = m_strBGImage;
			str=strParentBG.AllocSysString();
			Bitmap myBMP2(str);
			SysFreeString(str);
			Rect rect2(wrect.left,wrect.top,wrect.Width(),wrect.Height());
			
            graphics.DrawImage(&myBMP2,rect2);

		}
			graphics.DrawImage(&myBMP,rect);


		if(!m_strBGText.IsEmpty())
		{
			memDC.SetBkMode(TRANSPARENT);
			memDC.SetTextColor(RGB(0,0,0));
			CRect rc(ClientRc);
			//rc.DeflateRect(7,10);
			CFont* pOldFont = memDC.SelectObject(&m_FontText);
			memDC.DrawText( m_strBGText, &rc, DT_CENTER|DT_VCENTER);//|DT_SINGLELINE );
			memDC.SelectObject(pOldFont);
            pOldFont->DeleteObject();

		}
		graphics.ReleaseHDC(memDC.m_hDC );

		pDC->BitBlt(0,0,ClientRc.Width(),ClientRc.Height(),&memDC,0,0,SRCCOPY);
	}	
	memDC.SelectObject(OldBitmap);//**조동환
    memDC.DeleteDC();//**조동환
    bitmap.DeleteObject();//**조동환
}


void CBaseButton::fnSetButtonPosition(int xPos, int yPos)
{
	m_xPosition = xPos;
	m_yPosition = yPos;
	SetWindowPos(NULL,xPos,yPos,0,0,SWP_SHOWWINDOW|SWP_NOSIZE|SWP_NOZORDER);
}

void CBaseButton::fnSetButtonSize(int b_width, int b_height)
{
	m_buttonWidth = b_width;
	m_buttonHeight = b_height;

}

void CBaseButton::fnSetButtonStatus(BUTTON_STATUS ButtonStatus)
{
	m_ButtonStatus = ButtonStatus;
	Invalidate(FALSE);
}

void CBaseButton::fnSetDrawText(CString strText)
{
	m_strBGText = strText;
}

void CBaseButton::fnSetDrawFont(int size, int weigth, CString strFontFace)
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
		ANSI_CHARSET,							// nCharSet
		OUT_DEFAULT_PRECIS,							// nOutPrecision
		CLIP_DEFAULT_PRECIS,						// nClipPrecision
		DEFAULT_QUALITY,							// nQuality
		DEFAULT_PITCH | FF_SWISS,					// nPitchAndFamily 
		strFontFace);								// lpszFacename
}

HBRUSH CBaseButton::CtlColor(CDC* pDC, UINT nCtlColor) 
{
	// TODO: Change any attributes of the DC here
	return (HBRUSH)::GetStockObject(NULL_BRUSH); 
	
	// TODO: Return a non-NULL brush if the parent's handler should not be called
	//return NULL;
}

void CBaseButton::fnSetNoDrawImageBK(BOOL bNoDraw)
{
	m_bNoDrawBkImage = TRUE;
}

int CBaseButton::fnGetItemInt()
{
    TRACE("%d",StrToInt(m_strBGText));

	return StrToInt(m_strBGText);
}
