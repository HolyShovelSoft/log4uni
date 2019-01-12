using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public class RuntimeLogHandler: ILogHandler
    {
        public static bool IsTypedLogging { get; set; } 
        private static readonly ILog CommonLogger = LogManager.GetLogger("Common");
        private static readonly Dictionary<Type, ILog> Loggers = new Dictionary<Type, ILog>();

        private static ILog GetLogger(Object context)
        {
            if (context && IsTypedLogging)
            {
                var type = context.GetType();
                ILog typedLog;
                if (!Loggers.TryGetValue(type, out typedLog) || typedLog == null)
                {
                    typedLog = LogManager.GetLogger(type);
                }

                return typedLog;
            }

            return CommonLogger;
        }
        
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var logger = GetLogger(context);
            
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                {
                    logger.ForError()?.CallFormat(format, args);
                }
                    break;
                case LogType.Warning:
                {
                    logger.ForWarn()?.CallFormat(format, args);
                }
                    break;
                case LogType.Log:
                {
                    logger.ForInfo()?.CallFormat(format, args);
                }
                    break;
            }
        }

        public void LogException(Exception exception, Object context)
        {
            var logger = GetLogger(context);
            if(exception == null) return;

            logger.ForFatal()?.Call(exception.Message, exception);
        }
    }
}