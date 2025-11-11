using s_oil.Services;
using s_oil.Utils;

namespace s_oil
{
    public partial class MainForm : Form
    {
        private Panel _containerPanel;
        private Navigator _navigator;

        public MainForm()
        {
            InitializeComponent();
            
            // ? 전체 화면 설정 추가
            ConfigureFullScreen();
            
            _ = InitializeApplicationAsync();
        }

        /// <summary>
        /// 전체 화면 모드 설정
        /// </summary>
        private void ConfigureFullScreen()
        {
            try
            {
                // 1. 창 크기 및 위치 설정
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.StartPosition = FormStartPosition.Manual;
                
                // 2. 1080x1920 크기 설정
                this.ClientSize = new Size(1080, 1920);
                this.Size = new Size(1080, 1920);
                
                // 3. 화면 왼쪽 상단에 위치 (0, 0)
                this.Location = new Point(0, 0);
                
                // 4. 최상위 창으로 설정
                this.TopMost = false; // 개발 중에는 false, 배포 시 true
                
                // 5. 크기 조절 및 최소화/최대화 버튼 제거
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ControlBox = false;
                
                // 6. 여백 제거
                this.Padding = new Padding(0);
                this.Margin = new Padding(0);
                
                Logger.Info($"전체 화면 모드 설정 완료: {this.ClientSize.Width}x{this.ClientSize.Height}");
            }
            catch (Exception ex)
            {
                Logger.Error($"전체 화면 모드 설정 중 오류: {ex.Message}", ex);
            }
        }

        private async Task InitializeApplicationAsync()
        {
            // 로딩 메시지 표시
            ShowLoadingMessage("프로그램 시작 중입니다....");

            try
            {
                // 1. Container 패널 설정 (최우선 추가)
                SetupContainer();

                // 2. ApplicationContext 초기화 (디바이스, DB 등)
                await s_oil.Services.ApplicationContext.Instance.InitializeAsync();

                // 3. Navigator 초기화 (Container가 준비된 후)
                _navigator = new Navigator(_containerPanel);
                s_oil.Services.ApplicationContext.Instance.SetNavigator(_navigator);

                // 4. 로딩 메시지 숨김 (HomePage 표시 직전)
                HideLoadingMessage();

                // 5. HomePage 표시 (비동기 작업 완료)
                _navigator.ShowHomePage();

                Logger.Info("MainForm 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"애플리케이션 초기화 실패: {ex.Message}", ex);
                MessageBox.Show($"애플리케이션 초기화 중 오류 발생: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void SetupContainer()
        {
            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Control,
                Name = "MainContainer",
                Padding = new Padding(0), // ? 여백 제거
                Margin = new Padding(0)   // ? 여백 제거
            };
            this.Controls.Add(_containerPanel);
            _containerPanel.BringToFront(); // Container를 최상단으로
            
            Logger.Info("MainContainer 설정 완료");
        }

        private void ShowLoadingMessage(string message)
        {
            var loadingLabel = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Pretendard Variable", 16, FontStyle.Bold),
                Name = "LoadingLabel",
                BackColor = Color.White
            };
            this.Controls.Add(loadingLabel);
            loadingLabel.BringToFront();
            this.Refresh(); // UI 즉시 업데이트
            
            Logger.Info($"로딩 메시지 표시: {message}");
        }

        private void HideLoadingMessage()
        {
            var loadingLabel = this.Controls.Find("LoadingLabel", false).FirstOrDefault();
            if (loadingLabel != null)
            {
                this.Controls.Remove(loadingLabel);
                loadingLabel.Dispose();
                Logger.Info("로딩 메시지 숨김 완료");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                Logger.Info("MainForm 종료 시작");
                
                // 디바이스 연결 해제
                s_oil.Services.ApplicationContext.Instance.BUDevice?.Disconnect();
                
                Logger.Info("MainForm 종료 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"MainForm 종료 중 오류: {ex.Message}", ex);
            }
            
            base.OnFormClosed(e);
        }

        // ? 키 입력 처리 (ESC로 종료, F11로 전체화면 토글)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // 개발 모드에서만 ESC 키로 종료 가능
            if (keyData == Keys.Escape)
            {
                #if DEBUG
                var result = MessageBox.Show("프로그램을 종료하시겠습니까?", "확인", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
                return true;
                #endif
            }
            
            // F11 키로 전체화면 토글
            if (keyData == Keys.F11)
            {
                if (this.FormBorderStyle == FormBorderStyle.None)
                {
                    // 일반 창 모드로 전환
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                    Logger.Info("일반 창 모드로 전환");
                }
                else
                {
                    // 전체 화면 모드로 전환
                    ConfigureFullScreen();
                    Logger.Info("전체 화면 모드로 전환");
                }
                return true;
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
