using System;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using s_oil.Utils;

/// <summary>
/// 소켓 동기 통신 클래스
/// </summary>
public class SClient : IDisposable
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
                }
            }

            // 리소스 폐기 완료
            IsDisposed = true;
        }
    }
    #endregion

    #region 이벤트 관련 
    /// <summary>
    /// Socket 이벤트 핸들러 대리자
    /// </summary>
    /// <param name="param">문자형 매개변수</param>
    public delegate void SocketStringEventHandler(string param);
    /// <summary>
    /// Socket 데이터 전송 이벤트 핸들러 대리자
    /// </summary>
    /// <param name="bytesSent">전송된 데이터</param>
    /// <param name="bOk">전송 성공 여부</param>
    /// <param name="bytesRcv">수신된 데이터수</param>
    /// <param name="rcvData">수신된 데이터</param>
    public delegate void SocketSendEventHandler(int bytesSent, bool bOk, int bytesRcv, byte[] rcvData);
    /// <summary>
    /// Socket 데이터 수신 이벤트 핸들러 대리자
    /// </summary>
    /// <param name="bytesRcv">수신된 데이터수</param>
    /// <param name="rcvData">수신된 데이터</param>
    public delegate void SocketReceiveEventHandler(int bytesRcv, byte[] rcvData);
    /// <summary>
    /// 비동기 Socket로부터 데이터가 전송되었을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SocketSendEventHandler DataSent;
    /// <summary>
    /// 비동기 Socket로부터 데이터를 수신했을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SocketReceiveEventHandler DataReceive;
    /// <summary>
    /// 비동기 연결 콜백함수
    /// </summary>
    public SocketStringEventHandler ConnectCallback;
    /// <summary>
    /// 비동기 Socket 상태가 변경되었을 때 발생하는 이벤트 핸들러
    /// </summary>
    public SocketStringEventHandler StatusChanged;
    #endregion

    #region Attributes
    /// <summary>
    /// 연결할 서버 주소
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 연결할 서버 포트번호
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 스트림에서 Nagle 알고리즘을 사용하는지 여부를 나타내는 System.Net.Sockets.Socket 값
    /// - false : System.Net.Sockets.Socket에서 Nagle 알고리즘을 사용함
    /// - true : System.Net.Sockets.Socket에서 Nagle 알고리즘을 사용안함
    /// </summary>
    private bool NoDelay = true;
    /// <summary>
    /// System.Net.Sockets.Socket.Send 호출이 완료되어야 하는 제한 시간
    /// - 단위 : 밀리초
    /// - 0 또는 -1 : 기본값, 시간 제한이 없음
    /// - 1부터 499 사이의 값으로 속성을 설정하면 값이 500으로 변경됨
    /// </summary>
    private int SendTimeout = 2000;
    /// <summary>
    /// System.Net.Sockets.Socket.Receive 호출이 완료되어야 하는 제한 시간
    /// - 단위 : 밀리초
    /// - 0 또는 -1 : 기본값, 시간 제한이 없음
    /// </summary>
    private int ReceiveTimeout = 2000;

    /// <summary>
    /// 전송한 데이터 바이트수
    /// </summary>
    public int SentLength = 0;
    /// <summary>
    /// 수신된 데이터 바이트수
    /// </summary>
    public int RcvLength = 0;
    /// <summary>
    /// 수신된 데이터 바이트
    /// </summary>
    public byte[] RcvBuffer = new byte[StateObject.BufferSize];

    /// <summary>
    /// 연결할 서버 소켓
    /// </summary>
    public Socket Sock { get; set; }

    /// <summary>
    /// 소켓 연결 성공 여부
    /// </summary>
    public bool IsConnected { get { return Sock != null ? Sock.Connected : false; } }
    #endregion

    /// <summary>
    /// 생성자
    /// </summary>
    public SClient()
    {
        Sock = null;
        IP = string.Empty;
        Port = 0;
    }

    /// <summary>
    /// 서버에 연결하기
    /// </summary>
    /// <param name="ip">연결할 서버 주소</param>
    /// <param name="port">연결할 서버 포트번호</param>
    /// <param name="noDelay">
    /// 스트림에서 Nagle 알고리즘을 사용하는지 여부를 나타내는 System.Net.Sockets.Socket 값
    /// - false : System.Net.Sockets.Socket에서 Nagle 알고리즘을 사용함
    /// - true : System.Net.Sockets.Socket에서 Nagle 알고리즘을 사용안함
    /// </param>
    /// <param name="sendTimeout">
    /// System.Net.Sockets.Socket.Send 호출이 완료되어야 하는 제한 시간
    /// - 단위 : 밀리초
    /// - 0 또는 -1 : 기본값, 시간 제한이 없음
    /// - 1부터 499 사이의 값으로 속성을 설정하면 값이 500으로 변경됨
    /// </param>
    /// <param name="receiveTimeout">
    /// System.Net.Sockets.Socket.Receive 호출이 완료되어야 하는 제한 시간
    /// - 단위 : 밀리초
    /// - 0 또는 -1 : 기본값, 시간 제한이 없음
    /// </param>
    /// <returns>true=연결 성공</returns>
    public bool Connect(string ip, int port, bool noDelay = true, int sendTimeout = 2000, int receiveTimeout = 2000)
    {
        try
        {
            if (!Funcs.IsValidIP(ip))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("<{0}>는 정합성이 맞지않은 IP입니다.", ip));
                return false;
            }

            IP = ip;
            Port = port;
            NoDelay = noDelay;
            SendTimeout = sendTimeout;
            ReceiveTimeout = receiveTimeout;

            IPAddress ipAddress = IPAddress.Parse(IP);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);
            Sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if (Sock != null)
            {
                try
                {
                    Sock.NoDelay = NoDelay;
                    Sock.SendTimeout = SendTimeout;
                    Sock.ReceiveTimeout = ReceiveTimeout;
                    Sock.Connect(remoteEP);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock)
                    {
                        System.Collections.ArrayList selectArray = new System.Collections.ArrayList();
                        selectArray.Add(Sock);

                        Socket.Select(selectArray, selectArray, null, 2000);

                        if (selectArray.Count == 0)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            if (ConnectCallback != null)
                                ConnectCallback("Fail");
                            return false;
                        }

                        if (ConnectCallback != null)
                            ConnectCallback("OK");
                        return true;
                    }
                }
                Funcs.Delay(100);

                if (ConnectCallback != null)
                {
                    if (Sock.Connected)
                    {
                        ConnectCallback("OK");
                        return true;
                    }
                    else
                        ConnectCallback("Fail");
                }
                else
                {
                    if (Sock.Connected)
                    {
                        return true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        try
        {
            if (ConnectCallback != null)
                ConnectCallback(string.Format("Failed"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// 서버에 연결 해제하기
    /// </summary>
    public void Disconnect()
    {
        try
        {
            if (Sock != null)
            {
                if (Sock.Connected)
                {
                    Sock.Shutdown(SocketShutdown.Both);
                    //System.Diagnostics.Debug.WriteLine("Sock.ShutDown() is successful!");
                    Sock.Close();
                    //System.Diagnostics.Debug.WriteLine("Sock.Close() is successful!");
                    Sock.Dispose();
                    //System.Diagnostics.Debug.WriteLine("Sock.Dispose() is successful!");
                    Sock = null;
                    //System.Diagnostics.Debug.WriteLine("Sock=null is successful!");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// 서버에 명령 전송하기 (existsRcv=true이면 전송 후 응답받기)
    /// </summary>
    /// <param name="data">전송할 바이트형 데이터</param>
    /// <param name="existsRcv">전송 후 수신되는 데이터가 있는지 여부</param>
    public async void Send(byte[] data, bool existsRcv)
    {
        try
        {
            if (Sock != null)
            {
                var ctTask = Task.Run(() =>
                {
                    StartSend(data, existsRcv);
                });
                await ctTask;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        try
        {
            if (DataSent != null)
                DataSent(0, false, 0, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// 서버에 명령 전송 시작 (existsRcv=true이면 전송 후 응답받기)
    /// </summary>
    /// <param name="data">전송할 바이트형 데이터</param>
    /// <param name="existsRcv">전송 후 수신되는 데이터가 있는지 여부</param>
    /// <returns>전송된 바이트수</returns>
    private void StartSend(byte[] data, bool existsRcv)
    {
        try
        {
            // buffer로 메시지를 받고 Receive함수로 메시지가 올 때까지 대기한다.
            if (existsRcv)
            {
                RcvLength = 0;
                RcvBuffer = new byte[StateObject.BufferSize];
                Array.Clear(RcvBuffer, 0, RcvBuffer.Length);
                Sock.BeginReceive(RcvBuffer, 0, RcvBuffer.Length, SocketFlags.None, Receive, this);
            }
            else
                SentLength = Sock.Send(data);//, data.Length, SocketFlags.None);
        }
        catch (SocketException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// 데이터 수신하기
    /// </summary>
    /// <param name="result">비동기 결과</param>
    private void Receive(IAsyncResult result)
    {
        try
        {
            // 접속이 연결되어 있으면...
            if (Sock != null && Sock.Connected)
            {
                // EndReceive를 호출하여 데이터 사이즈를 받는다.
                // EndReceive는 대기를 끝내는 것이다.
                RcvLength = this.Sock.EndReceive(result);
                //System.Diagnostics.Debug.WriteLine(Funcs.ByteToString(RcvBuffer, Encoding.Default));

                if (DataSent != null)
                    DataSent(SentLength, true, RcvLength, RcvBuffer);

                // buffer로 메시지를 받고 Receive함수로 메시지가 올 때까지 대기한다.
                //this.Sock.BeginReceive(RcvBuffer, 0, RcvBuffer.Length, SocketFlags.None, Receive, this);
            }
        }
        catch (SocketException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// 서버에 명령 전송하기
    /// </summary>
    /// <param name="data">전송할 바이트형 데이터</param>
    /// <returns>전송된 바이트수</returns>
    public int Send(byte[] data)
    {
        try
        {
            if (Sock != null)
            {
                SentLength = Sock.Send(data);
            }
        }
        catch (SocketException)
        {
            try
            {
                // 연결이 끊어져 있으면 재연결하고 
                // 연결되면 다시 전송한다.
                Sock.Disconnect(true);

                if (Connect(IP, Port) && Sock != null)
                {
                    SentLength = Sock.Send(data);
                }
            }
            catch (SocketException ex2)
            {
                System.Diagnostics.Debug.WriteLine(ex2.ToString());
            }
            catch (Exception ex2)
            {
                System.Diagnostics.Debug.WriteLine(ex2.ToString());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        try
        {
            if (DataSent != null)
                DataSent(SentLength, true, 0, null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return SentLength;
    }

    /// <summary>
    /// 서버로부터 데이터 수신하기
    /// </summary>
    /// <param name="rcvData">수신된 데이터</param>
    /// <returns>수신된 데이터의 바이트수</returns>
    public int Receive(out byte[] rcvData)
    {
        rcvData = null;

        int rcvCount = 0;

        try
        {
            if (Sock != null && Sock.Connected)
            {
                var rcvBuff = new byte[StateObject.BufferSize];
                rcvCount = Sock.Receive(rcvBuff);

                rcvData = new byte[rcvCount + 1];
                Array.Copy(rcvBuff, rcvData, rcvCount);
                rcvData[rcvCount] = 0;
#if _PRINT_DATA_
                string msg = "";
                foreach (var v in rcvData)
                    msg += string.Format("{0:X2} ", v);
                System.Diagnostics.Debug.WriteLine("SClient.Receive: " + msg);
#endif
            }
        }
        catch (SocketException ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        try
        {
            if (DataReceive != null)
                DataReceive(rcvCount, rcvData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        return rcvCount;
    }
}