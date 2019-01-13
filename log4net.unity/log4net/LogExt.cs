namespace log4net
{
    public static class LogExt
    {
        internal enum LogType
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }

        internal static bool IsEnabled(this ILog log, LogType logType)
        {
            if (log == null) return false;
            switch (logType)
            {
                case LogType.Debug:
                    return log.IsDebugEnabled;
                case LogType.Info:
                    return log.IsInfoEnabled;
                case LogType.Warn:
                    return log.IsWarnEnabled;
                case LogType.Error:
                    return log.IsErrorEnabled;
                case LogType.Fatal:
                    return log.IsFatalEnabled;
            }
            return false;
        } 

        private static LogMethod? For(ILog log, LogType logType)
        {
            if (log.IsEnabled(logType))
            {
                return new LogMethod(log, logType);
            }

            return null;
        }

        public static LogMethod? Debug(this ILog log)
        {
            return For(log, LogType.Debug);
        }
        
        public static LogMethod? Info(this ILog log)
        {
            return For(log, LogType.Info);
        }
        
        public static LogMethod? Warn(this ILog log)
        {
            return For(log, LogType.Warn);
        }
        
        public static LogMethod? Error(this ILog log)
        {
            return For(log, LogType.Error);
        }
        
        public static LogMethod? Fatal(this ILog log)
        {
            return For(log, LogType.Fatal);
        }
    }
}