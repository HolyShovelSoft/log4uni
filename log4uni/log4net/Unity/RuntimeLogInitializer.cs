using System;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace log4net.Unity
{
    public static class RuntimeLogInitializer
    {
        private static bool Initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Init()
        {
            if (Initialized) return;
            Initialized = true;

            FillUnityContext();

            ConfigProcessor.ReconfigureLoggers();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
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

        private static void FillUnityContext()
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
        }
    }
}