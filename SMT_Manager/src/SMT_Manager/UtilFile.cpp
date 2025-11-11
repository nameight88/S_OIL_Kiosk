///////////////////////////////////////////////////////////////////////////////////
//
// 제작일 : 오래됨
// 제작자 : 고성준
// 설  명 : 파일 관련 유틸 클래스
// 
// 수정일 : 
// 수정자 : 
// 설  명 : 
//
///////////////////////////////////////////////////////////////////////////////////

#include "StdAfx.h"
#include "UtilFile.h"


// 최초 한번 체크하는 사항들. 내부에서만 사용.
int CUtilFile::m_nChecked = 0;

// 생성자
CUtilFile::CUtilFile(void)
{
}

// 소멸자
CUtilFile::~CUtilFile(void)
{
}

// "../Log/"위치에 파일 로그를 남깁니다.
CString CUtilFile::LogData(CString strPart,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%04d%02d%02d.txt"), strPart, time.GetYear(), time.GetMonth(), time.GetDay());
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}

// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.(strPart_Date_strName.txt)
CString CUtilFile::LogDataGroup(CString strPart,CString strName,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%04d%02d%02d_%s.txt"), strPart, time.GetYear(), time.GetMonth(), time.GetDay(), strName);
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}


// "../Log/"위치에 파일 로그를 남깁니다.월단위
CString CUtilFile::LogDataMonth(CString strPart,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%02dm.txt"), strPart, time.GetMonth());
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}

// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.월단위(strPart_Month_strName.txt)
CString CUtilFile::LogDataGroupMonth(CString strPart,CString strName,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%02dm_%s.txt"), strPart, time.GetMonth(), strName);
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}


// "../Log/"위치에 파일 로그를 남깁니다.일단위
CString CUtilFile::LogDataDay(CString strPart,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%02d.txt"), strPart, time.GetDay());
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}

// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.일단위(strPart_Day_strName.txt)
CString CUtilFile::LogDataGroupDay(CString strPart,CString strName,CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = _T("");
	CString  strLog = _T("");
	CString  strMsg = _T("");
	CTime time = CTime::GetCurrentTime();

	if (!(CUtilFile::m_nChecked & CUtilFile::CREATE_LOG))
		::CreateDirectory("..\\Log", NULL);

	strFileName.Format(_T("../Log/%s_%02d_%s.txt"), strPart, time.GetDay(), strName);
	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if(Status.m_mtime.GetMonth() != time.GetMonth()) // Is not same, delete and create
		{
			if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
				return strLog;
#ifdef _UNICODE
			BYTE UnicodeFlag[2] = {0xFF,0xFE};
			file.Write(UnicodeFlag , 2 );
#endif
		}
		else
		{
			if( !file.Open(strFileName,CFile::modeWrite) )
				return strLog;
		}
			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return strLog;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");
	strLog = time.Format(_T("%Y-%m-%d %H:%M:%S"));
	strLog += _T("  ") + strMsg;

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strLog.GetBuffer() , strLog.GetLength() );
	strLog.ReleaseBuffer();
#endif
	file.Close();


	return strLog;
}

// 파일에 문자열을 추가합니다.
void CUtilFile::WriteText(CString strPath, CString strFmt, ...)
{
	CFileStatus Status;
	CFile	 file;
	CString  strFileName = strPath;
	CString  strMsg = _T("");

	if(CFile::GetStatus(strFileName, Status)) // File Exist
	{   
		if( !file.Open(strFileName,CFile::modeWrite) )
			return;			
	}
	else // File not found
	{
		if( !file.Open(strFileName,CFile::modeCreate | CFile::modeWrite) )
			return;
#ifdef _UNICODE
		BYTE UnicodeFlag[2] = {0xFF,0xFE};
		file.Write(UnicodeFlag , 2 );
#endif
	}

    va_list args;
    va_start(args, strFmt);
    strMsg.FormatV(strFmt, args);
    va_end(args);
	strMsg += _T("\r\n");

	file.SeekToEnd();
#ifdef _UNICODE
	file.Write(strLog.GetBuffer() , strLog.GetLength()*2 );
	strLog.ReleaseBuffer();
#else
	file.Write(strMsg.GetBuffer() , strMsg.GetLength() );
	strMsg.ReleaseBuffer();
#endif
	file.Close();
}


// 바이트 값을 Hex값으로 변환합니다.
CString CUtilFile::ConvertBytetoHex(BYTE* pData, int size)
{
	CString retval = "";
	CString strBuf;

	if (pData != NULL)
	{
		// 핵사변환
		for (int nIndex = 0; nIndex < size; nIndex++)
		{
			strBuf.Format("%02X ", pData[nIndex]);
			retval += strBuf;
		}
	}

	return retval;
}

// 바이트 값을 Hex값으로 변환합니다.단 뒤로부터 변환합니다.(little-endian인 경우 이용)
CString CUtilFile::ReversConvertBytetoHex(BYTE* pData, int size)
{
	CString retval = "";
	CString strBuf;

	// 핵사변환
	for (int nIndex = size-1; nIndex >= 0; nIndex--)
	{
		strBuf.Format("%02X", pData[nIndex]);
		retval += strBuf;
	}	

	return retval;
}

// 파일경로에서 파일명을(확장자 포함) 알아옵니다.
CString CUtilFile::GetFileName(CString strFilePath)
{
	CString retval;
	int nPos;

	
	while (strFilePath.Replace('\\', '/') > 0);

	nPos = strFilePath.ReverseFind('/') + 1;
	if (nPos >= 0)
	{
		retval = strFilePath.Mid(nPos, strFilePath.GetLength() - nPos);
	}
	else
	{
		retval = strFilePath;
	}

	return retval;
}


// char 문자열의 찾기 연산자. 파일로부터 읽어온 문자열의 분석에 사용
int CUtilFile::FindString(TCHAR* szRead, const TCHAR* szFind)
{
	int retval = -1;
	int nIndex = 0;
	int nLenFind = strlen(szFind);
	int nMaxFind = strlen(szRead) - nLenFind + 1;
	BOOL bEqual;

	
	while (retval < 0 && nIndex < nMaxFind)
	{
		bEqual = TRUE;
		for (int n = 0; n < nLenFind; n++)
		{
			if (szRead[nIndex + n] != szFind[n])
			{
				bEqual = FALSE;
			}
		}
		if (bEqual)
		{
			retval = nIndex;
		}

		nIndex++;
	}

	return retval;
}

// char 문자열의 뒤로부터 찾기 연산자. 파일로부터 읽어온 문자열의 분석에 사용
int CUtilFile::ReverseFindString(TCHAR* szRead, const TCHAR* szFind)
{
	int retval = -1;
	int nIndex = 0;
	int nLenFind = strlen(szFind);
	int nMaxFind = strlen(szRead) - strlen(szFind) + 1;
	BOOL bEqual;
	
	
	nIndex = nMaxFind;
	while (retval < 0 && nIndex >= 0)
	{
		bEqual = TRUE;
		for (int n = 0; n < nLenFind; n++)
		{
			if (szRead[nIndex + n] != szFind[n])
			{
				bEqual = FALSE;
			}
		}
		if (bEqual)
		{
			retval = nIndex;
		}

		nIndex--;
	}

	return retval;
}

// 경로내의 파일들을 모두 알아옵니다.
int CUtilFile::FindFilePath(CString strRootPath, CString strFindKey, CStringArray& aryFile)
{
	int			retval = 0;
	int			nCount = 0;
	CFileFind	finder;
	CString		strName;
	BOOL		bFind;


	if (strRootPath.GetLength() > 0 &&
		strRootPath.ReverseFind('\\') < strRootPath.GetLength() - 1 &&
		strRootPath.ReverseFind('/') < strRootPath.GetLength() - 1)
	{
		if (strRootPath.Find('\\') <= 0)
			strName.Format("%s/%s", strRootPath, strFindKey);
		else
			strName.Format("%s\\%s", strRootPath, strFindKey);
	}
	else
	{
		strName = strRootPath + strFindKey;
	}

	bFind = finder.FindFile(strName);
	nCount = 0;
	while(bFind)
	{
		bFind = finder.FindNextFile();
		
		if (!finder.IsDirectory() && !finder.IsDots())
		{
			nCount++;
		}
	}

	bFind = finder.FindFile(strName);
	retval = 0;
	aryFile.SetSize(nCount);
	while(bFind)
	{
		bFind = finder.FindNextFile();
		
		if (!finder.IsDirectory() && !finder.IsDots())
		{
			retval++;
			if (retval <= nCount)
			{
				aryFile[retval-1] = finder.GetFilePath();
			}
			else
			{
				aryFile.Add(finder.GetFilePath());
			}
		}
	}

	return retval;
}


// 시스템 폴더 경로들을 알아옵니다.(SHGetSpecialFolderPath 사용)
CString CUtilFile::GetSpecialFolderPath(int csidl)
{

	CString	retval;
	TCHAR	szPath[MAX_PATH];

	SHGetSpecialFolderPath(NULL, szPath, csidl, FALSE);
	retval = szPath;

	return retval;

}

// 프로그램의 루트 폴더 절대경로를 알아옵니다.(마지막 \ 없음)
CString CUtilFile::GetModulePath()
{
	TCHAR	szPath[MAX_PATH];
	CString strPath;
	CString retval;
	int		nFind;

	::GetModuleFileName(NULL, szPath, MAX_PATH);
	strPath = szPath;
	strPath.Replace('/','\\');
	nFind = strPath.ReverseFind('\\');

	if (nFind > 0)
	{
		retval = strPath.Left(nFind);
	}

	return retval;
}