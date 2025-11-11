#define NEW_PLC_CONTROL
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using s_oil.Utils;


/// <summary>
/// BU 제어 클래스
/// </summary>
public class BU : IDisposable
{
    #region IDisposable 계승에 필요함
    /// <summary>
    /// Dispose가 호출되었는지 여부
    /// </summary>
    private bool IsDisposed = false;
    /// <summary>
    /// IDisposable을 구현합니다.
    /// </summary>
    /// <remarks>
    /// - 이 방법을 가상으로 만들지 마십시오.
    /// - 파생 클래스는 이 메서드를 재정의할 수 없어야 합니다.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose(bool disposing)는 두 가지 별개의 시나리오에서 실행됨
    /// </summary>
    /// <param name="disposing">
    /// - true : 메서드가 사용자 코드에 의해 직접 또는 간접적으로 호출된 것. 
    ///          관리되는 리소스와 관리되지 않는 리소스를 처분할 수 있음.
    /// - false : 메서드가 종료자 내부에서 런타임에 의해 호출된 것
    ///           다른 개체를 참조하면 안됨. 관리되지 않는 리소스만 폐기할 수 있음.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        // Dispose가 이미 호출되었는지 확인
        if (!this.IsDisposed)
        {
            // true이면 관리되는 리소스와 관리되지 않는 리소스를 모두 삭제
            if (disposing)
            {
                // 관리 리소스를 폐기
                if (Sock != null)
                {
                    Disconnect();
                    Sock.Dispose();
                    Sock = null;
                }
            }

            // 리소스 폐기 완료
            IsDisposed = true;
        }
    }
    #endregion

    #region 상수 정의
    private const byte PLC_CMD_STX = 0x02;
    private const byte PLC_CMD_ETX = 0x03;
    private const byte PLC_CMD_ACK = 0x06;
    private const byte PLC_CMD_NAK = 0x15;
    private const byte PLC_CMD_STAT = 0x30;
    private const byte PLC_CMD_OPEN = 0X31;
    private const byte PLC_CMD_HOLD = 0x50;
    private const byte PLC_CMD_CLOSE = 0x51;
    private const byte PLC_CMD_OPEN_CU = 0x52;

    /// <summary>
    /// 박스 총개수
    /// </summary>
    private const int CNT_UNIT = 22;
    #endregion

    #region public Attributes
    /// <summary>
    /// 비동기 Socket 상태가 변경되었을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SClient.SocketStringEventHandler StatusChanged;
    /// <summary>
    /// 비동기 Socket로부터 데이터를 송신했을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SClient.SocketSendEventHandler DataSent;
    /// <summary>
    /// 비동기 Socket 연결 시도가 있을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SClient.SocketStringEventHandler ConnectCallback;

    /// <summary>
    /// 동기 소켓동신 개체
    /// </summary>
    public SClient Sock { get; set; }

    /// <summary>
    /// PLC IP
    /// </summary>
    public string ServerIP { get; set; }
    /// <summary>
    /// PLC Port (default=5000)
    /// </summary>
    public int ServerPort = 5000;

    /// <summary>
    /// 소켓 연결 성공 여부
    /// </summary>
    /// <remarks>
    /// System.Net.Sockets.Socket이 마지막으로 Socket.Send 또는 Socket.Receive 작업을 수행할 때 원격 호스트에 연결되었는지 여부를 나타내는 값
    /// </remarks>
    public bool IsConnected
    {
        get
        {
            return (Sock != null && Sock.IsConnected);
        }
    }

    /// <summary>
    /// 명령 전송의 성공 여부
    /// </summary>
    public bool SendOK = false;
    /// <summary>
    /// Error Message
    /// </summary>
    public string ErrorMsg { get; set; }

    /// <summary>
    /// 보관함 오픈 여부 판단 기준
    /// - true : 각 비트의 값이 1이면 닫힘
    /// - false : 각 비트의 값이 0이면 닫힘
    /// </summary>
    /// <remarks>
    /// [보관함 오픈 여부(16비트)]
    /// - 구성 : 1234567812345678 
    /// - 각 비트의 값 : 0 또는 1
    /// - 비트값 의미 : 16번, 15번, ...., 2번, 1번 보관함의 오픈 여부
    /// </remarks>
    public bool IsOpenCloseInverse = true;

    #region Propertices
    private byte MK_SUM(byte[] A) { return ((byte)(A[0] + A[1] + A[2] + A[3])); }
    private byte MK_RESUM(byte[] A) { return ((byte)(A[0] + A[1] + A[2] + A[3] + A[4] + A[5] + A[6] + A[7])); }
    private byte MK_SUM20(byte[] A) { return ((byte)(A[0] + A[1] + A[2] + A[3] + A[4])); }
    private byte MK_RESUM20(byte[] A) { return ((byte)(A[0] + A[1] + A[2] + A[3] + A[4] + A[5] + A[6] + A[7] + A[8] + A[9] + A[10])); }
    private byte MK_CUADDR20(int A) { return ((byte)(((A - 1) / CNT_UNIT))); }
    private byte MK_BOXADDR20(int A) { return ((byte)(((A - 1) % CNT_UNIT))); }
    private byte MK_CUADDR(int A) { return ((byte)(((A)))); }
    private byte MK_BOXADDR(int A) { return ((byte)(((A)))); }

#if NEW_PLC_CONTROL
    private byte MK_ADDR(int A) { return ((byte)((((A - 1) / 16) << 4) | ((A - 1) % 16) & 0x0f)); }
#else
    private byte MK_ADDR(int A) { return ((byte)(( (A/16) << 4) | ((A%16)-1)&0x0f ));}
#endif

    #endregion

    /// <summary>
    /// 생성자
    /// </summary>
    public BU()
    {
        try
        {
            Sock = null;
        }
        catch (Exception ex)
        {
            Logger.Error("BU default constructor failed.", ex);
        }
    }
    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="serverIP">PLC IP</param>
    /// <param name="serverPort">PLC Port</param>
    /// <param name="existsBaseEvent">기본 이벤트를 설정할지 여부 (default=false)</param>
    public BU(string serverIP, int serverPort, bool existsBaseEvent = false)
    {
        try
        {
            ServerIP = serverIP;
            ServerPort = serverPort;
            Sock = new SClient();

            if (existsBaseEvent)
            {
                Sock.StatusChanged += OnStatusChanged;
                Sock.ConnectCallback += OnConnectCallback;
                Sock.DataSent += OnDataSent;
            }
            Logger.Info($"BU constructor successful for {serverIP}:{serverPort}.");
        }
        catch (Exception ex)
        {
            Logger.Error($"BU constructor failed for {serverIP}:{serverPort}.", ex);
        }
    }

    /// <summary>
    /// PLC 정보 (보관함 번호, PLC번호)
    /// </summary>
    public Dictionary<int, int> PLCMap;

    /// <summary>
    /// 모든 보관함과 매칭된 모든 PLC 번호 구하기
    /// </summary>
    /// <param name="iniPlcPath">PLP 정보 파일</param>
    /// <returns>PLC 정보 (보관함 번호, PLC번호)</returns>
    public Dictionary<int, int> InitPLC(string iniPlcPath)
    {
        Dictionary<int, int> PLCMap = new Dictionary<int, int>();

        try
        {
            IniParser ini = new IniParser(iniPlcPath);
            if (ini.IsNull())
            {
                Logger.Warning("PLC info file is missing.");
                return PLCMap;
            }

            int count = Funcs.OToInt32(ini.GetSetting("PLC", "SLOT_CNT", "0"));
            for (int i = 1; i <= count; ++i)
            {
                string slotName = string.Format("SLOT_{0:D3}", i);
                PLCMap[i] = Funcs.OToInt32(ini.GetSetting("PLC", slotName, "0"));
            }

            return PLCMap;
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to initialize PLC map.", ex);
            return PLCMap;
        }

    }

    /// <summary>
    /// 보관함과 연결된 PLC 번호 구하기
    /// </summary>
    /// <param name="boxNo">보관함 번호</param>
    /// <returns>PLC 번호</returns>
    public int GetPLCNo(int boxNo)
    {
        try
        {
            string iniPath = string.Format(@"{0}\PLC.ini", System.Windows.Forms.Application.StartupPath);
            Logger.Info($"Loading PLC mapping from: {iniPath}");
            
            IniParser ini = new IniParser(iniPath);
            if (ini.IsNull())
            {
                Logger.Warning($"PLC info file is missing: <{iniPath}>");
                
                // PLC1.ini 파일도 시도해보기
                string altPath = string.Format(@"{0}\PLC1.ini", System.Windows.Forms.Application.StartupPath);
                Logger.Info($"Trying alternative PLC mapping file: {altPath}");
                
                ini = new IniParser(altPath);
                if (ini.IsNull())
                {
                    Logger.Warning($"Alternative PLC info file is also missing: <{altPath}>");
                    Logger.Info($"Using default mapping: Box {boxNo} -> PLC {boxNo}");
                    return boxNo; // 기본 매핑: 사물함 번호 = PLC 번호
                }
            }

            string slotName = string.Format("SLOT_{0:D3}", boxNo);
            string plcNoStr = ini.GetSetting("PLC", slotName, "");
            
            if (string.IsNullOrEmpty(plcNoStr))
            {
                // PLC1 섹션에서도 시도
                plcNoStr = ini.GetSetting("PLC1", slotName, "");
                Logger.Info($"PLC mapping from PLC1 section - {slotName}: {plcNoStr}");
            }
            else
            {
                Logger.Info($"PLC mapping from PLC section - {slotName}: {plcNoStr}");
            }
            
            int plcNo = Funcs.OToInt32(plcNoStr);
            
            if (plcNo <= 0)
            {
                Logger.Warning($"Invalid PLC mapping for box {boxNo} ({slotName} = '{plcNoStr}')");
                Logger.Info($"Using default mapping: Box {boxNo} -> PLC {boxNo}");
                return boxNo; // 기본 매핑
            }
            
            Logger.Info($"PLC mapping successful: Box {boxNo} -> PLC {plcNo}");
            return plcNo;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to get PLC number for box {boxNo}.", ex);
            Logger.Info($"Using default mapping due to error: Box {boxNo} -> PLC {boxNo}");
            return boxNo; // 오류시 기본 매핑
        }
    }

    /// <summary>
    /// 상태 이상 이벤트 설정하기
    /// </summary>
    /// <param name="callBack">상태 이상 이벤트</param>
    public void SetStatusChanged(SClient.SocketStringEventHandler callBack)
    {
        try
        {
            Sock.StatusChanged += new SClient.SocketStringEventHandler(callBack);
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to set StatusChanged callback.", ex);
        }
    }

    /// <summary>
    /// 비동기 Socket 연결 시도 이벤트 설정하기
    /// </summary>
    /// <param name="callBack">상태 이상 이벤트</param>
    public void SetConnectCallback(SClient.SocketStringEventHandler callBack)
    {
        try
        {
            Sock.ConnectCallback += new SClient.SocketStringEventHandler(callBack);
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to set ConnectCallback.", ex);
        }
    }

    /// <summary>
    /// 데이터 송신 이벤트 설정하기
    /// </summary>
    /// <param name="callBack">데이터 송신 이벤트</param>
    public void SetDataSent(SClient.SocketSendEventHandler callBack)
    {
        try
        {
            Sock.DataSent += new SClient.SocketSendEventHandler(callBack);
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to set DataSent callback.", ex);
        }
    }

    /// <summary>
    /// PLC 소켓 상태가 변경되었을 때
    /// </summary>
    /// <param name="status">상태</param>
    private void OnStatusChanged(string status)
    {
        try
        {
            string[] lines = status.Replace("\r\n", "\r").Replace("\n", "\r")
                        .Split('\r');
            ErrorMsg = "";
            if (lines != null)
            {
                foreach (var v in lines)
                    ErrorMsg += v + Environment.NewLine;
            }
            Logger.Info($"PLC status changed: {status}");
            if (StatusChanged != null)
                StatusChanged(status);
        }
        catch (Exception ex)
        {
            Logger.Error("Error in OnStatusChanged.", ex);
        }
    }
    /// <summary>
    /// 데이터가 전송되었을 때
    /// </summary>
    /// <param name="bytesSent">전송된 데이터수</param>
    /// <param name="bOk">전송 성공 여부</param>
    /// <param name="bytesRcv">수신된 데이터수</param>
    /// <param name="rcvData">수신된 데이터</param>
    private void OnDataSent(int bytesSent, bool bOk, int bytesRcv, byte[] rcvData)
    {
        try
        {
            SendOK = bOk;

            if (DataSent != null)
                DataSent(bytesSent, bOk, bytesRcv, rcvData);
        }
        catch (Exception ex)
        {
            Logger.Error("Error in OnDataSent.", ex);
        }
    }

    /// <summary>
    /// 소켓 연결 성공 여부
    /// </summary>
    /// <param name="isConnected">소켓 연결 성공 여부 (OK=성공)</param>
    private void OnConnectCallback(string isConnected)
    {
        try
        {
            if (ConnectCallback != null)
                ConnectCallback(isConnected);
        }
        catch (Exception ex)
        {
            Logger.Error("Error in OnConnectCallback.", ex);
        }
    }
    #endregion

    /// <summary>
    /// PLC 소켓 연결하기
    /// </summary>
    /// <param name="serverIP">PLC IP</param>
    /// <param name="serverPort">PLC Port</param>
    /// <returns>true=성공, false=실패</returns>
    public bool Connect(string serverIP, int serverPort)
    {
        try
        {
            ServerIP = serverIP;
            ServerPort = serverPort;

            bool bRet = Sock.Connect(serverIP, serverPort);
            System.Threading.Thread.Sleep(10);
            return bRet;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to connect to PLC at {serverIP}:{serverPort}.", ex);
            return false;
        }
    }

    /// <summary>
    /// 소켓 연결 해제하기
    /// </summary>
    public void Disconnect()
    {
        try
        {
            if (Sock != null)
            {
                Sock.Disconnect();
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to disconnect from PLC.", ex);
        }
    }

    /// <summary>
    /// 소켓 연결 해제 및 닫기
    /// </summary>
    public void Close()
    {
        try
        {
            if (Sock != null)
            {
                Sock.Disconnect();
                Sock.Dispose();
                Sock = null;
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to close PLC connection.", ex);
        }
    }

    /// <summary>
    /// 박스 오픈하기
    /// </summary>
    /// <param name="boxNo">박스번호</param>
    /// <param name="timeoutMilli">대기시간(단위:밀리초, default=2000)</param>
    /// <returns>true=성공, false=실패</returns>
    public bool OpenBox(int boxNo, bool bDisconnect, int timeoutMilli = 2000)
    {
        SendOK = false;

        if (boxNo < 1)
        {
            Logger.Error($"Invalid box number: {boxNo}");
            return false;
        }

        try
        {
            Logger.Info($"Starting to open box {boxNo}");
            
            // 보관함 번호에 매칭되어 있는 PLC 번호
            int plcNo = GetPLCNo(boxNo);
            
            if (plcNo <= 0)
            {
                Logger.Error($"Invalid PLC number ({plcNo}) for box {boxNo}");
                return false;
            }
            
            Logger.Info($"Box {boxNo} mapped to PLC {plcNo}");

            // 보관함 오픈 시도 최대 횟수
            const int maxRepeat = 3;
            // 오픈 시도 시작시간
            DateTime startTime = DateTime.Now;

            // 보관함 오픈 시도
            for (int i = 0; i < maxRepeat; ++i)
            {
                Logger.Info($"Opening attempt {i + 1}/{maxRepeat} for box {boxNo} (PLC {plcNo})");
                
                if (!OpenPLC(plcNo))
                {
                    Logger.Error($"Failed to send open command for box {boxNo} (attempt {i + 1})");
                    
                    if (i < maxRepeat - 1) // 마지막 시도가 아니면 재시도
                    {
                        Logger.Info("Waiting 750ms before retry...");
                        Funcs.Delay(750);
                        continue;
                    }
                    else
                    {
                        Logger.Error($"All open attempts failed for box {boxNo}");
                        return false;
                    }
                }

                // 명령 전송 성공 후 잠시 대기
                Funcs.Delay(750);

                var ts = DateTime.Now - startTime;
                Logger.Info($"Checking if box {boxNo} is open after {ts.TotalSeconds:F1} seconds");
                
                // 타임아웃 체크
                if (ts.TotalSeconds > i * 5 + 5)
                {
                    Logger.Error($"Timeout opening box {boxNo} (PLC {plcNo}) - Elapsed: {ts.TotalSeconds:F1}s");
                    return false;
                }

                // 열림 상태 확인
                if (IsOpenPLC(plcNo))
                {
                    Logger.Info($"? Box {boxNo} (PLC {plcNo}) opened successfully!");
                    return true;
                }
                else
                {
                    Logger.Warning($"Box {boxNo} (PLC {plcNo}) is not open yet (attempt {i + 1})");
                }
            }
            
            Logger.Error($"Failed to open box {boxNo} after {maxRepeat} attempts");
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error($"An exception occurred while opening box {boxNo}.", ex);
            return false;
        }
        finally
        {
            // 최종 PLC 연결을 종료한다.
            if (bDisconnect)
            {
                try
                {
                    Disconnect();
                    Logger.Info("PLC connection closed");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Error disconnecting PLC: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 박스 오픈하기
    /// </summary>
    /// <param name="plcNo">보관함에 매칭된 PLC 번호 (one-based)</param>
    /// <returns>true=성공, false=실패</returns>
    public bool OpenPLC(int plcNo)
    {
        SendOK = false;

        if (plcNo < 1)
        {
            Logger.Error($"Invalid PLC number: {plcNo}");
            return false;
        }

        try
        {
            if (Sock != null)
            {
                if (!Sock.IsConnected)
                {
                    Logger.Info($"Connecting to PLC: {ServerIP}:{ServerPort}");
                    if (!Sock.Connect(ServerIP, ServerPort))
                    {
                        Logger.Error($"Connection to [{ServerIP}:{ServerPort}] failed.");
                        return false;
                    }
                    Logger.Info("PLC connection successful");
                }

                // 보관함 오픈 명령어
                byte[] sendData = new byte[]
                {
                    PLC_CMD_STX,
                    MK_ADDR(plcNo),
                    PLC_CMD_OPEN,
                    PLC_CMD_ETX,
                    0x00,
                };
                sendData[4] = MK_SUM(sendData);

                // 명령어 로그 출력 (디버깅용)
                string cmdLog = string.Join(" ", sendData.Select(b => $"0x{b:X2}"));
                Logger.Info($"Sending open command to PLC {plcNo}: {cmdLog}");

                int sentBytes = Sock.Send(sendData);
                bool success = sentBytes == 5;
                
                if (success)
                {
                    Logger.Info($"Open command sent successfully to PLC {plcNo} ({sentBytes} bytes)");
                }
                else
                {
                    Logger.Error($"Failed to send open command to PLC {plcNo} - Sent: {sentBytes}, Expected: 5");
                }
                
                return success;
            }
            else
            {
                Logger.Error("Socket is null");
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to execute OpenPLC for PLC {plcNo}.", ex);
            return false; // 예외 발생시 false 반환 (원본 코드에서 true였던 버그 수정)
        }
    }

    /// <summary>
    /// 보관함이 오픈되었는지 체크하기
    /// - true : 보관함이 열림
    /// - false : 보관함이 닫힘
    /// </summary>
    /// <param name="plcNo">보관함에 매칭된 PLC 번호 (one-based)</param>
    /// <returns>true=보관함이 오픈되어 있음</returns>
    public bool IsOpenPLC(int plcNo)
    {
        if (plcNo < 1)
            return false;

        try
        {
            string msg = "";
            #region PLC 연결 상태 확인 후 재연결함
            if (!Sock.IsConnected)
            {
                if (!Sock.Connect(ServerIP, ServerPort))
                    return false;
            }
            #endregion

            #region 보관함 상태 체크 명령어 전송
            // 보관함 상태 체크 명령어
            byte[] sendData = new byte[]
            {
                PLC_CMD_STX,
                MK_ADDR(plcNo),
                PLC_CMD_STAT,
                PLC_CMD_ETX,
                0x00,
            };
            sendData[4] = MK_SUM(sendData);

            if (Sock.Send(sendData) != 5)
            {
                Logger.Error("Box status check command send Error!!!");
                msg = "";
                if (sendData != null && sendData.Length > 0)
                {
                    foreach (var v in sendData)
                        msg += string.Format("{0:X2} ", v);
                }
                Logger.Error("Box status check command: " + msg);
                return false;
            }
            else
            {
                Logger.Info("Box status check command sent successfully!!!");
            }

            //Funcs.Delay(300);
            #endregion

            #region 보관함 상태 체크 명령어 전송 후 응답 데이터 수신
            byte[] rcvData;
            if (Sock.Receive(out rcvData) != 9)
            {
                Logger.Error("Error receiving response after sending box status check command!!!");

                msg = "";
                if (rcvData != null && rcvData.Length > 0)
                {
                    foreach (var v in rcvData)
                        msg += string.Format("{0:X2} ", v);
                }
                Logger.Error("Response data after box status check command: " + msg);
                return false;
            }
            else
            {
                //Logger.Info("Response received successfully after sending box status check command!!!");
            }

            #endregion


            #region 마지막 체크썸 값이 정상인지 체크
            byte cSum = 0;
            cSum = MK_RESUM(rcvData);

            if (cSum != rcvData[8])
            {
                Logger.Warning($"Checksum mismatch in response. CheckSum:{cSum:X2} != rcvData[8]:{rcvData[8]:X2}");
                return false;
            }
#if _PRINT_DATA_
            foreach (var v in rcvData)
                msg += string.Format("{0:X2} ", v);
            Logger.Info("BU.Receive: " + msg);
#endif
            #endregion

            #region 해당 PLC번호에 해당하는 보관함이 오픈되어 있는지의 여부 
            ushort cRes = 0;
            cRes |= (ushort)rcvData[3];
            cRes |= (ushort)(rcvData[4] << 8);

#if NEW_PLC_CONTROL
            var isOpened = (0x01 << (((plcNo - 1) % 16) & 0x0f)) & cRes;

            if (isOpened != 0)
            {
                if (IsOpenCloseInverse)
                {
                    //Logger.Info($"{plcNo} box is closed.");
                    return false;
                }
                else
                {
                    //Logger.Info($"{plcNo} box is open.");
                    return true;
                }
            }
            else
            {
                if (IsOpenCloseInverse)
                {
                    //Logger.Info($"{plcNo} box is open.");
                    return true;
                }
                else
                {
                    //Logger.Info($"{plcNo} box is closed.");
                    return false;
                }
            }
#else
            // 해당 PLC번호에 해당하는 보관함이 오픈되어 있는지의 여부 
            var isOpened = (0x01 << (((plcNo % 16) - 1) & 0x0f)) & cRes;
            // isOpened = 0 이면 닫힘
            return isOpened != 0;
#endif
            #endregion
        }
        catch (Exception ex)
        {
            Logger.Error($"An exception occurred in IsOpenPLC for PLC {plcNo}.", ex);
        }

        return false;
    }

    public bool IsOpenBox(int boxNo, ref bool isOpen, int timeoutMilli = 2000)
    {
        int plcNo = GetPLCNo(boxNo);

        isOpen = IsOpenPLC(plcNo);

        Disconnect();

        return true;
    }

    /// <summary>
    /// 소켓 연결 여부 구하기
    /// </summary>
    /// <returns>소켓 연결 여부</returns>
    public bool GetConnected()
    {
        if (Sock == null)
            return false;
        else
            return Sock.IsConnected;
    }
}