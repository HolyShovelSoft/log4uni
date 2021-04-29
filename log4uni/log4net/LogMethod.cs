using System;
using System.Globalization;
using log4net.Core;
using log4net.Unity;
using log4net.Util;

namespace log4net
{
    public struct LogMethod
    {
        private readonly ILog target;
        private readonly LogExt.LogType logType;

        internal LogMethod(ILog target, LogExt.LogType logType)
        {
            this.target = target;
            this.logType = logType;
        }

        public void Call(string message)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.Debug(message);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.Info(message);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.Warn(message);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.Error(message);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.Fatal(message);
                }
                    break;
            }
        }
        
        public void Call(string message, Exception exception)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.Debug(message, exception);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.Info(message, exception);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.Warn(message, exception);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.Error(message, exception);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.Fatal(message, exception);
                }
                    break;
            }
        }

        public void CallFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.DebugFormat(formatProvider, format, args);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.InfoFormat(formatProvider, format, args);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.WarnFormat(formatProvider, format, args);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.ErrorFormat(formatProvider, format, args);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.FatalFormat(formatProvider, format, args);
                }
                    break;
            }
        }

        public void CallFormat(string format, object arg0)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.DebugFormat(format, arg0);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.InfoFormat(format, arg0);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.WarnFormat(format, arg0);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.ErrorFormat(format, arg0);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.FatalFormat(format, arg0);
                }
                    break;
            }
        }
        
        public void CallFormat(string format, object arg0, object arg1)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.DebugFormat(format, arg0, arg1);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.InfoFormat(format, arg0, arg1);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.WarnFormat(format, arg0, arg1);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.ErrorFormat(format, arg0, arg1);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.FatalFormat(format, arg0, arg1);
                }
                    break;
            }
        }
        
        public void CallFormat(string format, object arg0, object arg1, object arg2)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.DebugFormat(format, arg0, arg1, arg2);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.InfoFormat(format, arg0, arg1, arg2);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.WarnFormat(format, arg0, arg1, arg2);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.ErrorFormat(format, arg0, arg1, arg2);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.FatalFormat(format, arg0, arg1, arg2);
                }
                    break;
            }
        }
        
        public void CallFormat(string format, params object[] args)
        {
            if (!target.IsEnabled(logType)) return;
            switch (logType)
            {
                case LogExt.LogType.Debug:
                {
                    target.DebugFormat(format, args);
                }
                    break;
                case LogExt.LogType.Info:
                {
                    target.InfoFormat(format, args);
                }
                    break;
                case LogExt.LogType.Warn:
                {
                    target.WarnFormat(format, args);
                }
                    break;
                case LogExt.LogType.Error:
                {
                    target.ErrorFormat(format, args);
                }
                    break;
                case LogExt.LogType.Fatal:
                {
                    target.FatalFormat(format, args);
                }
                    break;
            }
        }

        private Level GetLevel()
        {
            switch (logType)
            {
                case LogExt.LogType.Debug:
                    return Level.Debug;
                case LogExt.LogType.Info:
                    return Level.Info;
                case LogExt.LogType.Warn:
                    return Level.Warn;
                case LogExt.LogType.Error:
                    return Level.Error;
                case LogExt.LogType.Fatal:
                    return Level.Fatal;
            }
            return Level.Verbose;
        }

        public void CallFormat(UnityEngine.Object ctx, string format, params object[] args)
        {
            if (!target.IsEnabled(logType)) return;
            var evt = new LoggingEvent(ThisDeclaringType, target.Logger.Repository, target.Logger.Name, GetLevel(), new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
            if (ctx != null)
                evt.Properties[UnityDefaultLogAppender.UnityContext] = ctx;
            target.Logger.Log(evt);
        }
        public void Call(UnityEngine.Object ctx, string msg)
        {
            if (!target.IsEnabled(logType)) return;
            var evt = new LoggingEvent(ThisDeclaringType, target.Logger.Repository, target.Logger.Name, GetLevel(), msg, null);
            if(ctx != null)
                evt.Properties[UnityDefaultLogAppender.UnityContext] = ctx;
            target.Logger.Log(evt);
        }

        public void Call(UnityEngine.Object ctx, string msg, Exception e)
        {
            if (!target.IsEnabled(logType)) return;
            var evt = new LoggingEvent(ThisDeclaringType, target.Logger.Repository, target.Logger.Name, GetLevel(), msg, e);
            if (ctx != null)
                evt.Properties[UnityDefaultLogAppender.UnityContext] = ctx;
            target.Logger.Log(evt);
        }

        private static readonly Type ThisDeclaringType = typeof(LogImpl);
    }
    }