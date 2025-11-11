using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace s_oil.Utils
{
    /// <summary>
    /// 음성 안내(TTS) 및 효과음 재생을 관리하는 클래스
    /// </summary>
    public class SoundManager : IDisposable
    {
        private static readonly Lazy<SoundManager> _instance = new Lazy<SoundManager>(() => new SoundManager());
        private readonly SpeechSynthesizer _synthesizer;
        private readonly SoundPlayer _soundPlayer;
        private bool _isDisposed = false;
        private bool _isSpeaking = false;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static SoundManager Instance => _instance.Value;

        private SoundManager()
        {
            _synthesizer = new SpeechSynthesizer();
            _soundPlayer = new SoundPlayer();

            // 기본 설정
            _synthesizer.Volume = 100; // 0-100
            _synthesizer.Rate = 0;     // -10 ~ 10 (느림 ~ 빠름)

            // 한국어 음성 선택 (설치된 경우)
            SelectKoreanVoice();

            // 이벤트 핸들러 등록
            _synthesizer.SpeakCompleted += OnSpeakCompleted;

            Logger.Info("SoundManager 초기화 완료");
        }

        /// <summary>
        /// 한국어 음성 선택 (없으면 기본 음성 사용)
        /// </summary>
        private void SelectKoreanVoice()
        {
            try
            {
                foreach (var voice in _synthesizer.GetInstalledVoices())
                {
                    var info = voice.VoiceInfo;
                    // 한국어 음성 찾기
                    if (info.Culture.Name.StartsWith("ko", StringComparison.OrdinalIgnoreCase))
                    {
                        _synthesizer.SelectVoice(info.Name);
                        Logger.Info($"한국어 음성 선택: {info.Name}");
                        return;
                    }
                }
                Logger.Warning("한국어 음성을 찾을 수 없어 기본 음성을 사용합니다.");
            }
            catch (Exception ex)
            {
                Logger.Error($"음성 선택 중 오류: {ex.Message}", ex);
            }
        }

        #region TTS (음성 안내)

        /// <summary>
        /// 텍스트를 음성으로 읽어줍니다 (동기)
        /// </summary>
        /// <param name="text">읽을 텍스트</param>
        public void Speak(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return;

                if (_isSpeaking)
                {
                    _synthesizer.SpeakAsyncCancelAll();
                }

                _isSpeaking = true;
                _synthesizer.SpeakAsync(text);
                Logger.Info($"TTS 재생: {text}");
            }
            catch (Exception ex)
            {
                Logger.Error($"TTS 재생 중 오류: {ex.Message}", ex);
                _isSpeaking = false;
            }
        }

        /// <summary>
        /// 텍스트를 음성으로 읽어줍니다 (비동기)
        /// </summary>
        /// <param name="text">읽을 텍스트</param>
        public async Task SpeakAsync(string text)
        {
            await Task.Run(() => Speak(text));
        }

        /// <summary>
        /// 음성 재생 완료 이벤트
        /// </summary>
        private void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            _isSpeaking = false;
            Logger.Info("TTS 재생 완료");
        }

        /// <summary>
        /// 현재 재생 중인 음성을 중지합니다
        /// </summary>
        public void StopSpeaking()
        {
            try
            {
                if (_isSpeaking)
                {
                    _synthesizer.SpeakAsyncCancelAll();
                    _isSpeaking = false;
                    Logger.Info("TTS 재생 중지");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"TTS 중지 중 오류: {ex.Message}", ex);
            }
        }

        #endregion

        #region 숫자 한글 변환

        /// <summary>
        /// 숫자를 한글로 변환합니다 (1~999 지원)
        /// </summary>
        /// <param name="number">변환할 숫자</param>
        /// <returns>한글 숫자 (예: 1 -> "일", 12 -> "십이", 123 -> "백이십삼")</returns>
        private string NumberToKorean(int number)
        {
            try
            {
                if (number == 0) return "영";
                if (number < 0 || number > 999) 
                    return number.ToString(); // 범위 벗어나면 숫자 그대로 반환

                string[] units = { "", "일", "이", "삼", "사", "오", "육", "칠", "팔", "구" };
                StringBuilder result = new StringBuilder();

                // 백의 자리
                int hundreds = number / 100;
                if (hundreds > 0)
                {
                    if (hundreds == 1)
                        result.Append("백");
                    else
                        result.Append(units[hundreds] + "백");
                }

                // 십의 자리
                int tens = (number % 100) / 10;
                if (tens > 0)
                {
                    if (tens == 1)
                        result.Append("십");
                    else
                        result.Append(units[tens] + "십");
                }

                // 일의 자리
                int ones = number % 10;
                if (ones > 0)
                {
                    result.Append(units[ones]);
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warning($"숫자 한글 변환 실패: {number}, {ex.Message}");
                return number.ToString();
            }
        }

        /// <summary>
        /// 사물함 번호를 한글로 변환합니다 (TTS 최적화)
        /// </summary>
        /// <param name="lockerNumber">사물함 번호</param>
        /// <returns>한글 번호 (예: "일번", "십이번", "백이십삼번")</returns>
        private string LockerNumberToKorean(int lockerNumber)
        {
            return NumberToKorean(lockerNumber) + "번";
        }

        #endregion

        #region 키오스크 전용 음성 안내

        /// <summary>
        /// 사물함 클릭 시 음성 안내
        /// </summary>
        /// <param name="lockerNumber">사물함 번호</param>
        public void SpeakLockerSelected(int lockerNumber)
        {
            // 숫자를 한글로 변환: 1 -> "일번", 12 -> "십이번"
            string koreanNumber = LockerNumberToKorean(lockerNumber);
            string message = $"{koreanNumber} 사물함이 선택되었습니다.";

            Speak(message);
            Logger.Info($"사물함 선택 음성: {message} (원본: {lockerNumber}번)");
        }

        /// <summary>
        /// 결제 시작 음성 안내
        /// </summary>
        /// <param name="amount">결제 금액</param>
        public void SpeakPaymentStart(int amount)
        {
            Speak($"결제 금액은 {amount:N0}원입니다. 카드를 투입해 주세요.");
        }

        /// <summary>
        /// 결제 완료 음성 안내
        /// </summary>
        public void SpeakPaymentComplete()
        {
            Speak("결제가 완료되었습니다. 감사합니다.");
        }

        /// <summary>
        /// 결제 실패 음성 안내
        /// </summary>
        public void SpeakPaymentFailed()
        {
            Speak("결제에 실패했습니다. 다시 시도해 주세요.");
        }

        /// <summary>
        /// 사물함 오픈 음성 안내
        /// </summary>
        /// <param name="lockerNumbers">열린 사물함 번호들</param>
        public void SpeakBoxOpened(List<int> lockerNumbers)
        {
            if (lockerNumbers == null || lockerNumbers.Count == 0)
            {
                Speak("사물함이 열렸습니다.");
                return;
            }

            // 여러 개의 사물함 번호를 한글로 변환
            string koreanNumbers = string.Join(", ", lockerNumbers.Select(n => LockerNumberToKorean(n)));
            string message = $"{koreanNumbers} 사물함이 열렸습니다.";
            
            Speak(message);
            Logger.Info($"사물함 오픈 음성: {message} (원본: {string.Join(", ", lockerNumbers)})");
        }

        /// <summary>
        /// 에러 음성 안내
        /// </summary>
        /// <param name="errorMessage">에러 메시지</param>
        public void SpeakError(string errorMessage = "오류가 발생했습니다.")
        {
            Speak(errorMessage);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    StopSpeaking();
                    _synthesizer?.Dispose();
                    _soundPlayer?.Dispose();
                }
                _isDisposed = true;
            }
        }

        ~SoundManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
