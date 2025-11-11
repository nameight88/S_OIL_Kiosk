#if !defined(AFX_BASEDLG_H__5FD5D606_DD74_4278_B34D_168CF77EB71F__INCLUDED_)
#define AFX_BASEDLG_H__5FD5D606_DD74_4278_B34D_168CF77EB71F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// BaseDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CBaseDlg dialog
class CAutoLockEv
{
public:
	// Param Ev must be Manual Reset Event
	CAutoLockEv(CEvent *Ev)	{ m_pEv = Ev; }
	~CAutoLockEv()			{ UnLock();   }
	BOOL Lock()				{ return m_pEv->SetEvent(); }
	BOOL UnLock()			{ return m_pEv->ResetEvent(); }
	BOOL IsLocked()			{ return ( ::WaitForSingleObject(m_pEv->m_hObject,0) == WAIT_OBJECT_0 ); }
private:
	CEvent *m_pEv;
};

#define UM_DLG_INIT_MSG		WM_USER+100
#define TIMER_ID_DLG_SHOW	100

#define DLG_GUI_LOCK 	CAutoLockEv Lock(m_pGUIWorkEv); \
						if( Lock.IsLocked() ){ \
							return; \
						}	\
						if( !Lock.Lock() ){ \
							return;	\
						} \
						fnUpdateShowTime(); \
					 

#define DLG_GUI_LOCK_MSG 	CAutoLockEv Lock(m_pGUIWorkEv); \
						if( Lock.IsLocked() ){ \
							return 0; \
						}	\
						if( !Lock.Lock() ){ \
							return 0;	\
						} \
						fnUpdateShowTime(); \
					 
/*
typedef enum _tagPLAY_SOUND_TYPE
{
	SOUND_NONE = 0,
	SOUND_OPEN_BOX = 1,
	SOUND_BOX_UNUSEABLE,
	SOUND_REQ_CLOSE_BOX,
	SOUND_OPEN_BUTTON_DOWN,
	SOUND_SEL_RIGHT_BOX_BUTTON,
	SOUND_COMPLETE_BUTTON,
	SOUND_NOT_EXIST_FIND,
	SOUND_COMPLETE_DELIVERY,
	SOUND_BOX_OPEN_CLOSE_RETRY,
	SOUND_INPUT_RECV_USER_HPPHONE,
	SOUND_SEL_DELIVERY_COMPANY=11,
	SOUND_INPUT_DELIVERY_HPPHONE,
	SOUND_MIS_INPUT_HPPHONE,
	SOUND_INPUT_IDENT_NUM,
	SOUND_MIS_INPUT_IDENT_NUM,
	SOUND_RECALL_IDENT_NUM,
	SOUND_SEL_DOWN_BOX_BUTTON,
	SOUND_USE_ONLY_POST_REG,
	SOUND_NOT_EXIST_PORT_REG,
	SOUND_SEL_BOX_INPUT_PHONES_OPEN_DOWN=101,
	SOUND_FIND_BUTTON_RECALL_SECUNUM,
	SOUND_SELECT_SERVICE=201,
	SOUND_SEL_EMPTY_BOX,
	SOUND_SEL_DELIVERY_REGION,
	SOUND_SEL_ACCOUNT_TYPE,
	SOUND_INPUT_THING_COMPLETE_BUTTON_DOWN,
	SOUND_TMONEY_CREDIT_CARD,
	SOUND_ACCOUNT_COMPLETE,
	SOUND_CLINIC_REQ_INPUT_THING_COMPLETE_BUTTON_DOWN,
	SOUND_CLINIC_REQ_COMPLETE,
	SOUND_SEND_BACK_INPUT_THING_COMPLETE_BUTTON_DOWN=210,
	SOUND_SEND_BACK_COMPLETE,
	SOUND_DELIVERY_INPUT_THING_COMPLETE_BUTTON_DOWN,
	SOUND_DELIVERY_COMPLETE,
	SOUND_OUTPUT_THING_CLOSE_BOX,
	SOUND_CLINIC_INPUT_THING_COMPLETE_BUTTON_DOWN,
	SOUND_CLINIC_DELIVERY_COMPLETE,
	SOUND_SENDBACK_FIND_RECALL_SECUNUM,
	SOUND_POSTREG_INPUT_COMPLETE_BUTTON_DOWN,
	SOUND_POSTREG_SEL_BOX_INPUT_USER_INFO,
	SOUND_POSTREG_NOT_ITS_BOX=220,
	SOUND_POSTREG_SEL_DOWN_EMPTY_BOX,
	SOUND_BARCODE,
	SOUND_BEFORE_INPUT_HO,
	SOUND_SEL_EMPTY_BOX_INPUT_DONGHO_HPPHONE,
	SOUND_WELCOME_XI=225,
	SOUND_SEL_EMPTY_BOX_INPUT_HPPHONE,
	SOUND_INPUT_SELF_HPPHONE,
	SOUND_REQ_DELIVERY_EXIST,
	SOUND_DELIVERY_BUTTON_DOWN,
	SOUND_REQ_CLINIC_EXIST=230,
	SOUND_CLINIC_BUTTON_DOWN,
	SOUND_SEL_DOWN_CLINIC_BOX,
	SOUND_POSTREG_SEL_RIGHT_BOX,
	SOUND_DELIVERY_REQ_COMPLETE

} PLAY_SOUND_TYPE;
*/

class CBaseDlg : public CDialog
{
// Construction
	DECLARE_DYNAMIC(CBaseDlg)
public:
	void fnSetBkTransMode(BOOL bTransParent = TRUE){m_bTransParentBK = bTransParent;}
	void fnSetBkImage(CString strIMagePath,BOOL bAutoSize = TRUE,BOOL bShowWindow = TRUE);
	void fnSetSubImageNo1(CString strImagePath,int xPos,int yPos);
	void fnSetSubImageNo2(CString strImagePath,int xPos,int yPos);
	void fnSetSubImageNo3(CString strImagePath,int xPos,int yPos);
	void fnUpdateShowTime(){m_DlgShowTime = CTime::GetCurrentTime() + CTimeSpan(0,0,0,m_DlgShowTimeSec);}
	BOOL fnStartDlgAlive(BOOL bStart = TRUE,int AliveTimeSec = 30);
	void fnSetNoLogoImage(BOOL bNoLogo = TRUE){m_bNoLogoImage = bNoLogo;}
//	int  fnCallMessageDlg(CString strMsg, MESSAGETYPE type, int Align,BOOL NoYOffset=FALSE);
	int  fnDisPlayBox(CString strMsg,CString strTitle = _T(""),int BoxType = MB_OK);
	static BOOL fnCheckExistSecuNumber(CString strSecuNumber);
	static BOOL fnMakeSecuNumber(CString &strSecuNumber);
	static BOOL fnMakeSecuNumber2(CString &strSecuNumber);
	virtual LRESULT fnOnComboSelChangeMessage(WPARAM wParam,LPARAM lParam);

	void fnSetBoxOpenFlag() {m_bBoxOpenFlag = TRUE;}
	void fnSetCompleteFlag(){m_bCompleteFlag = TRUE;}
	BOOL IsBoxOpenFlag(){return m_bBoxOpenFlag;}
	BOOL IsCompleteFlag(){return m_bCompleteFlag;}

protected:
	virtual void fnDrawBKImage(CPaintDC &dc);
	virtual void fnDrawBKImage2(CPaintDC &dc);

public:
//	static void fnPlaySound(CString strSound , int key, UINT fuSound = SND_ASYNC);

public:
	CBaseDlg(UINT nIDTemplate,CWnd* pParent = NULL);   // standard constructor

protected:
	CString		m_strBGImage;
	CString		m_strSubImage1;
	CString		m_strSubImage2;
	CString		m_strSubImage3;
	CPoint		m_SubImagePt1;
	CPoint		m_SubImagePt2;
	CPoint		m_SubImagePt3;
	BOOL		m_bNoLogoImage;

	CBaseStatic m_XILogoStatic;
	CBaseStatic m_KPLogoStatic;

	CEvent		*m_pGUIWorkEv;
	CTime		m_DlgShowTime;
	int			m_DlgShowTimeSec;
	UINT		m_TimerIDShowTime;

	BOOL		m_bBoxOpenFlag;
	BOOL		m_bCompleteFlag;
	BOOL		m_bTransParentBK;

// È«µ¿¼º
//////////
public:
	static BOOL fnSendSMS(CString strMsg,CString strSendPhone,CString strRecvPhone);
//////////


// Dialog Data
	//{{AFX_DATA(CBaseDlg)
	//enum { IDD = _UNKNOWN_RESOURCE_ID_ };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CBaseDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CBaseDlg)
	afx_msg void OnPaint();
	virtual BOOL OnInitDialog();
	afx_msg LRESULT OnNcHitTest(CPoint point);
	afx_msg BOOL OnEraseBkgnd(CDC* pDC);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	virtual void OnCancel();
public:
	afx_msg void OnTimer(UINT_PTR nIDEvent);
//	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg void OnDestroy();
protected:
	virtual void OnOK();
};


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_BASEDLG_H__5FD5D606_DD74_4278_B34D_168CF77EB71F__INCLUDED_)
