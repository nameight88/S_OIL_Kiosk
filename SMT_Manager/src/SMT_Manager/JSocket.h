///////////////////////////////////////////////////////////////////////////////////
//
// 제작일 : 2013.02.28
// 제작자 : 고성준
// 설  명 : 기반 소켓.
// 
// 수정일 : 
// 수정자 : 
// 설  명 : 
//
///////////////////////////////////////////////////////////////////////////////////


#ifndef __JSOCKET_H__
#define __JSOCKET_H__

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

// 기반소켓
class CJSocket  
{
public:
	CJSocket(BOOL bAutoClose = FALSE);
	virtual ~CJSocket();

	BOOL   Create( int nSocketType = SOCK_STREAM );
	BOOL   Shutdown(int how = SD_BOTH);
	BOOL   IsSocket(SOCKET sock = INVALID_SOCKET);
	BOOL   IsConnected(SOCKET sock = INVALID_SOCKET);
	int    GetSockPort(SOCKET sock = INVALID_SOCKET);
	void   Attach(SOCKET new_sock);
	void   Close(void);
	SOCKET Detach(void);
	SOCKET GetSock(void);
	SOCKET Accept(void);

	static BOOL GetHostIP( LPTSTR hostip );
	BOOL   Listen(int port, BOOL bLocalhostOnly = FALSE, LPCTSTR szIPAddress = NULL );
	
	BOOL   SetSockOpt( int nOptionName, const void* lpOptionValue, int nOptionLen, int nLevel = SOL_SOCKET );
	BOOL   AsyncSelect( HWND hWnd,  unsigned int wMsg, long lEvent = FD_READ | FD_WRITE | FD_OOB | FD_ACCEPT | FD_CONNECT | FD_CLOSE );
	BOOL   Select( BOOL bReceive );
	
	void    ReadString(char* buf, int length);
	BOOL    SetTimeout(int millisecs );
	int     GetPeerPort(void);
	BOOL    GetSockName(SOCKADDR* lpSockAddr, int* lpSockAddrLen);
	CString GetSockName(void);
	CString GetPeerName(void);

	virtual BOOL Connect( LPCTSTR host, UINT port);
	virtual int  Read( LPVOID lpBuffer, UINT nSize, int nFlags = 0);
	virtual int  Send( LPVOID lpBuffer, UINT nSize, int nFlags = 0);
	virtual BOOL ReadExact( LPVOID lpBuffer, UINT nSize);
	virtual BOOL SendExact( LPVOID lpBuffer, UINT nSize);
	virtual BOOL SendExactBlock( LPVOID lpBuffer, UINT nSize);
		

public:
	SOCKET m_Socket;

protected:
	BOOL m_bLocalhostOnly;
	HWND m_hAsyncWnd;
};

#endif // __JSOCKET_H__
