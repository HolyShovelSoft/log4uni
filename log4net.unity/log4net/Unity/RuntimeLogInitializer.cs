using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    internal static class RuntimeLogInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Init()
        {
            var threadName = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(threadName))
            {
                Thread.CurrentThread.Name = "main";    
            }
            ConfigProcessor.ReconfigureLoggers();
        }
    }
}