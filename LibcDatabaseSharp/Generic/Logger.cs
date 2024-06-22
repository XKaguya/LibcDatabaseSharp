using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using LibcDatabaseSharp.Enum;

namespace LibcDatabaseSharp.Generic
{
    public class Logger
    {
        private static StringBuilder logBuilder = new StringBuilder();
        private static object _lockObj = new object();
        public static string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "log.log");
        private static LogLevel logLevel = LogLevel.Info;

        public static void LogInfo(string message, bool showSrc = true)
        {
            Log(LogLevel.Info, message, showSrc);
        }

        public static void LogDebug(string message, bool showSrc = true)
        {
            Log(LogLevel.Debug, message, showSrc);
        }

        public static void LogFatal(string message, bool showSrc = true)
        {
            Log(LogLevel.Fatal, message, showSrc);
        }

        public static void LogError(string message, bool showSrc = true)
        {
            Log(LogLevel.Error, message, showSrc);
        }
        
        private static void Log(LogLevel level, string message, bool showSrc)
        {
            lock (_lockObj)
            {
                if (logLevel >= level)
                {
                    string logEntry;
                    
                    if (showSrc)
                    {
                        logEntry = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [{GetCallerName()}] [{level}] {message}";
                    }
                    else
                    {
                        logEntry = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} [{level}] {message}";
                    }
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
            
            LogInfo($"Set Loglevel to {logLevel}");

            return logLevel;
        }
        
        public static void ClearLog()
        {
            lock (_lockObj)
            {
                logBuilder.Clear();
            }
        }
        
        private static string GetCallerName()
        {
            MethodBase currentMethod = MethodBase.GetCurrentMethod();

            int frameCount = 0;

            while (true)
            {
                frameCount++;
                
                StackFrame callerFrame = new StackFrame(frameCount);
                MethodBase callerMethod = callerFrame.GetMethod();

                if (callerMethod == null)
                {
                    return "Unknown Caller";
                }
                
                if (callerMethod.DeclaringType.Name == currentMethod.DeclaringType.Name || callerMethod.DeclaringType.Namespace != "LibcDatabaseSharp")
                {
                    continue;
                }
                else
                {
                    if (callerMethod.Name == "MoveNext")
                    {
                        string cleanMethodName = Regex.Replace(callerMethod.DeclaringType.Name, @"^<(\w+)>.*$", "$1");
                        
                        return cleanMethodName;
                    }

                    return callerMethod.Name;
                }
            }
        }
    }
}