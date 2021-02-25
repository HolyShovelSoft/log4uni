using System;
using log4net.Appender;
using log4net.Core;
using UnityEngine;
using Object = System.Object;

namespace log4net.Unity
{
    public class UnityDefaultLogAppender: AppenderSkeleton
    {
        public const string UnityContext = "unity:context";

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

            var ctx = loggingEvent.LookupProperty(UnityContext) as UnityEngine.Object;

            if (level.Value < WarnLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Log, ctx, "{0}", message);
            }
            else if (level.Value >= WarnLevel && level.Value < ErrorLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Warning, ctx, "{0}", message);
            }
            else if(level.Value >= ErrorLevel)
            {
                UnityDefaultLogHandler.unityLogHandler?.LogFormat(LogType.Error, ctx, "{0}", message);
            }
        }
    }
}