using log4net.Appender;
using log4net.Core;
using UnityEngine;

namespace log4net.Unity
{
    public class UnityConsoleAppender: AppenderSkeleton
    {
        private static readonly int ErrorLevel = Level.Error.Value;
        private static readonly int WarnLevel = Level.Warn.Value;
        
        protected override void Append(LoggingEvent loggingEvent)
        {
            if(!Application.isEditor) return;

            var level = loggingEvent.Level;
            
            if(level == null) return;
            var message = RenderLoggingEvent(loggingEvent);
            
            if (level.Value < WarnLevel)
            {
                Debug.Log(message);
            }
            else if (level.Value >= WarnLevel && level.Value < ErrorLevel)
            {
                Debug.LogWarning(message);
            }
            else if(level.Value >= ErrorLevel)
            {
                Debug.LogError(message);
            }
        }
    }
}