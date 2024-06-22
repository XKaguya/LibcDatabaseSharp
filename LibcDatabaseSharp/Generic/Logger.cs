using System.Text;
using LibcDatabaseSharp.Enum;

namespace LibcDatabaseSharp.Generic
{
    public class Logger
    {
        private static StringBuilder logBuilder = new StringBuilder();
        private static object _lockObj = new object();
        public static string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.log");
        private static LogLevel logLevel = LogLevel.Info;

        public static void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        public static void LogFatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        private static void Log(LogLevel level, string message)
        {
            lock (_lockObj)
            {
                if (logLevel >= level)
                {
                    string logEntry = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [{level}] {message}";
                    logBuilder.AppendLine(logEntry);
                    Console.WriteLine(logEntry);
                
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
            }
        }
        
        public static string GetLog()
        {
            lock (_lockObj)
            {
                return logBuilder.ToString();
            }
        }

        public static LogLevel SetLogLevel(LogLevel logLeveltoSet)
        {
            logLevel = logLeveltoSet;

            return logLevel;
        }
        
        public static void ClearLog()
        {
            lock (_lockObj)
            {
                logBuilder.Clear();
            }
        }
    }
}