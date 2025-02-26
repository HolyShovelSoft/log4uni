using UnityEditor;
using UnityEngine;

namespace log4net.Unity
{
    public static class EditorHelper
    {
        private const string MAKE_DEFAULT_CONFIG_MENU_PATH = "Tools/log4net/Make Default Config";
        private const string TEST_LOGS_MENU_PATH = "Tools/log4net/Test Logs";
        
        private static readonly ILog TestLogger = LogManager.GetLogger("Test");


       [MenuItem(MAKE_DEFAULT_CONFIG_MENU_PATH)]
        private static void MakeDefaultConfig()
        {
            var path = EditorUtility.SaveFilePanel(
                "Save default config",
                Application.dataPath,
                "log4net.xml",
                "xml"
            );

            if (!string.IsNullOrEmpty(path))
            {
                ConfigProcessor.SaveDefaultConfig(path);
                AssetDatabase.Refresh();
            }
        }

        

        [MenuItem(TEST_LOGS_MENU_PATH)]
        private static void TestLogs()
        {
            Debug.Log("Test 'Debug.Log' message");
            Debug.LogWarning("Test 'Debug.LogWarning' message");
            Debug.LogError("Test 'Debug.LogError' message");
            TestLogger.Debug()?.Call("Test 'ILog.Debug' message");
            TestLogger.Info()?.Call("Test 'ILog.Info' message");
            TestLogger.Warn()?.Call("Test 'ILog.Warn' message");
            TestLogger.Error()?.Call("Test 'ILog.Error' message");
            TestLogger.Fatal()?.Call("Test 'ILog.Fatal' message");
        }
    }
}