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
using PayControl;

namespace s_oil.Forms.Customer
{
    public partial class CardPayPage : Form, IContextAware
    {
        private s_oil.Services.ApplicationContext? _context;
        private int _paymentAmount = 0;
        private bool _isProcessingPayment = false;

        // PayForm 스타일 변수들
        public bool payStarted = false;
        public bool isPaid = false;
        public bool isCanceled = false;
        public Task? payTask;
        public System.Windows.Forms.Timer? checkPaidTimer = null;

        // 선택된 사물함 정보
        private List<LockerButton> _selectedLockers = new List<LockerButton>();
        private string _areaCode = "";

        //  결제 정보 저장용 필드 추가
        private PayControl.PaymentResult? _lastPaymentResult = null;


        public CardPayPage()
        {
            InitializeComponent();
            LoadAreaCode();
            InitializePayCtrl();
            
            // VisibleChanged 이벤트 핸들러 등록
            this.VisibleChanged += CardPayPage_VisibleChanged;
        }

        /// <summary>
        /// INI 파일에서 지역 코드 로드
        /// </summary>
        private void LoadAreaCode()
        {
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                _areaCode = iniParser.GetSetting("AreaConfig", "AREACODE");
                Logger.Info($"Area code loaded: {_areaCode}");
            }
            catch (Exception ex)
            {
                Logger.Error($"지역 코드 로드 실패: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// 결제 금액과 선택된 사물함 정보를 설정합니다
        /// </summary>
        /// <param name="amount">결제 금액</param>
        /// <param name="selectedLockers">선택된 사물함들</param>
        public void SetPaymentInfo(int amount, List<LockerButton> selectedLockers = null)
        {
            try
            {
                Logger.Info($"[SETINFO] 결제 정보 설정 시작 - 금액: {amount:N0}원, 사물함 수: {(selectedLockers?.Count ?? 0)}개");
                
                // 이전 상태 초기화
                _isProcessingPayment = false;
                payStarted = false;
                isPaid = false;
                isCanceled = false;
                payTask = null;
                
                _paymentAmount = amount;
                _selectedLockers = selectedLockers ?? new List<LockerButton>();

                // 가격 총합을 price_amount TextBox에 표시
                if (price_amount != null)
                {
                    // 개별 상품 가격 출력
                    if (_selectedLockers.Count > 0)
                    {
                        int totalPrice = _selectedLockers.Sum(l => l.Price);
                        price_amount.Text = $"{totalPrice:N0}";
                        Logger.Info($"price_amount 업데이트: {totalPrice:N0}원 (사물함 {_selectedLockers.Count}개)");
                        
                        // 각 사물함별 가격 로그
                        foreach (var locker in _selectedLockers)
                        {
                            Logger.Info($"  - 사물함 #{locker.LockerNumber}: {locker.ProductName} {locker.Price:N0}원");
                        }
                    }
                    else
                    {
                        price_amount.Text = $"{amount:N0}";
                        Logger.Info($"price_amount 업데이트: {amount:N0}원");
                    }
                }

                
                Logger.Info($"[SETINFO] 결제 정보 설정 완료 - 금액: {amount:N0}원, 사물함 수: {_selectedLockers.Count}개");
            }
            catch (Exception ex)
            {
                Logger.Error($"[SETINFO] 결제 정보 설정 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// PayCtrl 초기화
        /// </summary>
        private void InitializePayCtrl()
        {
            try
            {
                Logger.Info("[PAYCTRL-INIT] PayCtrl 초기화 시작");
                
                // PayCtrl은 정적(static) 클래스로 보이며, 별도 초기화가 필요 없음
                // Cancel() 호출 후에도 Pay()를 다시 호출할 수 있어야 함
                // DLL 내부에서 상태를 관리하므로 여기서는 로깅만 수행
                
                Logger.Info("[PAYCTRL-INIT] PayCtrl은 정적 객체로 별도 초기화 불필요");
                Logger.Info("[PAYCTRL-INIT] PayCtrl 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"[PAYCTRL-INIT] PayCtrl 초기화 실패: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// PayControl.PayCtrl 결제 시작
        /// </summary>
        public void OnPay()
        {
            if (_paymentAmount <= 0)
            {
                return;
            }

            if (_isProcessingPayment)
            {
                Logger.Warning("[ONPAY] 이미 결제가 진행 중입니다 - 중복 호출 방지");
                return;
            }

            // ? VCat.exe 프로세스 확인
            try
            {
                var vcatProcesses = System.Diagnostics.Process.GetProcessesByName("VCat");
                Logger.Info($"[VCAT-CHECK] VCat.exe 프로세스 수: {vcatProcesses.Length}");
                
                if (vcatProcesses.Length > 0)
                {
                    foreach (var proc in vcatProcesses)
                    {
                        Logger.Info($"[VCAT-CHECK] VCat.exe PID: {proc.Id}, StartTime: {proc.StartTime}");
                    }
                }
                else
                {
                    Logger.Warning("[VCAT-CHECK] VCat.exe가 실행되지 않았습니다");
                }
            }
            catch (Exception vcatEx)
            {
                Logger.Error($"[VCAT-CHECK] VCat.exe 확인 중 오류: {vcatEx.Message}", vcatEx);
            }

            // 이전 Task가 아직 실행 중이면 대기
            if (payTask != null && !payTask.IsCompleted)
            {
                Logger.Warning("[ONPAY] 이전 결제 Task가 아직 완료되지 않음 - 강제 취소 후 재시작");
                try
                {
                    // 강제 취소
                    CancelPayment();
                    
                    // 취소 후 충분한 대기 시간
                    System.Threading.Thread.Sleep(1000);
                    Logger.Info("[ONPAY] 이전 결제 취소 완료 - 재시작 준비");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[ONPAY] 이전 Task 취소 중 오류: {ex.Message}", ex);
                    return;
                }
            }

            try
            {
                // 상태 초기화
                _isProcessingPayment = true;
                payStarted = false;
                isPaid = false;
                isCanceled = false;
                

                Logger.Info($"[PAYMENT] PayCtrl 카드 결제 시작 - 금액: {_paymentAmount:N0}원");
                Logger.Info($"[PAYMENT] Form Handle: {this.Handle}, IsHandleCreated: {this.IsHandleCreated}");
                Logger.Info($"[PAYMENT] Form Type: {this.GetType().FullName}");

                // ? PayControl.dll 정보 확인
                try
                {
                    var assembly = System.Reflection.Assembly.GetAssembly(typeof(PayCtrl));
                    Logger.Info($"[PAYCTRL-DLL] DLL 경로: {assembly?.Location ?? "Unknown"}");
                    Logger.Info($"[PAYCTRL-DLL] DLL 버전: {assembly?.GetName().Version ?? new Version(0,0,0,0)}");
                }
                catch (Exception dllEx)
                {
                    Logger.Error($"[PAYCTRL-DLL] DLL 정보 확인 중 오류: {dllEx.Message}", dllEx);
                }

                // 결제 금액 캡처
                var paymentAmount = _paymentAmount;
                
                Logger.Info($"[PAYMENT] 결제 시작 - 금액: {paymentAmount}원");

                //  Pay() 호출 전 타이밍 기록
                var payStartTime = DateTime.Now;
                Logger.Info($"[PAYCTRL-TIMING] Pay() 호출 시작 시각: {payStartTime:HH:mm:ss.fff}");

                // PayCtrl - Task로 결제 처리
                payTask = Task.Run(() =>
                {
                    Logger.Info("[TASK] Task.Run() 람다 진입 성공!");
                    
                    try
                    {
                        payStarted = true;
                        Logger.Info("[TASK] payStarted = true 설정 완료");
                        
                        //  PayCtrl.PayWithResult() 호출 - 결제 정보 포함
                        Logger.Info($"[PAYCTRL] PayWithResult() 호출 시작 (금액: {paymentAmount})");
 
                        PayControl.PaymentResult? paymentResult = null;
                        string errorDetail = "";

                        try
                        {
                            Logger.Info("[PAYCTRL] PayCtrl.PayWithResult() 직접 호출 시작");
                            
                            //  PayWithResult() 호출 시간 측정 및 결제 정보 받기
                            var callStartTime = DateTime.Now;
                            paymentResult = PayCtrl.PayWithResult(this, paymentAmount);
                            var callEndTime = DateTime.Now;
                            var callDuration = (callEndTime - callStartTime).TotalMilliseconds;
                            
                            Logger.Info($"[PAYCTRL] PayCtrl.PayWithResult() 호출 완료 - 성공: {paymentResult.Success}");
                            Logger.Info($"[PAYCTRL-TIMING] PayWithResult() 소요 시간: {callDuration:F0}ms");
                            
                            // 결제 정보 상세 로그
                            Logger.Info($"[PAYCTRL-RESULT] 메시지: {paymentResult.Message}");
                            Logger.Info($"[PAYCTRL-RESULT] 카드번호: {paymentResult.CardNumber}");
                            Logger.Info($"[PAYCTRL-RESULT] 승인번호: {paymentResult.ApprovalNumber}");
                            Logger.Info($"[PAYCTRL-RESULT] 승인일시: {paymentResult.ApprovalDate}");
                            
                            // 즉시 False 반환 감지
                            if (!paymentResult.Success && callDuration < 1000)
                            {
                                Logger.Error($"[PAYCTRL-FAIL] PayWithResult()가 {callDuration:F0}ms 만에 실패 - VCat.exe 미실행 또는 단말기 미연결 가능성");
                                errorDetail = "카드 단말기 연결 오류 - VCat.exe가 실행되지 않았거나 단말기가 연결되지 않았습니다";
                            }
                            else if (!paymentResult.Success)
                            {
                                // 결제 실패 시 추가 정보 수집
                                Logger.Warning("[PAYCTRL] PayWithResult() 결제 실패");
                                errorDetail = paymentResult.Message ?? "카드 리더기 오류 또는 결제 거부";
                            }
                        }
                        catch (Exception payEx)
                        {
                            Logger.Error($"[PAYCTRL-EXCEPTION] PayWithResult() 호출 중 예외 발생: {payEx.Message}", payEx);
                            Logger.Error($"[PAYCTRL-EXCEPTION] Exception Type: {payEx.GetType().FullName}");
                            Logger.Error($"[PAYCTRL-EXCEPTION] StackTrace: {payEx.StackTrace}");
                            errorDetail = $"결제 예외: {payEx.Message}";
                            
                            // 예외 발생 시 기본 PaymentResult 생성
                            paymentResult = new PayControl.PaymentResult
                            {
                                Success = false,
                                Message = errorDetail
                            };
                        }
                        
                        isPaid = paymentResult.Success;
                        Logger.Info($"[PAYCTRL] PayWithResult() 결제 완료 - 결과: {isPaid}, 오류: {errorDetail}");

                        // Form이 닫혔는지 확인
                        if (this.IsDisposed || !this.IsHandleCreated)
                        {
                            Logger.Warning("[TASK] Form이 이미 닫힘 - UI 업데이트 생략");
                            return;
                        }

                        // 결과를 UI 스레드에서 처리
                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                if (isPaid)
                                {
                                    Logger.Info("[PAYMENT] 결제 성공!");
                                    
                                    // ??결제 정보 저장 (다음 단계에서 DB/영수증 처리에 사용)
                                    _lastPaymentResult = paymentResult;
                                    
                                    //  결제 성공 상세 정보 로그
                                    Logger.Info($"[PAYMENT-SUCCESS] ========== 결제 성공 상세 정보 ==========");
                                    Logger.Info($"[PAYMENT-SUCCESS] 카드번호: {paymentResult.CardNumber}");
                                    Logger.Info($"[PAYMENT-SUCCESS] 승인번호: {paymentResult.ApprovalNumber}");
                                    Logger.Info($"[PAYMENT-SUCCESS] 승인일시: {paymentResult.ApprovalDate}");
                                    Logger.Info($"[PAYMENT-SUCCESS] 결제메시지: {paymentResult.Message}");
                                    Logger.Info($"[PAYMENT-SUCCESS] 결제금액: {_paymentAmount:N0}원");
                                    Logger.Info($"[PAYMENT-SUCCESS] ==========================================");
                                    
                                    //  음성으로 결제 정보 안내 (MessageBox 대체)
                                    try
                                    {
                                        // 승인번호를 음성으로 안내
                                        if (!string.IsNullOrEmpty(paymentResult.ApprovalNumber))
                                        {
                                            SoundManager.Instance.Speak($"결제가 승인되었습니다.");
                                      
                                        }
                                    }
                                    catch (Exception voiceEx)
                                    {
                                        Logger.Warning($"[PAYMENT-VOICE] 승인번호 음성 안내 실패: {voiceEx.Message}");
                                    }
                                    
                                    OnPaymentSuccessWithNotice();
                                }
                                else
                                {
                                    Logger.Warning($"[PAYMENT] 결제 실패 - 오류: {errorDetail}");
                                    
                                    //  결제 실패 상세 정보 로그
                                    Logger.Warning($"[PAYMENT-FAIL] ========== 결제 실패 상세 정보 ==========");
                                    Logger.Warning($"[PAYMENT-FAIL] 실패 메시지: {paymentResult?.Message ?? "알 수 없음"}");
                                    Logger.Warning($"[PAYMENT-FAIL] 오류 상세: {errorDetail}");
                                    Logger.Warning($"[PAYMENT-FAIL] 결제금액: {_paymentAmount:N0}원");
                                    Logger.Warning($"[PAYMENT-FAIL] ==========================================");
                                    
                                    // 실패 원인 분석
                                    string failReason = paymentResult?.Message ?? "결제가 거부되었습니다.";
                                    if (!string.IsNullOrEmpty(errorDetail))
                                    {
                                        failReason = errorDetail;
                                    }
                                    
                                    OnPaymentFailedWithNotice(failReason);
                                }
                            }));
                        }
                        catch (InvalidOperationException invokeEx)
                        {
                            Logger.Error($"[INVOKE-ERROR] Form Handle 오류 (Form 닫힘?): {invokeEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[TASK-ERROR] 결제 Task 실행 중 오류: {ex.Message}", ex);
                        Logger.Error($"[TASK-ERROR] Exception Type: {ex.GetType().FullName}");
                        Logger.Error($"[TASK-ERROR] StackTrace: {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            Logger.Error($"[TASK-ERROR] InnerException: {ex.InnerException.Message}", ex.InnerException);
                        }
                        
                        payStarted = true;
                        isPaid = false;

                        if (this.IsDisposed || !this.IsHandleCreated)
                        {
                            Logger.Warning("[TASK-ERROR] Form이 이미 닫힘 - UI 업데이트 생략");
                            return;
                        }

                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                OnPaymentFailedWithNotice($"결제 처리 중 오류: {ex.Message}");
                            }));
                        }
                        catch (Exception invokeEx)
                        {
                            Logger.Error($"[INVOKE-ERROR] UI 스레드 Invoke 오류: {invokeEx.Message}", invokeEx);
                        }
                    }
                });

                // Task 생성 완료 로그
                Logger.Info($"[TASK] Task.Run() 생성 완료 (Task ID: {payTask.Id}, Status: {payTask.Status})");
                
                // Task 상태 모니터링
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(100);
                        Logger.Info($"[MONITOR] Task 상태 (100ms): Status={payTask.Status}, Completed={payTask.IsCompleted}, Faulted={payTask.IsFaulted}");
                        
                        if (payTask.IsFaulted && payTask.Exception != null)
                        {
                            var baseEx = payTask.Exception.GetBaseException();
                            Logger.Error($"[MONITOR-FAULT] Task Faulted (100ms): {baseEx.Message}", baseEx);
                            Logger.Error($"[MONITOR-FAULT] Exception Type: {baseEx.GetType().FullName}");
                            Logger.Error($"[MONITOR-FAULT] All Exceptions: {string.Join(", ", payTask.Exception.InnerExceptions.Select(e => e.Message))}");
                        }
                        
                        await Task.Delay(900);
                        Logger.Info($"[MONITOR] Task 상태 (1초): Status={payTask.Status}, Completed={payTask.IsCompleted}, Faulted={payTask.IsFaulted}");
                        
                        if (payTask.IsFaulted && payTask.Exception != null)
                        {
                            var baseEx = payTask.Exception.GetBaseException();
                            Logger.Error($"[MONITOR-FAULT] Task Faulted (1초): {baseEx.Message}", baseEx);
                            Logger.Error($"[MONITOR-FAULT] Exception Type: {baseEx.GetType().FullName}");
                            Logger.Error($"[MONITOR-FAULT] StackTrace: {baseEx.StackTrace}");
                        }
                    }
                    catch (Exception monitorEx)
                    {
                        Logger.Error($"[MONITOR-ERROR] 모니터링 중 오류: {monitorEx.Message}", monitorEx);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"[PAYMENT-ERROR] 결제 시작 중 오류: {ex.Message}", ex);
                Logger.Error($"[PAYMENT-ERROR] StackTrace: {ex.StackTrace}");
                _isProcessingPayment = false;
            }
        }

        /// <summary>
        /// 결제 성공 처리 (Notice 포함)
        /// </summary>
        private void OnPaymentSuccessWithNotice()
        {
            try
            {
                // 결제 완료 음성 안내
                try
                {
                    SoundManager.Instance.SpeakPaymentComplete();
                    Logger.Info("결제 완료 음성 안내 재생");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"결제 완료 음성 안내 실패: {ex.Message}");
                }
                
                // PayForm 정책 - DB에 결제 정보 저장
                bool saveResult = SavePaymentToDatabase();

                if (saveResult)
                {

                    // 영수증 출력 시도
                    Task.Run(async () =>
                    {
                        try
                        {
                            await PrintReceiptForPayment();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"영수증 출력 중 오류: {ex.Message}", ex);
                            // 영수증 출력 실패해도 사물함은 계속해서 처리
                        }
                    });

                    // 구매한 사물함 정보 수집
                    List<int> purchasedBoxNumbers = _selectedLockers.Select(l => l.LockerNumber).ToList();

                    if (purchasedBoxNumbers.Count > 0)
                    {
                        Logger.Info($"결제 완료 후 {purchasedBoxNumbers.Count}개 사물함 열기 시작");

                        // Notice로 결제 완료 알림
                        _context?.Navigator?.ShowPaymentNotice(
                            "결제가 성공적으로 완료되었습니다.",
                            true,
                            3,
                            () => 
                            {
                                // Notice 닫힌 후 OpenBoxPage로 이동
                                Logger.Info("결제 완료 알림 후 OpenBoxPage로 이동");
                                

                                // 사물함 오픈 음성 안내
                                try
                                {
                                    SoundManager.Instance.SpeakBoxOpened(purchasedBoxNumbers);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Warning($"사물함 오픈 음성 안내 실패: {ex.Message}");
                                }
                                
                                _context?.Navigator?.ShowOpenBoxPage(purchasedBoxNumbers);
                            }
                        );
                    }
                    else
                    {
                        Logger.Info("선택된 사물함이 없어 사물함 열기를 건너뜁니다.");

                        // 테스트 결제 완료 알림
                        _context?.Navigator?.ShowPaymentNotice(
                            "테스트 결제가 완료되었습니다!",
                            true,
                            3,
                            () => OnNextBtn()
                        );
                    }
                }
                else
                {
                    Logger.Error("사물함 정보 저장에서 DB 저장 실패");
                    
                    // ? InitializePayCtrl() 제거 - PayCtrl은 상태를 유지해야 함
                    // Notice로 결제 DB 저장 오류 알림
                    _context?.Navigator?.ShowPaymentNotice(
                        "결제 중 오류가 발생하였습니다\n관리자에게 문의해주세요\n",
                        false,
                        5,
                        () => OnHomeBtn()
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 성공 처리 중 오류: {ex.Message}", ex);
                
                // ? InitializePayCtrl() 제거 - PayCtrl은 상태를 유지해야 함
                // Notice로 결제 처리 오류 알림
                _context?.Navigator?.ShowPaymentNotice(
                    "결제 완료 처리 중 오류가 발생했습니다",
                    false,
                    3,
                    () => OnHomeBtn()
                );
            }

        }

        /// <summary>
        /// 결제 정보를 바탕으로 영수증을 출력합니다
        /// </summary>
        private async Task PrintReceiptForPayment()
        {
            try
            {
                Logger.Info("영수증 출력 준비 중...");

                // 프린터 연결 확인
                if (_context?.PrinterDevice == null)
                {
                    Logger.Warning("프린터 디바이스가 초기화되지 않았습니다.");
                    return;
                }

                // 영수증 데이터 생성
                var receiptData = CreateReceiptData();

                // 영수증 출력
                bool printResult = await _context.PrinterDevice.PrintReceiptAsync(receiptData);

                if (printResult)
                {
                    Logger.Info("영수증 출력 성공");

                }
                else
                {
                    Logger.Warning("영수증 출력 실패");

                }
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 출력 중 오류: {ex.Message}", ex);

            }
        }

        /// <summary>
        /// 결제 실패 처리 (Notice 포함)
        /// </summary>
        private void OnPaymentFailedWithNotice(string errorMessage)
        {
            try
            {
                //  결제 실패 음성 안내
                try
                {
                    SoundManager.Instance.SpeakPaymentFailed();
                    Logger.Info("결제 실패 음성 안내 재생");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"결제 실패 음성 안내 실패: {ex.Message}");
                }

                // Notice를 통한 결제 실패 알림 (PayForm 스타일)
                string displayMessage = GetUserFriendlyErrorMessage(errorMessage);
                
                // ? InitializePayCtrl() 제거 - PayCtrl은 상태를 유지해야 함
                _context?.Navigator?.ShowPaymentNotice(
                    $"결제에 실패하여 초기 화면으로 돌아갑니다\n\n{displayMessage}",
                    false,
                    5,
                    () => OnHomeBtn()
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"결제 실패 처리 중 오류: {ex.Message}", ex);
            }
            finally
            {
                _isProcessingPayment = false;
            }
        }

        /// <summary>
        /// 사용자 친화적 오류 메시지 생성
        /// </summary>
        /// <param name="errorMessage">원본 오류 메시지</param>
        /// <returns>사용자 친화적 메시지</returns>
        private string GetUserFriendlyErrorMessage(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
                return "알 수 없는 오류가 발생했습니다.";

            return "결제 중 문제가 발생했습니다.";
        }

        /// <summary>
        /// 결제 취소 처리 (PayCtrl 방식) - 개선된 버전
        /// </summary>
        private void CancelPayment()
        {
            try
            {
                Logger.Info("[CANCEL] 결제 취소 시작");
                
                // 1. 먼저 Task가 실행 중이면 대기
                if (payTask != null && !payTask.IsCompleted)
                {
                    try
                    {
                        Logger.Info("[CANCEL] 결제 Task 대기 중...");
                        // Task가 완료될 때까지 최대 3초 대기 (2초 → 3초로 증가)
                        if (!payTask.Wait(TimeSpan.FromSeconds(3)))
                        {
                            Logger.Warning("[CANCEL] 결제 Task가 3초 내에 완료되지 않음");
                        }
                        else
                        {
                            Logger.Info("[CANCEL] 결제 Task 완료 확인");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"[CANCEL] 결제 Task 대기 중 오류: {ex.Message}", ex);
                    }
                }
                
                // 2. PayCtrl.Cancel() 호출
                try
                {
                    PayCtrl.Cancel();
                    Logger.Info("[CANCEL] PayCtrl.Cancel() 호출 완료");
                    
                    // PayCtrl.Cancel() 후 충분한 대기 시간 제공 (VCat.exe 종료 대기)
                    System.Threading.Thread.Sleep(500);
                    Logger.Info("[CANCEL] PayCtrl 정리 대기 완료");
                }
                catch (Exception ex)
                {
                    Logger.Error($"[CANCEL] PayCtrl.Cancel() 호출 중 오류: {ex.Message}", ex);
                }
                
                // 3. 결제 관련 플래그 초기화
                isCanceled = true;
                isPaid = false;
                payStarted = false;
                _isProcessingPayment = false;

                // 4. Task 정리
                payTask = null;

                Logger.Info("[CANCEL] PayCtrl 결제 취소 처리 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"[CANCEL] 결제 취소 처리 중 오류: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// 취소 버튼 클릭
        /// /// </summary>
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isProcessingPayment)
                {
                    var result = MessageBox.Show("결제가 진행 중입니다. 정말 취소하시겠습니까?",
                        "확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                        return;
                    
                    //  결제 취소 음성 안내
                    try
                    {
                        SoundManager.Instance.Speak("결제를 취소합니다.");
                        Logger.Info("결제 취소 음성 안내 재생");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"결제 취소 음성 안내 실패: {ex.Message}");
                    }
                    
                    // 결제 취소
                    CancelPayment();
                }
                else
                {
                    //  홈으로 이동 음성 안내
                    try
                    {
                        SoundManager.Instance.Speak("처음 화면으로 돌아갑니다.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"홈 이동 음성 안내 실패: {ex.Message}");
                    }
                }
                
                OnHomeBtn();
            }
            catch (Exception ex)
            {
                Logger.Error($"취소 버튼 클릭 처리 중 오류: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 테스트 결제 버튼 클릭 (실제 결제 흐름과 동일하게 개선)
        /// </summary>
        private void BtnTestPay_Click(object sender, EventArgs e)
        {
            try
            {
                Logger.Info("테스트 결제 버튼 클릭");

                // 실제 선택된 사물함이 있으면 그대로 사용
                if (_selectedLockers.Count > 0 && _paymentAmount > 0)
                {
                    Logger.Info($"테스트 결제 - 선택된 사물함: {_selectedLockers.Count}개, 금액: {_paymentAmount:N0}원");
                    
                    // 실제 결제와 동일한 성공 처리
                    SimulateTestPaymentSuccess();
                }
                else
                {
                
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            OnBackBtn();
                        }));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"테스트 결제 중 오류: {ex.Message}", ex);
        
            }
        }

        /// <summary>
        ///  테스트 결제 성공 시뮬레이션 (실제 결제와 동일한 흐름)
        /// </summary>
        private void SimulateTestPaymentSuccess()
        {
            try
            {
                Logger.Info("테스트 결제 성공 시뮬레이션 시작");
                
                _isProcessingPayment = true;

          

                //  1초 후 결제 성공 처리 (실제 결제와 동일)
                Task.Delay(1000).ContinueWith(_ =>
                {
                    this.Invoke(new Action(() =>
                    {
                        try
                        {
                         

                            //  DB에 결제 정보 저장 (실제와 동일)
                            bool saveResult = SavePaymentToDatabase();

                            if (saveResult)
                            {


                                // 영수증 출력 시도 (실제와 동일)
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        await PrintReceiptForPayment();
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"영수증 출력 중 오류: {ex.Message}", ex);
                                    }
                                });

                                // 구매한 사물함 번호 수집
                                List<int> purchasedBoxNumbers = _selectedLockers.Select(l => l.LockerNumber).ToList();

                                Logger.Info($"? 테스트 결제 완료 - {purchasedBoxNumbers.Count}개 사물함: {string.Join(", ", purchasedBoxNumbers)}");

                                // Notice로 테스트 결제 완료 알림 (실제와 동일)
                                _context?.Navigator?.ShowPaymentNotice(
                                    $" 테스트 결제 완료!\n{_selectedLockers.Count}개 사물함 ({_paymentAmount:N0}원)",
                                    true,
                                    3,
                                    () => 
                                    {
                                        //  Notice 닫힌 후 OpenBoxPage로 이동 (실제와 동일)
                                        Logger.Info("테스트 결제 완료 알림 후 OpenBoxPage로 이동");                                                                        
                                        _context?.Navigator?.ShowOpenBoxPage(purchasedBoxNumbers);
                                    }
                                );
                            }
                            else
                            {
                                Logger.Error("테스트 결제 - DB 저장 실패");
                       
                                _context?.Navigator?.ShowPaymentNotice(
                                    "테스트 결제 중 DB 저장 오류",
                                    false,
                                    3,
                                    () => OnHomeBtn()
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"테스트 결제 성공 처리 중 오류: {ex.Message}", ex);
                                              
                            _context?.Navigator?.ShowPaymentNotice(
                                "테스트 결제 완료 처리 중 오류",
                                false,
                                3,
                                () => OnHomeBtn()
                            );
                        }
                        finally
                        {
                            _isProcessingPayment = false;
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"테스트 결제 시뮬레이션 중 오류: {ex.Message}", ex);
                _isProcessingPayment = false;

            }
        }

        private void FormProducts_Shown(object sender, EventArgs e)
        {
            Logger.Info("[SHOWN] FormProducts_Shown 이벤트 시작"); 
            
            this.ActiveControl = null;

            // 상태 초기화 (재진입 시 안전성 확보)
            try
            {
                Logger.Info("[SHOWN] 상태 초기화 시작");
                _isProcessingPayment = false;
                payStarted = false;
                isPaid = false;
                isCanceled = false;
                payTask = null;
                


                Logger.Info("[SHOWN] 상태 초기화 완료");
            }
            catch (Exception ex)
            {
                Logger.Error($"[SHOWN] 상태 초기화 중 오류: {ex.Message}", ex);
            }

            // 테스트 모드 여부 확인 및 UI 업데이트
            try
            {
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string testMode = iniParser.GetSetting("Is_Test", "Test", "false");

                Logger.Info($"[SHOWN] 테스트 모드 설정: {testMode}"); 

                if (testMode.ToLower() == "true")
                {
                


                    if (_paymentAmount > 0 && _selectedLockers.Count > 0)
                    {
                        //UpdateInstruction($"테스트 모드 - {_selectedLockers.Count}개 상품 선택됨\n'테스트 결제' 버튼을 클릭하세요");
                    }
                    else if (_paymentAmount > 0)
                    {
                        //UpdateInstruction("테스트 모드 - 카드를 삽입하거나 접촉시켜 주세요");
                    }
                    else
                    {
                        //UpdateInstruction("선택된 상품이 없습니다\n상품 선택 페이지로 돌아갑니다");
                        
                        Task.Delay(2000).ContinueWith(_ =>
                        {
                            this.Invoke(new Action(() => OnBackBtn()));
                        });
                        return;
                    }

               
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[SHOWN] 테스트 모드 확인 중 오류: {ex.Message}", ex);
            }

            // 페이지가 표시될 때 자동으로 결제 시작 (금액이 설정되어 있고 테스트 모드가 아닌 경우)
            TryStartAutoPayment();
        }

        /// <summary>
        /// VisibleChanged 이벤트 핸들러 - 2번째 이후 진입 시에도 결제 시작
        /// </summary>
        private void CardPayPage_VisibleChanged(object sender, EventArgs e)
        {
            // Form이 보이는 상태가 되었을 때만 처리
            if (this.Visible)
            {
                Logger.Info("[VISIBLE] CardPayPage 표시됨");
                
                // Shown 이벤트가 이미 발생했다면 여기서 결제 시작
                if (this.IsHandleCreated && !_isProcessingPayment)
                {
                    Logger.Info("[VISIBLE] 2번째 이후 진입 감지 - 결제 시작 시도");
                    TryStartAutoPayment();
                }
            }
        }

        /// <summary>
        /// 자동 결제 시작 시도 (Shown 및 VisibleChanged에서 공통 사용)
        /// </summary>
        private void TryStartAutoPayment()
        {
            if (_paymentAmount > 0)
            {
                Logger.Info($"[AUTO-PAY] 결제 금액 확인: {_paymentAmount:N0}원 - 자동 결제 시작 가능");
                
                var iniPath = Path.Combine(Application.StartupPath, "SMT_Kiosk.ini");
                var iniParser = new IniParser(iniPath);
                string testMode = iniParser.GetSetting("Is_Test", "Test", "false");

                // 실제 결제 모드에서만 자동 결제 시작
                if (testMode.ToLower() != "true")
                {
                    Logger.Info("[AUTO-PAY] 실제 결제 모드 - 1초 후 OnPay() 호출 예약");
                    
                    // ?? 결제 시작 음성 안내
                    try
                    {
                        SoundManager.Instance.SpeakPaymentStart(_paymentAmount);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"결제 시작 음성 안내 실패: {ex.Message}");
                    }
                    
                    Task.Delay(1000).ContinueWith(_ =>
                    {
                        if (this.IsDisposed || !this.IsHandleCreated)
                        {
                            Logger.Warning("[AUTO-PAY] Form이 이미 닫혔거나 Handle이 생성되지 않음 - 결제 시작 취소");
                            return;
                        }

                        try
                        {
                            this.Invoke(new Action(() =>
                            {
                                // 재확인: 이미 결제 중이면 시작하지 않음
                                if (_isProcessingPayment)
                                {
                                    Logger.Warning("[AUTO-PAY] 이미 결제가 진행 중 - OnPay() 호출 생략");
                                    return;
                                }
                                
                                Logger.Info("[AUTO-PAY] OnPay() 호출 직전"); 
                                OnPay(); // 자동 결제 시작
                            }));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"[AUTO-PAY] OnPay() 호출 중 오류: {ex.Message}", ex);
                        }
                    });
                }
                else
                {
                    Logger.Info("[AUTO-PAY] 테스트 모드 활성화 - 자동 결제 시작하지 않음");
                }
            }
            else
            {
                Logger.Warning($"[AUTO-PAY] 결제 금액이 0 이하: {_paymentAmount}원 - 자동 결제 시작 안 함");
                
                //  금액 오류 안내
                try
                {
                    SoundManager.Instance.SpeakError("결제 금액이 설정되지 않았습니다.");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"금액 오류 음성 안내 실패: {ex.Message}");
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                Logger.Info("[CLOSE] Form 닫기 이벤트 시작");
                
                // VisibleChanged 이벤트 핸들러 제거
                this.VisibleChanged -= CardPayPage_VisibleChanged;
                
                CancelPayment();
                
                
                Logger.Info("[CLOSE] Form 닫기 처리 완료");
                base.OnFormClosed(e);
            }
            catch (Exception ex)
            {
                Logger.Error($"[CLOSE] CardPayPage 종료 중 오류: {ex.Message}", ex);
            }
        }

        public void SetContext(s_oil.Services.ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 홈 버튼 (OnHomeBtn)
        /// </summary>
        public void OnHomeBtn()
        {
            try
            {
                // 결제 중이면 먼저 취소
                if (_isProcessingPayment)
                {
                    CancelPayment();
                    System.Threading.Thread.Sleep(500); // 취소 후 대기
                }
                
                _context?.Navigator?.ShowHomePage();
                Logger.Info("홈 화면으로 이동");
            }
            catch (Exception ex)
            {
                Logger.Error($"홈 버튼 처리 중 오류: {ex.Message}", ex);
                this.Close();
            }
        }

        /// <summary>
        /// 뒤로 버튼 (OnBackBtn)
        /// </summary>
        public void OnBackBtn()
        {
            try
            {
                // 결제 중이면 먼저 취소
                if (_isProcessingPayment)
                {
                    CancelPayment();
                    System.Threading.Thread.Sleep(500); // 취소 후 대기
                }
                
                _context?.Navigator?.ShowCustomerProductBuyPage();
                Logger.Info("이전 화면으로 이동");
            }
            catch (Exception ex)
            {
                Logger.Error($"뒤로 버튼 처리 중 오류: {ex.Message}", ex);
                OnHomeBtn();
            }
        }

        /// <summary>
        /// 다음 버튼 (OnNextBtn)
        /// </summary>
        public void OnNextBtn()
        {
            try
            {
                OnHomeBtn();
            }
            catch (Exception ex)
            {
                Logger.Error($"다음 버튼 처리 중 오류: {ex.Message}", ex);
                this.Close();
            }
        }

        /// <summary>
        /// DB에 결제 정보 저장
        /// </summary>
        private bool SavePaymentToDatabase()
        {
            try
            {
                bool allSuccess = true;
                string userCode = $"USER_{DateTime.Now:yyyyMMddHHmmss}";
                
                // ?? 실제 결제 정보 사용
                string approvalNumber = "UNKNOWN"; // 기본값
                string cardNumber = "****-0000"; // 기본값
                
                if (_lastPaymentResult != null)
                {
                    // 승인번호 추출 (confirmKey에 저장)
                    if (!string.IsNullOrEmpty(_lastPaymentResult.ApprovalNumber))
                    {
                        approvalNumber = _lastPaymentResult.ApprovalNumber;
                        Logger.Info($"[DB-SAVE] 승인번호 추출 성공: {approvalNumber}");
                    }
                    else
                    {
                        Logger.Warning("[DB-SAVE] 승인번호가 비어있습니다. 기본값 사용");
                    }
                    
                    // 카드번호 추출
                    if (!string.IsNullOrEmpty(_lastPaymentResult.CardNumber))
                    {
                        cardNumber = _lastPaymentResult.CardNumber;
                        Logger.Info($"[DB-SAVE] 카드번호 추출 성공: {cardNumber}");
                    }
                    else
                    {
                        Logger.Warning("[DB-SAVE] 카드번호가 비어있습니다. 기본값 사용");
                    }
                    
                    Logger.Info($"[DB-SAVE] 승인일시: {_lastPaymentResult.ApprovalDate}");
                    Logger.Info($"[DB-SAVE] 결제메시지: {_lastPaymentResult.Message}");
                }
                else
                {
                    Logger.Warning("[DB-SAVE] _lastPaymentResult가 null입니다. 기본값 사용");
                }

                foreach (var locker in _selectedLockers)
                {
                    var payment = new Payment
                    {
                        areaCode = _areaCode ?? "",
                        boxNo = locker.LockerNumber,
                        userCode = userCode,
                        payType = 1, // 카드결제
                        payAmount = locker.Price,
                        payPhone = "",
                        confirmKey = approvalNumber, //  승인번호를 confirmKey에 저장
                        cardNumber = cardNumber, //  실제 카드번호 저장
                        payTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), // 현재 시간 저장 (DB에서 getDate() 사용)
                        approvalNumber = approvalNumber, // 추가 필드 (있는 경우)
                        payStatus = 1, // 성공
                        errorMessage = ""
                    };

                    Logger.Info($"[DB-SAVE] 사물함 {locker.LockerNumber} 저장 시도:");
                    Logger.Info($"  - 승인번호(confirmKey): {payment.confirmKey}");
                    Logger.Info($"  - 카드번호: {payment.cardNumber}");
                    Logger.Info($"  - 결제시간: {payment.payTime}");
                    Logger.Info($"  - 금액: {payment.payAmount}원");

                    bool paymentSaved = DBConnector.Instance.SavePayment(payment);
                    bool boxUpdated = DBConnector.Instance.UpdateBoxAfterPayment(
                        _areaCode, locker.LockerNumber, userCode, "", approvalNumber, locker.Price);

                    if (!paymentSaved || !boxUpdated)
                    {
                        Logger.Error($"[DB-SAVE] 사물함 {locker.LockerNumber} 결제 정보 저장 실패");
                        allSuccess = false;
                    }
                    else
                    {
                        Logger.Info($"[DB-SAVE] 사물함 {locker.LockerNumber} 결제 정보 저장 성공");
                        Logger.Info($"   승인번호: {approvalNumber}");
                        Logger.Info($"   카드번호: {cardNumber}");
                    }
                }

                if (allSuccess)
                {
                    Logger.Info($"[DB-SAVE] 전체 결제 정보 저장 성공 - {_selectedLockers.Count}개 사물함");
                }
                else
                {
                    Logger.Error($"[DB-SAVE] 일부 결제 정보 저장 실패");
                }

                return allSuccess;
            }
            catch (Exception ex)
            {
                Logger.Error($"[DB-SAVE] 결제 정보 DB 저장 중 오류: {ex.Message}", ex);
                Logger.Error($"[DB-SAVE] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// 현재 결제 정보를 바탕으로 영수증 데이터를 생성합니다
        /// </summary>
        private Services.ReceiptData CreateReceiptData()
        {
            try
            {
                // ?? 실제 결제 정보 추출
                string cardNumber = "****-****-****-0000"; // 기본값
                string approvalNumber = "승인번호없음"; // 기본값
                string approvalDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 기본값
                
                if (_lastPaymentResult != null)
                {
                    if (!string.IsNullOrEmpty(_lastPaymentResult.CardNumber))
                    {
                        cardNumber = _lastPaymentResult.CardNumber;
                        Logger.Info($"[RECEIPT] 영수증 카드번호: {cardNumber}");
                    }
                    
                    if (!string.IsNullOrEmpty(_lastPaymentResult.ApprovalNumber))
                    {
                        approvalNumber = _lastPaymentResult.ApprovalNumber;
                        Logger.Info($"[RECEIPT] 영수증 승인번호: {approvalNumber}");
                    }
                    
                    if (!string.IsNullOrEmpty(_lastPaymentResult.ApprovalDate))
                    {
                        approvalDate = _lastPaymentResult.ApprovalDate;
                        Logger.Info($"[RECEIPT] 영수증 승인일시: {approvalDate}");
                    }
                }
                else
                {
                    Logger.Warning("[RECEIPT] _lastPaymentResult가 null입니다. 기본값 사용");
                }

                var receiptData = new Services.ReceiptData
                {
                    // 회사 정보
                    CompanyName = "일죽주유소",
                    CompanyAddress = "경기도 안성시 일죽면 서동대로 7381",

                    // 거래 정보
                    TransactionDate = DateTime.Now,

                    // 상품 정보 - 여러 상품 지원
                    FuelType = GetSelectedProductsDescription(),
                    UnitPrice = _selectedLockers.Count > 0 ? (decimal)_selectedLockers.Average(l => l.Price) : _paymentAmount,
                    Quantity = _selectedLockers.Count,
                    TotalAmount = _paymentAmount,

                    // 결제 정보
                    InputAmount = _paymentAmount,
                    ChangeAmount = 0,
                    PaymentMethod = "신용카드",
                    CardNumber = cardNumber, // ?? 실제 카드번호
                    ApprovalNumber = approvalNumber, // ?? 실제 승인번호
                    ApprovalDate = approvalDate // ?? 실제 승인일시
                };

                // 여러 상품 상세 정보 추가
                if (_selectedLockers.Count > 0)
                {
                    foreach (var locker in _selectedLockers)
                    {
                        receiptData.ProductItems.Add(new Services.ReceiptProductItem
                        {
                            ProductName = string.IsNullOrEmpty(locker.ProductName) 
                                ? $"사물함 #{locker.LockerNumber}" 
                                : locker.ProductName,
                            Price = locker.Price,
                            Quantity = 1
                        });
                    }
                    
                    Logger.Info($"[RECEIPT] 영수증 데이터 생성 완료 - {receiptData.ProductItems.Count}개 상품");
                    Logger.Info($"[RECEIPT] 카드번호: {cardNumber}, 승인번호: {approvalNumber}, 승인일시: {approvalDate}");
                }

                return receiptData;
            }
            catch (Exception ex)
            {
                Logger.Error($"영수증 데이터 생성 중 오류: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 선택된 상품들의 설명을 생성합니다
        /// </summary>
        private string GetSelectedProductsDescription()
        {
            try
            {
                if (_selectedLockers.Count == 0)
                {
                    return "키오스크 결제";
                }

                if (_selectedLockers.Count == 1)
                {
                    var locker = _selectedLockers[0];
                    return string.IsNullOrEmpty(locker.ProductName) 
                        ? $"사물함 #{locker.LockerNumber}" 
                        : locker.ProductName;
                }

                // 여러 개인 경우
                return $"사물함 {_selectedLockers.Count}개";
            }
            catch (Exception ex)
            {
                Logger.Error($"상품 설명 생성 중 오류: {ex.Message}", ex);
                return "키오스크 결제";
            }
        }
    }
}