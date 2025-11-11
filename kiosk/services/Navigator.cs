using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using s_oil.Utils;

namespace s_oil.Services
{
    // 각각의 Forms들의 보여주는 여부를 결정하는 클래스 Show/Hide
    public class Navigator
    {
        private readonly Panel _container;
        private Form? _currentForm;
        private readonly ApplicationContext _context; // ApplicationContext 참조 추가

        public Navigator(Panel container)
        {
            _container = container;
            _context = ApplicationContext.Instance; // ApplicationContext 인스턴스 할당
        }

        public void ShowPage<T>() where T : Form, new()
        {
            try
            {
                // 현재 폼 숨김
                _currentForm?.Hide();

                // 안전하게 폼 가져오기 또는 생성
                Form form;
                try
                {
                    form = ApplicationContext.Instance.GetForm<T>();
                }
                catch (Exception ex)
                {
                    Logger.Warning($"기존 {typeof(T).Name} 폼을 찾을 수 없음. 새로 생성합니다: {ex.Message}");
                    form = ApplicationContext.Instance.CreateForm<T>();
                }

                // 컨테이너에서 모든 컨트롤 제거
                _container.Controls.Clear();

                // 폼 설정을 컨테이너에 맞게 조정
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                // 폼 표시 전에 데이터 새로고침 (ProductPage, ProductStatus, AdminControl인 경우)
                RefreshFormDataBeforeShow(form);

                // 폼을 컨테이너에 추가하고 표시
                _container.Controls.Add(form);
                form.Show();
                form.BringToFront();

                _currentForm = form;

                Logger.Info($"{typeof(T).Name} 페이지 표시 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"페이지 표시 중 오류 발생 ({typeof(T).Name}): {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 폼 표시 전에 데이터를 새로고치습니다
        /// </summary>
        /// <param name="form">새로고침할 폼</param>
        private void RefreshFormDataBeforeShow(Form form)
        {
            try
            {
                // ProductPage인 경우
                if (form is Forms.Customer.ProductPage productPage)
                {
                    Logger.Info("ProductPage 데이터 새로고침 시작");
                    productPage.LoadProductPageData();
                    Logger.Info("ProductPage 데이터 새로고침 완료");
                }
                // ProductStatus인 경우
                else if (form is Forms.Customer.ProductStatus productStatus)
                {
                    Logger.Info("ProductStatus 데이터 새로고침 시작");
                    // RefreshProductList 메서드 호출
                    productStatus.RefreshProductList();
                    Logger.Info("ProductStatus 데이터 새로고침 완료");
                }
                // AdminControl인 경우
                else if (form is Forms.AdminControl adminControl)
                {
                    Logger.Info("AdminControl 데이터 새로고침 시작");
                    // RefreshProductList 메서드 호출
                    adminControl.RefreshProductList();
                    Logger.Info("AdminControl 데이터 새로고침 완료");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"폼 데이터 새로고침 중 오류: {ex.Message}");
                // 오류가 발생해도 폼은 표시되도록 예외를 던지지 않음
            }
        }

        /// <summary>
        /// 모든 폼 숨기기
        /// </summary>
        private void HideAllForms()
        {
            try
            {
                _currentForm?.Hide();
                _container.Controls.Clear();
            }
            catch (Exception ex)
            {
                Logger.Error($"폼 숨기기 중 오류: {ex.Message}", ex);
            }
        }

        // 키오스크 첫 페이지로 이동
        public void ShowHomePage()
        {
            try
            {
                ShowPage<Forms.HomePage>();
            }
            catch (Exception ex)
            {
                Logger.Error($"홈페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        // 관리자 비밀번호 페이지로 이동
        public void ShowAdminPasswordPage()
        {
            try
            {
                ShowPage<Forms.AdminPassword>();
            }
            catch (Exception ex)
            {
                Logger.Error($"관리자 비밀번호 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        // 관리자 제어 페이지로 이동
        public void ShowAdminControlPage()
        {
            try
            {
                ShowPage<Forms.AdminControl>();
            }
            catch (Exception ex)
            {
                Logger.Error($"관리자 제어 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        // 상품 현황 페이지로 이동
        public void ShowCustomerProductStatusPage()
        {
            try
            {
                ShowPage<Forms.Customer.ProductStatus>();
            }
            catch (Exception ex)
            {
                Logger.Error($"상품 현황 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        public void ShowCustomerProductPage()
        {
            try
            {
                ShowPage<Forms.Customer.ProductPage>();
            }
            catch (Exception ex)
            {
                Logger.Error($"상품 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        // 고객 상품 구매 페이지로 이동 (ProductPage와 동일)
        public void ShowCustomerProductBuyPage()
        {
            try
            {
                ShowPage<Forms.Customer.ProductPage>();
            }
            catch (Exception ex)
            {
                Logger.Error($"상품 구매 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        public void ShowCustomerPaymentPage()
        {
            try
            {
                ShowPage<Forms.Customer.CardPayPage>();
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 알림/공지 페이지 표시
        /// </summary>
        public void ShowNoticePage()
        {
            try
            {
                ShowPage<Forms.Customer.Notice>();
            }
            catch (Exception ex)
            {
                Logger.Error($"알림 페이지 표시 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 범용 알림 표시 (모달 형태)
        /// </summary>
        /// <param name="title">제목</param>
        /// <param name="message">메시지</param>
        /// <param name="type">알림 타입</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        /// <param name="onClose">닫기 시 실행할 액션</param>
        public void ShowNotice(string title, string message,
            Forms.Customer.Notice.NoticeType type = Forms.Customer.Notice.NoticeType.Info,
            int autoCloseSeconds = 0, Action onClose = null)
        {
            try
            {
                var notice = ApplicationContext.Instance.GetForm<Forms.Customer.Notice>();
                
                // ? Notice 폼 크기를 867x717로 고정
                notice.Size = new Size(867, 717);
                notice.ClientSize = new Size(867, 717);
                notice.MinimumSize = new Size(867, 717);
                notice.MaximumSize = new Size(867, 717);
                notice.FormBorderStyle = FormBorderStyle.None;
                notice.StartPosition = FormStartPosition.CenterScreen;
                
                notice.ShowNotice(title, message, type, autoCloseSeconds, onClose);
                
                Logger.Info($"Notice 표시 완료 - 크기: {notice.Size.Width}x{notice.Size.Height}");
            }
            catch (Exception ex)
            {
                Logger.Error($"알림 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///  결제 관련 알림 표시
        /// </summary>
        /// <param name="message">결제 메시지</param>
        /// <param name="isSuccess">성공 여부</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        /// <param name="onClose">닫기 시 실행할 액션</param>
        public void ShowPaymentNotice(string message, bool isSuccess, int autoCloseSeconds = 3, Action onClose = null)
        {
            try
            {
                var notice = ApplicationContext.Instance.GetForm<Forms.Customer.Notice>();
                
                // ? Notice 폼 크기를 867x717로 고정
                notice.Size = new Size(867, 717);
                notice.ClientSize = new Size(867, 717);
                notice.MinimumSize = new Size(867, 717);
                notice.MaximumSize = new Size(867, 717);
                notice.FormBorderStyle = FormBorderStyle.None;
                notice.StartPosition = FormStartPosition.CenterScreen;
                
                notice.ShowPaymentNotice(message, isSuccess, autoCloseSeconds, onClose);
                
                Logger.Info($"결제 Notice 표시 완료 - 크기: {notice.Size.Width}x{notice.Size.Height}");
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 알림 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사물함 동작 알림 표시
        /// </summary>
        /// <param name="boxNumber">사물함 번호</param>
        /// <param name="action">동작</param>
        /// <param name="isSuccess">성공 여부</param>
        /// <param name="autoCloseSeconds">자동 닫기 시간</param>
        public void ShowBoxOperationNotice(int boxNumber, string action, bool isSuccess, int autoCloseSeconds = 3)
        {
            try
            {
                var notice = ApplicationContext.Instance.GetForm<Forms.Customer.Notice>();
                
                // ? Notice 폼 크기를 867x717로 고정
                notice.Size = new Size(867, 717);
                notice.ClientSize = new Size(867, 717);
                notice.MinimumSize = new Size(867, 717);
                notice.MaximumSize = new Size(867, 717);
                notice.FormBorderStyle = FormBorderStyle.None;
                notice.StartPosition = FormStartPosition.CenterScreen;
                
                notice.ShowBoxOperationNotice(boxNumber, action, isSuccess, autoCloseSeconds);
                
                Logger.Info($"사물함 동작 Notice 표시 완료 - 크기: {notice.Size.Width}x{notice.Size.Height}");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 동작 알림 표시 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 사물함 열기 페이지로 이동
        /// </summary>
        /// <param name="purchasedBoxNumbers">구매한 사물함 번호 목록</param>
        public void ShowOpenBoxPage(List<int> purchasedBoxNumbers = null)
        {
            try
            {
                Logger.Info($"사물함 열기 페이지로 이동 - 구매한 사물함: {(purchasedBoxNumbers?.Count ?? 0)}개");

                HideAllForms();

                var openBoxPage = _context.GetForm<Forms.Customer.OpenBoxPage>();

                // 구매한 사물함 정보 설정
                if (purchasedBoxNumbers != null && purchasedBoxNumbers.Count > 0)
                {
                    openBoxPage.SetPurchasedBoxes(purchasedBoxNumbers);
                }

                openBoxPage.Show();
                openBoxPage.BringToFront();

                Logger.Info("사물함 열기 페이지 표시 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"사물함 열기 페이지 이동 중 오류: {ex.Message}", ex);
            }
        }
    }
}