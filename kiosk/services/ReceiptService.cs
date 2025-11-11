using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using s_oil.Utils;
using s_oil.device;
using System.Windows.Forms;

namespace s_oil.Services
{
    /// <summary>
    /// 영수증 템플릿을 처리하고 프린터 출력을 담당하는 서비스
    /// </summary>
    public class ReceiptService
    {
        private readonly Printer _printer;
        private string _templatePath;

        public ReceiptService(Printer printer)
        {
            _printer = printer ?? throw new ArgumentNullException(nameof(printer));
            _templatePath = Path.Combine(Application.StartupPath, "device", "Receipt_template.txt");
        }

        /// <summary>
        /// 템플릿 파일 경로 설정
        /// </summary>
        /// <param name="templatePath">템플릿 파일 경로</param>
        public void SetTemplatePath(string templatePath)
        {
            _templatePath = templatePath;
        }

        /// <summary>
        /// 영수증 데이터를 프린터로 출력하여 인쇄합니다 (한글 최적화)
        /// </summary>
        /// <param name="receiptData">영수증 데이터</param>
        /// <returns>출력 성공 여부</returns>
        public async Task<bool> PrintReceiptAsync(ReceiptData receiptData)
        {
            try
            {
                // 프린터 연결 확인 및 재연결 시도
                if (!_printer.IsConnected)
                {
                    Logger.Warning("프린터가 연결되지 않음. 재연결 시도...");
                    bool connected = await _printer.ConnectAsync();
                    if (!connected)
                    {
                        Logger.Error("프린터 연결 실패");
                        return false;
                    }
                }

                // 템플릿 파일 읽기
                string template = await ReadTemplateAsync();
                if (string.IsNullOrEmpty(template))
                {
                    Logger.Error("영수증 템플릿 읽기 실패");
                    return false;
                }

                // 템플릿에 데이터 치환
                string receiptContent = ProcessTemplate(template, receiptData);
                
                Logger.Info("영수증 출력 시작 (ReceiptService - 한글 최적화)");
                
                // 한글 최적화된 출력 방식 사용
                await PrintReceiptWithKoreanSupport(receiptContent);
                
                // 용지 절단 (약간의 지연 후)
                await Task.Delay(500);
                _printer.CutPaper();
                
                Logger.Info("영수증 출력 완료 (ReceiptService)");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 출력 중 오류 (ReceiptService): {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 한글 지원 영수증 출력 (ReceiptService 전용)
        /// </summary>
        /// <param name="content">영수증 내용</param>
        private async Task PrintReceiptWithKoreanSupport(string content)
        {
            try
            {
                // Printer 클래스의 한글 최적화 기능 활용
                if (_printer != null)
                {
                    // 프린터 한글 모드 재설정 (Printer 클래스의 private 메서드와 유사한 로직)
                    await ResetPrinterForKorean();
                    
                    // 영수증 내용을 줄별로 출력
                    string[] lines = content.Split('\n');
                    
                    foreach (string line in lines)
                    {
                        try
                        {
                            string cleanLine = line.TrimEnd('\r');
                            
                            // 빈 줄 처리
                            if (string.IsNullOrWhiteSpace(cleanLine))
                            {
                                await Task.Delay(10);
                                continue;
                            }
                            
                            // 각 줄을 Print 메서드로 출력 (Print 메서드에 한글 최적화 적용됨)
                            _printer.Print(cleanLine);
                            await Task.Delay(50); // 각 줄 사이 짧은 지연
                        }
                        catch (Exception lineEx)
                        {
                            Logger.Warning($"줄 출력 중 오류 (ReceiptService): {lineEx.Message}, 줄: {line}");
                        }
                    }
                    
                    Logger.Info($"영수증 출력 완료 (ReceiptService): {lines.Length}줄");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"한글 지원 영수증 출력 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 한글 출력을 위한 프린터 재설정 (ReceiptService용)
        /// </summary>
        private async Task ResetPrinterForKorean()
        {
            try
            {
                // 프린터가 연결되어 있고 Printer 클래스의 TestConnection으로 확인
                if (_printer != null && _printer.IsConnected)
                {
                    // 간단한 재연결 테스트
                    _printer.TestConnection();
                    await Task.Delay(100);
                    
                    Logger.Info("프린터 한글 모드 재설정 완료 (ReceiptService)");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"프린터 한글 모드 재설정 중 오류 (ReceiptService): {ex.Message}");
            }
        }

        /// <summary>
        /// 템플릿 파일을 비동기적으로 읽습니다
        /// </summary>
        /// <returns>템플릿 내용</returns>
        private async Task<string> ReadTemplateAsync()
        {
            try
            {
                if (!File.Exists(_templatePath))
                {
                    Logger.Error($"Template file not found: {_templatePath}");
                    return string.Empty;
                }

                return await File.ReadAllTextAsync(_templatePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error reading template file: {ex.Message}", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 템플릿의 플레이스홀더를 실제 데이터로 치환합니다
        /// </summary>
        /// <param name="template">템플릿 내용</param>
        /// <param name="data">영수증 데이터</param>
        /// <returns>처리된 영수증 내용</returns>
        private string ProcessTemplate(string template, ReceiptData data)
        {
            var result = template;

            // 회사 정보
            result = result.Replace("{company_name}", data.CompanyName ?? "");
            result = result.Replace("{phone_number}", data.PhoneNumber ?? "");
            result = result.Replace("{company_number}", data.CompanyNumber ?? "");
            result = result.Replace("{ceo_name}", data.CeoName ?? "");
            result = result.Replace("{company_address}", data.CompanyAddress ?? "");

            // 날짜/시간 정보
            result = result.Replace("{YYYY-MM-DD}", data.TransactionDate.ToString("yyyy-MM-dd"));
            result = result.Replace("{HH:mm:ss}", data.TransactionDate.ToString("HH:mm:ss"));
            result = result.Replace("{gas_pump_number}", data.GasPumpNumber ?? "");
            result = result.Replace("{transaction_number}", data.TransactionNumber ?? "");

            // 상품 정보
            result = result.Replace("{fuel_type}", data.FuelType ?? "경유");
            result = result.Replace("{unit_price}", data.UnitPrice.ToString("N0"));
            result = result.Replace("{quantity}", data.Quantity.ToString("N3"));
            result = result.Replace("{total_amount}", data.TotalAmount.ToString("N0"));

            // 결제 정보
            result = result.Replace("{input_amount}", data.InputAmount.ToString("N0"));
            result = result.Replace("{change_amount}", data.ChangeAmount.ToString("N0"));
            result = result.Replace("{payment_method}", data.PaymentMethod ?? "");
            result = result.Replace("{card_number}", data.CardNumber ?? "");
            result = result.Replace("{approval_number}", data.ApprovalNumber ?? "");
            
            //  승인일시 추가
            string approvalDate = !string.IsNullOrEmpty(data.ApprovalDate) 
                ? data.ApprovalDate 
                : data.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
            result = result.Replace("{approval_date}", approvalDate);

            // 환불 정보 (하위 호환성을 위해 유지)
            result = result.Replace("{refund_amount}", data.RefundAmount.ToString("N0"));

            return result;
        }

        /// <summary>
        /// 영수증 출력 테스트
        /// </summary>
        /// <returns>테스트 성공 여부</returns>
        public async Task<bool> TestPrintAsync()
        {
            try
            {
                var testData = new ReceiptData
                {
                    CompanyName = "S-OIL 테스트 주유소",
                    PhoneNumber = "02-1234-5678",
                    CompanyNumber = "123-45-67890",
                    CeoName = "홍길동",
                    CompanyAddress = "서울시 강남구 테스트로 123",
                    TransactionDate = DateTime.Now,
                    GasPumpNumber = "01",
                    TransactionNumber = "T" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    FuelType = "경유",
                    UnitPrice = 1468,
                    Quantity = 20.000m,
                    TotalAmount = 29360,
                    InputAmount = 30000,
                    ChangeAmount = 640,
                    PaymentMethod = "신용카드",
                    CardNumber = "****-****-****-5678",
                    ApprovalNumber = "A" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    ApprovalDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // 승인일시 추가
                    RefundAmount = 0
                };

                return await PrintReceiptAsync(testData);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in test print: {ex.Message}", ex);
                return false;
            }
        }
    }

    /// <summary>
    /// 영수증 데이터 클래스
    /// </summary>
    public class ReceiptData
    {
        // 회사 정보
        public string CompanyName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string CompanyNumber { get; set; } = "";
        public string CeoName { get; set; } = "";
        public string CompanyAddress { get; set; } = "";

        // 거래 정보
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string GasPumpNumber { get; set; } = "";
        public string TransactionNumber { get; set; } = "";

        // 상품 정보
        public string FuelType { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }

        // 결제 정보
        public decimal InputAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string ApprovalNumber { get; set; } = "";
        //  승인일시 필드 추가
        public string ApprovalDate { get; set; } = "";

        // 환불 정보 (필요시)
        public decimal RefundAmount { get; set; }

        // 여러 상품 구매를 위한 리스트
        public List<ReceiptProductItem> ProductItems { get; set; } = new List<ReceiptProductItem>();
    }

    /// <summary>
    /// 영수증 상품 항목 클래스
    /// </summary>
    public class ReceiptProductItem
    {
        public string ProductName { get; set; } = "";
        public int Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}