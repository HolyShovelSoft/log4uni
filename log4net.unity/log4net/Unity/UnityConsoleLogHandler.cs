using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public class UnityConsoleLogHandler: ILogHandler
    {

        private static ILogHandler _unityLogHandler;
        private static ILogHandler _log4NetLogHandler;
        
        private static bool _useInRuntime = true;

        public static bool UseInRuntime
        {
            get { return _useInRuntime; }
            set
            {
                if (_useInRuntime == value) return;
                _useInRuntime = value;
                UpdateLogHandler();
            }
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool IsTypedLogging { get; set; }

        private static void CheckHandlers()
        {
            if(Application.isEditor) return;
            if (_unityLogHandler == null) _unityLogHandler = Debug.unityLogger.logHandler;
            if (_log4NetLogHandler == null) _log4NetLogHandler = new UnityConsoleLogHandler();
        }
        
        internal static void UpdateLogHandler()
        {
            if(Application.isEditor) return;
            CheckHandlers();
            Debug.unityLogger.logHandler = _useInRuntime ? _log4NetLogHandler : _unityLogHandler;
        }
        
        private static readonly ILog CommonLogger = LogManager.GetLogger("Common");
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
                method?.CallFormat(format, args);    
            }
            else
            {
                method?.Call(format);
            }
        }

        public void LogException(Exception exception, Object context)
        {
            var logger = GetLogger(context);
            if(exception == null) return;

            logger.Fatal()?.Call(exception.Message, exception);
        }

        internal UnityConsoleLogHandler(){ }
    }
}