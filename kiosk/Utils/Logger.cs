using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace s_oil.Utils
{
    /// <summary>
    /// 파일에 로그를 기록하는 유틸리티 클래스입니다.
    /// </summary>
    public static class Logger
    {
        private static readonly string logDirectory;
        private static readonly object _lock = new object();

        /// <summary>
        /// 정적 생성자: 로그 디렉터리를 확인하고 없으면 생성합니다.
        /// </summary>
        static Logger()
        {
            try
            {
                logDirectory = Path.Combine(Application.StartupPath, "LOG");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
            }
            catch (Exception ex)
            {
                // 폴백: 로그 디렉터리 생성 실패 시 디버그 콘솔에 출력합니다.
                System.Diagnostics.Debug.WriteLine($"Failed to create log directory: {ex}");
            }
        }

        /// <summary>
        /// 로그 파일에 메시지를 기록합니다.
        /// </summary>
        /// <param name="level">로그 레벨 (INFO, WARN, ERROR)</param>
        /// <param name="message">기록할 메시지</param>
        private static void WriteLog(string level, string message)
        {
            if (string.IsNullOrEmpty(logDirectory)) return;

            try
            {
                // 로그 파일 경로는 "LOG/ServerLog_YYYY-MM-DD.log" 형식입니다.
                string logFilePath = Path.Combine(logDirectory, $"ServerLog_{DateTime.Now:yyyy-MM-dd}.log");
                // 로그 메시지 형식: "YYYY-MM-DD HH:mm:ss.fff [LEVEL] 메시지"
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

                // 다중 스레드 환경에서 파일 접근을 동기화합니다.
                lock (_lock)
                {
                    File.AppendAllText(logFilePath, logMessage, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // 폴백: 파일 쓰기 실패 시 디버그 콘솔에 출력합니다.
                System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex}");
            }
        }

        /// <summary>
        /// 정보 수준의 로그를 기록합니다.
        /// </summary>
        /// <param name="message">기록할 메시지</param>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 경고 수준의 로그를 기록합니다.
        /// </summary>
        /// <param name="message">기록할 메시지</param>
        public static void Warning(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// 오류 수준의 로그를 기록합니다.
        /// </summary>
        /// <param name="message">기록할 메시지</param>
        /// <param name="ex">발생한 예외 (선택 사항)</param>
        public static void Error(string message, Exception ex = null)
        {
            if (ex != null)
            {
                message += $"\nException: {ex}";
            }
            WriteLog("ERROR", message);
        }
    }
}
