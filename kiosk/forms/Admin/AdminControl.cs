using s_oil.models;
using s_oil.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.Utils;
using System.IO;

namespace s_oil.Forms
{
    public partial class AdminControl : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        private List<s_oil.Utils.LockerButton> selectedLockers = new List<s_oil.Utils.LockerButton>();
        private Product? selectedProduct = null; // 선택된 상품 정보
        
        // ? 추가: 업데이트 후 열어야 할 사물함 번호 리스트
        private List<int> _updateSelectedBoxNumbers = new List<int>();

        public AdminControl()
        {
            InitializeComponent();
            InitializeProductList();
        }

        /// <summary>
        /// product_info_list ListView 초기화 (순수 상품 정보만 표시)
        /// </summary>
        private void InitializeProductList()
        {
            // ListView 설정
            product_info_list.View = View.Details;
            product_info_list.FullRowSelect = true;
            product_info_list.GridLines = false; // 그리드 라인 제거
            product_info_list.Font = new Font("Pretendard Variable", 14F, FontStyle.Bold);
            product_info_list.BorderStyle = BorderStyle.None; // 외곽 테두리 제거
            product_info_list.HeaderStyle = ColumnHeaderStyle.Nonclickable; // 헤더 클릭 비활성화

            // 배경색과 텍스트 색상 설정
            product_info_list.ForeColor = Color.FromArgb(0, 72, 37); // 텍스트 색상

            // 컬럼 추가 - tblProduct의 순수 상품 정보만 표시
            product_info_list.Columns.Add("상품코드", 150, HorizontalAlignment.Center);
            product_info_list.Columns.Add("상품명", 300, HorizontalAlignment.Left);
            product_info_list.Columns.Add("가격", 150, HorizontalAlignment.Right);
            product_info_list.Columns.Add("상품유형", 150, HorizontalAlignment.Center);
            product_info_list.Columns.Add("재고상태", 150, HorizontalAlignment.Center);

            // 컬럼 헤더 스타일 개선
            foreach (ColumnHeader column in product_info_list.Columns)
            {
                column.TextAlign = column.Index switch
                {
                    0 => HorizontalAlignment.Center, // 상품코드
                    1 => HorizontalAlignment.Left,   // 상품명
                    2 => HorizontalAlignment.Right,  // 가격
                    3 => HorizontalAlignment.Center, // 상품유형
                    4 => HorizontalAlignment.Center, // 재고상태
                    _ => HorizontalAlignment.Left
                };
            }

            // 이벤트 핸들러 추가
            product_info_list.DoubleClick += Product_info_list_DoubleClick;
            product_info_list.Click += Product_info_list_Click;

            // 초기 데이터 로드 - 순수 상품 정보만
            LoadProductsFromDatabase();
        }

        /// <summary>
        /// product_info_list 더블클릭 이벤트 - 상품 상세 정보 표시
        /// </summary>
        private void Product_info_list_DoubleClick(object? sender, EventArgs e)
        {
            try
            {
                if (product_info_list.SelectedItems.Count > 0)
                {
                    var selectedItem = product_info_list.SelectedItems[0];
                    var product = selectedItem.Tag as Product;
                    
                    if (product != null)
                    {
                        ShowProductDetail(product);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product_info_list_DoubleClick 오류: {ex.Message}");
                Logger.Error($"Product_info_list_DoubleClick 오류: {ex.Message}", ex);
                MessageBox.Show($"상품 상세 정보 표시 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// product_info_list 클릭 이벤트 - 상품 선택 (사물함이 먼저 선택된 경우에만 활성화)
        /// </summary>
        private void Product_info_list_Click(object? sender, EventArgs e)
        {
            try
            {
                // 새로운 워크플로우: 사물함이 먼저 선택되었는지 확인
                if (selectedLockers == null || selectedLockers.Count == 0)
                {
                    MessageBox.Show("먼저 상품을 배치할 사물함을 선택해주세요.", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine("상품 선택 무시: 사물함이 먼저 선택되어야 합니다.");
                    return;
                }

                if (product_info_list.SelectedItems.Count > 0)
                {
                    var selectedItem = product_info_list.SelectedItems[0];
                    if (selectedItem?.Tag is Product newSelectedProduct)
                    {
                        // 이전 상품 선택 해제
                        ClearProductSelection();
                        
                        // 새로운 상품 선택
                        selectedProduct = newSelectedProduct;
                        
                        // 선택된 상품 하이라이트
                        selectedItem.BackColor = Color.FromArgb(200, 230, 255); // 연한 파란색
                        selectedItem.Font = new Font("Pretendard Variable", 14F, FontStyle.Bold);
                        
                        // 선택된 사물함 정보 표시
                        string lockerNumbers = string.Join(", ", selectedLockers.Select(l => l.LockerNumber));
                        Console.WriteLine($"상품 '{selectedProduct?.productName ?? "Unknown"}' 선택됨");
                        Console.WriteLine($"배치 대상 사물함: {lockerNumbers}번");
                        
                        // 사물함들을 특별한 색상으로 표시 (배치 준비 상태)
                        HighlightSelectedLockersForProduct();
                    }
                    else
                    {
                        Console.WriteLine("선택된 아이템에 유효한 상품 정보가 없습니다.");
                    }
                }
                else
                {
                    // 선택 해제된 경우
                    selectedProduct = null;
                    ClearProductSelection();
                    RestoreSelectedLockersDisplay();
                    Console.WriteLine("상품 선택이 해제됨");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product_info_list_Click 오류: {ex.Message}");
                Logger.Error($"Product_info_list_Click 오류: {ex.Message}", ex);
                MessageBox.Show($"상품 선택 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 오류 발생 시 안전한 상태로 초기화
                selectedProduct = null;
                ClearProductSelection();
            }
        }

        /// <summary>
        /// 선택된 사물함들을 상품 배치 준비 상태로 표시
        /// </summary>
        private void HighlightSelectedLockersForProduct()
        {
            try
            {
                foreach (var locker in selectedLockers)
                {
                    if (locker != null)
                    {
                        // 상품이 선택되었을 때는 오렌지색으로 표시 (배치 준비 상태)
                        locker.BackColor = Color.FromArgb(255, 200, 100); // 연한 오렌지
                    }
                }
                Console.WriteLine($"선택된 {selectedLockers.Count}개 사물함을 배치 준비 상태로 표시");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HighlightSelectedLockersForProduct 오류: {ex.Message}");
                Logger.Error($"HighlightSelectedLockersForProduct 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 선택된 사물함들을 원래 선택 상태로 복원
        /// </summary>
        private void RestoreSelectedLockersDisplay()
        {
            try
            {
                foreach (var locker in selectedLockers)
                {
                    if (locker != null)
                    {
                        // 사물함만 선택된 상태로 복원 (연한 파란색)
                        locker.BackColor = Color.FromArgb(173, 216, 230); // 연한 파란색
                    }
                }
                Console.WriteLine($"선택된 {selectedLockers.Count}개 사물함을 기본 선택 상태로 복원");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RestoreSelectedLockersDisplay 오류: {ex.Message}");
                Logger.Error($"RestoreSelectedLockersDisplay 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 상품 선택 해제
        /// </summary>
        private void ClearProductSelection()
        {
            try
            {
                foreach (ListViewItem item in product_info_list.Items)
                {
                    item.BackColor = Color.Transparent;
                    var product = item.Tag as Product;
                    if (product != null)
                    {
                        // 상품 유형에 따른 원래 색상 복원
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
            catch (Exception ex)
            {
                Console.WriteLine($"ClearProductSelection 오류: {ex.Message}");
                Logger.Error($"ClearProductSelection 오류: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// DB에서 순수 상품 정보만 가져와서 표시합니다
        /// </summary>
        public void LoadProductsFromDatabase()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    Console.WriteLine("AdminControl - AREACODE를 찾을 수 없습니다.");
                    return;
                }

                // 기존 아이템 모두 제거
                product_info_list.Items.Clear();

                // DB에서 순수 상품 정보만 가져오기
                List<Product> products = DBConnector.Instance.GetProducts(areaCode);
                
                if (products == null)
                {
                    Console.WriteLine("AdminControl - 상품 정보를 가져올 수 없습니다.");
                    return;
                }
                
                Console.WriteLine($"AdminControl - 상품 목록 로드: {products.Count}개의 상품 정보");

                // 각 상품별 사물함 배치 현황 확인
                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                if (boxes == null)
                {
                    boxes = new List<BoxMaster>(); // 빈 리스트로 초기화
                    Console.WriteLine("AdminControl - 사물함 정보를 가져올 수 없어 빈 리스트로 처리합니다.");
                }

                foreach (var product in products)
                {
                    if (product == null) continue; // null 체크 추가

                    try
                    {
                        // ListView 아이템 생성
                        ListViewItem item = new ListViewItem(product.productCode ?? ""); // 상품코드
                        item.SubItems.Add(product.productName ?? ""); // 상품명
                        item.SubItems.Add($"{product.price:N0}원"); // 가격

                        // 상품 유형 표시
                        string productType = product.productType switch
                        {
                            1 => "요소수",
                            2 => "차량보조제",
                            _ => "기타"
                        };
                        item.SubItems.Add(productType);

                        // 재고 상태 표시 (해당 상품이 배치된 사물함 개수)
                        int stockCount = 0;
                        try
                        {
                            stockCount = boxes.Count(b => 
                                !string.IsNullOrEmpty(b?.productCode) &&
                                b.productCode == product.productCode && 
                                b.useState == 2); // 사용가능한 상태의 사물함만
                        }
                        catch (Exception stockEx)
                        {
                            Console.WriteLine($"재고 개수 계산 중 오류: {stockEx.Message}");
                        }
                        
                        string stockStatus = stockCount > 0 ? $"{stockCount}개 배치" : "재고없음";
                        item.SubItems.Add(stockStatus);

                        // 상품 유형에 따른 색상 설정
                        item.Font = new Font("Pretendard Variable", 12F, FontStyle.Bold);
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

                        product_info_list.Items.Add(item);
                    }
                    catch (Exception itemEx)
                    {
                        Console.WriteLine($"상품 아이템 처리 중 오류 (상품코드: {product?.productCode ?? "N/A"}): {itemEx.Message}");
                        continue; // 다음 상품으로 계속
                    }
                }

                Console.WriteLine($"AdminControl - 상품 목록에 {products.Count}개 상품 표시 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AdminControl - 상품 목록 로드 중 오류: {ex.Message}");
                Logger.Error($"LoadProductsFromDatabase 오류: {ex.Message}", ex);
                MessageBox.Show($"상품 정보를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 오류 발생 시 UI 상태 정리
                try
                {
                    product_info_list.Items.Clear();
                    selectedProduct = null;
                    ClearProductSelection();
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"오류 후 정리 작업 중 오류: {cleanupEx.Message}");
                }
            }
        }

        /// <summary>
        /// 상품 상세 정보를 표시합니다
        /// </summary>
        /// <param name="product">상품 정보</param>
        public void ShowProductDetail(Product product)
        {
            try
            {
                if (product == null)
                {
                    MessageBox.Show("상품 정보가 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                string message = $"상품 상세 정보\n\n";
                message += $"상품 코드: {product.productCode ?? "N/A"}\n";
                message += $"상품명: {product.productName ?? "N/A"}\n";
                message += $"가격: {product.price:N0}원\n";
                
                string productType = product.productType switch
                {
                    1 => "요소수",
                    2 => "차량보조제", 
                    _ => "기타"
                };
                message += $"상품 유형: {productType}\n\n";
                
                // 배치된 사물함 정보
                if (!string.IsNullOrEmpty(areaCode))
                {
                    List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                    var productBoxes = boxes.Where(b => b.productCode == product.productCode).ToList();
                    
                    if (productBoxes.Count > 0)
                    {
                        var availableBoxes = productBoxes.Where(b => b.useState == 2).Select(b => b.BoxNo).ToList();
                        var unavailableBoxes = productBoxes.Where(b => b.useState == 1).Select(b => b.BoxNo).ToList();
                        
                        message += $"배치된 사물함:\n";
                        if (availableBoxes.Count > 0)
                        {
                            message += $"  사용가능: {string.Join(", ", availableBoxes)}번 ({availableBoxes.Count}개)\n";
                        }
                        if (unavailableBoxes.Count > 0)
                        {
                            message += $"  사용불가: {string.Join(", ", unavailableBoxes)}번 ({unavailableBoxes.Count}개)\n";
                        }
                    }
                    else
                    {
                        message += "배치된 사물함: 없음";
                    }
                }

                MessageBox.Show(message, "상품 상세 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShowProductDetail 오류: {ex.Message}");
                Logger.Error($"ShowProductDetail 오류: {ex.Message}", ex);
                MessageBox.Show($"상품 상세 정보 표시 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 상품 목록을 새로고침합니다
        /// </summary>
        public void RefreshProductList()
        {
            try
            {
                LoadProductsFromDatabase();
                RefreshLockerButtons();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshProductList 오류: {ex.Message}");
                Logger.Error($"RefreshProductList 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사물함 버튼들의 상태를 새로고침합니다 (관리자용)
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
                var lockerButtons = this.Controls.OfType<s_oil.Utils.LockerButton>().ToList();

                foreach (var button in lockerButtons)
                {
                    if (button == null) continue;

                    // ?? 관리자 모드 유지 (새로고침 시에도 관리자 모드 상태 보존)
                    button.IsAdminMode = true;

                    int boxNo = button.LockerNumber;
                    var box = boxes.FirstOrDefault(b => b.BoxNo == boxNo);
                    
                    if (box != null)
                    {
                        if (box.useState == 1) // 판매불가능 (품절/사용불가)
                        {
                            // ?? 관리자 모드에서는 품절 함도 선택 가능하게 설정
                            button.SetLockerState(true, true); // IsUsing=true, 관리자 모드에서는 강제 활성화
                            
                            // 품절 상태의 제품 정보도 표시
                            if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                            {
                                // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                button.SetProductNameOnly(box.productName, box.price);
                            }
                            else
                            {
                                button.SetProductNameOnly("", 0);
                            }
                        }
                        else // 기본값은 판매가능 (useState == 2 또는 기타)
                        {
                            button.SetLockerState(false, true);
                            
                            if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                            {
                                // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                button.SetProductNameOnly(box.productName, box.price);
                            }
                            else
                            {
                                button.SetProductNameOnly("", 0);
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
                
                Console.WriteLine($"AdminControl - RefreshLockerButtons: 관리자 모드에서 {lockerButtons.Count}개 버튼 새로고침 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RefreshLockerButtons 오류: {ex.Message}");
                Logger.Error($"RefreshLockerButtons 오류: {ex.Message}", ex);
            }
        }

        private void FormProducts_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
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
                MessageBox.Show($"페이지 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdminControl_Load(object sender, EventArgs e)
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
                Console.WriteLine($"AdminControl - JOIN으로 조회된 박스 개수: {boxes.Count}");

                var lockerButtons = this.Controls.OfType<s_oil.Utils.LockerButton>().ToList();
                Console.WriteLine($"AdminControl - LockerButton 개수: {lockerButtons.Count}"); // 디버깅용

                foreach (var button in lockerButtons)
                {
                    if (button == null) continue;

                    button.Click += LockerButton_Click; // 이벤트 핸들러 할당
                        
                    // 관리자 모드 활성화 - 품절 함도 선택 가능하게 설정
                    button.IsAdminMode = true;
                    
                    int boxNo = 0;
                    if (int.TryParse(button.Name.Replace("lockerButton", ""), out boxNo))
                    {
                        // 이미지 로드 로직...
                        var resources = new System.ComponentModel.ComponentResourceManager(typeof(AdminControl));

                        Image normalImage = null;
                        Image pressedImage = null;
                        Image soldoutImage = null;

                        try
                        {
                            normalImage = (Image)resources.GetObject($"lockerButton{boxNo}.NormalImage");
                            pressedImage = (Image)resources.GetObject($"lockerButton{boxNo}.PressedImage");
                            soldoutImage = (Image)resources.GetObject($"lockerButton{boxNo}.UsingImage");
                            Console.WriteLine($"AdminControl - Box {boxNo}: .resx에서 이미지 로드 성공");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"AdminControl - Box {boxNo} 이미지 리소스 로드 실패: {ex.Message}");
                            // 백업: 파일 시스템에서 로드 시도
                            string normalImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "normal_box", $"normal_box{boxNo}.png");
                            string pressedImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "pressed_box", $"pressed_box{boxNo}.png");
                            string soldoutImagePath = Path.Combine(Application.StartupPath, "resource", "box_image", "soldout_box", $"soldout_box{boxNo}.png");

                            if (File.Exists(normalImagePath))
                            {
                                normalImage = Image.FromFile(normalImagePath);
                                Console.WriteLine($"AdminControl - Box {boxNo}: normalImage 파일 로드 성공");
                            }
                            if (File.Exists(pressedImagePath))
                            {
                                pressedImage = Image.FromFile(pressedImagePath);
                                Console.WriteLine($"AdminControl - Box {boxNo}: pressedImage 파일 로드 성공");
                            }
                            if (File.Exists(soldoutImagePath))
                            {
                                soldoutImage = Image.FromFile(soldoutImagePath);
                                Console.WriteLine($"AdminControl - Box {boxNo}: soldoutImage 파일 로드 성공");
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
                            if (box.useState == 1) // 판매불가능 (품절/사용불가)
                            {
                                // ?? 관리자 모드에서는 품절 함도 선택 가능하게 설정
                                button.SetLockerState(true, true); // IsUsing=true, 관리자 모드에서는 강제 활성화
                                
                                // 품절 상태의 제품 정보도 표시
                                if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                                {
                                    // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                    button.SetProductNameOnly(box.productName, box.price);
                                }
                                else
                                {
                                    button.SetProductNameOnly("", 0);
                                }
                            }
                            else // 기본값은 판매가능 (useState == 2 또는 기타)
                            {
                                button.SetLockerState(false, true);

                                if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                                {
                                    // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                                    button.SetProductNameOnly(box.productName, box.price);
                                }
                                else
                                {
                                    button.SetProductNameOnly("", 0);
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
                }

                // 관리자 화면 로드 시에도 상품 목록 새로고침
                RefreshProductList();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"AdminControl - 오류 상세: {ex}");
            }
        }

        /// <summary>
        /// 사물함 클릭 이벤트 - 새로운 워크플로우: 사물함 먼저 선택 (관리자 모드: 품절 함도 선택 가능)
        /// </summary>
        private void LockerButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var clickedButton = sender as s_oil.Utils.LockerButton;
                //Console.WriteLine($"AdminControl: LockerButton_Click 이벤트 발생 - Button {clickedButton?.LockerNumber}");

                if (clickedButton == null || !clickedButton.Enabled)
                {
                    //Console.WriteLine($"AdminControl - 클릭 무시: Enabled={clickedButton?.Enabled}");
                    return;
                }

                // 관리자 모드에서는 품절 함(IsUsing=true)도 선택 가능
                bool isNowSelected = clickedButton.IsSelected;

                if (isNowSelected)
                {
                    if (!selectedLockers.Contains(clickedButton))
                    {
                        selectedLockers.Add(clickedButton);
                        
                        // 품절 상태인지 확인하여 다른 색상으로 표시
                        if (clickedButton.IsUsing)
                        {
                            // 품절 함 선택 시 빨간색 계열로 표시
                            clickedButton.BackColor = Color.FromArgb(255, 180, 180);
                            Console.WriteLine($"AdminControl - 품절 함 선택 추가: {clickedButton.LockerNumber}번 사물함");
                        }
                        else
                        {
                            // 일반 함 선택 시 파란색 계열로 표시
                            clickedButton.BackColor = Color.FromArgb(173, 216, 230);
                            Console.WriteLine($"AdminControl - 일반 함 선택 추가: {clickedButton.LockerNumber}번 사물함");
                        }
                    }
                }
                else
                {
                    if (selectedLockers.Contains(clickedButton))
                    {
                        selectedLockers.Remove(clickedButton);
                        // 선택 해제 시 원래 색상으로 복원
                        clickedButton.BackColor = Color.Transparent;
                        Console.WriteLine($"AdminControl - 선택 해제: {clickedButton.LockerNumber}번 사물함");
                    }
                }

                // 상태 메시지 표시
                UpdateStatusMessage();

                Console.WriteLine($"AdminControl - 현재 선택된 사물함 수: {selectedLockers.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LockerButton_Click 오류: {ex.Message}");
                Logger.Error($"LockerButton_Click 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 상태 메시지를 업데이트합니다 (품절 함 정보 포함)
        /// </summary>
        private void UpdateStatusMessage()
        {
            try
            {
                if (selectedLockers.Count > 0)
                {
                    var normalLockers = selectedLockers.Where(l => !l.IsUsing).ToList();
                    var soldoutLockers = selectedLockers.Where(l => l.IsUsing).ToList();
                    
                    string lockerNumbers = string.Join(", ", selectedLockers.Select(l => l.LockerNumber));
                    Console.WriteLine($"[단계 1] 선택된 사물함: {lockerNumbers}번");
                    
                    if (soldoutLockers.Count > 0)
                    {
                        string soldoutNumbers = string.Join(", ", soldoutLockers.Select(l => l.LockerNumber));
                        Console.WriteLine($"  - 정상 함: {normalLockers.Count}개");
                        Console.WriteLine($"  - 품절 함: {soldoutLockers.Count}개 ({soldoutNumbers}번)");
                    }
                    
                    Console.WriteLine($"[단계 2] 이제 상품 목록에서 배치할 상품을 선택해주세요.");
                    
                    if (soldoutLockers.Count > 0)
                    {
                        Console.WriteLine($"  ?? 품절 함에 상품을 배치하면 정상 상태로 변경됩니다.");
                    }
                }
                else
                {
                    Console.WriteLine($"[단계 1] 먼저 상품을 배치할 사물함을 선택해주세요.");
                    Console.WriteLine($"  ?? 관리자 모드에서는 품절 함도 선택 가능합니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateStatusMessage 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 선택된 사물함들의 리스트를 반환합니다
        /// </summary>
        public List<s_oil.Utils.LockerButton> GetSelectedLockers()
        {
            return selectedLockers?.ToList() ?? new List<s_oil.Utils.LockerButton>();
        }

        /// <summary>
        /// 모든 사물함 선택을 해제합니다
        /// </summary>
        public void ClearAllLockerSelections()
        {
            try
            {
                if (selectedLockers != null)
                {
                    foreach (var locker in selectedLockers)
                    {
                        if (locker != null)
                        {
                            locker.IsSelected = false;
                            locker.UpdateButtonImage();
                            locker.BackColor = Color.Transparent; // 배경색도 초기화
                        }
                    }
                    selectedLockers.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearAllLockerSelections 오류: {ex.Message}");
                Logger.Error($"ClearAllLockerSelections 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 모든 LockerButton들을 normalImage로 초기화합니다
        /// </summary>
        private void ResetAllLockerButtonsToNormal()
        {
            try
            {
                var lockerButtons = this.Controls.OfType<s_oil.Utils.LockerButton>().ToList();
                
                foreach (var button in lockerButtons)
                {
                    if (button != null)
                    {
                        button.IsSelected = false;
                        button.BackColor = Color.Transparent;
                        button.UpdateButtonImage();
                    }
                }

                // 컬렉션들을 안전하게 초기화
                if (selectedLockers != null)
                {
                    selectedLockers.Clear();
                }
                
                // selectedProduct를 안전하게 null로 설정
                selectedProduct = null;
                
                // 상품 선택 해제
                ClearProductSelection();

                Console.WriteLine($"AdminControl - 모든 LockerButton들을 normalImage로 초기화 완료: {lockerButtons.Count}개 버튼 처리");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetAllLockerButtonsToNormal 오류: {ex.Message}");
                Logger.Error($"ResetAllLockerButtonsToNormal 오류: {ex.Message}", ex);
                
                // 오류 발생 시에도 최소한의 정리 작업 수행
                try
                {
                    selectedProduct = null;
                    selectedLockers?.Clear();
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"정리 작업 중 오류: {cleanupEx.Message}");
                }
            }
        }

        /// <summary>
        /// 상품 입력 버튼 클릭 - 새로운 워크플로우: 사물함 → 상품 → 입력
        /// </summary>
        private void product_input_button_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("상품 입력 버튼 클릭됨 - 새로운 워크플로우 처리 시작");

                // 1차 체크: 사물함이 먼저 선택되었는지 확인
                if (selectedLockers == null || selectedLockers.Count == 0)
                {
                    MessageBox.Show("먼저 상품을 배치할 사물함을 선택해주세요.\n\n[단계 1] 사물함 선택 → [단계 2] 상품 선택 → [단계 3] 입력 버튼", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine("상품 입력 실패: 사물함이 먼저 선택되어야 합니다.");
                    return;
                }

                // 2차 체크: 상품이 선택되었는지 확인
                if (selectedProduct == null)
                {
                    MessageBox.Show("배치할 상품을 선택해주세요.\n\n[단계 1] 사물함 선택 ?\n[단계 2] 상품 선택 ← 여기\n[단계 3] 입력 버튼", "안내", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Console.WriteLine("상품 입력 실패: 상품이 선택되지 않았습니다.");
                    return;
                }

                // 3차 안전성 체크: selectedProduct의 필수 필드들 확인
                if (string.IsNullOrEmpty(selectedProduct.productCode))
                {
                    MessageBox.Show("선택된 상품의 정보가 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("상품 입력 실패: 상품 코드가 비어있습니다.");
                    return;
                }

                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    MessageBox.Show("AREACODE를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("상품 입력 실패: AREACODE를 찾을 수 없습니다.");
                    return;
                }

                Console.WriteLine($"새로운 워크플로우 - 사물함: {selectedLockers.Count}개, 상품: {selectedProduct.productName}({selectedProduct.productCode})");

                // 확인 메시지
                string lockerNumbers = string.Join(", ", selectedLockers.Where(l => l != null).Select(l => l.LockerNumber.ToString()));
                var result = MessageBox.Show(
                    $"[새로운 워크플로우 완료]\n\n선택된 사물함: {lockerNumbers}번\n선택된 상품: '{selectedProduct.productName ?? selectedProduct.productCode}'\n\n위 사물함들에 상품을 배치하시겠습니까?",
                    "상품 배치 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    Console.WriteLine("상품 입력 취소: 사용자가 취소했습니다.");
                    return;
                }

                // ? 업데이트할 사물함 번호 리스트 초기화
                _updateSelectedBoxNumbers.Clear();

                // 각 선택된 사물함에 상품 배치
                int successCount = 0;
                int totalCount = selectedLockers.Count;
                int soldoutToNormalCount = 0; // 품절에서 정상으로 변경된 함 개수

                Console.WriteLine($"사물함 배치 시작: {totalCount}개 사물함 처리");

                foreach (var locker in selectedLockers.ToList()) // ToList()로 복사본 생성
                {
                    if (locker == null)
                    {
                        Console.WriteLine("Null 사물함 건너뜀");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"사물함 {locker.LockerNumber} 처리 시작 (품절상태: {locker.IsUsing})");

                        bool success = DBConnector.Instance.UpdateBoxProductAssignment(
                            areaCode,
                            locker.LockerNumber,
                            selectedProduct.productCode,
                            2); // 사용가능 상태로 변경

                        if (success)
                        {
                            successCount++;
                            
                            // ? 업데이트 성공 시 열 사물함 리스트에 추가
                            _updateSelectedBoxNumbers.Add(locker.LockerNumber);

                            // 품절 함이었다면 카운트
                            if (locker.IsUsing)
                            {
                                soldoutToNormalCount++;
                            }

                            // 사물함 UI 업데이트 (품절 상태 해제)
                            locker.SetLockerState(false, true); // 정상 상태로 변경
                            // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                            locker.SetProductNameOnly(selectedProduct.productName ?? "", selectedProduct.price);
                            locker.UpdateButtonImage();
                            locker.BackColor = Color.Transparent; // 배경색 초기화
                            Console.WriteLine($"사물함 {locker.LockerNumber}에 상품 '{selectedProduct.productName}' 배치 성공 (품절→정상: {locker.IsUsing})");
                        }
                        else
                        {
                            Console.WriteLine($"사물함 {locker.LockerNumber}에 상품 배치 실패");
                        }
                    }
                    catch (Exception lockerEx)
                    {
                        Console.WriteLine($"사물함 {locker.LockerNumber} 처리 중 오류: {lockerEx.Message}");
                        Logger.Error($"사물함 {locker.LockerNumber} 처리 중 오류: {lockerEx.Message}", lockerEx);
                        continue;
                    }
                }

                Console.WriteLine($"사물함 배치 완료: {successCount}/{totalCount}개 성공 (품절→정상: {soldoutToNormalCount}개)");

                // 결과 메시지 개선
                if (successCount == totalCount)
                {
                    string soldoutMessage = soldoutToNormalCount > 0 ? $"\n품절 해제: {soldoutToNormalCount}개 함" : "";
                    MessageBox.Show(
                        $"? 상품 배치가 완료되었습니다!\n\n상품: {selectedProduct.productName}\n성공: {successCount}개 사물함{soldoutMessage}\n\n새로운 워크플로우로 성공적으로 처리되었습니다.",
                        "배치 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Console.WriteLine($"새로운 워크플로우 - 상품 배치 성공: {successCount}/{totalCount}개 사물함");
                }
                else if (successCount > 0)
                {
                    string soldoutMessage = soldoutToNormalCount > 0 ? $"\n품절 해제: {soldoutToNormalCount}개 함" : "";
                    MessageBox.Show(
                        $"?? 상품 배치가 부분적으로 완료되었습니다.\n\n상품: {selectedProduct.productName}\n성공: {successCount}개 / 전체: {totalCount}개{soldoutMessage}",
                        "배치 부분 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Console.WriteLine($"새로운 워크플로우 - 상품 배치 부분 성공: {successCount}/{totalCount}개 사물함");
                }
                else
                {
                    MessageBox.Show(
                        "? 상품 배치에 실패했습니다.\n로그를 확인해주세요.",
                        "배치 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Console.WriteLine($"새로운 워크플로우 - 상품 배치 전체 실패: 0/{totalCount}개 사물함");
                }

                // UI 초기화 및 새로고침
                ResetAllLockerButtonsToNormal();
                RefreshProductList();
                
                // ? 사물함 업데이트 성공 시 자동으로 열기
                if (_updateSelectedBoxNumbers.Count > 0)
                {
                    StartAutoBoxOpening();
                }
                
                Console.WriteLine($"새로운 워크플로우 완료: {selectedProduct?.productName ?? "Unknown"} -> {successCount}개 사물함 성공");
            }
            catch (Exception ex)
            {
                string errorMessage = $"상품 배치 중 오류가 발생했습니다: {ex.Message}";
                MessageBox.Show(errorMessage, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"product_input_button_Click 오류: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Logger.Error($"product_input_button_Click 오류: {ex.Message}", ex);

                // 오류 발생 시 안전한 상태로 복구 시도
                try
                {
                    Console.WriteLine("오류 후 상태 복구 시도");
                    ResetAllLockerButtonsToNormal();
                    RefreshProductList();
                }
                catch (Exception recoveryEx)
                {
                    Console.WriteLine($"상태 복구 중 추가 오류: {recoveryEx.Message}");
                }
            }
        }


        /// <summary>
        /// 자동 사물함 열기 시작
        /// </summary>
        private void StartAutoBoxOpening()
        {
            try
            {
                if (_updateSelectedBoxNumbers.Count == 0)
                {
                    Logger.Warning("열 사물함이 없습니다.");
                    return;
                }

                Task.Delay(2000).ContinueWith(_ =>
                {
                    this.Invoke(new Action(() =>
                    {
                        OpenUpdateBoxes();
                    }));
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"자동 사물함 열기 시작 중 오류: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// 선택한 함 상품 변경 후 열기
        /// 해당 함들의 상품정보들이 들어가고 해당 함이 열리는 기능
        /// </summary>
        private async void OpenUpdateBoxes()
        {
            try
            {
                if (_context?.BUDevice == null)
                {
                    Logger.Warning("BU 디바이스가 초기화되지 않았습니다.");
                    MessageBox.Show("사물함 제어 디바이스가 연결되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (_updateSelectedBoxNumbers == null || _updateSelectedBoxNumbers.Count == 0)
                {
                    Logger.Warning("열 사물함이 없습니다.");
                    return;
                }

                Logger.Info($"사물함 {_updateSelectedBoxNumbers.Count}개 열기 시작");

                int successCount = 0;
                int failCount = 0;
                List<int> openedBoxes = new List<int>();
                List<int> failedBoxes = new List<int>();

                foreach (int boxNumber in _updateSelectedBoxNumbers)
                {
                    try
                    {
                        Logger.Info($"사물함 {boxNumber}번 열기 시도");

                        bool result = _context.BUDevice.OpenBox(boxNumber, false, 5000);

                        if (result)
                        {
                            Logger.Info($"? 사물함 {boxNumber}번 열기 성공");
                            successCount++;
                            openedBoxes.Add(boxNumber);
                        }
                        else
                        {
                            Logger.Error($"? 사물함 {boxNumber}번 열기 실패");
                            failCount++;
                            failedBoxes.Add(boxNumber);
                        }

                        await Task.Delay(500); // 사물함 간 500ms 딜레이
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"사물함 {boxNumber}번 열기 중 예외: {ex.Message}", ex);
                        failCount++;
                        failedBoxes.Add(boxNumber);
                    }
                }

                // ? 결과 메시지 표시
                Logger.Info($"사물함 열기 완료 - 성공: {successCount}개, 실패: {failCount}개");

                if (failCount == 0)
                {
                    // 모두 성공
                    string openedNumbers = string.Join(", ", openedBoxes);
                    MessageBox.Show(
                        $" 사물함 열기 완료!\n\n열린 사물함: {openedNumbers}번\n총 {successCount}개 사물함이 열렸습니다.",
                        "사물함 열기 성공",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else if (successCount > 0)
                {
                    // 부분 성공
                    string openedNumbers = string.Join(", ", openedBoxes);
                    string failedNumbers = string.Join(", ", failedBoxes);
                    MessageBox.Show(
                        $" 사물함 열기 부분 완료\n\n열린 사물함: {openedNumbers}번 ({successCount}개)\n실패한 사물함: {failedNumbers}번 ({failCount}개)",
                        "사물함 열기 부분 성공",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    // 전체 실패
                    string failedNumbers = string.Join(", ", failedBoxes);
                    MessageBox.Show(
                        $" 사물함 열기 실패\n\n실패한 사물함: {failedNumbers}번\n\nBU 디바이스 연결 상태를 확인해주세요.",
                        "사물함 열기 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                // ? 리스트 초기화
                _updateSelectedBoxNumbers.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenUpdateBoxes 실행 중 오류: {ex.Message}", ex);
                MessageBox.Show($"사물함 열기 중 오류가 발생했습니다:\n{ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 오류 발생 시에도 리스트 초기화
                _updateSelectedBoxNumbers?.Clear();
            }
        }


        private void out_button_Click(object sender, EventArgs e)
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
                MessageBox.Show($"페이지 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lockerButton2_Click(object sender, EventArgs e)
        {
            // 기존 개별 클릭 이벤트는 제거하고 통합된 LockerButton_Click으로 처리
        }

        private void lockerButton6_Click(object sender, EventArgs e)
        {
            // 기존 개별 클릭 이벤트는 제거하고 통합된 LockerButton_Click으로 처리
        }

        private void lockerButton22_Click(object sender, EventArgs e)
        {
            // 기존 개별 클릭 이벤트는 제거하고 통합된 LockerButton_Click으로 처리
        }

        /// <summary>
        /// 입고 버튼 클릭 - 품절 상태의 사물함을 판매 가능 상태로 변경하고 열기
        /// </summary>
        private void store_button_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("입고 버튼 클릭됨 - 품절 함 판매 가능 처리 시작");

                // 1차 체크: 사물함이 선택되었는지 확인
                if (selectedLockers == null || selectedLockers.Count == 0)
                {
                    MessageBox.Show(
                        "먼저 입고할 사물함을 선택해주세요.\n\n품절 상태의 사물함을 선택하고 입고 버튼을 누르면\n판매 가능 상태로 변경됩니다.",
                        "안내",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Console.WriteLine("입고 실패: 사물함이 선택되지 않았습니다.");
                    return;
                }

                // 2차 체크: 품절 상태(IsUsing=true)의 사물함만 필터링
                var soldoutLockers = selectedLockers.Where(l => l.IsUsing).ToList();

                if (soldoutLockers.Count == 0)
                {
                    MessageBox.Show(
                        "선택된 사물함 중 품절 상태의 사물함이 없습니다.\n\n입고 기능은 품절 상태(빨간색)의 사물함만 처리합니다.",
                        "안내",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Console.WriteLine("입고 실패: 품절 상태의 사물함이 없습니다.");
                    return;
                }

                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    MessageBox.Show("AREACODE를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("입고 실패: AREACODE를 찾을 수 없습니다.");
                    return;
                }

                // 확인 메시지
                string soldoutNumbers = string.Join(", ", soldoutLockers.Select(l => l.LockerNumber));
                var result = MessageBox.Show(
                    $" 입고 처리\n\n품절 함: {soldoutNumbers}번 ({soldoutLockers.Count}개)\n\n위 사물함들을 판매 가능 상태로 변경하고 열겠습니까?",
                    "입고 확인",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    Console.WriteLine("입고 취소: 사용자가 취소했습니다.");
                    return;
                }

                // 입고 처리할 사물함 번호 리스트 초기화
                _updateSelectedBoxNumbers.Clear();

                // 각 품절 함을 판매 가능 상태로 변경
                int successCount = 0;
                int totalCount = soldoutLockers.Count;

                Console.WriteLine($"입고 처리 시작: {totalCount}개 품절 사물함 처리");

                foreach (var locker in soldoutLockers.ToList())
                {
                    if (locker == null)
                    {
                        Console.WriteLine("Null 사물함 건너뜀");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"사물함 {locker.LockerNumber} 입고 처리 시작 (품절→판매가능)");

                        // DB에서 useState를 2(판매가능)로 변경
                        bool success = DBConnector.Instance.UpdateBoxState(
                            areaCode,
                            locker.LockerNumber,
                            2); // useState = 2 (판매가능)

                        if (success)
                        {
                            successCount++;
                            
                            // 열기 위한 리스트에 추가
                            _updateSelectedBoxNumbers.Add(locker.LockerNumber);

                            // 사물함 UI 업데이트 (품절 상태 해제)
                            locker.SetLockerState(false, true); // IsUsing=false (정상 상태)
                            locker.UpdateButtonImage();
                            locker.BackColor = Color.Transparent; // 배경색 초기화
                            
                            Console.WriteLine($"사물함 {locker.LockerNumber} 입고 처리 성공 (품절→판매가능)");
                        }
                        else
                        {
                            Console.WriteLine($"사물함 {locker.LockerNumber} 입고 처리 실패");
                        }
                    }
                    catch (Exception lockerEx)
                    {
                        Console.WriteLine($"사물함 {locker.LockerNumber} 처리 중 오류: {lockerEx.Message}");
                        Logger.Error($"사물함 {locker.LockerNumber} 입고 처리 중 오류: {lockerEx.Message}", lockerEx);
                        continue;
                    }
                }

                Console.WriteLine($"입고 처리 완료: {successCount}/{totalCount}개 성공");

                // 결과 메시지
                if (successCount == totalCount)
                {
                    MessageBox.Show(
                        $" 입고 처리 완료!\n\n품절 해제: {soldoutNumbers}번 ({successCount}개)\n\n사물함이 판매 가능 상태로 변경되었습니다.",
                        "입고 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    Console.WriteLine($"입고 처리 성공: {successCount}/{totalCount}개 사물함");
                }
                else if (successCount > 0)
                {
                    MessageBox.Show(
                        $"? 입고 처리 부분 완료\n\n성공: {successCount}개 / 전체: {totalCount}개",
                        "입고 부분 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Console.WriteLine($"입고 처리 부분 성공: {successCount}/{totalCount}개 사물함");
                }
                else
                {
                    MessageBox.Show(
                        " 입고 처리 실패\n\n로그를 확인해주세요.",
                        "입고 실패",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    Console.WriteLine($"입고 처리 전체 실패: 0/{totalCount}개 사물함");
                }

                // UI 초기화 및 새로고침
                ResetAllLockerButtonsToNormal();
                RefreshProductList();
                
                // 입고 처리 성공 시 자동으로 사물함 열기
                if (_updateSelectedBoxNumbers.Count > 0)
                {
                    StartAutoBoxOpening();
                }
                
                Console.WriteLine($"입고 처리 완료: {successCount}개 사물함 판매 가능 상태로 변경");
            }
            catch (Exception ex)
            {
                string errorMessage = $"입고 처리 중 오류가 발생했습니다: {ex.Message}";
                MessageBox.Show(errorMessage, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"store_button_Click 오류: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Logger.Error($"store_button_Click 오류: {ex.Message}", ex);

                // 오류 발생 시 안전한 상태로 복구 시도
                try
                {
                    Console.WriteLine("오류 후 상태 복구 시도");
                    ResetAllLockerButtonsToNormal();
                    RefreshProductList();
                }
                catch (Exception recoveryEx)
                {
                    Console.WriteLine($"상태 복구 중 추가 오류: {recoveryEx.Message}");
                }
            }
        }
    }
}
