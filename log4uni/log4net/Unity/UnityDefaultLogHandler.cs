using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public class UnityDefaultLogHandler: ILogHandler
    {
        public static ILogger DefaultUnityLogger { get; private set; }

        internal static ILogHandler unityLogHandler;
        private static ILogHandler _log4NetLogHandler;
        internal static string applicationDataPath;
        internal static Version unityVersion;
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool IsTypedLogging { get; set; }
        
        internal static void UpdateLogHandler()
        {
            if (unityLogHandler != null) return;
            unityLogHandler = Debug.unityLogger.logHandler;
            DefaultUnityLogger = new Logger(unityLogHandler);
            if (_log4NetLogHandler == null) _log4NetLogHandler = new UnityDefaultLogHandler();
            Debug.unityLogger.logHandler = _log4NetLogHandler;

            LogLog.LogReceived += (source, args) =>
            {
                var prefix = args?.LogLog.Prefix ?? "log4net: ";
                if (prefix.ToLower().Contains("warn"))
                {
                    unityLogHandler.LogFormat(LogType.Warning, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
                else if (prefix.ToLower().Contains("error"))
                {
                    unityLogHandler.LogFormat(LogType.Error, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
                else
                {
                    unityLogHandler.LogFormat(LogType.Log, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
            };
        }
        
        private static readonly ILog CommonLogger = LogManager.GetLogger("Unity");
        private static readonly Dictionary<Type, ILog> Loggers = new Dictionary<Type, ILog>();

        private static ILog GetLogger(Object context)
        {
            if (!context || !IsTypedLogging) return CommonLogger;
            var type = context.GetType();
            ILog typedLog;
            if (Loggers.TryGetValue(type, out typedLog) && typedLog != null) return typedLog;
            typedLog = LogManager.GetLogger(type);
            Loggers[type] = typedLog;

            return typedLog;

        }
        
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var logger = GetLogger(context);
            LogMethod? method = null;
            
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                {
                    method = logger.Error();
                }
                    break;
                case LogType.Warning:
                {
                    method = logger.Warn();
                }
                    break;
                case LogType.Log:
                {
                    method = logger.Info();
                }
                    break;
            }
            if (args?.Length > 0)
            {
                method?.CallFormat(context, format, args);    
            }
            else
            {
                method?.Call(context, format);
            }
        }

        void AddNestedType(StringBuilder sb, Type type)
        {
            if (type.IsNested)
            {
                AddNestedType(sb, type.DeclaringType);
                sb.Append("+");
            }

            sb.Append(type.Name);
        }
        
        public void LogException(Exception exception, Object context)
        {
            var logger = GetLogger(context);
            if(exception == null) return;
            

            logger.Error()?.Call(context, exception.UnityMessageWithStack(), exception);
        }

        private UnityDefaultLogHandler(){ }
    }
}