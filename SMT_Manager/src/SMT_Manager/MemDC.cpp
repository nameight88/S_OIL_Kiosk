#include "stdafx.h"
#include "MemDC.h"

MemDC::MemDC(CDC* pDC) : CDC()
{
    ASSERT(pDC != NULL);

    m_pDC = pDC;
    m_bMemDC = !pDC->IsPrinting();

    if (m_bMemDC)
    {
        pDC->GetClipBox(&m_rect);
        CreateCompatibleDC(pDC);
        m_bitmap.CreateCompatibleBitmap(pDC, m_rect.Width(), m_rect.Height());
        m_oldBitmap = SelectObject(&m_bitmap);
        SetWindowOrg(m_rect.left, m_rect.top);
    }
    else
    {
        m_bPrinting = pDC->m_bPrinting;
        m_hDC = pDC->m_hDC;
        m_hAttribDC = pDC->m_hAttribDC;
    }
}

MemDC::~MemDC()
{
    if (m_bMemDC)
    {
        m_pDC->BitBlt(m_rect.left, m_rect.top, m_rect.Width(), m_rect.Height(),
            this, m_rect.left, m_rect.top, SRCCOPY);

        SelectObject(m_oldBitmap);
    }
    else
    {
        m_hDC = m_hAttribDC = NULL;
    }
}
