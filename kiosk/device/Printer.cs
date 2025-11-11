using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.Services;
using s_oil.Utils;

namespace s_oil.device
{
    /// <summary>
    /// ESC/POS Command Set for Thermal Printers.
    /// Commands have been revised for stable Korean language support based on the HMK-072 series manual.
    /// </summary>
    public static class PrinterCommands
    {
        // --- Basic Commands ---
        public static readonly byte[] INITIALIZE = { 0x1B, 0x40 };         // Initialize printer
        public static readonly byte[] LINE_FEED = { 0x0A };             // Line feed
        public static readonly byte[] CUT_PAPER = { 0x1D, 0x56, 0x42, 0x00 }; // Cut paper (partial cut)
        public static readonly byte[] BOLD_ON = { 0x1B, 0x45, 0x01 };     // Bold on
        public static readonly byte[] BOLD_OFF = { 0x1B, 0x45, 0x00 };    // Bold off
        public static readonly byte[] ALIGN_LEFT = { 0x1B, 0x61, 0x00 };   // Align left
        public static readonly byte[] ALIGN_CENTER = { 0x1B, 0x61, 0x01 }; // Align center
        public static readonly byte[] ALIGN_RIGHT = { 0x1B, 0x61, 0x02 };  // Align right
        public static readonly byte[] FONT_A = { 0x1B, 0x4D, 0x00 };      // Default font A
        public static readonly byte[] FONT_B = { 0x1B, 0x4D, 0x01 };      // Small font B
        public static readonly byte[] RESET_STYLE = { 0x1B, 0x21, 0x00 };  // Reset all text styles

        // --- Left Margin Setting ---
        /// <summary>
        /// Set Left Margin (ESC GS L nL nH)
        /// 왼쪽 여백을 설정하여 전체 텍스트를 오른쪽으로 이동
        /// 기본값: 10 문자 (약 5mm 이동)
        /// </summary>
        public static readonly byte[] SET_LEFT_MARGIN_10 = { 0x1D, 0x4C, 0x0A, 0x00 }; // 10 dots left margin
        public static readonly byte[] SET_LEFT_MARGIN_20 = { 0x1D, 0x4C, 0x14, 0x00 }; // 20 dots left margin
        public static readonly byte[] SET_LEFT_MARGIN_30 = { 0x1D, 0x4C, 0x1E, 0x00 }; // 30 dots left margin
        public static readonly byte[] SET_LEFT_MARGIN_40 = { 0x1D, 0x4C, 0x28, 0x00 }; // 40 dots left margin

        // --- Optimized Korean Support Commands ---
        /// <summary>
        /// Select International Character Set: Korea (ESC R 13)
        /// Reference: HMK-072 Manual Page 56. This is crucial for telling the printer to expect Korean characters.
        /// </summary>
        public static readonly byte[] SELECT_KOREAN_CHARSET = { 0x1B, 0x52, 0x0D };

        /// <summary>
        /// Enable Hangul (2-Byte) Mode (FS &)
        /// Reference: HMK-072 Manual Page 60. This command enables the processing of 2-byte character codes.
        /// </summary>
        public static readonly byte[] ENABLE_HANGUL_MODE = { 0x1C, 0x26 };

        /// <summary>
        /// Set Code Page to CP949 (Wansung/EUC-KR) (ESC t 13)
        /// NOTE: This command can conflict with 2-byte mode and is often not needed if the character set is selected correctly.
        /// </summary>
        public static readonly byte[] SET_CODE_PAGE_949 = { 0x1B, 0x74, 0x0D };
    }

    public class Printer
    {
        private SerialPort serialPort;
        // 외부 템플릿 파일 경로는 더 이상 사용하지 않습니다 (코드 내장 템플릿 사용)

        public bool IsConnected => serialPort != null && serialPort.IsOpen;
        public event Action<bool> ConnectionStatusChanged;

        public Printer()
        {
            // Register EUC-KR encoding provider for .NET Core/.NET 5+ compatibility
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
            var iniParser = new IniParser(iniPath);
            var comPort = iniParser.GetSetting("Printer", "COM");
            var baudRate = Funcs.OToInt32(iniParser.GetSetting("Printer", "BAUDRATE"));
            Logger.Info($"SMT_Kiosk ini Printer Config - COM: {comPort}, BaudRate: {baudRate}");

            // 코드 내장 템플릿을 사용하므로 외부 파일 경로가 더 이상 필요하지 않습니다.
            // _receiptTemplatePath = Path.Combine(Application.StartupPath, "device", "Receipt_template.txt");

            try
            {
                serialPort = new SerialPort(comPort, baudRate);
                serialPort.DataReceived += SerialPort_DataReceived;
                Logger.Info($"Printer instance created with {comPort}, {baudRate} baud rate (using embedded template)");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create Printer instance", ex);
                throw;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadExisting();
                Logger.Info($"Printer received data: {data}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error in SerialPort_DataReceived", ex);
            }
        }

        public async Task<bool> ConnectAsync()
        {
            return await Task.Run(() => Connect());
        }

        public bool Connect()
        {
            try
            {
                if (serialPort == null)
                {
                    Logger.Error("SerialPort is null");
                    return false;
                }
                if (serialPort.IsOpen)
                {
                    Logger.Info("Printer is already connected");
                    return true;
                }

                serialPort.Open();

                // Initialize printer with the correct Korean settings
                InitializePrinter();

                Logger.Info($"Printer connected successfully to {serialPort.PortName}");
                ConnectionStatusChanged?.Invoke(true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Unexpected error connecting to printer on {serialPort?.PortName}: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Initializes the printer with a stable command sequence for Korean printing.
        /// </summary>
        private void InitializePrinter()
        {
            try
            {
                if (!IsConnected) return;

                // 1. Hardware Reset
                serialPort.Write(PrinterCommands.INITIALIZE, 0, PrinterCommands.INITIALIZE.Length);
                Thread.Sleep(200); // Wait for initialization to complete

                // 2. Set Left Margin (텍스트를 오른쪽으로 이동)
                // 원하는 여백 크기에 따라 변경 가능:
                 //SET_LEFT_MARGIN_10 = 작은 이동(기본 권장)
                // SET_LEFT_MARGIN_20 = 중간 이동
                // SET_LEFT_MARGIN_30 = 큰 이동
                // SET_LEFT_MARGIN_40 = 매우 큰 이동
                serialPort.Write(PrinterCommands.SET_LEFT_MARGIN_20, 0, PrinterCommands.SET_LEFT_MARGIN_20.Length);
                Thread.Sleep(100);

                // 3. Set Korean Character Mode
                // This sequence is more reliable. The combination of selecting the Korean character set
                // and enabling 2-byte Hangul mode is sufficient. Sending an additional code page
                // command (ESC t n) can cause conflicts.
                serialPort.Write(PrinterCommands.SELECT_KOREAN_CHARSET, 0, PrinterCommands.SELECT_KOREAN_CHARSET.Length); // ESC R 13
                Thread.Sleep(50);

                serialPort.Write(PrinterCommands.ENABLE_HANGUL_MODE, 0, PrinterCommands.ENABLE_HANGUL_MODE.Length); // FS &
                Thread.Sleep(50);

                // The following command is removed as it might conflict with the 2-byte Hangul mode.
                // serialPort.Write(PrinterCommands.SET_CODE_PAGE_949, 0, PrinterCommands.SET_CODE_PAGE_949.Length); // ESC t 13

                // 4. Set Default Print Style
                serialPort.Write(PrinterCommands.ALIGN_LEFT, 0, PrinterCommands.ALIGN_LEFT.Length);
                Thread.Sleep(50);
                serialPort.Write(PrinterCommands.FONT_A, 0, PrinterCommands.FONT_A.Length);
                Thread.Sleep(50);

                Logger.Info("Printer initialized with optimized Korean support and left margin.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error initializing printer: {ex.Message}", ex);
            }
        }

        public void Close()
        {
            try
            {
                if (IsConnected)
                {
                    serialPort.Close();
                    Logger.Info("Printer disconnected successfully");
                    ConnectionStatusChanged?.Invoke(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error closing printer connection", ex);
            }
        }

        public void Print(string text)
        {
            try
            {
                if (IsConnected)
                {
                    byte[] textData = GetEncodedBytes(text + "\n"); // Add newline for simplicity
                    serialPort.Write(textData, 0, textData.Length);
                    Logger.Info($"Printed text (EUC-KR encoding): {text.Substring(0, Math.Min(50, text.Length))}...");
                }
                else
                {
                    Logger.Warning("Cannot print: Printer is not connected");
                    throw new InvalidOperationException("Printer is not connected.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error printing text: {ex.Message}", ex);
                throw;
            }
        }

        public void CutPaper()
        {
            try
            {
                if (IsConnected)
                {
                    // Add some line feeds before cutting for better presentation
                    serialPort.Write(PrinterCommands.LINE_FEED, 0, PrinterCommands.LINE_FEED.Length);
                    serialPort.Write(PrinterCommands.LINE_FEED, 0, PrinterCommands.LINE_FEED.Length);
                    serialPort.Write(PrinterCommands.LINE_FEED, 0, PrinterCommands.LINE_FEED.Length);
                    Thread.Sleep(100);
                    serialPort.Write(PrinterCommands.CUT_PAPER, 0, PrinterCommands.CUT_PAPER.Length);
                    Logger.Info("Paper cut command sent");
                }
                else
                {
                    Logger.Warning("Cannot cut paper: Printer is not connected");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error cutting paper: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Encodes text to EUC-KR (CP949), the standard for most Korean thermal printers.
        /// </summary>
        private byte[] GetEncodedBytes(string text)
        {
            try
            {
                // EUC-KR 인코딩 시 지원되지 않는 문자는 '?'로 대체(Fallback)합니다.
                // 이렇게 하면 인코딩 중 예외가 발생하지 않습니다.
                Encoding eucKrEncoding = Encoding.GetEncoding("EUC-KR",
                                                              new EncoderReplacementFallback("?"),
                                                              new DecoderReplacementFallback("?"));
                return eucKrEncoding.GetBytes(text);
            }
            catch (Exception ex)
            {
                // 위의 Fallback 처리로 인해 이 catch 블록에 도달할 가능성은 거의 없지만,
                // 만약을 위해 로깅과 최종 Fallback 로직을 유지합니다.
                Logger.Error($"Text encoding to EUC-KR failed unexpectedly: {ex.Message}", ex);
                return Encoding.ASCII.GetBytes(new string(text.Select(c => c < 128 ? c : '?').ToArray()));
            }
        }

        public void Dispose()
        {
            try
            {
                Close();
                serialPort?.Dispose();
                serialPort = null;
                Logger.Info("Printer resources disposed");
            }
            catch (Exception ex)
            {
                Logger.Error("Error disposing printer resources", ex);
            }
        }

        public async Task<bool> PrintReceiptAsync(ReceiptData receiptData)
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.Warning("Printer not connected. Attempting to reconnect...");
                    if (!await ConnectAsync())
                    {
                        Logger.Error("Failed to connect to printer.");
                        return false;
                    }
                }

                string template = await ReadReceiptTemplateAsync();
                if (string.IsNullOrEmpty(template))
                {
                    Logger.Error("Failed to read receipt template.");
                    return false;
                }

                string receiptContent = ProcessReceiptTemplate(template, receiptData);

                Logger.Info("Starting receipt print job.");

                PrintReceiptInternal(receiptContent);

                await Task.Delay(500); // Wait for print buffer
                CutPaper();

                Logger.Info("Receipt printing complete.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during receipt printing: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 코드에 내장된 영수증 템플릿을 반환합니다.
        /// 외부 파일에 의존하지 않아 더 안정적이고 빠릅니다.
        /// </summary>
        private string GetEmbeddedReceiptTemplate()
        {
            return @"
===================================================
                   영  수  증
===================================================

상호명:{company_name}
사업자No:435-85-03272
주소: {company_address}
거래일시: {YYYY-MM-DD} {HH:mm:ss}

===================================================

{product_lines}

---------------------------------------------------
합계금액                     {total_amount}원
받은금액                     {input_amount}원

---------------------------------------------------
카드번호: {card_number}
승인일시: {approval_date}
승인번호: {approval_number}

---------------------------------------------------
이용해 주셔서 감사합니다
===================================================
";
        }

        private async Task<string> ReadReceiptTemplateAsync()
        {
            try
            {
                // 코드 내장 템플릿을 사용하여 외부 파일 의존성을 제거합니다.
                Logger.Info("Using embedded receipt template (no external file dependency)");
                return GetEmbeddedReceiptTemplate();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error getting embedded receipt template: {ex.Message}", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 기본 템플릿 생성 메서드 (더 이상 사용되지 않음)
        /// 코드 내장 템플릿을 사용하므로 외부 파일 생성이 불필요합니다.
        /// </summary>
      
        private string ProcessReceiptTemplate(string template, ReceiptData data)
        {
            var sb = new StringBuilder(template);

            sb.Replace("{company_name}", data.CompanyName ?? "S-OIL 키오스크");
            sb.Replace("{phone_number}", data.PhoneNumber ?? "1588-1234");
            sb.Replace("{company_number}", data.CompanyNumber ?? "123-45-67890");
            sb.Replace("{ceo_name}", data.CeoName ?? "대표자명");
            sb.Replace("{company_address}", data.CompanyAddress ?? "서울시 강남구");

            sb.Replace("{YYYY-MM-DD}", data.TransactionDate.ToString("yyyy-MM-dd"));
            sb.Replace("{HH:mm:ss}", data.TransactionDate.ToString("HH:mm:ss"));
            sb.Replace("{gas_pump_number}", data.GasPumpNumber ?? "01");
            sb.Replace("{transaction_number}", data.TransactionNumber ?? new Random().Next(10000, 99999).ToString());

            // 상품 라인 생성 (개선된 버전)
            string productLines = GenerateProductLines(data);
            sb.Replace("{product_lines}", productLines);

            // 이전 단일 상품 플레이스홀더는 하위 호환성을 위해 유지
            sb.Replace("{fuel_type}", data.FuelType ?? "키오스크 상품");
            sb.Replace("{unit_price}", data.UnitPrice.ToString("N0"));
            sb.Replace("{quantity}", data.Quantity.ToString("N0"));
            sb.Replace("{total_amount}", data.TotalAmount.ToString("N0"));

            sb.Replace("{input_amount}", data.InputAmount.ToString("N0"));
            sb.Replace("{change_amount}", data.ChangeAmount.ToString("N0"));
            
            //  결제 정보 추가
            sb.Replace("{payment_method}", data.PaymentMethod ?? "신용카드");
            sb.Replace("{card_number}", data.CardNumber ?? "****-****-****-0000");
            sb.Replace("{approval_number}", data.ApprovalNumber ?? "승인번호없음");
            
            //  승인일시 추가 (현재 거래일시 사용, 또는 별도 필드 사용 가능)
            string approvalDate = !string.IsNullOrEmpty(data.ApprovalDate) 
                ? data.ApprovalDate 
                : data.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
            sb.Replace("{approval_date}", approvalDate);

            sb.Replace("{refund_amount}", data.RefundAmount.ToString("N0"));

            Logger.Info("Receipt template processed successfully.");
            return sb.ToString();
        }

        /// <summary>
        /// 여러 상품의 상세 정보를 포함한 상품 라인을 생성합니다
        /// </summary>
        private string GenerateProductLines(ReceiptData data)
        {
            try
            {
                var sb = new StringBuilder();
                
                // 상품 목록이 있는 경우
                if (data.ProductItems != null && data.ProductItems.Count > 0)
                {
                    sb.AppendLine("상품명                 단가       수량");
                    sb.AppendLine("---------------------------------------------------");
                    
                    foreach (var item in data.ProductItems)
                    {
                        string productName = (item.ProductName ?? "상품").PadRight(15);
                        string price = $"{item.Price:N0}원".PadLeft(10);
                        string quantity = $"x {item.Quantity}".PadLeft(8);
                        
                        sb.AppendLine($"{productName} {price} {quantity}");
                    }
                }
                else
                {
                    // 기존 방식 (하위 호환성)
                    sb.AppendLine("상품명         단가     수량       금액");
                    sb.AppendLine($"{data.FuelType ?? "키오스크 상품"}      {data.UnitPrice:N0}원   X   {data.Quantity}개 =    {data.TotalAmount:N0}원");
                }
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error($"상품 라인 생성 중 오류: {ex.Message}", ex);
                // 오류 발생 시 기본 형식 반환
                return $"상품명           단가     수량       금액\n{data.FuelType ?? "키오스크 상품"}        {data.UnitPrice:N0}원   X   {data.Quantity}개 =    {data.TotalAmount:N0}원";
            }
        }

        private void PrintReceiptInternal(string content)
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.Warning("Cannot print receipt: Printer is not connected.");
                    throw new InvalidOperationException("Printer is not connected.");
                }

                // Ensure printer is in the correct state for Korean printing before the job
                InitializePrinter();

                string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    ApplyLineStyle(line);
                    Print(line); // Use the main Print method which adds a newline
                    Thread.Sleep(30); // Small delay between lines can help with some printers
                }

                Logger.Info($"Receipt content sent to printer: {lines.Length} lines.");
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred in PrintReceiptInternal: {ex.Message}", ex);
                throw;
            }
        }

        private void ApplyLineStyle(string line)
        {
            if (!IsConnected) return;

            // Reset styles first
            serialPort.Write(PrinterCommands.RESET_STYLE, 0, PrinterCommands.RESET_STYLE.Length);
            serialPort.Write(PrinterCommands.BOLD_OFF, 0, PrinterCommands.BOLD_OFF.Length);
            serialPort.Write(PrinterCommands.ALIGN_LEFT, 0, PrinterCommands.ALIGN_LEFT.Length);

            // Center alignment for titles
            if (line.Contains("S-OIL") || line.Contains("영수증") || line.Contains("감사합니다"))
            {
                serialPort.Write(PrinterCommands.ALIGN_CENTER, 0, PrinterCommands.ALIGN_CENTER.Length);
            }

            // Bold style for important lines
            if (line.Contains("S-OIL") || line.Contains("영수증") || line.Contains("합계금액") || line.Contains("결제 완료"))
            {
                serialPort.Write(PrinterCommands.BOLD_ON, 0, PrinterCommands.BOLD_ON.Length);
            }
            Thread.Sleep(10);
        }

        /// <summary>
        /// 프린터 연결 상태를 테스트합니다.
        /// </summary>
        /// <returns>연결 테스트 성공 여부</returns>
        public bool TestConnection()
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.Warning("프린터가 연결되지 않았습니다.");
                    return false;
                }

                // 간단한 연결 테스트를 위해 초기화 명령 전송
                serialPort.Write(PrinterCommands.INITIALIZE, 0, PrinterCommands.INITIALIZE.Length);
                Thread.Sleep(100);
                
                Logger.Info("프린터 연결 테스트 성공");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"프린터 연결 테스트 실패: {ex.Message}", ex);
                return false;
            }
        }


        /// <summary>
        /// 영수증 출력 테스트를 수행합니다.
        /// </summary>
        /// <returns>테스트 성공 여부</returns>
        public async Task<bool> TestReceiptPrintAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    Logger.Warning("프린터가 연결되지 않았습니다. 연결을 시도합니다.");
                    if (!await ConnectAsync())
                    {
                        Logger.Error("프린터 연결 실패");
                        return false;
                    }
                }

                Logger.Info("영수증 출력 테스트 시작 (내장 템플릿 사용)");

                // 테스트용 영수증 데이터 생성
                var testReceiptData = new ReceiptData
                {
                    CompanyName = "S-OIL 키오스크 테스트점",
                    PhoneNumber = "02-1234-5678",
                    CompanyNumber = "123-45-67890",
                    CeoName = "김대표",
                    CompanyAddress = "서울시 강남구 테스트로 123",
                    TransactionDate = DateTime.Now,
                    GasPumpNumber = "테스트박스01",
                    TransactionNumber = "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    FuelType = "테스트 상품",
                    UnitPrice = 10000,
                    Quantity = 1,
                    TotalAmount = 10000,
                    InputAmount = 10000,
                    ChangeAmount = 0,
                    PaymentMethod = "신용카드",
                    CardNumber = "****-****-****-1234",
                    ApprovalNumber = "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    RefundAmount = 0
                };

                // 영수증 출력 테스트
                bool result = await PrintReceiptAsync(testReceiptData);

                if (result)
                {
                    Logger.Info("영수증 출력 테스트 성공 (내장 템플릿)");
                }
                else
                {
                    Logger.Error("영수증 출력 테스트 실패");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 출력 테스트 중 오류: {ex.Message}", ex);
                return false;
            }
        }
    }
}

