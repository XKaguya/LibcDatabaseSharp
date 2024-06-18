using System.Text;

namespace LibcDatabaseSharp.Generic
{
    public class Logger
    {
        private static StringBuilder logBuilder = new StringBuilder();
        private static object _lockObj = new object();
        public static string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.log");

        public static void LogInfo(string message)
        {
            Log("INFO", message);
        }

        public static void LogDebug(string message)
        {
            Log("DEBUG", message);
        }

        public static void LogFatal(string message)
        {
            Log("FATAL", message);
        }

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        private static void Log(string level, string message)
        {
            lock (_lockObj)
            {
                string logEntry = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [{level}] {message}";
                logBuilder.AppendLine(logEntry);
                Console.WriteLine(logEntry);
                
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
        }
        
        public static string GetLog()
        {
            lock (_lockObj)
            {
                return logBuilder.ToString();
            }
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