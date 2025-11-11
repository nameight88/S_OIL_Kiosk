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
    public partial class ProductPage : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        private List<LockerButton> selectedLockers = new List<LockerButton>();

        public ProductPage()
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
            Product_list.GridLines = false; // 그리드 라인 제거
            Product_list.Font = new Font("Pretendard Variable", 20F, FontStyle.Bold);
            Product_list.BorderStyle = BorderStyle.None; // 외곽 테두리 제거
            Product_list.HeaderStyle = ColumnHeaderStyle.Nonclickable; // 헤더 클릭 비활성화

            // 배경색과 선택색 설정 (더 깔끔한 모양을 위해)
            //Product_list.BackColor = Color.FromArgb(255, 255, 246); // 배경색을 폼과 동일하게
            Product_list.ForeColor = Color.FromArgb(0, 72, 37); // 텍스트 색상
            // 컬럼 추가
            Product_list.Columns.Add("상품명", 400, HorizontalAlignment.Left);
            Product_list.Columns.Add("가격", 200, HorizontalAlignment.Right);
            Product_list.Columns.Add("사물함 번호", 150, HorizontalAlignment.Center);

            // 컬럼 헤더 스타일 개선
            foreach (ColumnHeader column in Product_list.Columns)
            {
                column.TextAlign = column.Index == 1 ? HorizontalAlignment.Right :
                                   column.Index == 2 ? HorizontalAlignment.Center : HorizontalAlignment.Left;
            }

            // 초기 총 가격 설정
            UpdateTotalPrice();
        }

        public void SetContext(s_oil.Services.ApplicationContext context)
        {
            _context = context;
        }

        private void ProductPage_Load(object sender, EventArgs e)
        {
            LoadProductPageData();
        }

        /// <summary>
        /// ProductPage 데이터를 로드합니다 (외부에서 호출 가능)
        /// </summary>
        public void LoadProductPageData()
        {
            try
            {
                // ?? 페이지 진입 시 음성 안내
                SoundManager.Instance.Speak("구매하실 상품을 선택해주세요.");
                
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
                Console.WriteLine($"ProductPage - JOIN으로 조회된 박스 개수: {boxes.Count}");

                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();
                Console.WriteLine($"ProductPage - LockerButton 개수: {lockerButtons.Count}");

                foreach (var button in lockerButtons)
                {
                    if (button == null) continue;

                    button.Click -= LockerButton_Click; // 기존 이벤트 제거
                    button.Click += LockerButton_Click; // 이벤트 핸들러 할당

                    button.IsAdminMode = false;

                    int boxNo = 0;
                    if (int.TryParse(button.Name.Replace("lockerButton", ""), out boxNo))
                    {
                        // 이미지는 이미 로드되어 있으므로 재설정 불필요
                        button.LockerNumber = boxNo;

                        // JOIN으로 가져온 데이터에서 해당 박스 찾기
                        var box = boxes.FirstOrDefault(b => b.BoxNo == boxNo);
                        if (box != null)
                        {
                            if (box.useState == 1)
                            {
                                button.SetLockerState(true, false);
                                Console.WriteLine($"ProductPage - Box {boxNo}: 품절 상태로 설정");
                            }
                            else
                            {
                                button.SetLockerState(false, true);

                                if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                                {
                                    // 제품명만 화면에 표시하지만, 가격 정보는 내부적으로 유지
                                    // 예: DB에서 "요소수 3개"를 가져오면 화면에는 "요소수\n3개"로 표시
                                    // 하지만 Price 속성에는 가격이 저장되어 결제 시 사용됨
                                    button.SetProductNameOnly(box.productName, box.price);
                                    
                                    Console.WriteLine($"ProductPage - Box {boxNo}: {box.productName} ({box.price:N0}원) 설정 (화면에는 이름만 표시)");
                                }
                                else
                                {
                                    button.SetProductInfo("",0);
                                }
                            }
                        }
                        else
                        {
                            button.SetLockerState(false, true);
                            button.SetProductInfo("",0);
                        }

                        button.UpdateButtonImage();
                    }
                }

                // 선택 상태 초기화
                ResetAllLockerButtonsToNormal();

                Logger.Info("ProductPage 데이터 로드 완료");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터를 불러오는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error($"ProductPage LoadProductPageData 오류: {ex.Message}", ex);
            }
        }

        private void home_butoon_Click(object sender, EventArgs e)
        {
            try
            {
                //  음성 안내 - 홈으로 이동
                SoundManager.Instance.Speak("처음으로 이동합니다.");
                
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

        private void card_pay_button_Cluck(object sender, EventArgs e)
        {
            try
            {
                // 선택된 상품이 있는지 확인
                if (selectedLockers.Count == 0)
                {
                    // 음성 안내 - 상품 미선택
                    SoundManager.Instance.Speak("구매할 상품을 선택해주세요.");
                    MessageBox.Show("구매할 상품을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 총 금액 계산
                int totalAmount = GetTotalPrice();
                if (totalAmount <= 0)
                {
                    //  음성 안내 - 금액 오류
                    //SoundManager.Instance.SpeakError("결제 금액이 올바르지 않습니다.");
                    MessageBox.Show("결제 금액이 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                //  음성 안내 - 결제 페이지 이동
                SoundManager.Instance.Speak("결제 페이지로 이동합니다.");
                
                Logger.Info($"결제 페이지로 이동 - 선택된 상품: {selectedLockers.Count}개, 총 금액: {totalAmount:N0}원");
                Logger.Info($"선택된 사물함: {string.Join(", ", selectedLockers.Select(l => l.LockerNumber))}");

                //  CardPayPage 가져오기
                var cardPayPage = _context.GetForm<Forms.Customer.CardPayPage>();

                //  선택된 사물함 정보와 금액 전달
                cardPayPage.SetPaymentInfo(totalAmount, new List<LockerButton>(selectedLockers));

                //  CardPayPage 표시
                _context?.Navigator?.ShowCustomerPaymentPage();

                this.ActiveControl = null;
            }
            catch (Exception ex)
            {
                // ?? 음성 안내 - 오류 발생
                //SoundManager.Instance.SpeakError("오류가 발생했습니다.");
                MessageBox.Show($"결제 페이지로 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error($"결제 페이지 이동 중 오류: {ex.Message}", ex);
            }
        }

        private void LockerButton_Click(object? sender, EventArgs e)
        {
            var clickedButton = sender as LockerButton;
            Console.WriteLine($"ProductPage: LockerButton_Click 이벤트 발생 - Button {clickedButton?.LockerNumber}");

            if (clickedButton == null || clickedButton.IsUsing || !clickedButton.Enabled)
            {
                //Console.WriteLine($"ProductPage - 클릭 무시: IsUsing={clickedButton?.IsUsing}, Enabled={clickedButton?.Enabled}");
                return;
            }

            // 현재 상태 확인 (LockerButton의 OnClick에서 이미 IsSelected가 토글된 후)
            bool isNowSelected = clickedButton.IsSelected;

            // ?? 음성 안내 추가 (선택된 경우에만)
            if (isNowSelected)
            {
                try
                {
                    SoundManager.Instance.SpeakLockerSelected(clickedButton.LockerNumber);
                    //Logger.Info($"음성 안내: {clickedButton.LockerNumber}번 사물함 선택");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"음성 안내 재생 실패: {ex.Message}");
                }
            }

            //Console.WriteLine($"ProductPage - 클릭 후 상태: IsSelected={isNowSelected}");
            //Console.WriteLine($"ProductPage - 이미지 상태: {(isNowSelected ? "PressedImage (선택됨)" : "NormalImage (선택 해제됨)")}");

            if (isNowSelected)
            {
                // PressedImage로 변경됨 = 상품 가격 추가
                if (!selectedLockers.Contains(clickedButton))
                {
                    selectedLockers.Add(clickedButton);
                    AddProductToList(clickedButton);
                }
            }
            else
            {
                // NormalImage로 변경됨 = 상품 가격 제거
                if (selectedLockers.Contains(clickedButton))
                {
                    selectedLockers.Remove(clickedButton);
                    RemoveProductFromList(clickedButton);
                }
            }

            // 총 가격 실시간 업데이트
            UpdateTotalPrice();
        }

        /// <summary>
        /// Product_list에 상품을 추가합니다
        /// </summary>
        /// <param name="locker">선택된 사물함 버튼</param>
        private void AddProductToList(LockerButton locker)
        {
            try
            {
                // 상품명이 비어있으면 기본 텍스트 사용
                string productName = string.IsNullOrEmpty(locker.ProductName)
                    ? $"사물함 #{locker.LockerNumber}"
                    : locker.ProductName;

                string priceText = locker.Price > 0
                    ? $"{locker.Price:N0}원"
                    : "가격 미정";

                // ListView에 아이템 추가
                ListViewItem item = new ListViewItem(productName);
                item.SubItems.Add(priceText);
                item.SubItems.Add(locker.LockerNumber.ToString());
                item.Tag = locker.LockerNumber; // 사물함 번호를 태그로 저장

                Product_list.Items.Add(item);

                //Console.WriteLine($"Product_list에 추가됨: {productName} - {priceText} (사물함 #{locker.LockerNumber})");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Product_list 추가 중 오류: {ex.Message}");
                Logger.Error($"AddProductToList 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Product_list에서 상품을 제거합니다
        /// </summary>
        /// <param name="locker">선택 해제된 사물함 버튼</param>
        private void RemoveProductFromList(LockerButton locker)
        {
            try
            {
                // 해당 사물함 번호와 일치하는 아이템 찾아서 제거
                for (int i = Product_list.Items.Count - 1; i >= 0; i--)
                {
                    if (Product_list.Items[i].Tag != null &&
                        (int)Product_list.Items[i].Tag == locker.LockerNumber)
                    {
                        Product_list.Items.RemoveAt(i);
                        Console.WriteLine($"Product_list에서 제거됨: 사물함 #{locker.LockerNumber}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Product_list 제거 중 오류: {ex.Message}");
                Logger.Error($"RemoveProductFromList 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 총 가격을 계산하고 price_result에 업데이트합니다
        /// </summary>
        private void UpdateTotalPrice()
        {
            try
            {
                int totalPrice = selectedLockers.Sum(locker => locker.Price);

                // 선택된 상품이 같으면 강조된 스타일로 표시
                if (selectedLockers.Count > 0)
                {
                    price_result.Text = $"{totalPrice:N0} ({selectedLockers.Count}개 선택)";
                    price_result.ForeColor = Color.FromArgb(206, 14, 14);
                }
                else
                {
                    //price_result.Text = "상품을 선택해주세요";
                    //price_result.ForeColor = Color.FromArgb(128, 128, 128); // 회색으로 표시
                }

                // 텍스트 정렬 및 스타일 설정
                price_result.TextAlign = HorizontalAlignment.Center;
                price_result.Font = new Font("Pretendard Variable", 25F, FontStyle.Bold);
                //price_result.BackColor = Color.FromArgb(255, 255, 246);
                price_result.ReadOnly = true;

                //Console.WriteLine($"총 가격 업데이트: {totalPrice:N0}원 ({selectedLockers.Count}개 상품 선택됨)");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"총 가격 업데이트 중 오류: {ex.Message}");
                Logger.Error($"UpdateTotalPrice 오류: {ex.Message}", ex);
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
            }
            selectedLockers.Clear();

            // Product_list와 price_result도 초기화
            Product_list.Items.Clear();
            UpdateTotalPrice();

            //Console.WriteLine("모든 선택이 해제되었습니다.");
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

                // selectedLockers 리스트와 Product_list도 초기화
                price_result.Text = $"";
                selectedLockers.Clear();
                Product_list.Items.Clear();
                UpdateTotalPrice();
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
        /// 결제 페이지로 이동합니다
        /// </summary>
        public void ProceedToPayment()
        {
            if (selectedLockers.Count == 0)
            {
                MessageBox.Show("구매할 상품을 선택해주세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // 선택된 상품 정보를 전달하면서 결제 페이지로 이동
                _context?.Navigator?.ShowCustomerPaymentPage();
                Console.WriteLine($"ProductPage - 결제 페이지로 이동: {selectedLockers.Count}개 상품 선택됨, 총 가격: {GetTotalPrice():N0}원");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"결제 페이지로 이동 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
