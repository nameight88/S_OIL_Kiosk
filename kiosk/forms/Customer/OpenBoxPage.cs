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
using s_oil.Utils;
using s_oil.models;
using System.IO;

namespace s_oil.Forms.Customer          
{
    public partial class OpenBoxPage : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        private List<int> _purchasedBoxNumbers = new List<int>();
        private Dictionary<int, LockerButton> _purchasedButtons = new Dictionary<int, LockerButton>();
        private Dictionary<int, BoxMaster> _boxDataMap = new Dictionary<int, BoxMaster>(); // 박스 데이터 매핑
        private System.Windows.Forms.Timer _autoReturnTimer;
        private int _autoReturnSeconds = 30;

    
        public OpenBoxPage()
        {
            InitializeComponent();
            InitializeOpenBoxPage();
        }

        /// <summary>
        /// OpenBoxPage UI 초기화
        /// </summary>
        private void InitializeOpenBoxPage()
        {
            try
            {
                this.Text = "사물함 열기";
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;

                //  box_open_notice_content TextBox 스타일 설정 (Designer와 동기화)
                box_open_notice_content.Multiline = true;
                box_open_notice_content.TextAlign = HorizontalAlignment.Center;
                box_open_notice_content.ReadOnly = true;
                box_open_notice_content.BorderStyle = BorderStyle.None;
                box_open_notice_content.BackColor = Color.White;
                box_open_notice_content.ForeColor = Color.FromArgb(82, 82, 82);
                box_open_notice_content.Font = new Font("Pretendard Variable", 60F, FontStyle.Bold);
                box_open_notice_content.TabStop = false; // 탭 이동 비활성화
                box_open_notice_content.Cursor = Cursors.No; // 마우스 커서를 금지 아이콘으로 변경
                box_open_notice_content.Enabled = true; // Enable 상태 유지 (텍스트 표시용)
                box_open_notice_content.ShortcutsEnabled = false; // 단축키 비활성화

                // button1 (홈으로 이동 버튼) 이벤트 연결
                button1.Click += OnHomeButton_Click;

                // 모든 사물함 버튼 초기화
                InitializeLockerButtons();

                Logger.Info("OpenBoxPage UI 초기화 완료 (box_open_notice_content 읽기 전용 설정)");
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenBoxPage UI 초기화 중 오류: {ex.Message}", ex);
            }
        }

     
        /// <summary>
        /// 사물함 버튼들 초기화
        /// </summary>
        private void InitializeLockerButtons()
        {
            try
            {
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();

                foreach (var button in lockerButtons)
                {
                    if (int.TryParse(button.Name.Replace("lockerButton", ""), out int boxNo))
                    {
                        button.LockerNumber = boxNo;
                        button.Text = boxNo.ToString();
                        button.IsAdminMode = false;
                        button.Enabled = false;
                        button.IsUsing = false;
                        button.IsSelected = false;

                        button.Click += LockerButton_Click;
                    }
                }

                Logger.Info($"사물함 버튼 초기화 완료: {lockerButtons.Count}개");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 버튼 초기화 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 구매한 사물함 정보로 페이지 설정
        /// </summary>
        public void SetPurchasedBoxes(List<int> purchasedBoxNumbers)
        {
            try
            {
                _purchasedBoxNumbers = purchasedBoxNumbers ?? new List<int>();
                _purchasedButtons.Clear();
                _boxDataMap.Clear();

                Logger.Info($"구매한 사물함 설정: {string.Join(", ", _purchasedBoxNumbers)}");

                //  모든 사물함 데이터 로드 (품절 상태 + 상품 정보)
                LoadAllBoxData();

                // 안내 메시지 설정
                SetNoticeMessage();

                // 구매한 사물함 버튼들 강조 표시
                HighlightPurchasedBoxes();

                // 자동 사물함 열기 시작
                StartAutoBoxOpening();

                // 자동 홈 이동 타이머 시작
                StartAutoReturnTimer();
            }
            catch (Exception ex)
            {
                Logger.Error($"구매한 사물함 설정 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///  모든 사물함 데이터 로드
        /// </summary>
        private void LoadAllBoxData()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");

                if (string.IsNullOrEmpty(areaCode))
                {
                    Logger.Warning("AREACODE를 찾을 수 없습니다.");
                    return;
                }

                //  JOIN을 사용하여 모든 박스와 제품 정보를 한 번에 가져오기
                List<BoxMaster> boxes = DBConnector.Instance.GetBoxesWithProductsByAreaCode(areaCode);
                Logger.Info($" OpenBoxPage - JOIN으로 조회된 박스 개수: {boxes.Count}");

                // 모든 박스 데이터 매핑 (구매한 박스 + 품절 박스 모두)
                foreach (var box in boxes)
                {
                    _boxDataMap[box.BoxNo] = box;
                }

                Logger.Info($"? 박스 데이터 로드 완료: {_boxDataMap.Count}개");

                // ? 모든 사물함 버튼에 상품 정보 + 품절 상태 반영 (ProductPage와 동일)
                UpdateAllLockerButtonsWithData();
            }
            catch (Exception ex)
            {
                Logger.Error($"박스 데이터 로드 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///  모든 사물함 버튼에 상품 정보 + 품절 상태 반영 (ProductPage/ProductStatus와 동일)
        /// </summary>
        private void UpdateAllLockerButtonsWithData()
        {
            try
            {
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();

                foreach (var button in lockerButtons)
                {
                    if (button == null) continue;

                    int boxNo = button.LockerNumber;

                    if (_boxDataMap.TryGetValue(boxNo, out var box))
                    {
                        // ? 품절 상태 설정 (ProductPage와 동일)
                        if (box.useState == 1)
                        {
                            // 품절 (SoldoutImage)
                            button.SetLockerState(true, false);
                            Logger.Info($"? 사물함 {boxNo}번: 품절 상태로 설정");
                        }
                        else
                        {
                            // 판매 가능 (NormalImage)
                            button.SetLockerState(false, true);
                        }

                        // ? 상품 정보 설정 (ProductPage와 동일)
                        if (!string.IsNullOrEmpty(box.productCode) && !string.IsNullOrEmpty(box.productName))
                        {
                            // 제품명만 화면에 표시하지만 가격 정보는 내부적으로 유지
                            button.SetProductNameOnly(box.productName, box.price);
                            Logger.Info($" 사물함 {boxNo}번: {box.productName} ({box.price:N0}원) 설정 (화면에는 이름만 표시)");
                        }
                        else
                        {
                            button.SetProductInfo("",0);
                        }
                    }
                    else
                    {
                        // DB에 데이터 없는 경우 기본값
                        button.SetLockerState(false, true);
                        button.SetProductInfo("",0);
                    }

                    button.UpdateButtonImage();
                }

                Logger.Info($" {lockerButtons.Count}개 사물함 버튼에 상품 정보 + 품절 상태 반영 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 버튼 데이터 업데이트 중 오류: {ex.Message}", ex);
            }
        }

       
        /// <summary>
        /// 안내 메시지 설정
        /// </summary>
        private void SetNoticeMessage()
        {
            try
            {
                if (_purchasedBoxNumbers.Count == 0)
                {
                    box_open_notice_content.Text = "구매한 사물함이 없습니다.";
                    return;
                }

                if (_purchasedBoxNumbers.Count == 1)
                {
                    box_open_notice_content.ForeColor = Color.FromArgb(82, 82, 82);
                    box_open_notice_content.Font = new Font("Pretendard Variable", 60F, FontStyle.Bold);
                    box_open_notice_content.Text = $" {_purchasedBoxNumbers[0]}번 ";
                }
                else
                {
                    string boxNumbersText = string.Join(", ", _purchasedBoxNumbers);
                    box_open_notice_content.Text = $" {boxNumbersText}번 ";
                }

                Logger.Info($"안내 메시지 설정: {box_open_notice_content.Text}");
            }
            catch (Exception ex)
            {
                Logger.Error($"안내 메시지 설정 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 구매한 사물함들 강조 표시 + 상품 정보 표시
        /// </summary>
        private void HighlightPurchasedBoxes()
        {
            try
            {
                var lockerButtons = this.Controls.OfType<LockerButton>().ToList();

                foreach (var button in lockerButtons)
                {
                    if (_purchasedBoxNumbers.Contains(button.LockerNumber))
                    {
                        // PressedImage로 표시 (상품 정보는 이미 UpdateAllLockerButtonsWithData에서 설정됨)
                        button.IsSelected = true;
                        button.Enabled = true;
                        button.UpdateButtonImage();
                        
                        _purchasedButtons[button.LockerNumber] = button;

                        Logger.Info($" 사물함 {button.LockerNumber}번 PressedImage로 강조 표시");
                    }
                    else
                    {
                        // 구매하지 않은 사물함
                        button.IsSelected = false;
                        button.Enabled = false;
                        button.UpdateButtonImage();
                    }
                }

                Logger.Info($"총 {_purchasedButtons.Count}개 사물함을 PressedImage로 표시");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 강조 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 자동 사물함 열기 시작
        /// </summary>
        private void StartAutoBoxOpening()
        {
            try
            {
                if (_purchasedBoxNumbers.Count == 0)
                {
                    Logger.Warning("열 사물함이 없습니다.");
                    return;
                }

                Task.Delay(2000).ContinueWith(_ =>
                {
                    this.Invoke(new Action(() =>
                    {
                        OpenPurchasedBoxes();
                    }));
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"자동 사물함 열기 시작 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 구매한 사물함들 열기
        /// </summary>
        private async void OpenPurchasedBoxes()
        {
            try
            {
                if (_context?.BUDevice == null)
                {
                    Logger.Warning("BU 디바이스가 초기화되지 않았습니다.");
                    UpdateNoticeMessage("사물함을 열 수 없습니다.", Color.Red);
                    return;
                }

                Logger.Info($"구매한 사물함 {_purchasedBoxNumbers.Count}개 열기 시작");

                int successCount = 0;
                int failCount = 0;
                List<int> openedBoxes = new List<int>();
                List<int> failedBoxes = new List<int>();

                foreach (int boxNumber in _purchasedBoxNumbers)
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
                            //await ShowOpenSuccessEffect(boxNumber);
                        }
                        else
                        {
                            Logger.Error($"? 사물함 {boxNumber}번 열기 실패");
                            failCount++;
                            failedBoxes.Add(boxNumber);
                            //await ShowOpenFailureEffect(boxNumber);
                        }

                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"사물함 {boxNumber}번 열기 중 예외: {ex.Message}", ex);
                        failCount++;
                        failedBoxes.Add(boxNumber);
                        //await ShowOpenFailureEffect(boxNumber);
                    }
                }

                DisplayOpeningResult(successCount, failCount, openedBoxes, failedBoxes);

                Logger.Info($"사물함 열기 완료 - 성공: {successCount}개, 실패: {failCount}개");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 열기 처리 중 오류: {ex.Message}", ex);
                UpdateNoticeMessage("사물함을 열 수 없습니다.", Color.Red);
            }
        }

       
        /// <summary>
        /// 사물함 열기 결과 표시
        /// </summary>
        private void DisplayOpeningResult(int successCount, int failCount, List<int> openedBoxes, List<int> failedBoxes)
        {
            try
            {
                if (failCount == 0)
                {
                    if (successCount == 1)
                    {
                        UpdateNoticeMessage($" {openedBoxes[0]}번 ", Color.FromArgb(82, 82, 82));
                    }
                    else
                    {
                        string boxList = string.Join(", ", openedBoxes);
                        UpdateNoticeMessage($" {boxList}번 ", Color.FromArgb(82, 82, 82));
                    }
                }
               
                else
                {
                    string successList = string.Join(", ", openedBoxes);
                    string failList = string.Join(", ", failedBoxes);
                    UpdateNoticeMessage($"열린 함: {successList}번\n실패한 함: {failList}번\n관리자에게 문의하세요.", Color.Black);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"결과 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 안내 메시지 업데이트
        /// </summary>
        private void UpdateNoticeMessage(string message, Color color)
        {
            try
            {
                box_open_notice_content.Text = message;
                box_open_notice_content.ForeColor = color;
                box_open_notice_content.Font = new Font("Pretendard Variable", 60F, FontStyle.Bold);
                box_open_notice_content.Update();
            }
            catch (Exception ex)
            {
                Logger.Error($"안내 메시지 업데이트 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 자동 홈 이동 타이머 시작
        /// </summary>
        private void StartAutoReturnTimer()
        {
            try
            {
                _autoReturnSeconds = 30;
                _autoReturnTimer = new System.Windows.Forms.Timer();
                _autoReturnTimer.Interval = 1000;
                _autoReturnTimer.Tag = _autoReturnSeconds;
                _autoReturnTimer.Tick += AutoReturnTimer_Tick;
                _autoReturnTimer.Start();

                Logger.Info($"자동 홈 이동 타이머 시작: {_autoReturnSeconds}초");
            }
            catch (Exception ex)
            {
                Logger.Error($"자동 홈 이동 타이머 시작 중 오류: {ex.Message}", ex);
            }
        }

        private void AutoReturnTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                int remainingSeconds = (int)_autoReturnTimer.Tag - 1;

                if (remainingSeconds <= 0)
                {
                    _autoReturnTimer.Stop();
                    _autoReturnTimer.Dispose();
                    _autoReturnTimer = null;

                    ShowReturnHomeNotice();
                }
                else
                {
                    _autoReturnTimer.Tag = remainingSeconds;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"자동 홈 이동 타이머 중 오류: {ex.Message}", ex);
            }
        }

        private void ShowReturnHomeNotice()
        {
            try
            {
                Logger.Info("30초 경과 - 홈으로 돌아가기 Notice 표시");

                _context?.Navigator?.ShowNotice(
                    "안내",
                    "",
                    Notice.NoticeType.Info,
                    30,
                    () => GoToHomePage()
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"홈으로 돌아가기 Notice 표시 중 오류: {ex.Message}", ex);
                GoToHomePage();
            }
        }

        private void LockerButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is LockerButton clickedButton)
                {
                    Logger.Info($"사물함 {clickedButton.LockerNumber}번 버튼 클릭 (수동 재시도)");

                    if (_purchasedBoxNumbers.Contains(clickedButton.LockerNumber))
                    {
                        TryOpenBox(clickedButton.LockerNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 버튼 클릭 처리 중 오류: {ex.Message}", ex);
            }
        }

        private async void TryOpenBox(int boxNumber)
        {
            try
            {
                if (_context?.BUDevice == null)
                {
                    UpdateNoticeMessage("사물함 제어 장치에 문제가 있습니다.", Color.Black);
                    return;
                }

                Logger.Info($"수동으로 사물함 {boxNumber}번 열기 재시도");
                UpdateNoticeMessage($" {boxNumber}번을 다시 열고 있습니다... ", Color.Black);

                bool result = _context.BUDevice.OpenBox(boxNumber, false, 5000);

                if (result)
                {
                    Logger.Info($" 수동 열기 성공: {boxNumber}번");
                    UpdateNoticeMessage($" {boxNumber}번이 열렸습니다! ", Color.Black);
                    //await ShowOpenSuccessEffect(boxNumber);
                }
                else
                {
                    Logger.Error($"? 수동 열기 실패: {boxNumber}번");
                    UpdateNoticeMessage($" {boxNumber}번 열기 실패\n다시 시도하거나 관리자에게 문의하세요. ", Color.Black);
                    //await ShowOpenFailureEffect(boxNumber);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"수동 사물함 열기 중 오류: {ex.Message}", ex);
                UpdateNoticeMessage("사물함 열기 중 오류가 발생했습니다.", Color.Red);
            }
        }

        private void OnHomeButton_Click(object sender, EventArgs e)
        {
            try
            {
                GoToHomePage();
            }
            catch (Exception ex)
            {
                Logger.Error($"홈 버튼 클릭 처리 중 오류: {ex.Message}", ex);
            }
        }

        private void GoToHomePage()
        {
            try
            {
                if (_autoReturnTimer != null)
                {
                    _autoReturnTimer.Stop();
                    _autoReturnTimer.Dispose();
                    _autoReturnTimer = null;
                }

                ResetAllLockerButtons();

                Logger.Info("홈 페이지로 이동");
                _context?.Navigator?.ShowHomePage();
                this.Hide();
            }
            catch (Exception ex)
            {
                Logger.Error($"홈 페이지 이동 중 오류: {ex.Message}", ex);
            }
        }

        private void ResetAllLockerButtons()
        {
            try
            {
                foreach (var button in _purchasedButtons.Values)
                {
                    button.IsSelected = false;
                    button.FlatAppearance.BorderSize = 0;
                    button.BackColor = Color.Transparent;
                    button.SetProductInfo("", 0 );
                    button.UpdateButtonImage();
                }

                _purchasedButtons.Clear();
                _purchasedBoxNumbers.Clear();
                _boxDataMap.Clear();


                Logger.Info("모든 사물함 버튼 + 상품 정보 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 버튼 초기화 중 오류: {ex.Message}", ex);
            }
        }

        public void SetContext(s_oil.Services.ApplicationContext context)
        {
            _context = context;
        }

        private void FormProducts_Shown(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        private void OpenBoxPage_Load(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("OpenBoxPage 로드 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenBoxPage 로드 중 오류: {ex.Message}", ex);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (_autoReturnTimer != null)
                {
                    _autoReturnTimer.Stop();
                    _autoReturnTimer.Dispose();
                    _autoReturnTimer = null;
                }

                ResetAllLockerButtons();
                base.OnFormClosed(e);
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenBoxPage 종료 중 오류: {ex.Message}", ex);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape || keyData == Keys.Home)
            {
                OnHomeButton_Click(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void box_open_notice_content_TextChanged(object sender, EventArgs e)
        {
        }
    }
}