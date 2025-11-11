using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.Services;
using s_oil.models;
using s_oil.Utils;
using System.IO;

namespace s_oil.Forms.Customer
{
    public partial class ProductStatus : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        private List<LockerButton> selectedLockers = new List<LockerButton>();

        public ProductStatus()
        {
            InitializeComponent();
            InitializeProductList();
        }

        /// <summary>
        /// Product_list ListView 초기화
        /// </summary>
        private void InitializeProductList()
        {
            // ListView 설정
            Product_list.View = View.Details;
            Product_list.FullRowSelect = true;
            Product_list.GridLines = false; // 그리드 라인 제거 (ProductPage와 동일)
            Product_list.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold); // ProductPage와 동일한 폰트
            Product_list.BorderStyle = BorderStyle.None; // 외곽 테두리 제거 (ProductPage와 동일)
            Product_list.HeaderStyle = ColumnHeaderStyle.Nonclickable; // 헤더 클릭 비활성화

            // 배경색과 텍스트 색상 설정 (ProductPage와 동일)
            Product_list.ForeColor = Color.FromArgb(0, 72, 37); // 텍스트 색상

            // 컬럼 추가 - 사물함번호 제거하고 상품 중심으로 변경
            Product_list.Columns.Add("상품코드", 120, HorizontalAlignment.Center);
            Product_list.Columns.Add("상품명", 400, HorizontalAlignment.Left);
            Product_list.Columns.Add("가격", 150, HorizontalAlignment.Right);
            Product_list.Columns.Add("상품유형", 150, HorizontalAlignment.Center);

            // 컬럼 헤더 스타일 개선
            foreach (ColumnHeader column in Product_list.Columns)
            {
                column.TextAlign = column.Index switch
                {
                    0 => HorizontalAlignment.Center, // 상품코드
                    1 => HorizontalAlignment.Left,   // 상품명
                    2 => HorizontalAlignment.Right,  // 가격
                    3 => HorizontalAlignment.Center, // 상품유형
                    _ => HorizontalAlignment.Left
                };
            }

            // 이벤트 핸들러 추가
            Product_list.DoubleClick += Product_list_DoubleClick;
            Product_list.Click += Product_list_Click;

            // 초기 데이터 로드
            LoadProductsFromDatabase();
        }

        /// <summary>
        /// Product_list 더블클릭 이벤트 - 상품 상세 정보 표시
        /// </summary>
        private void Product_list_DoubleClick(object? sender, EventArgs e)
        {
            try
            {
                if (Product_list.SelectedItems.Count > 0)
                {
                    var selectedItem = Product_list.SelectedItems[0];
                    var product = selectedItem.Tag as Product;
                    
                    if (product != null)
                    {
                        ShowProductDetail(product);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product_list_DoubleClick 오류: {ex.Message}");
                Logger.Error($"Product_list_DoubleClick 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Product_list 클릭 이벤트 - 해당 상품의 사물함들 찾기
        /// </summary>
        private void Product_list_Click(object? sender, EventArgs e)
        {
            try
            {
                if (Product_list.SelectedItems.Count > 0)
                {
                    var selectedItem = Product_list.SelectedItems[0];
                    var product = selectedItem.Tag as Product;
                    
                    if (product != null)
                    {
                        // 해당 상품이 들어있는 사물함들 찾기
                        HighlightProductLockers(product.productCode);
                        Console.WriteLine($"상품 '{product.productName}' 선택됨 - 해당 사물함들 하이라이트");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product_list_Click 오류: {ex.Message}");
                Logger.Error($"Product_list_Click 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 선택된 상품에 해당하는 사물함들을 하이라이트합니다
        /// </summary>
        /// <param name="productCode">상품 코드</param>
        private void HighlightProductLockers(string productCode)
        {
            try
            {
                // 기존 선택들 해제
                ClearAllSelections();

                // 해당 상품 코드를 가진 사물함들 찾아서 하이라이트
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();
                foreach (var button in lockerButtons)
                {
                    // 사물함의 상품 코드 확인 (임시로 ProductName에서 확인, 추후 개선 가능)
                    if (!string.IsNullOrEmpty(button.ProductName) && button.ProductName.Contains(productCode))
                    {
                        if (!button.IsUsing)
                        {
                            button.IsSelected = true;
                            button.UpdateButtonImage();
                            selectedLockers.Add(button);
                        }
                    }
                }

                Console.WriteLine($"상품코드 '{productCode}'에 해당하는 {selectedLockers.Count}개 사물함 하이라이트");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HighlightProductLockers 오류: {ex.Message}");
                Logger.Error($"HighlightProductLockers 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// DB에서 순수 상품 정보만 가져와서 표시합니다
        /// </summary>
        private void LoadProductsFromDatabase()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    Console.WriteLine("ProductStatus - AREACODE를 찾을 수 없습니다.");
                    return;
                }

                // 기존 아이템 모두 제거
                Product_list.Items.Clear();

                // DB에서 순수 상품 정보만 가져오기
                List<Product> products = DBConnector.Instance.GetProducts(areaCode);
                Console.WriteLine($"ProductStatus - 상품 목록 로드: {products.Count}개의 상품 정보");

                foreach (var product in products)
                {
                    // ListView 아이템 생성
                    ListViewItem item = new ListViewItem(product.productCode); // 상품코드
                    item.SubItems.Add(product.productName); // 상품명
                    item.SubItems.Add($"{product.price:N0}원"); // 가격

                    // 상품 유형 표시
                    string productType = product.productType switch
                    {
                        1 => "요소수",
                        2 => "차량보조제",
                        _ => "기타"
                    };
                    item.SubItems.Add(productType);

                    // 상품 유형에 따른 색상 설정 (더 깔끔하게 - ProductPage 스타일)
                    item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                    switch (product.productType)
                    {
                        case 1: // 요소수
                            item.ForeColor = Color.FromArgb(0, 100, 200);
                            break;
                        case 2: // 차량보조제
                            item.ForeColor = Color.FromArgb(200, 100, 0);
                            break;
                        default: // 기타
                            item.ForeColor = Color.FromArgb(0, 72, 37);
                            break;
                    }

                    // 상품 정보를 태그로 저장
                    item.Tag = product;

                    Product_list.Items.Add(item);
                }

                Console.WriteLine($"ProductStatus - 상품 목록에 {products.Count}개 상품 표시 완료");
                
                // 통계 정보 로그
                int ureaCount = products.Count(p => p.productType == 1);
                int carAidCount = products.Count(p => p.productType == 2);
                int etcCount = products.Count(p => p.productType == 0);
                Console.WriteLine($"ProductStatus - 요소수: {ureaCount}개, 차량보조제: {carAidCount}개, 기타: {etcCount}개");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductStatus - 상품 목록 로드 중 오류: {ex.Message}");
                Logger.Error($"LoadProductsFromDatabase 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 상품 목록을 새로고침합니다
        /// </summary>
        public void RefreshProductList()
        {
            try
            {
                // 상품 정보 새로고침
                LoadProductsFromDatabase();
                
                // 사물함 상태도 새로고침
                RefreshLockerButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshProductList 오류: {ex.Message}");
                Logger.Error($"RefreshProductList 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사물함 버튼들의 상태를 새로고침합니다
        /// </summary>
        private void RefreshLockerButtons()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    return;
                }

                // JOIN을 사용하여 박스와 제품 정보를 한 번에 가져오기
                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();

                foreach (var button in lockerButtons)
                {
                    if (button == null) continue;

                    button.IsAdminMode = false;

                    int boxNo = button.LockerNumber;
                    var box = boxes.FirstOrDefault(b => b.BoxNo == boxNo);
                    
                    if (box != null)
                    {
                        if (box.useState == 1)
                        {
                            button.SetLockerState(true, false);
                            
                            if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                            {
                                // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                button.SetProductNameOnly(box.productName, box.price);
                            }
                            else
                            {
                                button.SetProductInfo("", 0);
                            }
                        }
                        else
                        {
                            button.SetLockerState(false, true);
                            
                            if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                            {
                                // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                button.SetProductNameOnly(box.productName, box.price);
                            }
                            else
                            {
                                button.SetProductInfo("", 0);
                            }
                        }
                    }
                    else
                    {
                        button.SetLockerState(false, true);
                        button.SetProductInfo("", 0);
                    }
                    
                    button.UpdateButtonImage();
                }

                Console.WriteLine($"ProductStatus - RefreshLockerButtons: {lockerButtons.Count}개 버튼 새로고침 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshLockerButtons 오류: {ex.Message}");
                Logger.Error($"RefreshLockerButtons 오류: {ex.Message}", ex);
            }
        }

        public void SetContext(s_oil.Services.ApplicationContext context)
        {
            _context = context;
        }

    
        private void button_home_Click(object sender, EventArgs e)
        {
            try
            {
                // 홈페이지로 이동하기 전에 모든 LockerButton들을 normalImage로 초기화
                ResetAllLockerButtonsToNormal();
                
                _context?.Navigator?.ShowHomePage();
                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"홈페이지로 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void product_buy_page_Click(object sender, EventArgs e)
        {
            try
            {
                _context?.Navigator?.ShowCustomerProductBuyPage();
                this.ActiveControl = null;
                Console.WriteLine("ProductStatus에서 ProductPage로 이동");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"구매 페이지로 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 영수증 템플릿 테스트 기능 (개선된 프린터 사용)
        /// </summary>
        private async Task TestReceiptTemplate()
        {
            try
            {
                Console.WriteLine("영수증 템플릿 테스트 시작...");

                // ApplicationContext가 초기화되어 있는지 확인
                if (_context == null)
                {
                    this.Invoke(new Action(() => 
                        MessageBox.Show("ApplicationContext가 초기화되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    return;
                }

                // 장치 상태 확인
                var deviceStatus = _context.CheckAllDeviceStatus();
                Console.WriteLine($"장치 상태 - BU: {deviceStatus.BU}, Printer: {deviceStatus.Printer}");

                // 프린터가 연결되지 않은 경우 재연결 시도
                if (!deviceStatus.Printer)
                {
                    Console.WriteLine("프린터가 연결되지 않음. 재연결을 시도합니다.");
                    this.Invoke(new Action(() => 
                        MessageBox.Show("프린터가 연결되지 않았습니다. 재연결을 시도합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    
                    bool reconnected = await _context.CheckAndReconnectPrinterAsync();
                    if (!reconnected)
                    {
                        this.Invoke(new Action(() => 
                            MessageBox.Show("프린터 재연결에 실패했습니다. 프린터 연결을 확인해주세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                        return;
                    }
                }

                // 테스트 영수증 출력 (개선된 한글 지원)
                Console.WriteLine("테스트 영수증 출력 시작 (CP949 인코딩 사용)...");
                bool success = await _context.TestReceiptPrintAsync();

                if (success)
                {
                    this.Invoke(new Action(() => 
                        MessageBox.Show("영수증 테스트 출력이 완료되었습니다!\n(CP949 인코딩으로 한글 출력)", "성공", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    Console.WriteLine("영수증 테스트 출력 성공 (한글 지원 개선)!");
                }
                else
                {
                    this.Invoke(new Action(() => 
                        MessageBox.Show("영수증 테스트 출력에 실패했습니다. 로그를 확인해주세요.", "실패", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    Console.WriteLine("영수증 테스트 출력 실패!");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"영수증 테스트 중 오류 발생: {ex.Message}";
                this.Invoke(new Action(() => 
                    MessageBox.Show(errorMsg, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                Console.WriteLine(errorMsg);
                Logger.Error(errorMsg, ex);
            }
        }

        /// <summary>
        /// 키보드 단축키로 영수증 테스트 실행 (Ctrl+R)
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ctrl+R 키 조합으로 영수증 테스트 실행
            if (keyData == (Keys.Control | Keys.R))
            {
                // UI 스레드에서 async 메서드 호출
                Task.Run(async () =>
                {
                    try
                    {
                        await TestReceiptTemplate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"영수증 테스트 실행 중 오류: {ex.Message}");
                    }
                });
                return true;
            }

            // Ctrl+T 키 조합으로 템플릿 내용 미리보기
            if (keyData == (Keys.Control | Keys.T))
            {
                ShowReceiptTemplatePreview();
                return true;
            }

            // ?? Ctrl+P 키 조합으로 결제 테스트 실행
            if (keyData == (Keys.Control | Keys.P))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await TestPaymentDevice();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"결제 테스트 실행 중 오류: {ex.Message}");
                    }
                });
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// ?? 결제 디바이스 테스트 기능
        /// </summary>
        private async Task TestPaymentDevice()
        {
            try
            {
                Console.WriteLine("결제 디바이스 테스트 시작...");

                this.Invoke(new Action(() => 
                    MessageBox.Show("결제 테스트를 시작합니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information)));

                // ?? 먼저 데이터베이스 테이블 구조 확인
                var tableColumns = DBConnector.Instance.GetTableColumns("tblPayment");
                Logger.Info($"tblPayment 테이블 컬럼 확인: {string.Join(", ", tableColumns)}");
                
                if (tableColumns.Count > 0)
                {
                    Console.WriteLine($"tblPayment 테이블 컬럼: {string.Join(", ", tableColumns)}");
                }

                // ?? 결제 디바이스 연결 테스트 (수정됨)
                bool connectionTest = await _context.CheckAndReconnectPayDeviceAsync();
                
                if (!connectionTest)
                {
                    this.Invoke(new Action(() => 
                        MessageBox.Show("결제 단말기 연결에 실패했습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    return;
                }


                bool paymentTest = await _context.ProcessCardPaymentAsync(1000);

                if (paymentTest)
                {
                    // ?? 테스트 결제 성공 시 DB 저장도 테스트
                    bool dbTestResult = TestDatabaseSave();
                    
                    string message;
                    if (paymentTest && dbTestResult)
                    {
                        message = "결제 및 DB 저장 테스트가 성공적으로 완료되었습니다!";
                    }
                    else
                    {
                        message = $"결제 테스트 성공, DB 저장 테스트: {(dbTestResult ? "성공" : "실패")}";
                    }
                    
                    this.Invoke(new Action(() => 
                        MessageBox.Show(message, "테스트 결과", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    Console.WriteLine($"결제 테스트 성공! DB 테스트: {dbTestResult}");
                }
                else
                {
                    string errorMessage = DaouCtrl.outmsg ?? "알 수 없는 오류";
                    this.Invoke(new Action(() => 
                        MessageBox.Show($"결제 테스트에 실패했습니다.\n오류: {errorMessage}", "실패", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    Console.WriteLine("결제 테스트 실패!");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"결제 테스트 중 오류 발생: {ex.Message}";
                this.Invoke(new Action(() => 
                    MessageBox.Show(errorMsg, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                Console.WriteLine(errorMsg);
                Logger.Error(errorMsg, ex);
            }
        }

        /// <summary>
        /// ?? 데이터베이스 저장 테스트
        /// </summary>
        private bool TestDatabaseSave()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE", "SPGS00010002");

                var testPayment = new s_oil.models.Payment
                {
                    areaCode = areaCode,
                    boxNo = 999, // 테스트용 박스 번호
                    userCode = "TEST_USER",
                    payType = 1,
                    payAmount = 1000,
                    payPhone = "010-0000-0000",
                    confirmKey = "TEST_PAY",
                    cardNumber = "****-****-****-0000",
                    payTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    approvalNumber = "TEST_APPROVAL",
                    payStatus = 1,
                    errorMessage = ""
                };

                // 동적 저장 테스트
                bool result = DBConnector.Instance.SavePaymentDynamic(testPayment);
                
                if (!result)
                {
                    // 기본 저장 테스트
                    result = DBConnector.Instance.SavePayment(testPayment);
                }

                Logger.Info($"DB 저장 테스트 결과: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"DB 저장 테스트 중 오류: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 영수증 템플릿 내용 미리보기
        /// </summary>
        private void ShowReceiptTemplatePreview()
        {
            try
            {
                var templatePath = Path.Combine(Application.StartupPath, "device", "Receipt_template.txt");
                
                if (!File.Exists(templatePath))
                {
                    MessageBox.Show($"템플릿 파일을 찾을 수 없습니다: {templatePath}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string templateContent = File.ReadAllText(templatePath, Encoding.UTF8);
                
                // 샘플 데이터로 템플릿 미리보기
                var sampleData = new Services.ReceiptData
                {
                    CompanyName = "S-OIL 테스트점",
                    PhoneNumber = "02-1234-5678",
                    CompanyNumber = "123-45-67890",
                    CeoName = "홍길동",
                    CompanyAddress = "서울시 강남구 테스트로 123",
                    TransactionDate = DateTime.Now,
                    GasPumpNumber = "01",
                    TransactionNumber = "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    FuelType = "경유",
                    UnitPrice = 1468,
                    Quantity = 20.000m,
                    TotalAmount = 29360,
                    InputAmount = 30000,
                    ChangeAmount = 640,
                    PaymentMethod = "신용카드",
                    CardNumber = "****-****-****-1234",
                    ApprovalNumber = "AP" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                // 템플릿 처리
                string processedContent = ProcessTemplatePreview(templateContent, sampleData);

                // 미리보기 창 표시
                var previewForm = new Form
                {
                    Text = "영수증 템플릿 미리보기",
                    Size = new Size(500, 700),
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var textBox = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("맑은 고딕", 10F, FontStyle.Regular),
                    Dock = DockStyle.Fill,
                    Text = processedContent
                };

                previewForm.Controls.Add(textBox);
                previewForm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"템플릿 미리보기 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"템플릿 미리보기 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 템플릿 미리보기용 데이터 처리
        /// </summary>
        private string ProcessTemplatePreview(string template, Services.ReceiptData data)
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

            return result;
        }
    
        private void ProductStatus_Load(object sender, EventArgs e)
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    MessageBox.Show("AREACODE를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // JOIN을 사용하여 박스와 제품 정보를 한 번에 가져오기
                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                Console.WriteLine($"ProductStatus - JOIN으로 조회된 박스 개수: {boxes.Count}");

                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();
                Console.WriteLine($"LockerButton 개수: {lockerButtons.Count}"); // 디버깅용

                foreach (var button in lockerButtons)
                {
                    button.Click += LockerButton_Click; // 이벤트 핸들러 할당
                    
                    // ?? 고객 페이지(상품 상태)에서는 일반 모드로 설정 (품절 함 선택 불가)
                    button.IsAdminMode = false;
                    
                    int boxNo = 0;
                    if (int.TryParse(button.Name.Replace("lockerButton", ""), out boxNo))
                    {
                        // 이미지 로드 로직...
                        var resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductStatus));
                        
                        Image normalImage = null;
                        Image pressedImage = null;
                        Image soldoutImage = null;

                        try
                        {
                            normalImage = (Image)resources.GetObject($"lockerButton{boxNo}.NormalImage");
                            pressedImage = (Image)resources.GetObject($"lockerButton{boxNo}.PressedImage");
                            soldoutImage = (Image)resources.GetObject($"lockerButton{boxNo}.UsingImage");
                            Console.WriteLine($"Box {boxNo}: .resx에서 이미지 로드 성공");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Box {boxNo} 이미지 리소스 로드 실패: {ex.Message}");
                            // 백업: 파일 시스템에서 로드 시도
                            string normalImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "normal_box", $"normal_box{boxNo}.png");
                            string pressedImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "pressed_box", $"pressed_box{boxNo}.png");
                            string soldoutImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "soldout_box", $"soldout_box{boxNo}.png");

                            if (File.Exists(normalImagePath)) 
                            {
                                normalImage = Image.FromFile(normalImagePath);
                                Console.WriteLine($"Box {boxNo}: normalImage 파일 로드 성공");
                            }
                            if (File.Exists(pressedImagePath)) 
                            {
                                pressedImage = Image.FromFile(pressedImagePath);
                                Console.WriteLine($"Box {boxNo}: pressedImage 파일 로드 성공");
                            }
                            if (File.Exists(soldoutImagePath)) 
                            {
                                soldoutImage = Image.FromFile(soldoutImagePath);
                                Console.WriteLine($"Box {boxNo}: soldoutImage 파일 로드 성공");
                            }
                        }

                        button.NormalImage = normalImage;
                        button.PressedImage = pressedImage;
                        button.UsingImage = soldoutImage;

                        // 사물함 번호 설정
                        button.LockerNumber = boxNo;

                        // JOIN으로 가져온 데이터에서 해당 박스 찾기
                        var box = boxes.FirstOrDefault(b => b.BoxNo == boxNo);
                        if (box != null)
                        {
                            Console.WriteLine($"Box {boxNo}: useState = {box.useState}, productCode = {box.productCode}");
                            Console.WriteLine($"Box {boxNo}: productName = {box.productName}, price = {box.price}");
                            
                            if (box.useState == 1) // 판매불가능 (품절/사용불가)
                            {
                                // ?? 고객 모드에서는 품절 함 선택 불가 (기존 동작 유지)
                                button.SetLockerState(true, false); // IsUsing=true, Enabled=false
                                Console.WriteLine($"Box {boxNo}: 판매불가 설정 (품절)");
                            }
                            else // 기본값은 판매가능 (useState == 2 또는 기타)
                            {
                                // 사물함 상태를 "판매 가능"으로 설정
                                button.SetLockerState(false, true);
                                Console.WriteLine($"Box {boxNo}: 판매가능 설정");
                                
                                // JOIN으로 가져온 제품 정보 설정 (판매가능할 때만)
                                if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                                {
                                    // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                    button.SetProductNameOnly(box.productName, box.price);
                                    Console.WriteLine($"Box {boxNo}: JOIN 제품 정보 설정 - {box.productName}, {box.price:N0}원");
                                }
                                else
                                {
                                    // 제품 정보가 없는 경우 기본 텍스트 설정
                                    button.SetProductInfo("", 0);
                                    Console.WriteLine($"Box {boxNo}: 제품 정보 없음, 기본 텍스트 설정");
                                }
                            }
                        }
                        else
                        {
                            // 데이터베이스에 해당 박스가 없는 경우 기본값으로 판매가능 상태 설정
                            button.SetLockerState(false, true);
                            button.SetProductInfo("", 0);
                            Console.WriteLine($"Box {boxNo}: DB에 데이터 없음, 기본 설정 적용");
                        }

                        button.UpdateButtonImage();
                        Console.WriteLine($"Box {boxNo}: 이미지 업데이트 완료, 현재 배경 이미지: {(button.BackgroundImage != null ? "존재" : "없음")}");
                    }
                }

                // 제품 목록 새로고침 (Form Load 시에만 호출)
                RefreshProductList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LockerButton_Click(object? sender, EventArgs e)
        {
            var clickedButton = sender as LockerButton;
            Console.WriteLine($"ProductStatus: LockerButton_Click 이벤트 발생 - Button {clickedButton?.LockerNumber}");
            
            if (clickedButton == null || clickedButton.IsUsing || !clickedButton.Enabled)
            {
                Console.WriteLine($"클릭 무시: IsUsing={clickedButton?.IsUsing}, Enabled={clickedButton?.Enabled}");
                return;
            }

            Console.WriteLine($"클릭 전 상태: IsSelected={clickedButton.IsSelected}, selectedLockers.Count={selectedLockers.Count}");

            // 다중 선택 지원 - 여기서는 리스트 관리만 하고 IsSelected 변경은 OnClick에서 처리됨
            if (clickedButton.IsSelected)
            {
                // 이미 선택된 버튼을 클릭하면 선택 해제 (리스트에서 제거)
                if (selectedLockers.Contains(clickedButton))
                {
                    selectedLockers.Remove(clickedButton);
                    Console.WriteLine($"리스트에서 제거: {clickedButton.LockerNumber}");
                }
            }
            else
            {
                // 새로운 버튼 선택 (리스트에 추가)
                if (!selectedLockers.Contains(clickedButton))
                {
                    selectedLockers.Add(clickedButton);
                    Console.WriteLine($"리스트에 추가: {clickedButton.LockerNumber}");
                }
            }

            Console.WriteLine($"클릭 후 상태: IsSelected={clickedButton.IsSelected}, selectedLockers.Count={selectedLockers.Count}");
            
            // 선택된 사물함들의 정보를 출력 (디버깅용)
            Console.WriteLine($"현재 선택된 사물함 수: {selectedLockers.Count}");
            foreach (var locker in selectedLockers)
            {
                Console.WriteLine($"  사물함 번호: {locker.LockerNumber}, 제품: {locker.ProductName}, 가격: {locker.Price}");
            }

            // 상품 목록에서 해당 상품 하이라이트
            HighlightProductInList(clickedButton.LockerNumber, clickedButton.IsSelected);
        }

        /// <summary>
        /// 상품 목록에서 선택된 사물함의 상품을 하이라이트합니다
        /// </summary>
        /// <param name="lockerNumber">사물함 번호</param>
        /// <param name="isSelected">선택 여부</param>
        private void HighlightProductInList(int lockerNumber, bool isSelected)
        {
            try
            {
                // 해당 사물함의 상품코드를 찾기
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    return;
                }

                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                var box = boxes.FirstOrDefault(b => b.BoxNo == lockerNumber);
                
                if (box == null || string.IsNullOrEmpty(box.productCode))
                {
                    return;
                }

                // 상품 리스트에서 해당 상품코드를 가진 아이템 찾아서 하이라이트
                foreach (ListViewItem item in Product_list.Items)
                {
                    if (item.Text == box.productCode)
                    {
                        if (isSelected)
                        {
                            // 선택된 상품 하이라이트
                            item.ForeColor = Color.FromArgb(0, 0, 0); // 검은색 텍스트
                            item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                        }
                        else
                        {
                            // 선택 해제 시 원래 색상으로 복원 (배경색 제거)
                            item.BackColor = Color.Transparent;
                            item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                            var product = item.Tag as Product;
                            if (product != null)
                            {
                                switch (product.productType)
                                {
                                    case 1: // 요소수
                                        item.ForeColor = Color.FromArgb(0, 100, 200);
                                        break;
                                    case 2: // 차량보조제
                                        item.ForeColor = Color.FromArgb(200, 100, 0);
                                        break;
                                    default: // 기타
                                        item.ForeColor = Color.FromArgb(0, 72, 37);
                                        break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HighlightProductInList 오류: {ex.Message}");
                Logger.Error($"HighlightProductInList 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 선택된 사물함들의 리스트를 반환합니다
        /// </summary>
        public List<LockerButton> GetSelectedLockers()
        {
            return selectedLockers.ToList();
        }

        /// <summary>
        /// 모든 선택을 해제합니다
        /// </summary>
        public void ClearAllSelections()
        {
            foreach (var locker in selectedLockers)
            {
                locker.IsSelected = false;
                locker.UpdateButtonImage();
                // 상품 목록에서도 하이라이트 해제
                HighlightProductInList(locker.LockerNumber, false);
            }
            selectedLockers.Clear();
        }

        /// <summary>
        /// 모든 LockerButton들을 normalImage로 초기화합니다
        /// </summary>
        private void ResetAllLockerButtonsToNormal()
        {
            try
            {
                // 폼의 모든 LockerButton들을 찾아서 normalImage로 변경
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();
                
                foreach (var button in lockerButtons)
                {
                    // IsUsing이 아닌 버튼들만 선택 해제 (품절 상태는 유지)
                    if (!button.IsUsing)
                    {
                        button.IsSelected = false;
                        button.UpdateButtonImage();
                    }
                }

                // selectedLockers 리스트도 초기화
                selectedLockers.Clear();

                Console.WriteLine($"ProductStatus - 모든 LockerButton들을 normalImage로 초기화 완료: {lockerButtons.Count}개 버튼 처리");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetAllLockerButtonsToNormal 오류: {ex.Message}");
                Logger.Error($"ResetAllLockerButtonsToNormal 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 선택된 사물함들의 총 가격을 계산합니다
        /// </summary>
        public int GetTotalPrice()
        {
            return selectedLockers.Sum(locker => locker.Price);
        }

        /// <summary>
        /// 상품 상세 정보를 표시합니다
        /// </summary>
        /// <param name="product">상품 정보</param>
        public void ShowProductDetail(Product product)
        {
            try
            {
                string message = $"상품 상세 정보\n\n";
                message += $"상품 코드: {product.productCode}\n";
                message += $"상품명: {product.productName}\n";
                message += $"가격: {product.price:N0}원\n";
                
                string productType = product.productType switch
                {
                    1 => "요소수",
                    2 => "차량보조제", 
                    _ => "기타"
                };
                message += $"상품 유형: {productType}\n\n";
                
                // 해당 상품이 들어있는 사물함 정보 추가
                var availableLockers = GetAvailableLockersForProduct(product.productCode);
                if (availableLockers.Count > 0)
                {
                    message += $"사용 가능한 사물함: {string.Join(", ", availableLockers)}번\n";
                    message += $"총 {availableLockers.Count}개 사물함에서 구매 가능합니다!";
                }
                else
                {
                    message += "현재 사용 가능한 사물함이 없습니다.";
                }

                MessageBox.Show(message, "상품 상세 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowProductDetail 오류: {ex.Message}");
                Logger.Error($"ShowProductDetail 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 특정 상품에 사용 가능한 사물함 번호들을 반환합니다
        /// </summary>
        /// <param name="productCode">상품 코드</param>
        /// <returns>사용 가능한 사물함 번호 리스트</returns>
        private List<int> GetAvailableLockersForProduct(string productCode)
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    return new List<int>();
                }

                // JOIN을 사용하여 박스와 제품 정보를 한 번에 가져오기
                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                
                // 해당 상품코드를 가지고 있으면서 판매 가능한 사물함들 찾기
                var availableBoxes = boxes.Where(box => 
                    box.productCode == productCode && 
                    box.useState != 1 // 판매 가능
                ).Select(box => box.BoxNo).ToList();

                return availableBoxes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAvailableLockersForProduct 오류: {ex.Message}");
                Logger.Error($"GetAvailableLockersForProduct 오류: {ex.Message}", ex);
                return new List<int>();
            }
        }

        /// <summary>
        /// 상품 검색 기능 (상품명으로 검색)
        /// </summary>
        /// <param name="searchText">검색할 텍스트</param>
        public void SearchProducts(string searchText)
        {
            try
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadProductsFromDatabase(); // 전체 목록 다시 로드
                    return;
                }

                // 현재 Product_list의 모든 아이템을 검색
                foreach (ListViewItem item in Product_list.Items)
                {
                    var product = item.Tag as Product;
                    if (product != null)
                    {
                        bool isMatch = product.productName.ToLower().Contains(searchText.ToLower()) ||
                                      product.productCode.ToLower().Contains(searchText.ToLower());

                        if (isMatch)
                        {
                            // 검색 결과 하이라이트
                            item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                        }
                        else
                        {
                            // 원래 색상으로 복원 (배경색 제거, 텍스트 색상만)
                            item.BackColor = Color.Transparent;
                            item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                            switch (product.productType)
                            {
                                case 1: // 요소수
                                    item.ForeColor = Color.FromArgb(0, 100, 200);
                                    break;
                                case 2: // 차량보조제
                                    item.ForeColor = Color.FromArgb(200, 100, 0);
                                    break;
                                default: // 기타
                                    item.ForeColor = Color.FromArgb(0, 72, 37);
                                    break;
                            }
                        }
                    }
                }

                Console.WriteLine($"상품 검색 완료: '{searchText}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SearchProducts 오류: {ex.Message}");
                Logger.Error($"SearchProducts 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 상품 유형별 필터링
        /// </summary>
        /// <param name="productType">필터링할 상품 유형 (0: 전체, 1: 요소수, 2: 차량보조제)</param>
        public void FilterProductsByType(int productType = 0)
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    Console.WriteLine("ProductStatus - AREACODE를 찾을 수 없습니다.");
                    return;
                }

                // 기존 아이템 모두 제거
                Product_list.Items.Clear();

                // DB에서 순수 상품 정보만 가져오기
                List<Product> products = DBConnector.Instance.GetProducts(areaCode);

                // 상품 유형에 따라 필터링
                var filteredProducts = products.Where(product => 
                    productType == 0 || product.productType == productType
                ).ToList();

                foreach (var product in filteredProducts)
                {
                    // ListView 아이템 생성
                    ListViewItem item = new ListViewItem(product.productCode); // 상품코드
                    item.SubItems.Add(product.productName); // 상품명
                    item.SubItems.Add($"{product.price:N0}원"); // 가격

                    // 상품 유형 표시
                    string productTypeText = product.productType switch
                    {
                        1 => "요소수",
                        2 => "차량보조제",
                        _ => "기타"
                    };
                    item.SubItems.Add(productTypeText);

                    // 상품 유형에 따른 색상 설정
                    item.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
                    switch (product.productType)
                    {
                        case 1: // 요소수
                            item.ForeColor = Color.FromArgb(0, 100, 200);
                            break;
                        case 2: // 차량보조제
                            item.ForeColor = Color.FromArgb(200, 100, 0);
                            break;
                        default: // 기타
                            item.ForeColor = Color.FromArgb(0, 72, 37);
                            break;
                    }

                    // 상품 정보를 태그로 저장
                    item.Tag = product;

                    Product_list.Items.Add(item);
                }

                string typeMsg = productType switch
                {
                    1 => "요소수",
                    2 => "차량보조제",
                    _ => "전체 상품"
                };

                Console.WriteLine($"ProductStatus - {typeMsg} 필터링 완료: {filteredProducts.Count}개 상품 표시");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FilterProductsByType 오류: {ex.Message}");
                Logger.Error($"FilterProductsByType 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 상품 통계 정보를 반환합니다
        /// </summary>
        /// <returns>상품 통계 정보</returns>
        public (int totalProducts, int ureaProducts, int carAidProducts, int etcProducts) GetProductStatistics()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    return (0, 0, 0, 0);
                }

                List<Product> products = DBConnector.Instance.GetProducts(areaCode);

                int totalProducts = products.Count;
                int ureaProducts = products.Count(p => p.productType == 1);
                int carAidProducts = products.Count(p => p.productType == 2);
                int etcProducts = products.Count(p => p.productType == 0);

                return (totalProducts, ureaProducts, carAidProducts, etcProducts);
            }
            catch (Exception ex)
            {
                Logger.Error($"GetProductStatistics 오류: {ex.Message}", ex);
                return (0, 0, 0, 0);
            }
        }
    }
}