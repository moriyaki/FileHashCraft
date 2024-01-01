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

    public static class LogManager
    {
        /// <summary>
        /// ログの出力
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        /// <param name="level">ログの重要度：標準では Info</param>
        /// <param name="isLog">ファイルに出力するか：標準では出力する</param>
        public static void DebugLog(string message, LogLevel level = LogLevel.Info, bool isLog = true)
        {
            const string appName = "FileHashCraft";
            const string settingLogFile = "FileHashCraft.log";

            var localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            var logPath = Path.Combine(localAppDataPath, settingLogFile);
            string outputMessage = level switch
            {
                LogLevel.Warning => $"Warning : {message}",
                LogLevel.Error => $"Error : {message}",
                LogLevel.Exception => $"Exception : {message}",
                _ => $"Info : {message}",
            };
            if (!Directory.Exists(localAppDataPath))
            {
                Directory.CreateDirectory(logPath);
            }

            if (isLog)
            {
                using var writer = new StreamWriter(logPath, true);
                writer.WriteLine(outputMessage);
            }
            else
            {
                Debug.WriteLine(outputMessage);
            }
        }
    }
}
