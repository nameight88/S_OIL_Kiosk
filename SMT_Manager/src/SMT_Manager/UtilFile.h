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

#pragma once


#include <afx.h>


class CUtilFile
{
private:
	enum Checked
	{
		CREATE_LOG = 0x00000001,
	};


public:
	// 생성자
	CUtilFile(void);

	// 소멸자
	~CUtilFile(void);

	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.
	static CString LogData(CString strPart,CString strFmt, ...);
	
	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.(strPart_Date_strName.txt)
	static CString LogDataGroup(CString strPart,CString strName,CString strFmt, ...);
	
	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.월단위
	static CString LogDataMonth(CString strPart,CString strFmt, ...);
	
	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.월단위(strPart_Month_strName.txt)
	static CString LogDataGroupMonth(CString strPart,CString strName,CString strFmt, ...);
	
	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.일단위
	static CString LogDataDay(CString strPart,CString strFmt, ...);
	
	// "../Log/"위치에 파일 로그를 남기고 그 문자열을 출력합니다.일단위(strPart_Day_strName.txt)
	static CString LogDataGroupDay(CString strPart,CString strName,CString strFmt, ...);
	
	// 파일에 문자열을 추가합니다.
	static void WriteText(CString strPath,CString strFmt, ...);

	// 바이트 값을 Hex값으로 변환합니다.
	static CString ConvertBytetoHex(BYTE* pData, int size);

	// 바이트 값을 Hex값으로 변환합니다.단 뒤로부터 변환합니다.(Low 포멧인 경우 이용)
	static CString ReversConvertBytetoHex(BYTE* pData, int size);

	// 파일경로에서 파일명을(확장자 포함) 알아옵니다.
	static CString GetFileName(CString strFilePath);
	
	// char 문자열의 찾기 연산자. 파일로부터 읽어온 문자열의 분석에 사용
	static int FindString(TCHAR* szRead, const TCHAR* szFind);

	// char 문자열의 뒤로부터 찾기 연산자. 파일로부터 읽어온 문자열의 분석에 사용
	static int ReverseFindString(TCHAR* szRead, const TCHAR* szFind);

	// 경로내의 파일들을 모두 알아옵니다.
	static int FindFilePath(CString strRootPath, CString strFindKey, CStringArray& aryFile);

	// 시스템 폴더 경로들을 알아옵니다.(SHGetSpecialFolderPath 사용)
	static CString GetSpecialFolderPath(int csidl);

	// 프로그램의 루트 폴더 절대경로를 알아옵니다.(마지막 \ 없음)
	static CString GetModulePath();

private:

	// 최초 한번 체크하는 사항들. 내부에서만 사용.
	static int m_nChecked;

};
