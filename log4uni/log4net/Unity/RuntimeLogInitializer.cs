using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    internal static class RuntimeLogInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void NamesFixer()
        {
            var threadName = Thread.CurrentThread.Name;
            UnityDefaultLogHandler.applicationDataPath = Application.dataPath;
            if (string.IsNullOrEmpty(threadName))
            {
                Thread.CurrentThread.Name = "main";    
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Init()
        {
            UnityDefaultLogHandler.applicationDataPath = Application.dataPath;
            ConfigProcessor.ReconfigureLoggers();
        }
    }
}