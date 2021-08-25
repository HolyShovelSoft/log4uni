using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    public static class RuntimeLogInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void MainThreadNameFix()
        {
            try
            {
                var threadName = Thread.CurrentThread.Name;
                UnityDefaultLogHandler.applicationDataPath = Application.dataPath;
                if (string.IsNullOrEmpty(threadName))
                {
                    Thread.CurrentThread.Name = "main";    
                }
            }
            catch
            {
                //
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Init()
        {
            UnityDefaultLogHandler.applicationDataPath = Application.dataPath;
            try
            {
                var regex = new Regex("^([0-9]+\\.[0-9]+\\.[0-9]+)");
                var match = regex.Match(Application.unityVersion);
                UnityDefaultLogHandler.unityVersion = new Version(match.Value);
            }
            catch
            {
                UnityDefaultLogHandler.unityVersion = new Version();
            }
            ConfigProcessor.ReconfigureLoggers();
        }
    }
}