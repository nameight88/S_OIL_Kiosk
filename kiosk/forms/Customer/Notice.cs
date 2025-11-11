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

namespace s_oil.Forms.Customer
{
    /// <summary>
    /// 범용 알림/공지 시스템
    /// 결제, 사물함, 시스템 등 다양한 알림을 처리합니다
    /// </summary>
    public partial class Notice : Form
    {
        public enum NoticeType
        {
            Info,           // 일반 정보
            Success,        // 성공
            Warning,        // 경고
            Error,          // 오류
            Payment,        // 결제 관련
            System,         // 시스템 관련
            BoxOperation    // 사물함 동작 관련
        }

        private NoticeType _noticeType = NoticeType.Info;
        private int _autoCloseSeconds = 0;
        private System.Windows.Forms.Timer _autoCloseTimer;
        private Action _onCloseAction;
        private Action _onConfirmAction;

        public Notice()
        {
            InitializeComponent();
            InitializeNoticeUI();
        }

        /// <summary>
        /// 알림 UI 초기화 (Designer 요소들 활용)
        /// </summary>
        private void InitializeNoticeUI()
        {
            try
            {
                this.Text = "알림";
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.TopMost = true;

                //  notice_content 크기 및 위치 고정
                notice_content.Location = new Point(115, 280);
                notice_content.Size = new Size(690, 280);
                notice_content.Anchor = AnchorStyles.None; // 앵커 제거 (크기 고정)
                notice_content.Multiline = true;
                notice_content.TextAlign = HorizontalAlignment.Center;
                notice_content.ReadOnly = true;
                notice_content.BorderStyle = BorderStyle.None;
                notice_content.BackColor = Color.White;
                notice_content.ForeColor = Color.FromArgb(60, 60, 60);
                notice_content.Font = new Font("Pretendard Variable", 18F, FontStyle.Bold);
                notice_content.TabStop = false;
                notice_content.Cursor = Cursors.Arrow;

                //  confirm_button 크기 및 위치 고정
                confirm_button.Location = new Point(310, 566);
                confirm_button.Size = new Size(264, 94);
                confirm_button.Anchor = AnchorStyles.None; // 앵커 제거 (크기 고정)
                confirm_button.FlatStyle = FlatStyle.Flat;
                confirm_button.FlatAppearance.BorderSize = 0;
                confirm_button.BackColor = Color.Transparent;
                confirm_button.UseVisualStyleBackColor = false;
                confirm_button.Cursor = Cursors.No;

                // confirm_button 이벤트 연결
                confirm_button.Click += BtnConfirm_Click;

                Logger.Info("Notice UI 초기화 완료 (Designer 기반)");
            }
            catch (Exception ex)
            {
                Logger.Error($"Notice UI 초기화 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 일반 알림 표시
        /// </summary>
        /// <param name="title">제목 (배경이미지에 "알림"으로 고정되어 있음)</param>
        /// <param name="message">메시지</param>
        /// <param name="type">알림 타입</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간 (0이면 수동)</param>
        /// <param name="onClose">닫기 시 실행할 액션</param>
        public void ShowNotice(string title, string message, NoticeType type = NoticeType.Info, 
            int autoCloseSeconds = 0, Action onClose = null)
        {
            try
            {
                _noticeType = type;
                _autoCloseSeconds = autoCloseSeconds;
                _onCloseAction = onClose;

                // notice_content에 메시지 표시
                notice_content.Text = message;

                SetNoticeStyle(type);
                
          
                this.Show();
                this.BringToFront();
                
                Logger.Info($"Notice 표시: {title} - {message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Notice 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 결제 관련 알림 (PayForm 스타일)
        /// </summary>
        /// <param name="message">결제 메시지</param>
        /// <param name="isSuccess">성공 여부</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        /// <param name="onClose">닫기 시 실행할 액션</param>
        public void ShowPaymentNotice(string message, bool isSuccess, int autoCloseSeconds = 3, Action onClose = null)
        {
            string title = isSuccess ? "결제 완료" : "결제 실패";
            NoticeType type = isSuccess ? NoticeType.Success : NoticeType.Error;
            
            ShowNotice(title, message, type, autoCloseSeconds, onClose);
        }

        /// <summary>
        /// 사물함 동작 알림
        /// </summary>
        /// <param name="boxNumber">사물함 번호</param>
        /// <param name="action">동작 (열기, 닫기 등)</param>
        /// <param name="isSuccess">성공 여부</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        public void ShowBoxOperationNotice(int boxNumber, string action, bool isSuccess, int autoCloseSeconds = 3)
        {
            string title = $"사물함 {boxNumber}번 {action}";
            string message = isSuccess ? $"사물함 {boxNumber}번이 성공적으로 {action}되었습니다." 
                                      : $"사물함 {boxNumber}번 {action}에 실패했습니다.";
            NoticeType type = isSuccess ? NoticeType.Success : NoticeType.Warning;
            
            ShowNotice(title, message, type, autoCloseSeconds);
        }

        /// <summary>
        /// 시스템 알림
        /// </summary>
        /// <param name="message">시스템 메시지</param>
        /// <param name="isError">오류 여부</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        public void ShowSystemNotice(string message, bool isError = false, int autoCloseSeconds = 5)
        {
            string title = isError ? "시스템 오류" : "시스템 알림";
            NoticeType type = isError ? NoticeType.Error : NoticeType.System;
            
            ShowNotice(title, message, type, autoCloseSeconds);
        }

        /// <summary>
        /// 알림 타입에 따른 스타일 설정
        /// </summary>
        private void SetNoticeStyle(NoticeType type)
        {
            switch (type)
            {
                case NoticeType.Success:
                case NoticeType.Payment when _noticeType == NoticeType.Success:
                    notice_content.ForeColor = Color.FromArgb(0, 120, 0);
                    break;

                case NoticeType.Error:
                    notice_content.ForeColor = Color.FromArgb(180, 0, 0);
                    break;

                case NoticeType.Warning:
                    notice_content.ForeColor = Color.FromArgb(200, 100, 0);
                    break;

                case NoticeType.Payment:
                    notice_content.ForeColor = Color.FromArgb(0, 80, 160);
                    break;

                case NoticeType.BoxOperation:
                    notice_content.ForeColor = Color.FromArgb(80, 40, 120);
                    break;

                case NoticeType.System:
                    notice_content.ForeColor = Color.FromArgb(80, 80, 80);
                    break;

                default: // Info
                    notice_content.ForeColor = Color.FromArgb(60, 60, 60);
                    break;
            }
        }

      

        /// <summary>
        /// 원본 메시지 추출 (카운트다운 메시지 제거)
        /// </summary>
        /// <param name="currentMessage">현재 표시된 메시지</param>
        /// <returns>카운트다운 메시지가 제거된 원본 메시지</returns>
        private string GetOriginalMessage(string currentMessage)
        {
            if (string.IsNullOrEmpty(currentMessage))
                return "";

            // "N초 후 자동으로 닫힙니다." 패턴 제거
            var lines = currentMessage.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            var originalLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // 카운트다운 관련 메시지가 아닌 경우만 추가
                if (!string.IsNullOrEmpty(trimmedLine) && 
                    !trimmedLine.Contains("초 후 자동으로 닫힙니다") &&
                    !trimmedLine.Contains("초 후 자동으로") &&
                    !trimmedLine.Contains("자동으로 닫힙니다"))
                {
                    originalLines.Add(line);
                }
            }

            return string.Join("\n", originalLines).Trim();
        }

        /// <summary>
        /// 원본 메시지에 카운트다운 메시지 추가하여 업데이트
        /// </summary>
        /// <param name="originalMessage">원본 메시지</param>
        /// <param name="seconds">남은 시간(초)</param>
        private void UpdateMessageWithCountdown(string originalMessage, int seconds)
        {
            try
            {
                // 간단하게 카운트다운 시간과 함께 "처음 화면으로 돌아갑니다." 메시지만 표시
                //notice_content.Text = $"{seconds}초 후에 처음 화면으로 돌아갑니다.";
                notice_content.ForeColor = Color.FromArgb(0, 72, 37);
                notice_content.Font=new Font("Pretendard Variable", 30F, FontStyle.Bold);
                notice_content.Text = $"{seconds}초 후에 다음 화면으로 돌아갑니다.";
            }
            catch (Exception ex)
            {
                Logger.Error($"메시지 업데이트 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 확인 버튼 클릭
        /// </summary>
        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                _onConfirmAction?.Invoke();
                CloseNotice();
            }
            catch (Exception ex)
            {
                Logger.Error($"확인 버튼 클릭 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 알림 닫기
        /// </summary>
        private void CloseNotice()
        {
            try
            {
                if (_autoCloseTimer != null)
                {
                    _autoCloseTimer.Stop();
                    _autoCloseTimer.Dispose();
                    _autoCloseTimer = null;
                }

                _onCloseAction?.Invoke();
                this.Hide();
                
                Logger.Info("Notice 닫기 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"Notice 닫기 중 오류: {ex.Message}", ex);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (_autoCloseTimer != null)
                {
                    _autoCloseTimer.Stop();
                    _autoCloseTimer.Dispose();
                    _autoCloseTimer = null;
                }
                
                base.OnFormClosed(e);
            }
            catch (Exception ex)
            {
                Logger.Error($"Notice Form 종료 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 키보드 단축키 처리
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape || keyData == Keys.Enter)
            {
                BtnConfirm_Click(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
