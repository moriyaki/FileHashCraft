using System.Diagnostics;
using System.IO;

namespace FileHashCraft.Models
{
    /// <summary>
    /// ログレベル
    /// </summary>
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Exception,
        // 他のレベルも追加可能
    }

    public static class DebugManager
    {
        private static string PrepareDebug()
        {
            const string appName = "FileHashCraft";
            const string settingLogFile = "FileHashCraft.log";

            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            var logPath = Path.Combine(localAppDataPath, settingLogFile);

            if (!Directory.Exists(localAppDataPath))
            {
                Directory.CreateDirectory(logPath);
            }
            return logPath;
        }

        /// <summary>
        /// ログの出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        /// <param name="level">ログの重要度：標準では Info</param>
        /// <param name="isDebugOutput">Debug.WriteLine に出力するか：標準では出力しない</param>
        private static void WriteLine(string message, LogLevel level = LogLevel.Info, bool isDebugOutput = false)
        {
            string outputMessage = level switch
            {
                LogLevel.Warning => $"Warning : {message}",
                LogLevel.Error => $"Error : {message}",
                LogLevel.Exception => $"Exception : {message}",
                _ => $"Info : {message}",
            };
            var logPath = PrepareDebug();

            if (isDebugOutput)
            {
                App.Current?.Dispatcher?.Invoke(() => Debug.WriteLine(outputMessage));
            }
            else
            {
                App.Current?.Dispatcher?.Invoke(() =>
                {
                    using var writer = new StreamWriter(logPath, true);
                    writer.WriteLine(outputMessage);
                });
            }
        }

        /// <summary>
        /// 例外メッセージの出力
        /// </summary>
        /// <param name="message">メッセージ内容</param>
        /// <param name="isDebugOutput">Debug.WriteLineにも出力するか</param>
        public static void ExceptionWrite(string message, bool isDebugOutput = true)
        {
            WriteLine(message, LogLevel.Exception);
            if (isDebugOutput)
            {
                WriteLine(message, LogLevel.Exception, true);
            }
        }

        /// <summary>
        /// エラーメッセージの出力
        /// </summary>
        /// <param name="message">メッセージ内容</param>
        /// <param name="isDebugOutput">Debug.WriteLineにも出力するか</param>
        public static void ErrorWrite(string message, bool isDebugOutput = true)
        {
            WriteLine(message, LogLevel.Error);
            if (isDebugOutput)
            {
                Debug.WriteLine(message, LogLevel.Error, true);
            }
        }

        /// <summary>
        /// 警告メッセージの出力
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isDebugOutput">Debug.WriteLineにも出力するか</param>
        public static void WarningWrite(string message, bool isDebugOutput = false)
        {
            WriteLine(message, LogLevel.Warning);
            if (isDebugOutput)
            {
                Debug.WriteLine(message, LogLevel.Warning, true);
            }
        }

        /// <summary>
        /// 警告メッセージの出力
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isDebugOutput">Debug.WriteLineにも出力するか</param>
        public static void InfoWrite(string message, bool isDebugOutput = false)
        {
            WriteLine(message, LogLevel.Info);
            if (isDebugOutput)
            {
                Debug.WriteLine(message, LogLevel.Warning, true);
            }
        }
    }
}
