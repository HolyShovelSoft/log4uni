using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using log4net.Repository.Hierarchy;
using UnityEditor;
using UnityEngine;

namespace log4net.Unity
{
    public class EditorLog4NetConfigHandler : AssetPostprocessor
    {
        private const string MakeDefaultConfigMenuPath = "Tools/log4net/Make Default Config";
        private const string ResetDefaultConfigMenuPath = "Tools/log4net/Reset Config To Default";
        private const string TestLogsMenuPath = "Tools/log4net/Test Logs";
        
        private const string DefaultConfigPath = "Assets/log4net.xml";
        private const string DefaultConfig = 
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<log4net>\r\n" +
            "	<appender name=\"unityConsole\" type=\"log4net.Unity.UnityDefaultLogAppender\">\r\n" +
            "		<layout type=\"log4net.Layout.PatternLayout\">\r\n" +
            "			<conversionPattern value=\"[%thread][%level][%logger] %message\"/>\r\n" +
            "		</layout>\r\n" +
            "	</appender>\r\n" +
            "	<root>\r\n" +
            "		<level value=\"INFO\"/>\r\n" +
            "		<appender-ref ref=\"unityConsole\"/>\r\n" +
            "	</root>\r\n" +
            "</log4net>";

        private static ILog _testLogger = LogManager.GetLogger("Test");


        [MenuItem(ResetDefaultConfigMenuPath, false, 0)]
        private static void ResetDefaultConfig()
        {
            var path = Path.GetFullPath(DefaultConfigPath);
            File.WriteAllText(path, DefaultConfig, Encoding.UTF8);
            AssetDatabase.Refresh();
        }
        
        [MenuItem(MakeDefaultConfigMenuPath, false, 1)]
        private static void MakeDefaultConfig()
        {
            var tmp = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultConfigPath);
            if (tmp)
            {
                Debug.Log($"File at path '{DefaultConfigPath}' already exists. If you want reset it to default use command '{ResetDefaultConfigMenuPath}'");
                return;
            }
            ResetDefaultConfig();
        }

        private static void TryConfigureLog4Net()
        {
            var unityHandler = UnityConsoleLogHandler.unityLogHandler;
            
            var doc = new XmlDocument();
            try
            {
                var tmp = AssetDatabase.LoadAssetAtPath<TextAsset>(DefaultConfigPath);
                doc.LoadXml(tmp ? tmp.text : DefaultConfig);
            }
            catch(Exception e)
            {
                unityHandler.LogFormat(LogType.Error,null, $"Log4net config reading failed. Logging may not work properly. Error: {e.Message}");
                return;
            }
            
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.ResetConfiguration();
            Config.XmlConfigurator.Configure(doc.DocumentElement);

            if (hierarchy.Configured) return;
            
            var sb = new StringBuilder();
            foreach (var message in hierarchy.ConfigurationMessages.Cast<Util.LogLog>())
            {
                if (!string.IsNullOrEmpty(message.Message))
                {
                    sb.AppendLine(message.Message);
                }

                if (message.Exception != null)
                {
                    sb.AppendLine($"  exception: {message.Exception}");
                }
            }

            var messages = sb.ToString().Trim();

            unityHandler.LogFormat(LogType.Error, null,
                !string.IsNullOrEmpty(messages)
                    ? $"Configuration of log4net failed. Configuration Messages:\r\n{messages}"
                    : "Configuration of log4net failed.");
        }

        [MenuItem(TestLogsMenuPath)]
        private static void TestLogs()
        {
            Debug.Log("Test 'Debug.Log' message");
            Debug.LogWarning("Test 'Debug.LogWarning' message");
            Debug.LogError("Test 'Debug.LogError' message");
            _testLogger.Debug()?.Call("Test 'ILog.Debug' message");
            _testLogger.Info()?.Call("Test 'ILog.Info' message");
            _testLogger.Warn()?.Call("Test 'ILog.Warn' message");
            _testLogger.Error()?.Call("Test 'ILog.Error' message");
            _testLogger.Fatal()?.Call("Test 'ILog.Fatal' message");
        }
        
        [InitializeOnLoadMethod]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            RuntimeInitializer.Init();
            TryConfigureLog4Net();
        }

        static void OnPostprocessAllAssets(string[] ia, string[] da, string[] ma, string[] mf)
        {
            var all = ia.Concat(da).Concat(ma).Concat(mf).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToArray();
            
            if (all.Any(s => Path.GetFullPath(s) == Path.GetFullPath(DefaultConfigPath)))
            {
                TryConfigureLog4Net();
            }
        }
    }
}