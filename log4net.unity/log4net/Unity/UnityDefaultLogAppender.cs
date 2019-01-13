using System;
using log4net.Appender;
using log4net.Core;
using UnityEngine;

namespace log4net.Unity
{
    public class UnityDefaultLogAppender: AppenderSkeleton
    {
        private static readonly int ErrorLevel = Level.Error.Value;
        private static readonly int WarnLevel = Level.Warn.Value;
        
        protected override void Append(LoggingEvent loggingEvent)
        {
            var level = loggingEvent.Level;
            
            if(level == null) return;
            
            string message;
            
            try
            {
                message = RenderLoggingEvent(loggingEvent);
            }
            catch (Exception e)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogException(e, null);
                return;
            }
            
            if (level.Value < WarnLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Log, null, message);
            }
            else if (level.Value >= WarnLevel && level.Value < ErrorLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Warning, null, message);
            }
            else if(level.Value >= ErrorLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Error, null, message);
            }
        }
    }
}