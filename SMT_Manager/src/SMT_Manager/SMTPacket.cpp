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



#include "StdAfx.h"
#include "SMTPacket.h"

// Login 생성자
CSMTPacket::CSMTPacketLogin::CSMTPacketLogin(CSMTPacket& packet)
{
	if (packet.IsPacket() && packet.IsENQ() && packet.GetCommandCode() == CSMTPacket::CODE_CMD_LOGIN)
	{
		if (packet.GetDataByteSize()  == SIZE_AREA_CODE + SIZE_AREA_PW)
		{
			this->m_nTryNum = packet.GetDataByte();
			this->m_strAreaCode = packet.GetDataString(0, SIZE_AREA_CODE);
			this->m_strPassword = packet.GetDataString(SIZE_AREA_CODE, SIZE_AREA_PW);
		}
	}
}

// User(ID/PW) 생성자
CSMTPacket::CSMTPacketUser::CSMTPacketUser(CSMTPacket& packet)
{
	if (packet.IsPacket() && packet.IsENQ() && packet.GetCommandCode() == CSMTPacket::CODE_CMD_OPEN_ID)
	{
		if (packet.GetDataByteSize()  == 1 + SIZE_AREA_CODE + 2 + SIZE_USER_ID + SIZE_USER_PW)
		{
			this->m_nTryNum = packet.GetDataByte();
			this->m_strAreaCode = packet.GetDataString(1, SIZE_AREA_CODE);
			this->m_nBoxNo = (packet.GetDataByte(1 + SIZE_AREA_CODE) * 0x0100) + packet.GetDataByte(1 + SIZE_AREA_CODE + 1);
			this->m_strUserId = packet.GetDataString(1 + SIZE_AREA_CODE + 2, SIZE_USER_ID);
			this->m_strPassword = packet.GetDataString(1 + SIZE_AREA_CODE + 2 + SIZE_USER_ID, SIZE_USER_PW);
		}
	}
}

// Phone 생성자
CSMTPacket::CSMTPacketPhone::CSMTPacketPhone(CSMTPacket& packet)
{
	if (packet.IsPacket() && packet.IsENQ() && packet.GetCommandCode() == CSMTPacket::CODE_CMD_OPEN_PHONE)
	{
		if (packet.GetDataByteSize()  == 1 + SIZE_AREA_CODE + 2 + SIZE_PHONE)
		{
			this->m_nTryNum = packet.GetDataByte();
			this->m_strAreaCode = packet.GetDataString(1, SIZE_AREA_CODE);
			this->m_nBoxNo = (packet.GetDataByte(1 + SIZE_AREA_CODE) * 0x0100) + packet.GetDataByte(1 + SIZE_AREA_CODE + 1);
			this->m_strPhone = packet.GetDataString(1 + SIZE_AREA_CODE + 2, SIZE_PHONE);
			int nFind = this->m_strPhone.Find('0');
			if (nFind > 1)
				this->m_strPhone = "0" + this->m_strPhone.Mid(nFind-1);
		}
	}
}

// NFC 생성자
CSMTPacket::CSMTPacketNFC::CSMTPacketNFC(CSMTPacket& packet)
{
	if (packet.IsPacket() && packet.IsENQ() && packet.GetCommandCode() == CSMTPacket::CODE_CMD_OPEN_NFC)
	{
		if (packet.GetDataByteSize()  == SIZE_NFC_ID + SIZE_PHONE + 1)
		{
			this->m_nTryNum = packet.GetDataByte();
			this->m_strNfcID = packet.GetDataString(1, SIZE_NFC_ID);
			this->m_strPhone = packet.GetDataString(1 + SIZE_NFC_ID, SIZE_PHONE);
		}
	}
}

// SMS 생성자
CSMTPacket::CSMTPacketSMS::CSMTPacketSMS(CSMTPacket& packet)
{
	if (packet.IsPacket() && packet.IsENQ() && packet.GetCommandCode() == CSMTPacket::CODE_CMD_SMS)
	{
		if (packet.GetDataByteSize()  == SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+2+SIZE_SMS_PHONE+SIZE_SMS_PHONE+SIZE_SMS_MSG)
		{
			int n = 0;
			m_strUserID = packet.GetDataString(n, SIZE_SMS_STRING);					n += SIZE_SMS_STRING;
			m_strAreaCode = packet.GetDataString(n, SIZE_SMS_STRING);					n += SIZE_SMS_STRING;
			m_strAreaName = packet.GetDataString(n, SIZE_SMS_STRING);					n += SIZE_SMS_STRING;
			m_strSendCompany = packet.GetDataString(n, SIZE_SMS_STRING);				n += SIZE_SMS_STRING;
			m_nBoxNo = (packet.GetDataByte(n) * 0x0100) + packet.GetDataByte(n + 1);	n += 2;
			m_strSendPhone = packet.GetDataString(n, SIZE_SMS_PHONE);					n += SIZE_SMS_PHONE;
			m_strRecvPhone = packet.GetDataString(n, SIZE_SMS_PHONE);					n += SIZE_SMS_PHONE;
			m_strSMSMsg = packet.GetDataString(n, SIZE_SMS_MSG);
		}
	}
}

// 생성자
CSMTPacket::CSMTPacket(void)
{
}

// 소멸자
CSMTPacket::~CSMTPacket(void)
{
}

// 패킷 생성
BOOL CSMTPacket::Create(CByteArray msg)
{
	return this->Create(&msg[0], msg.GetSize());
}

// 패킷 생성
BOOL CSMTPacket::Create(BYTE* pMsg, int nSize)
{
	BOOL retval = FALSE;

	// length(STX + ACK/NAK + COD + LEN1 + LEN2 + SUM + ETX) = 7 
	if (nSize > 6) 
	{
		int startPos = -1;
		int endPos = -1;
		int len = 0;

		for (int i = 0; i < nSize-1; i++)
		{
			if (pMsg[i] == CODE_STX)
			{
				if (pMsg[i+1] == CODE_ENQ || pMsg[i+1] == CODE_ACK || pMsg[i+1] == CODE_NAK)
				{
					startPos = i;
					break;
				}
			}
		}

		if (startPos >= 0 && nSize > startPos+6)
		{

			len = (int)pMsg[startPos+3] * 0x0100 + (int)pMsg[startPos+4];
			endPos = startPos+len+6;

			if (endPos < nSize && pMsg[endPos] == CODE_ETX)
			{						
				BYTE sum = 0x00;
				int sumStart = startPos + 1;
				int sumEnd = endPos - 1;
				for (int i = sumStart; i < sumEnd; i++)
				{
					sum ^= pMsg[i];					
				}

				if (pMsg[sumEnd] == sum)
				{
					m_msg.SetSize(endPos+1);
					::memcpy(&m_msg[0], pMsg+startPos, len+7);
					retval = TRUE;
				}
			}
		}
	}

	return retval;

}

// 패킷 생성
BOOL CSMTPacket::Create()
{
	BOOL retval = TRUE;


	BYTE sum = 0x00;
	int n = 0;
	int len = 0;
	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_CONTINUE;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;


	return retval;
}

// 패킷 생성
BOOL CSMTPacket::Create(CString strAreaCode, CString strPassword)
{
	BOOL retval = TRUE;


	BYTE sum = 0x00;
	int n = 0;
	int len = SIZE_AREA_CODE+SIZE_AREA_PW;
	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_LOGIN;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	// AreaCode
	retval &= _SetDataString(n, SIZE_AREA_CODE, strAreaCode);
	for (int i = 0; i < SIZE_AREA_CODE; i++)
		sum ^= m_msg[n++];

	// Password
	retval &= _SetDataString(n, SIZE_AREA_PW, strPassword);
	for (int i = 0; i < SIZE_AREA_PW; i++)
		sum ^= m_msg[n++];

	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;


	return retval;
}

// 패킷 생성
BOOL CSMTPacket::Create(int nTryNum, CString strAreaCode, int nBoxNo, CString strUserID, CString strPassword)
{
	BOOL retval = TRUE;


	BYTE sum = 0x00;
	int n = 0;
	int len = SIZE_AREA_CODE+2+SIZE_USER_ID+SIZE_USER_PW+1;
	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_OPEN_ID;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	// TryNum
	m_msg[n] = nTryNum;
	sum ^= m_msg[n++];

	// AreaCode
	retval &= _SetDataString(n, SIZE_AREA_CODE, strAreaCode);
	for (int i = 0; i < SIZE_AREA_CODE; i++)
		sum ^= m_msg[n++];

	// BoxNo
	m_msg[n] = nBoxNo/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = nBoxNo%0x0100;
	sum ^= m_msg[n++];

	// UserID
	retval &= _SetDataString(n, SIZE_USER_ID, strUserID);
	for (int i = 0; i < SIZE_USER_ID; i++)
		sum ^= m_msg[n++];

	// Password
	retval &= _SetDataString(n, SIZE_USER_PW, strPassword);
	for (int i = 0; i < SIZE_USER_PW; i++)
		sum ^= m_msg[n++];


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;


	return retval;
}

// 패킷 생성
BOOL CSMTPacket::Create(int nTryNum, CString strAreaCode, int nBoxNo, CString strPhone)
{
	BOOL retval = TRUE;


	BYTE sum = 0x00;
	int n = 0;
	int len = SIZE_AREA_CODE+2+SIZE_PHONE+1;
	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_OPEN_PHONE;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	m_msg[n] = nTryNum;
	sum ^= m_msg[n++];

	// AreaCode
	retval &= _SetDataString(n, SIZE_AREA_CODE, strAreaCode);
	for (int i = 0; i < SIZE_AREA_CODE; i++)
		sum ^= m_msg[n++];

	// BoxNo
	m_msg[n] = nBoxNo/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = nBoxNo%0x0100;
	sum ^= m_msg[n++];

	// Phone
	retval &= _SetDataString(n, SIZE_PHONE, strPhone);
	for (int i = 0; i < SIZE_PHONE; i++)
		sum ^= m_msg[n++];


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;


	return retval;
}


// 패킷 생성
BOOL CSMTPacket::Create(int nTryNum, CString strNfcID, CString strPhone)
{
	BOOL retval = TRUE;


	BYTE sum = 0x00;
	int n = 0;
	int len = SIZE_NFC_ID+SIZE_PHONE+1;
	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_OPEN_NFC;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	m_msg[n] = nTryNum;
	sum ^= m_msg[n++];

	// NfcID
	retval &= _SetDataString(n, SIZE_NFC_ID, strNfcID);
	for (int i = 0; i < SIZE_NFC_ID; i++)
		sum ^= m_msg[n++];

	// Phone
	retval &= _SetDataString(n, SIZE_PHONE, strPhone);
	for (int i = 0; i < SIZE_PHONE; i++)
		sum ^= m_msg[n++];


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;

	// 최종 n값은 63

	return retval;
}

// 패킷 생성
BOOL CSMTPacket::Create(CString strUserID, CString strAreaCode, CString strAreaName, int nBoxNo, 
						CString strSendCompany, CString strSendPhone, CString strRecvPhone, CString strMsg)
{
	BOOL retval = TRUE;
	BOOL bMMS = FALSE;

	if (strMsg.GetLength() > 80)
	{
		bMMS = true;
	}

	BYTE sum = 0x00;
	int n = 0;
	int len = SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+2+SIZE_SMS_PHONE+SIZE_SMS_PHONE+SIZE_SMS_MSG;

	if (bMMS)
	{
		len = SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+SIZE_SMS_STRING+2+SIZE_SMS_PHONE+SIZE_SMS_PHONE+SIZE_MMS_MSG;
	}

	m_msg.SetSize(len+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ENQ;
	sum ^= m_msg[n++];

	m_msg[n] = CODE_CMD_SMS;
	if (bMMS) m_msg[n] = CODE_CMD_MMS;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = len/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = len%0x0100;
	sum ^= m_msg[n++];

	// USER ID
	retval &= _SetDataString(n, SIZE_SMS_STRING, strUserID);
	for (int i = 0; i < SIZE_SMS_STRING; i++)
		sum ^= m_msg[n++];

	// AreaCode
	retval &= _SetDataString(n, SIZE_SMS_STRING, strAreaCode);
	for (int i = 0; i < SIZE_SMS_STRING; i++)
		sum ^= m_msg[n++];

	// AreaName
	retval &= _SetDataString(n, SIZE_SMS_STRING, strAreaName);
	for (int i = 0; i < SIZE_SMS_STRING; i++)
		sum ^= m_msg[n++];

	// SendCompany
	retval &= _SetDataString(n, SIZE_SMS_STRING, strSendCompany);
	for (int i = 0; i < SIZE_SMS_STRING; i++)
		sum ^= m_msg[n++];

	// BoxNo
	m_msg[n] = nBoxNo/0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = nBoxNo%0x0100;
	sum ^= m_msg[n++];

	// SendPhone
	retval &= _SetDataString(n, SIZE_SMS_PHONE, strSendPhone);
	for (int i = 0; i < SIZE_SMS_PHONE; i++)
		sum ^= m_msg[n++];

	// RecvPhone
	retval &= _SetDataString(n, SIZE_SMS_PHONE, strRecvPhone);
	for (int i = 0; i < SIZE_SMS_PHONE; i++)
		sum ^= m_msg[n++];

	// Message
	if (bMMS)
	{
		retval &= _SetDataString(n, SIZE_MMS_MSG, strMsg);
		for (int i = 0; i < SIZE_MMS_MSG; i++)
			sum ^= m_msg[n++];
	}
	else
	{
		retval &= _SetDataString(n, SIZE_SMS_MSG, strMsg);
		for (int i = 0; i < SIZE_SMS_MSG; i++)
			sum ^= m_msg[n++];
	}


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;


	return retval;
}

// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateAck(BYTE* pData, int nSize)
{
	if (nSize > 0xFFFF)
		return FALSE;

	BYTE sum = 0x00;
	int n = 0;


	m_msg.SetSize(nSize+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_ACK;
	sum ^= m_msg[n++];

	m_msg[n] = ResultCode::CODE_SUCCESS;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = nSize / 0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = nSize % 0x0100;
	sum ^= m_msg[n++];

	memcpy(&m_msg[n], pData, nSize);
	for (int i = 0; i < nSize; i++)
	{
		sum ^= m_msg[n++];
	}


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;

	return TRUE;
}

// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateAck(CByteArray& data)
{
	return this->CreateAck(&data[0], data.GetCount());
}

// 성공 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateAck(CString strData)
{
	BOOL retval = this->CreateAck((BYTE*)strData.GetBuffer(), strData.GetLength());
	strData.ReleaseBuffer();

	return retval;
}

// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateNak(int nResultCode, BYTE* pData, int nSize)
{
	if (nSize > 0xFFFF)
		return FALSE;

	BYTE sum = 0x00;
	int n = 0;
	m_msg.SetSize(nSize+7);

	m_msg[n++] = CODE_STX;

	m_msg[n] = CODE_NAK;
	sum ^= m_msg[n++];

	m_msg[n] = nResultCode;
	sum ^= m_msg[n++];

	// LEN(2byte)
	m_msg[n] = nSize / 0x0100;
	sum ^= m_msg[n++];			
	m_msg[n] = nSize % 0x0100;
	sum ^= m_msg[n++];

	memcpy(&m_msg[n], pData, nSize);
	for (int i = 0; i < nSize; i++)
	{
		sum ^= m_msg[n++];
	}


	m_msg[n++] = sum;

	m_msg[n++] = CODE_ETX;

	return TRUE;
}

// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateNak(int nResultCode, CByteArray& data)
{
	return this->CreateNak(nResultCode, &data[0], data.GetCount());
}

// 실패 응답 패킷 생성(데이터 크기가 0xFFFF 초과시 FALSE 리턴)
BOOL CSMTPacket::CreateNak(int nResultCode, CString strData)
{
	BOOL retval = this->CreateNak(nResultCode, (BYTE*)strData.GetBuffer(), strData.GetLength());
	strData.ReleaseBuffer();

	return retval;
}

// 정상 생성된 페킷인지 알아옵니다.(받는쪽에서 Create()가 가능하면 TRUE)
BOOL CSMTPacket::IsPacket()
{
	return m_msg.GetSize() > 0;
}


// 요청 인지 알아옵니다.
BOOL CSMTPacket::IsENQ()
{
	return IsPacket() && m_msg[1] == CODE_ENQ;
}

// 응답(성공) 인지 알아옵니다.
BOOL CSMTPacket::IsACK()
{
	return IsPacket() && m_msg[1] == CODE_ACK;
}

// 응답(실패) 인지 알아옵니다.
BOOL CSMTPacket::IsNAK()
{
	return IsPacket() && m_msg[1] == CODE_NAK;
}

// ENQ 명령 코드를 알아옵니다.
BYTE CSMTPacket::GetCommandCode()
{
	BYTE retval = 0xFF;
	if (IsPacket())
	{
		retval = m_msg[2];
	}

	return retval;
}

// ACK/NAK 결과 코드(에러코드)를 알아옵니다.
BYTE CSMTPacket::GetResultCode()
{
	BYTE retval = 0xFF;
	if (IsPacket())
	{
		retval = m_msg[2];
	}

	return retval;
}

// 데이터를 문자열로 알아옵니다.
CString CSMTPacket::GetDataString(int nStart /* = 0*/)
{
	CString retval;

	if (IsPacket())
	{
		int len = (int)m_msg[3] * 0x0100 + (int)m_msg[4];

		if (nStart < len)
		{
			int	indexNull = len + 5;

			BYTE sum = m_msg[indexNull];
			m_msg[indexNull] = '\0';

			retval = (BYTE*)(&m_msg[5]) + nStart;
			retval.TrimRight();

			m_msg[indexNull] = sum;
		}
	}

	return retval;
}

// 데이터를 문자열로 알아옵니다.
CString CSMTPacket::GetDataString(int nStart, int nSize)
{
	CString retval;

	if (IsPacket())
	{
		int len = (int)m_msg[3] * 0x0100 + (int)m_msg[4];

		if (nStart + nSize <= len)
		{
			int	indexNull = nStart + nSize + 5;

			BYTE sum = m_msg[indexNull];
			m_msg[indexNull] = '\0';

			retval = (BYTE*)(&m_msg[5]) + nStart;
			retval.TrimRight();

			m_msg[indexNull] = sum;
		}
	}

	return retval;
}

// 데이터를 바이트로 알아옵니다.(리턴 기본값 0x00)
BYTE CSMTPacket::GetDataByte(int nStart /* = 0*/)
{
	BYTE retval = 0x00;

	if (IsPacket())
	{
		int len = (int)m_msg[3] * 0x0100 + (int)m_msg[4];
		if (nStart < len)
		{
			retval = m_msg[nStart+5];
		}
	}

	return retval;
}

// 데이터를 바이트 포인트로 알아옵니다.(리턴 기본값 NULL)
BYTE* CSMTPacket::GetDataBytePoint(int nStart /* = 0*/)
{
	BYTE* retval = NULL;

	if (IsPacket())
	{
		int len = (int)m_msg[3] * 0x0100 + (int)m_msg[4];
		if (nStart < len)
		{
			retval = &m_msg[nStart+5];
		}
	}


	return retval;
}

// 데이터의 크기를 알아옵니다.
int CSMTPacket::GetDataByteSize()
{
	int retval = 0;

	if (IsPacket())
	{
		retval = (int)m_msg[3] * 0x0100 + (int)m_msg[4];
	}

	return retval;
}

// 페킷 바이트 배열을 알아옵니다.(리턴 기본값 NULL)
BYTE* CSMTPacket::GetFullBytePoint()
{
	BYTE* retval = NULL;

	if (IsPacket())
	{
		retval = &this->m_msg[0];
	}

	return retval;
}

// 페킷 바이트 배열을 알아옵니다.
CByteArray& CSMTPacket::GetFullByteArray()
{
	return this->m_msg;
}

// 페킷 바이트 배열의 크기를 알아옵니다.
int CSMTPacket::GetFullByteSize()
{
	return this->m_msg.GetSize();
}



//////////////////////////////////////////////////////////////////////////////////////
// 내부 연산자

// 문자열 넣기
BOOL CSMTPacket::_SetDataString(int nStart, int nSize, CString& strData)
{
	BOOL retval = FALSE;

	memset(&m_msg[nStart], 0x20, nSize);
	if (strData.GetLength() <= nSize)
	{
		memcpy(&m_msg[nStart], strData.GetBuffer(), strData.GetLength());
		strData.ReleaseBuffer();
		retval = TRUE;
	}
	else
	{
		BOOL bChar = TRUE;

		memcpy(&m_msg[nStart], strData.GetBuffer(), nSize);
		strData.ReleaseBuffer();

		for (int i = nStart; i < nStart + nSize; i++)
		{
			if (m_msg[i] > 126)
				bChar = !bChar;
		}

		if (!bChar)
		{
			m_msg[nStart+nSize-1] = 0x20;
		}

		retval = TRUE;
	}

	return retval;

}