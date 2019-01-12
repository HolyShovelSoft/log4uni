using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    internal static class RuntimeInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var threadName = Thread.CurrentThread.Name;
            if (string.IsNullOrEmpty(threadName))
            {
                Thread.CurrentThread.Name = "main";    
            }
            
            if(Application.isEditor) return;
            
            Debug.unityLogger.logHandler = new RuntimeLogHandler();
        }
    }
}