using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.device;
using s_oil.Forms;
using s_oil.Forms.Customer;
using s_oil.Services;
using s_oil.Utils;

namespace s_oil.Services
{
    public class ApplicationContext
    {
        private static ApplicationContext? _instance;

        public static ApplicationContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApplicationContext();
                }
                return _instance;
            }
        }

        public string? AreaCode { get; set; }

        public BU BUDevice { get; set; }
        public Printer PrinterDevice { get; set; }
        public ReceiptService ReceiptService { get; set; }

        // ?? 결제 디바이스 관련 추가
        public object PayDevice { get; private set; }

        public DBConnector Database { get; set; }
        public Navigator Navigator { get; set; }

        private readonly Dictionary<Type, Form> _forms;

        private ApplicationContext()
        {
            _forms = new Dictionary<Type, Form>();
            Database = DBConnector.Instance;
            BUDevice = new BU();
            PrinterDevice = new Printer();
            ReceiptService = new ReceiptService(PrinterDevice);
            
            // ?? 결제 디바이스 초기화 (간단한 더미 객체)
            PayDevice = new { LastResultMessage = "" };
        }

        public async Task InitializeAsync()
        {
            try
            {
                Logger.Info("ApplicationContext 초기화 시작");

                InitializeDatabase();
                await InitializeDevicesAsync();
                InitializeServices();
                InitializeForms();
                
                //  SoundManager 초기화 테스트
                try
                {
                    Logger.Info("SoundManager 연결 테스트 시작");
                    // SoundManager는 싱글톤이므로 첫 접근 시 자동 초기화됨
                    var soundManagerTest = SoundManager.Instance;
                    Logger.Info("SoundManager 초기화 완료");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"SoundManager 초기화 실패: {ex.Message}");
                }

                Logger.Info("ApplicationContext 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"ApplicationContext 초기화 중 오류: {ex.Message}", ex);
                Logger.Error($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task InitializeDevicesAsync()
        {
            try
            {
                Logger.Info("디바이스 초기화 시작");

                // BU 디바이스 초기화 (실패해도 계속 진행)
                try
                {
                    await InitializeBUDeviceAsync();
                    Logger.Info("BU 디바이스 초기화 완료");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"BU 디바이스 초기화 실패: {ex.Message}");
                }

                // 프린터 디바이스 초기화 (타임아웃 설정)
                try
                {
                    Logger.Info("프린터 연결 시도 중...");
                    
                    // 5초 타임아웃 설정
                    var printerConnectTask = PrinterDevice.ConnectAsync();
                    var timeoutTask = Task.Delay(5000);
                    
                    var completedTask = await Task.WhenAny(printerConnectTask, timeoutTask);
                    
                    if (completedTask == printerConnectTask)
                    {
                        var printerConnected = await printerConnectTask;
                        Logger.Info($"프린터 디바이스 연결: {(printerConnected ? "성공" : "실패")}");
                    }
                    else
                    {
                        Logger.Warning("프린터 연결 타임아웃 (5초) - 계속 진행합니다.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"프린터 디바이스 연결 실패: {ex.Message}");
                }

                Logger.Info("디바이스 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"디바이스 초기화 중 심각한 오류: {ex.Message}", ex);
                // 디바이스 초기화 실패해도 프로그램은 계속 실행
            }
        }

        /// <summary>
        /// ?? BU 디바이스 초기화 (설정 파일 기반)
        /// </summary>
        private async Task InitializeBUDeviceAsync()
        {
            try
            {
                Logger.Info("BU 디바이스 초기화 시작");

                // INI 파일에서 PLC 설정 로드
                var iniPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);

                // PLC 설정 읽기
                string plcIP = iniParser.GetSetting("PLC", "PLC1_IP");
                string plcPortStr = iniParser.GetSetting("PLC", "PLC1_Port");
                string plcEnabledStr = iniParser.GetSetting("PLC", "PLC1_Enabled");

                bool plcEnabled = plcEnabledStr.ToLower() == "true";
                
                if (!plcEnabled)
                {
                    Logger.Info("PLC가 비활성화되어 있습니다. BU 디바이스를 기본 모드로 초기화합니다.");
                    BUDevice = new BU(); // 기본 생성자 사용
                    return;
                }

                if (int.TryParse(plcPortStr, out int plcPort))
                {
                    Logger.Info($"PLC 설정 로드 - IP: {plcIP}, Port: {plcPort}");
                    
                    // IP와 Port를 사용하여 BU 디바이스 초기화
                    BUDevice = new BU(plcIP, plcPort, false);
                    
                    // OpenCloseInverse 설정 로드
                    string openCloseInverseStr = iniParser.GetSetting("Database", "OpenCloseInverse", "1");
                    if (int.TryParse(openCloseInverseStr, out int openCloseInverse))
                    {
                        BUDevice.IsOpenCloseInverse = (openCloseInverse == 1);
                        Logger.Info($"OpenCloseInverse 설정: {BUDevice.IsOpenCloseInverse}");
                    }

                    //  PLC 연결 시도 
                    Logger.Info($"PLC 연결 시도 중: {plcIP}:{plcPort}");
                    
                    var connectTask = Task.Run(() => BUDevice.Connect(plcIP, plcPort));
                    var timeoutTask = Task.Delay(3000); // 3초 타임아웃
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == connectTask)
                    {
                        bool connected = await connectTask;
                        if (connected)
                        {
                            Logger.Info($" PLC 연결 성공: {plcIP}:{plcPort}");
                        }
                        else
                        {
                            Logger.Warning($" PLC 연결 실패: {plcIP}:{plcPort}");
                        }
                    }
                    else
                    {
                        Logger.Warning($" PLC 연결 타임아웃 (3초): {plcIP}:{plcPort}");
                    }

                    Logger.Info("BU 디바이스 초기화 완료 (PLC 설정 적용)");
                }
                else
                {
                    Logger.Warning($"잘못된 PLC 포트 설정: {plcPortStr}. 기본 설정으로 초기화합니다.");
                    BUDevice = new BU();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"BU 디바이스 초기화 중 오류: {ex.Message}", ex);
                
                // 오류 발생 시 기본 BU 디바이스라도 생성
                try
                {
                    BUDevice = new BU();
                    Logger.Info("기본 BU 디바이스로 대체 생성 완료");
                }
                catch (Exception fallbackEx)
                {
                    Logger.Error($"기본 BU 디바이스 생성도 실패: {fallbackEx.Message}", fallbackEx);
                }
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                Logger.Info("데이터베이스 초기화");
                // 데이터베이스 관련 초기화 작업
            }
            catch (Exception ex)
            {
                Logger.Error($"데이터베이스 초기화 중 오류: {ex.Message}", ex);
            }
        }

        private void InitializeServices()
        {
            try
            {
                Logger.Info("서비스 초기화");
                // 서비스 관련 초기화 작업
            }
            catch (Exception ex)
            {
                Logger.Error($"서비스 초기화 중 오류: {ex.Message}", ex);
            }
        }

        private void InitializeForms()
        {
            try
            {
                Logger.Info("폼 초기화 시작");
                
                // ?? 주요 Forms 미리 생성하여 렌더링 오류 방지
                try
                {
                    // HomePage 미리 생성
                    var homePage = CreateForm<Forms.HomePage>();
                    homePage.Hide(); // 숨김 처리
                    Logger.Info("HomePage 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"HomePage 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // ProductPage 미리 생성
                    var productPage = CreateForm<Forms.Customer.ProductPage
                        >();
                    productPage.Hide(); // 숨김 처리
                    Logger.Info("ProductPage 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"ProductPage 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // CardPayPage 미리 생성
                    var cardPayPage = CreateForm<Forms.Customer.CardPayPage>();
                    cardPayPage.Hide(); // 숨김 처리
                    Logger.Info("CardPayPage 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"CardPayPage 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // ProductStatus 미리 생성
                    var productStatus = CreateForm<Forms.Customer.ProductStatus>();
                    productStatus.Hide(); // 숨김 처리
                    Logger.Info("ProductStatus 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"ProductStatus 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // AdminPassword 미리 생성 (Admin 폴더에 있는지 확인)
                    var adminPassword = CreateForm<Forms.AdminPassword>();
                    adminPassword.Hide(); // 숨김 처리
                    Logger.Info("AdminPassword 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"AdminPassword 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // AdminControl 미리 생성 (Admin 폴더에 있는지 확인)
                    var adminControl = CreateForm<Forms.AdminControl>();
                    adminControl.Hide(); // 숨김 처리
                    Logger.Info("AdminControl 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"AdminControl 생성 실패: {ex.Message}", ex);
                }

                try
                {
                    // Notice 미리 생성
                    var notice = CreateForm<Forms.Customer.Notice>();
                    notice.Hide(); // 숨김 처리
                    Logger.Info("Notice 미리 생성 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Notice 생성 실패: {ex.Message}", ex);
                }

                Logger.Info("폼 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"폼 초기화 중 오류: {ex.Message}", ex);
            }
        }

        public T CreateForm<T>() where T : Form, new()
        {
            try
            {
                // 기존 폼이 있으면 제거
                if (_forms.ContainsKey(typeof(T)))
                {
                    var oldForm = _forms[typeof(T)];
                    try
                    {
                        oldForm?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"기존 폼 해제 중 오류: {ex.Message}");
                    }
                    _forms.Remove(typeof(T));
                }

                var form = new T();
                
                // IContextAware 인터페이스를 구현한 폼에 컨텍스트 설정
                if (form is IContextAware contextAware)
                {
                    contextAware.SetContext(this);
                }

                _forms[typeof(T)] = form;
                
                Logger.Info($"Form {typeof(T).Name} created successfully");
                return form;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error creating form {typeof(T).Name}: {ex.Message}", ex);
                
                //  Form 생성 실패 시 대체 처리
                try
                {
                    // 기본 생성자를 사용하여 다시 시도
                    var form = Activator.CreateInstance<T>();
                    if (form is IContextAware contextAware)
                    {
                        contextAware.SetContext(this);
                    }
                    
                    _forms[typeof(T)] = form;
                    Logger.Info($"Form {typeof(T).Name} created successfully (fallback method)");
                    return form;
                }
                catch (Exception fallbackEx)
                {
                    Logger.Error($"Fallback form creation also failed for {typeof(T).Name}: {fallbackEx.Message}", fallbackEx);
                    throw new InvalidOperationException($"Failed to create form {typeof(T).Name}", ex);
                }
            }
        }

        public T GetForm<T>() where T : Form, new()
        {
            try
            {
                if (_forms.TryGetValue(typeof(T), out Form? form))
                {
                    return (T)form;
                }

                //  폼이 없으면 새로 생성
                Logger.Warning($"Form {typeof(T).Name} not found. Creating new instance.");
                return CreateForm<T>();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting form {typeof(T).Name}: {ex.Message}", ex);
                throw new InvalidOperationException($"Form of type {typeof(T).Name} could not be retrieved or created.", ex);
            }
        }

        public void SetNavigator(Navigator navigator)
        {
            Navigator = navigator;
        }

        /// <summary>
        /// 영수증을 출력합니다
        /// </summary>
        /// <param name="receiptData">영수증 데이터</param>
        /// <returns>출력 성공 여부</returns>
        public async Task<bool> PrintReceiptAsync(ReceiptData receiptData)
        {
            try
            {
                // 프린터가 초기화되지 않은 경우
                if (PrinterDevice == null)
                {
                    Logger.Warning("PrinterDevice가 초기화되지 않았습니다.");
                    return false;
                }

                return await PrinterDevice.PrintReceiptAsync(receiptData);
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 출력 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 영수증 출력 테스트
        /// </summary>
        /// <returns>테스트 성공 여부</returns>
        public async Task<bool> TestReceiptPrintAsync()
        {
            try
            {
                // 프린터가 초기화되지 않은 경우
                if (PrinterDevice == null)
                {
                    Logger.Warning("PrinterDevice가 초기화되지 않았습니다.");
                    return false;
                }

                return await PrinterDevice.TestReceiptPrintAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 테스트 출력 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 프린터 디바이스 상태를 확인하고 필요시 재연결을 시도합니다
        /// </summary>
        /// <returns>프린터 연결 성공</returns>
        public async Task<bool> CheckAndReconnectPrinterAsync()
        {
            try
            {
                if (PrinterDevice == null)
                {
                    Logger.Warning("PrinterDevice가 null입니다. 재초기화를 시도합니다.");
                    PrinterDevice = new Printer();
                }

                if (!PrinterDevice.IsConnected)
                {
                    Logger.Info("프린터가 연결되지 않음. 재연결을 시도합니다.");
                    return await PrinterDevice.ConnectAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"프린터 상태 확인 및 재연결 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        ///  결제 디바이스 연결 확인 및 재연결
        /// </summary>
        /// <returns>결제 디바이스 연결 성공</returns>
        public async Task<bool> CheckAndReconnectPayDeviceAsync()
        {
            try
            {
                Logger.Info("결제 디바이스 연결 확인");
                
                //  테스트 모드 확인
                bool isTestMode = CheckTestMode();
                if (isTestMode)
                {
                    Logger.Info("테스트 모드가 활성화되어 있습니다. 연결 테스트를 건너뜁니다.");
                    return true; // 테스트 모드에서는 항상 성공으로 처리
                }
                
                // DaouCtrl을 사용한 연결 테스트
                bool testResult = await Task.Run(() => DaouCtrl.TestConnection());
                
                if (testResult)
                {
                    Logger.Info("결제 디바이스 연결 성공");
                }
                else
                {
                    Logger.Warning("결제 디바이스 연결 실패");
                }
                
                return testResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 디바이스 재연결 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        ///  카드 결제 처리
        /// </summary>
        /// <param name="amount">결제 금액</param>
        /// <returns>결제 성공 여부</returns>
        public async Task<bool> ProcessCardPaymentAsync(int amount)
        {
            try
            {
                Logger.Info($"카드 결제 처리 시작 - 금액: {amount:N0}원");
                
                // ?? 테스트 모드 확인
                bool isTestMode = CheckTestMode();
                if (isTestMode)
                {
                    Logger.Info($"테스트 모드로 결제 처리 - 금액: {amount:N0}원");
                }
                
                // DaouCtrl을 사용한 카드 결제
                bool paymentResult = await Task.Run(() => DaouCtrl.Pay(amount));
                
                if (paymentResult)
                {
                    Logger.Info("카드 결제 성공");
                }
                else
                {
                    Logger.Warning($"카드 결제 실패: {DaouCtrl.outmsg}");
                }
                
                return paymentResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"카드 결제 처리 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// ?? 테스트 모드 확인
        /// </summary>
        /// <returns>테스트 모드 여부</returns>
        private bool CheckTestMode()
        {
            try
            {
                var iniPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string testMode = iniParser.GetSetting("Is_Test", "Test", "false");
                
                bool isTestMode = testMode.ToLower() == "true";
                Logger.Info($"테스트 모드 설정: {isTestMode}");
                
                return isTestMode;
            }
            catch (Exception ex)
            {
                Logger.Error($"테스트 모드 확인 중 오류: {ex.Message}", ex);
                return false; // 오류 시 기본값으로 false 반환
            }
        }

        /// <summary>
        /// 디바이스 연결 상태를 확인합니다
        /// </summary>
        /// <returns>디바이스 연결 상태 정보</returns>
        public (bool BU, bool Printer) CheckAllDeviceStatus()
        {
            try
            {
                Logger.Info("디바이스 상태 확인 시작");

                // BU 디바이스 상태 확인 - 실제 연결 상태 체크
                bool buStatus = CheckBUDeviceStatus();
                
                // 프린터 디바이스 상태 확인
                bool printerStatus = PrinterDevice?.IsConnected ?? false;
                
                

                Logger.Info($"Device Status Check - BU: {buStatus}, Printer: {printerStatus}");
                
                // 상세한 디바이스 상태 로깅
                if (!buStatus)
                {
                    Logger.Warning("BU device is not connected. Box control may not work.");
                }
                if (!printerStatus)
                {
                    Logger.Warning("Printer device is not connected. Receipt printing may not work.");
                }
              

                return (buStatus, printerStatus);
            }
            catch (Exception ex)
            {
                Logger.Error($"디바이스 상태 확인 중 오류: {ex.Message}", ex);
                return (false, false);
            }
        }

        /// <summary>
        /// BU 디바이스 연결 상태를 확인합니다
        /// </summary>
        /// <returns>BU 디바이스 연결 상태</returns>
        private bool CheckBUDeviceStatus()
        {
            try
            {
                if (BUDevice == null)
                {
                    Logger.Info("BU 디바이스가 초기화되지 않았습니다.");
                    return false;
                }

                // 테스트 모드 확인
                if (CheckTestMode())
                {
                    Logger.Info(" 테스트 모드 - BU 디바이스 상태: 시뮬레이션 모드로 정상");
                    return true;
                }

                // BU 디바이스의 연결 상태 확인
                if (string.IsNullOrEmpty(BUDevice.ServerIP) || BUDevice.ServerPort <= 0)
                {
                    Logger.Warning($"BU 디바이스 연결 정보가 유효하지 않습니다. IP: {BUDevice.ServerIP}, Port: {BUDevice.ServerPort}");
                    return false;
                }

                // 실제 연결 상태 확인
                bool isConnected = BUDevice.IsConnected;
                Logger.Info($"BU 디바이스 연결 상태: {isConnected} (IP: {BUDevice.ServerIP}:{BUDevice.ServerPort})");

                // 연결되지 않은 경우 간단한 연결 테스트 시도
                if (!isConnected)
                {
                    Logger.Info("BU 디바이스 연결 테스트를 시도합니다...");
                    try
                    {
                        // 빠른 연결 테스트 (1초 타임아웃)
                        bool quickTest = TestBUConnectionQuick();
                        Logger.Info($"BU 디바이스 빠른 연결 테스트 결과: {quickTest}");
                        return quickTest;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"BU 디바이스 연결 테스트 중 오류: {ex.Message}");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"BU 디바이스 상태 확인 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 결제 디바이스 연결 상태를 확인합니다
        /// </summary>
        /// <returns>결제 디바이스 연결 상태</returns>
        private bool CheckPayDeviceStatus()
        {
            try
            {
                // 테스트 모드 확인
                if (CheckTestMode())
                {
                    Logger.Info("?? 테스트 모드 - 결제 디바이스 상태: 시뮬레이션 모드로 정상");
                    return true;
                }

                // DaouCtrl 연결 테스트
                bool payResult = DaouCtrl.TestConnection();
                Logger.Info($"결제 디바이스 연결 상태: {payResult}");
                
                return payResult;
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 디바이스 상태 확인 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// BU 디바이스 빠른 연결 테스트
        /// </summary>
        /// <returns>연결 테스트 결과</returns>
        private bool TestBUConnectionQuick()
        {
            try
            {
                if (BUDevice == null || string.IsNullOrEmpty(BUDevice.ServerIP))
                {
                    return false;
                }

                // 간단한 TCP 연결 테스트 (1초 타임아웃)
                using (var tcpClient = new System.Net.Sockets.TcpClient())
                {
                    var result = tcpClient.BeginConnect(BUDevice.ServerIP, BUDevice.ServerPort, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                    
                    if (success)
                    {
                        try
                        {
                            tcpClient.EndConnect(result);
                            Logger.Info($"BU 디바이스 빠른 연결 테스트 성공: {BUDevice.ServerIP}:{BUDevice.ServerPort}");
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Logger.Info($"BU 디바이스 빠른 연결 테스트 실패: {BUDevice.ServerIP}:{BUDevice.ServerPort} (1초 타임아웃)");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"BU 디바이스 빠른 연결 테스트 중 오류: {ex.Message}");
                return false;
            }
        }
    }
}