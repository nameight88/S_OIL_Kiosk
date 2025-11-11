using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using s_oil.Utils;
using System.Windows.Forms;

public class DaouCtrl
{
    public static TcpClient Client;
    public static StreamReader Reader;
    public static StreamWriter Writer;
    public static NetworkStream stream;
    public static Thread ReceiveThread;
    public static bool Connected;
    public static bool isConnected = false;
    public static string outmsg = "";
    public static IniParser iniParser;

    // 설정값들
    private static string ServerIP = "127.0.0.1";
    private static int ServerPort = 23738;
    private static string DeviceNo = "11869291";
    private static bool IsTestMode = false;

    /// <summary>
    /// INI 파일에서 설정 로드
    /// </summary>
    static DaouCtrl()
    {
        try
        {
            var iniPath = System.IO.Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
            iniParser = new IniParser(iniPath);
            
            if (!iniParser.IsNull())
            {
                ServerIP = iniParser.GetSetting("DaouVP2509", "ServerIP");
                ServerPort = int.Parse(iniParser.GetSetting("DaouVP2509", "ServerPort"));
                DeviceNo = iniParser.GetSetting("DaouVP2509", "DeviceNo");
                IsTestMode = iniParser.GetSetting("Is_Test", "Test", "true").ToLower() == "true";
                
                Logger.Info($"[Daou] 설정 로드 완료 - IP: {ServerIP}, Port: {ServerPort}, Device: {DeviceNo}, TestMode: {IsTestMode}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"[Daou] 설정 로드 실패: {ex.Message}", ex);
        }
    }

    public static void ConnectDaouServer()
    {
        try
        {
            if (Connected == true)
            {
                if (Reader != null)
                {
                    Reader.Close();
                }

                if (Writer != null)
                {
                    Writer.Close();
                }

                if (Client != null)
                {
                    Client.Close();
                }
            }

            Client = new TcpClient();
            Client.Connect(ServerIP, ServerPort);
            stream = Client.GetStream();

            Connected = true;
            Logger.Info($"[Daou] 서버 연결 성공: {ServerIP}:{ServerPort}");

            isConnected = true;

            Reader = new StreamReader(stream, System.Text.Encoding.Default);
            Writer = new StreamWriter(stream, System.Text.Encoding.Default);
        }
        catch (Exception ConnE)
        {
            Logger.Error($"[Daou] 서버 연결 실패: {ConnE.Message}", ConnE);
            Connected = false;
            isConnected = false;
        }
    }

    public static void DisconnectDaouServer()
    {
        try
        {
            if (Connected == true)
            {
                if (Reader != null)
                {
                    Reader.Close();
                }

                if (Writer != null)
                {
                    Writer.Close();
                }

                if (Client != null)
                {
                    Client.Close();
                }
            }

            Connected = false;
            isConnected = false;
            Logger.Info("[Daou] 서버 연결 해제");
        }
        catch (Exception ConnE)
        {
            Logger.Error($"[Daou] 연결 해제 실패: {ConnE.Message}", ConnE);
        }
    }

    public static bool ispossible = false;

    /// <summary>
    /// 카드 결제 처리
    /// </summary>
    /// <param name="amount">결제 금액 (선택사항, 기본값은 설정된 금액 사용)</param>
    /// <returns>결제 성공 여부</returns>
    public static bool Pay(int amount = 0)
    {
        try
        {
            Logger.Info($"[Daou] 결제 시작 - 금액: {amount:N0}원, 테스트 모드: {IsTestMode}");

            // 테스트 모드인 경우 즉시 성공 반환
            if (IsTestMode)
            {
                Task.Delay(2000).Wait(); // 실제 결제 시간 시뮬레이션
                outmsg = "Success";
                Logger.Info("[Daou] 테스트 모드 결제 성공");
                return true;
            }

            outmsg = "";

            if (isConnected == false)
            {
                ConnectDaouServer();
            }

            if (isConnected == true)
            {
                string cost = amount > 0 ? amount.ToString() : "1000"; // 기본 테스트 금액

                if (!string.IsNullOrEmpty(cost))
                {
                    Logger.Info($"[Daou] 결제 요청 - 금액: {cost}원");
                    try
                    {
                        StringBuilder req = new StringBuilder();
                        req.Clear();

                        // 전문구분
                        req = insertLeftJustify(req, "0100", 4);
                        // 업무구분
                        req = insertLeftJustify(req, "10", 2);

                        req = insertLeftJustify(req, DeviceNo, 8);    //단말기번호
                        req = insertLeftJustify(req, "0000", 4);
                        req = insertLeftJustify(req, "0", 14);

                        int payAmount = int.Parse(cost);  //결제 금액
                        int volAmount = 101;
                        int taxAmount = (int)(payAmount * 0.1);

                        req = insertLeftJustify(req, "00", 2);
                        req = insertLeftJustify(req, payAmount.ToString(), 12);
                        req = insertLeftJustify(req, volAmount.ToString(), 12);
                        req = insertLeftJustify(req, taxAmount.ToString(), 12);
                        req = insertLeftJustify(req, "", 12);
                        req = insertLeftJustify(req, DateTime.Now.ToString("yyyyMMdd"), 8);
                        req = insertLeftJustify(req, "", 12);

                        req = insertLeftJustify(req, "", 42);
                        req = insertLeftJustify(req, "", 6);
                        req = insertLeftJustify(req, "\x1c", 1);

                        Writer.Write(req.ToString());
                        Writer.Flush();

                        Logger.Info("[Daou] 결제 요청 전송 완료");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[Daou] 결제 요청 전송 실패: {ex.Message}", ex);
                        outmsg = "Send Error: " + ex.Message;
                        return false;
                    }
                }
                else
                {
                    Logger.Warning("[Daou] 결제 금액이 없습니다.");
                    outmsg = "Amount Error";
                    return false;
                }
            }
            else
            {
                Logger.Error("[Daou] 서버 연결이 되어있지 않습니다.");
                outmsg = "Connection Error";
                return false;
            }

            // 응답 대기
            DateTime startTime = DateTime.Now;
            while (string.IsNullOrEmpty(outmsg))
            {
                try
                {
                    // 타임아웃 체크 (30초)
                    if ((DateTime.Now - startTime).TotalSeconds > 30)
                    {
                        Logger.Error("[Daou] 결제 응답 타임아웃");
                        outmsg = "Timeout";
                        break;
                    }

                    if (stream.CanRead)
                    {
                        char[] c = new char[5000];
                        int adv = Reader.Read(c, 0, c.Length);

                        string s = new string(c);
                        string tempStr = s.Replace("\0", "");

                        if (tempStr.Length > 0)
                        {
                            Logger.Info($"[Daou] 응답 수신: {tempStr}");

                            if ((tempStr.Substring(0, 4) == "1000") || (tempStr.Substring(0, 4) == "2000"))
                            {
                                if (tempStr.Length > 26)
                                {
                                    if (tempStr.Substring(22, 4) != "0000")
                                    {
                                        outmsg = Message(tempStr);
                                    }
                                    else
                                    {
                                        outmsg = "Success";
                                    }
                                }
                                else
                                {
                                    outmsg = Message(tempStr);
                                }
                            }
                            else if (tempStr.Substring(0, 1) == "K" && tempStr.Substring(0, 4) != "K910")
                            {
                                outmsg = Message(tempStr);
                            }
                            else if (tempStr.Substring(0, 1) == "S")
                            {
                                outmsg = Message(tempStr);
                            }
                            else if (tempStr.Substring(0, 4) == "0000")
                            {
                                if (tempStr.Length > 4)
                                {
                                    if (tempStr.Substring(4, 1) == "C")
                                    {
                                        outmsg = Message(tempStr);
                                    }
                                    else
                                    {
                                        outmsg = "Failed(1): " + tempStr;
                                    }
                                }
                                else
                                {
                                    outmsg = "Failed(2): " + tempStr;
                                }
                            }
                            else
                            {
                                outmsg = "Failed(3): " + tempStr;
                            }
                        }
                    }

                    Task.Delay(100).Wait(); // CPU 사용률 감소
                }
                catch (Exception ex)
                {
                    Logger.Error($"[Daou] 응답 처리 중 오류: {ex.Message}", ex);
                    outmsg = "Response Error: " + ex.Message;
                    break;
                }
            }

            bool success = outmsg == "Success";
            Logger.Info($"[Daou] 결제 완료 - 결과: {outmsg}, 성공: {success}");
            return success;
        }
        catch (Exception ex)
        {
            Logger.Error($"[Daou] 결제 처리 중 예외 발생: {ex.Message}", ex);
            outmsg = "Exception: " + ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 연결 테스트
    /// </summary>
    /// <returns>연결 성공 여부</returns>
    public static bool TestConnection()
    {
        try
        {
            Logger.Info("[Daou] 연결 테스트 시작");
            
            if (IsTestMode)
            {
                Logger.Info("[Daou] 테스트 모드 - 연결 테스트 성공");
                return true;
            }

            ConnectDaouServer();
            bool result = isConnected;
            
            if (result)
            {
                Logger.Info("[Daou] 연결 테스트 성공");
            }
            else
            {
                Logger.Warning("[Daou] 연결 테스트 실패");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error($"[Daou] 연결 테스트 중 오류: {ex.Message}", ex);
            return false;
        }
    }

    public static StringBuilder insertLeftZero(StringBuilder target, string item, int maxLen)
    {
        int myLen = maxLen;

        if (item.Length < maxLen)
        {
            myLen = myLen - item.Length;
            for (int i = 0; i < myLen; i++)
            {
                target.Append("0");
            }

            target.Append(item);

            return target;
        }
        else if (item.Length == maxLen)
        {
            target.Append(item);

            return target;
        }
        else
        {
            for (int i = 0; i < myLen; i++)
            {
                target.Append(item[i]);
            }

            return target;
        }
    }

    public static StringBuilder insertLeftJustify(StringBuilder target, string item, int maxLen)
    {
        int myLen = maxLen;

        if (item.Length < maxLen)
        {
            target.Append(item);

            myLen = myLen - item.Length;

            for (int i = 0; i < myLen; i++)
            {
                target.Append(" ");
            }

            return target;
        }
        else if (item.Length == maxLen)
        {
            target.Append(item);

            return target;
        }
        else
        {
            for (int i = 0; i < myLen; i++)
            {
                target.Append(item[i]);
            }

            return target;
        }
    }

    public static string Message(string input)
    {
        string output = string.Empty;

        switch (input)
        {
            case "K001":
                output = "[" + input + "]" + "리더기 에러" + "\r\n";
                break;
            case "K002":
                output = "[" + input + "]" + "IC 카드 거래 불가" + "\r\n";
                break;
            case "K003":
                output = "[" + input + "]" + "M/S카드 거래 불가" + "\r\n";
                break;
            case "K004":
                output = "[" + input + "]" + "마그네틱 카드를 읽혀주세요(FALL-BACK)" + "\r\n";
                break;
            case "K005":
                output = "[" + input + "]" + "IC카드가 삽입되어 있습니다. 제거해주세요." + "\r\n";
                break;
            case "K006":
                output = "[" + input + "]" + "카드거절" + "\r\n";
                break;
            case "K007":
                output = "[" + input + "]" + "결제도중 카드를 제거하였습니다." + "\r\n";
                break;
            case "K008":
                output = "[" + input + "]" + "결제도중 카드를 제거하였습니다." + "\r\n";
                break;
            case "K011":
                output = "[" + input + "]" + "IC리더기 키 다운로드 요망" + "\r\n";
                break;
            case "K012":
                output = "[" + input + "]" + "리더기 템퍼 오류" + "\r\n";
                break;
            case "K013":
                output = "[" + input + "]" + "상호인증오류" + "\r\n";
                break;
            case "K014":
                output = "[" + input + "]" + "암/복호와 오류" + "\r\n";
                break;
            case "K015":
                output = "[" + input + "]" + "무결성 검사 실패(다우데이타 관리자에게 문의 하시기 바랍니다.)" + "\r\n";
                break;
            case "K101":
                output = "[" + input + "]" + "커맨드 전달 실패" + "\r\n";
                break;
            case "K102":
                output = "[" + input + "]" + "키트로닉스 모듈 타임아웃" + "\r\n";
                break;
            case "K901":
                output = "[" + input + "]" + "카드사 파라미터 데이터 오류" + "\r\n";
                break;
            case "K902":
                output = "[" + input + "]" + "카드사 파라미터 반영불가" + "\r\n";
                break;
            case "K990":
                output = "[" + input + "]" + "사용자 강제종료" + "\r\n";
                break;
            case "K997":
                output = "[" + input + "]" + "리더기에서 전달받은 데이터이상" + "\r\n";
                break;
            case "K998":
                output = "[" + input + "]" + "리더기 타임아웃" + "\r\n";
                break;
            case "K999":
                output = "[" + input + "]" + "전문 오류" + "\r\n";
                break;
            case "K081":
                output = "[" + input + "]" + "IC카드를 넣어주세요" + "\r\n";
                break;
            case "K082":
                output = "[" + input + "]" + "처리불가 상태" + "\r\n";
                break;
            case "K083":
                output = "[" + input + "]" + "카드 입력 취소" + "\r\n";
                break;
            case "S000":
                output = "[" + input + "]" + "외부 서명 데이터 입력" + "\r\n";
                break;
            case "S001":
                output = "[" + input + "]" + "외부 서명 데이터 입력 실패 ( Time Out )" + "\r\n";
                break;
            case "S100":
                output = "[" + input + "]" + "외부 서명 데이터 입력 성공" + "\r\n";
                break;
            case "S111":
                output = "[" + input + "]" + "외부 서명 데이터 입력 취소" + "\r\n";
                break;
            case "S052":
                output = "[" + input + "]" + "신용 카드 거래 취소" + "\r\n";
                break;
            case "S053":
                output = "[" + input + "]" + "현금 영수증 거래 취소" + "\r\n";
                break;
            case "S054":
                output = "[" + input + "]" + "현금 IC 거래 취소" + "\r\n";
                break;
            case "K110":
                output = "[" + input + "]" + "KIOSK 카드 리딩 완료" + "\r\n";
                break;
            default:
                output = "[" + input + "]" + "알 수 없는 응답" + "\r\n";
                break;
        }

        if (input.Length > 8)
        {
            if (input.Substring(4, 4) == "C010")
            {
                string ErrCode = input.Substring(0, 4);     // 에러 코드
                string ErrCnt = input.Substring(8, 2);      // 에러 갯수              00 : 정상 , 1 이상 에러 발생 갯수
                string DllCode = input.Substring(10, 2);     // DLL 에러               00 정상, 01 에러
                string NetCode = input.Substring(12, 2);     // 네트워크 상태          00 정상, 01 에러
                string AppCode = input.Substring(14, 2);     // 클라이언트 연결 상태  00 정상, 01 에러
                string ReaderCode = input.Substring(16, 2); // 리더기 상태            00 정상, 01 에러
                string SignCode = input.Substring(18, 2);   // 서명패드 상태          00 정상, 01 에러
                string IniCode = input.Substring(20, 2);    // INI 파일 상태          00 정상, 01 에러

                string strResultStat = "에러: " + ErrCnt;

                if (DllCode == "00")
                {
                    strResultStat += " DLL 상태: OK" + "\n" + "\r";
                }
                else
                {
                    strResultStat += " DLL 상태: FAIL" + "\n" + "\r";
                }

                if (NetCode == "00")
                {
                    strResultStat += " 네트워크 상태: OK" + "\n" + "\r";
                }
                else
                {
                    strResultStat += " 네트워크 상태: FAIL" + "\n" + "\r";
                }

                if (AppCode == "00")
                {
                    strResultStat += " 어플리케이션 상태: OK" + "\n";
                }
                else
                {
                    strResultStat += " 어플리케이션 상태: FAIL" + "\n";
                }

                if (ReaderCode == "00")
                {
                    strResultStat += " 리더기 상태: OK" + "\n";
                }
                else
                {
                    strResultStat += " 리더기 상태: FAIL" + "\n";
                }

                if (SignCode == "00")
                {
                    strResultStat += " 서명패드 상태: OK" + "\n";
                }
                else
                {
                    strResultStat += " 서명패드 상태: FAIL" + "\n";
                }

                if (IniCode == "00")
                {
                    strResultStat += " INI 상태: OK" + "\n" + "\r";
                }
                else
                {
                    strResultStat += " INI 상태: FAIL" + "\n" + "\r";
                }

                output = strResultStat;
            }
        }

        return output;
    }

    public static string getWordByByte(string src, int startCount, int byteCount)
    {
        System.Text.Encoding myEncoding = System.Text.Encoding.GetEncoding("ks_c_5601-1987");

        byte[] buf = myEncoding.GetBytes(src);

        return myEncoding.GetString(buf, startCount, byteCount);
    }
}