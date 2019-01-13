using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    internal static class RuntimeInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Init()
        {
            var threadName = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(threadName))
            {
                Thread.CurrentThread.Name = "main";    
            }

            UnityConsoleLogHandler.UpdateLogHandler();
        }
    }
}