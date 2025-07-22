using System;
using Common.Services;

namespace Common.Utils.Logging
{
    public static class Logger
    {
        private static LogLevel _currentLogLevel = LogLevel.Info;
    
        static Logger()
        {
            LogInternal($"[Logger] Initialized with log level: {_currentLogLevel}", LogLevel.Info);
        }

        public static void SetLogLevel(LogLevel newLevel)
        {
            _currentLogLevel = newLevel;
            LogInternal($"[Logger] LogLevel updated to: {_currentLogLevel}", LogLevel.Info);
        }

        [System.Diagnostics.Conditional("ENABLE_LOGGING")]
        public static void LogDevelopment(object source, string message) 
            => Log(LogLevel.Development, source, message);

        [System.Diagnostics.Conditional("ENABLE_LOGGING")]
        public static void LogInfo(object source, string message) 
            => Log(LogLevel.Info, source, message);

        [System.Diagnostics.Conditional("ENABLE_LOGGING")]
        public static void LogWarning(object source, string message) 
            => Log(LogLevel.Warning, source, message);

        [System.Diagnostics.Conditional("ENABLE_LOGGING")]
        public static void LogError(object source, string message) 
            => Log(LogLevel.Error, source, message);

        private static void Log(LogLevel level, object source, string message)
        {
            if (level < _currentLogLevel) return;
        
            // Определяем контекст для Unity
            UnityEngine.Object context = null;
            if (source is UnityEngine.Object unityObj)
            {
                context = unityObj;
            }
        
            string sourceName = GetSourceName(source);
            string formattedMessage = FormatMessage(sourceName, message, level);
            LogInternal(formattedMessage, level, context);
        }

        private static string GetSourceName(object source)
        {
            if (source is IService service) 
            {
                return $"{service.Name} v{service.Version}"; 
            } 
            
            if (source is string str) return str;
            if (source is UnityEngine.Object obj) return obj.name;
            if (source is Type type) return type.Name;
            return source?.GetType().Name ?? "Unknown";
        }

        private static string FormatMessage(string source, string message, LogLevel level)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss.fff"); 
            
            string color = GetColorForLevel(level);
            return $"<color={color}>[{timeStamp}] [{source}]: {message}</color> ";
        }

        private static string GetColorForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Development: return "#87CEEB";
                case LogLevel.Info: return "lime";
                case LogLevel.Warning: return "yellow";
                case LogLevel.Error: return "red";
                default: return "white";
            }
        }

        private static void LogInternal(string message, LogLevel level, UnityEngine.Object context = null)
        {
            switch (level)
            {
                case LogLevel.Development:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(message, context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message, context);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(message, context);
                    break;
                default:
                    UnityEngine.Debug.LogError($"[AppLogger] Unknown log level: {level}");
                    break;
            }
        }
    }
    
    public enum LogLevel
    {
        /// <summary>
        /// Сообщения, предназначенные исключительно для отладки во время разработки.
        /// Они должны быть видны только тогда, когда логгер установлен на самый подробный режим.
        /// </summary>
        Development = -1,
        /// <summary>
        /// Общие информационные сообщения о ходе выполнения программы.
        /// </summary>
        Info,
        /// <summary>
        /// Указывают на потенциальные проблемы, которые не приводят к сбою, но могут потребовать внимания.
        /// </summary>
        Warning,
        /// <summary>
        /// Сообщения о критических ошибках, которые требуют немедленного исправления и могли привести к нежелательному поведению или сбою.
        /// </summary>
        Error,
    }
}