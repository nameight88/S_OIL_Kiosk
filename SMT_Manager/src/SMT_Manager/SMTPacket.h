///////////////////////////////////////////////////////////////////////////////////
//
// 제작일 : 2013.02.13
// 제작자 : 고성준
// 설  명 : 기반 페킷 클래스. 모든 페킷은 이 클래스를 상속받아 구현하되 연산자만을 
//          추가하여 구현하여 다양한 페킷들의 잘못된 접근에 대한 치명적 오류를 사전에
//          차단한다.
// 
// 수정일 : 
// 수정자 : 
// 설  명 : 
//
///////////////////////////////////////////////////////////////////////////////////


#pragma once



#define SIZE_NFC_ID		30
#define SIZE_PHONE		25

#define SIZE_AREA_CODE	30
#define SIZE_USER_ID	50
#define SIZE_USER_PW	25
#define SIZE_AREA_PW	50

#define SIZE_SMS_STRING	50
#define SIZE_SMS_PHONE	30
#define SIZE_SMS_MSG	90
#define SIZE_MMS_MSG	512


class CSMTPacket
{

public:

	// 전송 코드
	enum Syntax
	{
		CODE_STX = 0xFF,
		CODE_ENQ = 0x05,
		CODE_ACK = 0x06,
		CODE_NAK = 0x15,
		CODE_CMD_LOGIN = 0x10,
		CODE_CMD_CONTINUE = 0x11,
		CODE_CMD_OPEN_ID = 0x30,
		CODE_CMD_OPEN_PHONE = 0x31,
		CODE_CMD_OPEN_NFC = 0x32,

		CODE_CMD_SMS = 0x33,
		CODE_CMD_MMS = 0x34,

		CODE_SPACE = 0x20,
		CODE_ETX = 0xFF,
	};

	// 응답 코드
	enum ResultCode
	{
		CODE_SUCCESS = 0x00, //ACK응답, 박스 열기 성공
		CODE_ERR_OPEN = 0x01, //NAK응답, 박스 열기 실패
		CODE_ERR_BOXNO = 0x02, //NAK응답, 요청ID가 다른 박스 사용자임
		CODE_ERR_COST = 0x04, //NAK응답, 요청ID에 부과된 비용이 있음. 
		CODE_ERR_NET = 0x40, //NAK응답, 제어부 연결 끊김.
		CODE_ERR_SYS = 0x80, //NAK응답, 시스템 정비 중
		CODE_ERR_LOGIN = 0x10, //NAK응답, 박스 열기 실패
	};

	// Login Packet
	class CSMTPacketLogin
	{
	public:
		CSMTPacketLogin(CSMTPacket& packet);
		~CSMTPacketLogin(){}

		int GetTryNum(){return m_nTryNum;}
		CString GetAreaCode(){return m_strAreaCode;}
		CString GetPassword(){return m_strPassword;}

	private:
		int		m_nTryNum;
		CString m_strAreaCode;
		CString m_strPassword;
	};

	// ID/PW Packet
	class CSMTPacketUser
	{
	public:
		CSMTPacketUser(CSMTPacket& packet);
		~CSMTPacketUser(){}

		int GetTryNum(){return m_nTryNum;}
		CString GetAreaCode(){return m_strAreaCode;}
		int GetBoxNo(){return m_nBoxNo;}
		CString GetUserId(){return m_strUserId;}
		CString GetPassword(){return m_strPassword;}

	private:
		int		m_nTryNum;
		CString m_strAreaCode;
		int		m_nBoxNo;
		CString m_strUserId;
		CString m_strPassword;
	};
	
	// Phone Packet
	class CSMTPacketPhone
	{
	public:
		CSMTPacketPhone(CSMTPacket& packet);
		~CSMTPacketPhone(){}

		int GetTryNum(){return m_nTryNum;}
		CString GetAreaCode(){return m_strAreaCode;}
		int GetBoxNo(){return m_nBoxNo;}
		CString GetPhone(){return m_strPhone;}

	private:
		int		m_nTryNum;
		CString m_strAreaCode;
		int		m_nBoxNo;
		CString m_strPhone;
	};

	// NFC Packet
	class CSMTPacketNFC
	{
	public:
		CSMTPacketNFC(CSMTPacket& packet);
		~CSMTPacketNFC(){}

		int GetTryNum(){return m_nTryNum;}
		CString GetNfcId(){return m_strNfcID;}
		CString GetPhone(){return m_strPhone;}

	private:
		int		m_nTryNum;
		CString m_strNfcID;
		CString m_strPhone;
	};

	// SMS Packet
	class CSMTPacketSMS
	{
	public:
		CSMTPacketSMS(CSMTPacket& packet);
		~CSMTPacketSMS(){}

		CString GetUserID(){return m_strUserID;}
		CString GetAreaCode(){return m_strAreaCode;}
		CString GetAreaName(){return m_strAreaName;}
		CString GetSendCompany(){return m_strSendCompany;}
		int GetBoxNo(){return m_nBoxNo;}
		CString GetSendPhone(){return m_strSendPhone;}
		CString GetRecvPhone(){return m_strRecvPhone;}
		CString GetSendPhone(int nMaxLen){if(m_strSendPhone.GetLength() > nMaxLen)return m_strSendPhone.Left(nMaxLen);else return m_strSendPhone;}
		CString GetRecvPhone(int nMaxLen){if(m_strRecvPhone.GetLength() > nMaxLen)return m_strRecvPhone.Left(nMaxLen);else return m_strRecvPhone;}
		CString GetSMSMsg(){return m_strSMSMsg;}
		
		void SetSendPhone(CString strSendPhone){ m_strSendPhone = strSendPhone;}
		void SetRecvPhone(CString strRecvPhone){ m_strRecvPhone = strRecvPhone;}
		void SetSMSMsg(CString strSMSMsg){ m_strSMSMsg = strSMSMsg;}

	private:
		CString m_strUserID;
		CString m_strAreaCode;
		CString m_strAreaName;
		CString m_strSendCompany;
		int		m_nBoxNo;
		CString m_strSendPhone;
		CString m_strRecvPhone;
		CString m_strSMSMsg;
	};

public:

	// 생성자
	CSMTPacket(void);

	// 소멸자
	virtual ~CSMTPacket(void);

	// 패킷 생성
	BOOL Create(CByteArray msg);

	// 패킷 생성
	BOOL Create(BYTE* pMsg, int nSize);

	// 패킷 생성
	BOOL Create();

	// 패킷 생성
	BOOL Create(CString strAreaCode, CString strPassword);

	// 패킷 생성
	BOOL Create(int nTryNum, CString strAreaCode, int nBoxNo, CString strUserID, CString strPassword);

	// 패킷 생성
	BOOL Create(int nTryNum, CString strAreaCode, int nBoxNo, CString strPhone);

	// 패킷 생성
	BOOL Create(int nTryNum, CString strNfcID, CString strPhone);

	// 패킷 생성
	BOOL Create(CString strUserID, CString strAreaCode, CString strAreaName, int nBoxNo, CString strSendCompany, CString strSendPhone, CString strRecvPhone, CString strMsg);

	// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateAck(BYTE* pData, int nSize);

	// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateAck(CByteArray& data);

	// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateAck(CString strData);

	// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateNak(int nResultCode, BYTE* pData, int nSize);

	// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateNak(int nResultCode, CByteArray& data);

	// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
	BOOL CreateNak(int nResultCode, CString strData);

	// 정상 생성된 페킷인지 알아옵니다.(받는쪽에서 Create()가 가능하면 TRUE)
	BOOL IsPacket();

	// 요청 인지 알아옵니다.
	BOOL IsENQ();

	// 응답(성공) 인지 알아옵니다.
	BOOL IsACK();

	// 응답(실패) 인지 알아옵니다.
	BOOL IsNAK();

	// ENQ 명령 코드를 알아옵니다.
	BYTE GetCommandCode();

	// ACK/NAK 결과 코드(에러코드)를 알아옵니다.
	BYTE GetResultCode();

	// 데이터를 문자열로 알아옵니다.
	CString GetDataString(int nStart = 0);

	// 데이터를 문자열로 알아옵니다.
	CString GetDataString(int nStart, int nSize);

	// 데이터를 바이트로 알아옵니다.(리턴 기본값 0x00)
	BYTE GetDataByte(int nStart = 0);

	// 데이터를 바이트 포인트로 알아옵니다.(리턴 기본값 NULL)
	BYTE* GetDataBytePoint(int nStart = 0);

	// 데이터의 크기를 알아옵니다.
	int GetDataByteSize();

	// 페킷 바이트 배열을 알아옵니다.(리턴 기본값 NULL)
	BYTE* GetFullBytePoint();

	// 페킷 바이트 배열을 알아옵니다.
	CByteArray& GetFullByteArray();

	// 페킷 바이트 배열의 크기를 알아옵니다.
	int GetFullByteSize();


	//////////////////////////////////////////////////////////////////////////////////////
	// 보조 페킷(데이터)

	// login
	CSMTPacketLogin GetPacketLogin(){return CSMTPacketLogin(*this);}

	// open id/pw
	CSMTPacketUser GetPacketUser(){return CSMTPacketUser(*this);}

	// open phone
	CSMTPacketPhone GetPacketPhone(){return CSMTPacketPhone(*this);}

	// open nfc
	CSMTPacketNFC GetPacketNFC(){return CSMTPacketNFC(*this);}

	// SMS MSG
	CSMTPacketSMS GetPacketSMS(){return CSMTPacketSMS(*this);}


private:
	//////////////////////////////////////////////////////////////////////////////////////
	// 내부 연산자

	// 문자열 넣기
	BOOL _SetDataString(int nStart, int nSize, CString& strData);

protected:
	CByteArray m_msg;

};