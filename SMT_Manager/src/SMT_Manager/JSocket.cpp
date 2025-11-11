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


#include "stdafx.h"
#include "JSocket.h"
#include <afxpriv.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CJSocket::CJSocket(BOOL bAutoClose)
		:m_Socket(INVALID_SOCKET)
		,m_bLocalhostOnly(FALSE)
		,m_hAsyncWnd(NULL)
{
}

CJSocket::~CJSocket()
{
}

BOOL CJSocket::Create( int nSocketType )
{
	if( (m_Socket = ::socket(PF_INET, nSocketType, IPPROTO_TCP)) == INVALID_SOCKET )
		return FALSE;
	
	// Disable Nagle's algorithm
	int one = 1;
	if( ::setsockopt(m_Socket, IPPROTO_TCP, TCP_NODELAY,
		(char*)&one, sizeof(one)) == SOCKET_ERROR ) {
		DWORD dwError = ::WSAGetLastError();
		
		Close();
		return FALSE;
	}
	
	return TRUE;
};

BOOL CJSocket::Connect( LPCTSTR host, UINT port )
{	
	struct sockaddr_in addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
#ifdef _UNICODE
	//char hostaddr[64] = {0};
	USES_CONVERSION;
	addr.sin_addr.s_addr = ::inet_addr(W2A(host));
	if( (int)addr.sin_addr.s_addr == INADDR_NONE ) 
	{
		struct hostent* hostinfo;
		hostinfo = ::gethostbyname(W2A(host));
		if( hostinfo && hostinfo->h_addr ) {
			addr.sin_addr.s_addr = ((struct in_addr*)hostinfo->h_addr)->s_addr;
		} else {
			WSASetLastError(WSAEINVAL);
			return FALSE;
		}
	}
#else
	addr.sin_addr.s_addr = ::inet_addr(host);
	if( (int)addr.sin_addr.s_addr == INADDR_NONE ) 
	{
		struct hostent* hostinfo;
		hostinfo = ::gethostbyname(host);
		if( hostinfo && hostinfo->h_addr ) {
			addr.sin_addr.s_addr = ((struct in_addr*)hostinfo->h_addr)->s_addr;
		} else {
			WSASetLastError(WSAEINVAL);
			return FALSE;
		}
	}
#endif
	addr.sin_port = ::htons(port);
	
	if( m_Socket == INVALID_SOCKET )
		if( !Create() ) return FALSE;
		
	return ( ::connect(m_Socket, (struct sockaddr*)&addr, sizeof(addr)) != SOCKET_ERROR );
};

BOOL CJSocket::Shutdown(int how ) 
{
	if( m_Socket == INVALID_SOCKET ) return TRUE;
	
	if( ::shutdown(m_Socket, how) != SOCKET_ERROR )
	{
		return FALSE;
	}
	return TRUE;
};

void CJSocket::Close(void) 
{
	if( m_Socket != INVALID_SOCKET ) {
		if( m_hAsyncWnd )
			AsyncSelect(m_hAsyncWnd, NULL, 0);
		::closesocket(m_Socket);
	}
	m_Socket = INVALID_SOCKET;
};

BOOL CJSocket::IsSocket(SOCKET sock)
{
	struct sockaddr_in info;
	if( sock == INVALID_SOCKET ) sock = m_Socket;
	if( sock == INVALID_SOCKET ) return TRUE;
	int info_size = sizeof(info);
	return ::getsockname(sock, (struct sockaddr*)&info, &info_size) >= 0;
};

BOOL CJSocket::IsConnected(SOCKET sock) 
{
	struct sockaddr_in info;
	if( sock == INVALID_SOCKET ) sock = m_Socket;
	if( sock == INVALID_SOCKET ) return FALSE;
	int info_size = sizeof(info);
	return ::getpeername(sock, (struct sockaddr*)&info, &info_size) >= 0;
};

int CJSocket::GetSockPort(SOCKET sock) 
{
	struct sockaddr_in info;
	if( sock == INVALID_SOCKET ) sock = m_Socket;
	if( sock == INVALID_SOCKET ) return FALSE;
	int info_size = sizeof(info);
	if( ::getsockname(sock, (struct sockaddr*)&info, &info_size) < 0 )
		return 0;
	return ::ntohs(info.sin_port);
};

SOCKET CJSocket::GetSock(void) 
{
	return m_Socket;
};

SOCKET CJSocket::Detach(void)
{
	if( m_Socket != INVALID_SOCKET ) 
	{
		if( m_hAsyncWnd )
			AsyncSelect(m_hAsyncWnd, NULL, 0);
	}
	
	SOCKET ret_sock = m_Socket;			
	m_Socket = INVALID_SOCKET;
	return ret_sock;
};

void CJSocket::Attach(SOCKET new_sock)
{
	m_Socket = new_sock;
};

BOOL CJSocket::Listen(int port, BOOL bLocalhostOnly , LPCTSTR szIPAddress  ) 
{
	if( m_Socket == INVALID_SOCKET )
		if( !Create() ) return FALSE;
		
	m_bLocalhostOnly = bLocalhostOnly;
	int one = 1;
	if( setsockopt(m_Socket, SOL_SOCKET, SO_REUSEADDR,
			(const char*)&one, sizeof(one)) == SOCKET_ERROR ) 
	{
		Close();
	}
		
	struct sockaddr_in addr;
	::memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(port);
	if (m_bLocalhostOnly)
	{
		addr.sin_addr.s_addr = htonl(INADDR_LOOPBACK);
	}
	else
	{
		if( szIPAddress )
		{	
			BOOL bBOOL = TRUE;
			addr.sin_addr.s_addr = inet_addr((const char*) szIPAddress );
			::setsockopt( m_Socket, SOL_SOCKET, SO_REUSEADDR, (const char FAR*)&bBOOL, sizeof(bBOOL) );
		} else
		{
			addr.sin_addr.s_addr = htonl(INADDR_ANY);
		}
	}
	if( bind(m_Socket, (struct sockaddr*)&addr, sizeof(addr)) == SOCKET_ERROR ) 
	{
		Close();
		return FALSE;
	};
		
	if (listen(m_Socket, 5) == SOCKET_ERROR) 
	{
		Close();
		return FALSE;
	}
	return TRUE;
};

BOOL CJSocket::GetHostIP( LPTSTR hostip )
{
	char stTemp[255];
	IN_ADDR inAddr;
	hostent* pHostent;
	
	if ( gethostname( stTemp, 255 ) == SOCKET_ERROR ) 
		return FALSE;
	
	pHostent = gethostbyname( stTemp );
	if ( !pHostent ) return FALSE;
	
	memcpy( &inAddr, pHostent->h_addr, 4 );
	
	wsprintf( hostip, _T("%d.%d.%d.%d"), 
		inAddr.S_un.S_un_b.s_b1,
		inAddr.S_un.S_un_b.s_b2,
		inAddr.S_un.S_un_b.s_b3,
		inAddr.S_un.S_un_b.s_b4 );
	return TRUE;
};

SOCKET CJSocket::Accept(void) 
{
	SOCKET new_sock = INVALID_SOCKET;
	if( (new_sock = ::accept( m_Socket, 0, 0 )) == INVALID_SOCKET ) 
	{
		return INVALID_SOCKET;
	}
	// Disable Nagle's algorithm
	int one = 1;
	if( ::setsockopt(new_sock, IPPROTO_TCP, TCP_NODELAY,
		(char*)&one, sizeof(one)) == SOCKET_ERROR ) 
	{
		DWORD dwError = ::WSAGetLastError();
		
		closesocket(new_sock);
		return INVALID_SOCKET;
	}
	
	return new_sock;
};

BOOL CJSocket::SetSockOpt( int nOptionName, const void* lpOptionValue, int nOptionLen, int nLevel )
{
	if( ::setsockopt( m_Socket, nLevel, nOptionName,
		(char*)lpOptionValue, nOptionLen) == SOCKET_ERROR ) 
	{
		return FALSE;
	}
	return TRUE;
};

BOOL CJSocket::AsyncSelect( HWND hWnd,  unsigned int wMsg, long lEvent )	
{
	if( m_hAsyncWnd && m_hAsyncWnd != hWnd && lEvent != NULL && wMsg != NULL)
		::WSAAsyncSelect(m_Socket, m_hAsyncWnd, 0, 0);
	if( lEvent != 0 )
		m_hAsyncWnd = hWnd;
	else 
		m_hAsyncWnd = NULL;
	return (::WSAAsyncSelect(m_Socket, hWnd, wMsg, lEvent) != SOCKET_ERROR);
};

BOOL CJSocket::Select( BOOL bReceive ) 
{
	fd_set readfds;
	fd_set writefds;
	fd_set exceptfds;
	timeval timeout;
	
	int nResult;
	FD_ZERO( &readfds );
	FD_ZERO( &writefds );
	FD_ZERO( &exceptfds );

	timeout.tv_sec  = 2;
	timeout.tv_usec = 0;
	
//	while( TRUE ) {
		if( bReceive )
			FD_SET( m_Socket, &readfds );
		else
			FD_SET( m_Socket, &writefds );
		FD_SET( m_Socket, &exceptfds );
		nResult = select( m_Socket + 1, &readfds, &writefds, &exceptfds, &timeout );
		if( nResult == SOCKET_ERROR ) return FALSE;
		
		if( FD_ISSET( m_Socket, &exceptfds ) ) return FALSE;
		if( FD_ISSET( m_Socket, &readfds ) )   return TRUE;
		if( FD_ISSET( m_Socket, &writefds ) )  return TRUE;
//	}
	return FALSE;
};

int CJSocket::Read( LPVOID lpBuffer, UINT nSize, int nFlags ) 
{
	return recv( m_Socket, (char*)lpBuffer, nSize, nFlags );
};

BOOL CJSocket::ReadExact( LPVOID lpBuffer, UINT nSize ) 
{
	LPBYTE lpReceiveBuffer = (LPBYTE)lpBuffer;
	UINT nRecevieSize = nSize;
	int nResult;
	
	while( nRecevieSize ) 
	{
		nResult = Read(lpReceiveBuffer, nRecevieSize);
		if((int)nRecevieSize == nResult ) break;
		
		if( nResult == 0 )
			return FALSE;
		
		if( nResult == SOCKET_ERROR ) 
		{
			return FALSE;

			//if(::WSAGetLastError() != WSAEWOULDBLOCK)
			//	return FALSE;
			//if(::WSAGetLastError() != WSAEINTR )
			//	return FALSE;
			//if( Select( TRUE ) == FALSE ) return FALSE;
			//continue;
		}
		lpReceiveBuffer += nResult;
		nRecevieSize -= nResult;
	}
	return TRUE;
};

int CJSocket::Send(LPVOID lpBuffer, UINT nSize, int nFlags ) 
{
	return send(m_Socket, (const char*)lpBuffer, nSize, nFlags);
};

BOOL CJSocket::SendExact(LPVOID lpBuffer, UINT nSize)	
{
	LPBYTE pSendBuffer = (LPBYTE)lpBuffer;
	UINT nSendSize = nSize;
	int nResult;
	
	while( nSendSize ) 
	{
		nResult = Send( pSendBuffer, nSendSize);
		if( nResult == (int)nSendSize ) break;
		
		if( nResult == SOCKET_ERROR ) 
		{
			
			if( WSAGetLastError() != WSAEWOULDBLOCK ) 
				return FALSE;
			if(::WSAGetLastError() != WSAEINTR )
				//if( Select( FALSE ) == FALSE ) return FALSE;
				continue;
		}
		pSendBuffer += nResult;
		nSendSize -= nResult;
	}			
	return TRUE;
};

BOOL CJSocket::SendExactBlock(LPVOID lpBuffer, UINT nSize)	
{
	LPBYTE pSendBuffer = (LPBYTE)lpBuffer;
	UINT nSendSize = nSize;
	while( nSendSize ) 
	{
		if( !SendExact( pSendBuffer, min(nSendSize,8192)) ) 
			return FALSE;
		pSendBuffer += min(nSendSize,8192);
		nSendSize -= min(nSendSize,8192);
	}			
	return TRUE;
};

void CJSocket::ReadString(char* buf, int length) 
{
	if (length > 0)
		ReadExact(buf, length);
	buf[length] = '\0';
	//Read a %d-byte string: length;
};

BOOL CJSocket::SetTimeout( int millisecs ) 
{
	int timeout=millisecs;
	if (setsockopt(m_Socket, SOL_SOCKET, SO_RCVTIMEO, (char*)&timeout, sizeof(timeout)) == SOCKET_ERROR)
		return FALSE;
	if (setsockopt(m_Socket, SOL_SOCKET, SO_SNDTIMEO, (char*)&timeout, sizeof(timeout)) == SOCKET_ERROR)
		return FALSE;
	return TRUE;
};

CString CJSocket::GetPeerName(void) 
{
	struct sockaddr_in	sockinfo;
	struct in_addr		address;
	int					sockinfosize = sizeof(sockinfo);
	CString				szName = _T("");
	
	::getpeername(m_Socket, (struct sockaddr*)&sockinfo, &sockinfosize);
	memcpy(&address, &sockinfo.sin_addr, sizeof(address));
	
	szName = inet_ntoa(address);
	if(szName.IsEmpty())
		return _T("<unavailable>");
	else
		return szName;
};

int CJSocket::GetPeerPort(void) 
{
	struct sockaddr_in	sockinfo;
	int					sockinfosize = sizeof(sockinfo);
	
	::getpeername(m_Socket, (struct sockaddr*)&sockinfo, &sockinfosize);
	
	return htons(sockinfo.sin_port);
};

BOOL CJSocket::GetSockName(SOCKADDR* lpSockAddr, int* lpSockAddrLen)
{
	return (::getsockname( m_Socket, lpSockAddr, lpSockAddrLen ) != SOCKET_ERROR );
}

CString CJSocket::GetSockName(void) 
{
	struct sockaddr_in	sockinfo;
	struct in_addr		address;
	int					sockinfosize = sizeof(sockinfo);
	CString				szName = _T("");
	
	GetSockName((struct sockaddr*)&sockinfo, &sockinfosize);
	memcpy(&address, &sockinfo.sin_addr, sizeof(address));
	
	szName = inet_ntoa(address);
	if (szName.IsEmpty())
		return _T("<unavailable>");
	else
		return szName;
};
