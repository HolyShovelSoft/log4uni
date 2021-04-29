using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net.Repository.Hierarchy;
using log4net.Unity.Config;
using UnityEngine;

namespace log4net.Unity
{
    public static class ConfigProcessor
    {
        public static bool IsConfigured => ((Hierarchy) LogManager.GetRepository()).Configured;
        
        internal class Comparer: IComparer<IConfigurator>
        {
            public static readonly IComparer<IConfigurator> Instance = new Comparer();
            
            public int Compare(IConfigurator x, IConfigurator y)
            {
                var xOrder = x?.Order ?? 0;
                var yOrder = y?.Order ?? 0;
                
                if (xOrder == yOrder)
                {
                    return 0;
                }

                if (xOrder > yOrder)
                {
                    return 1;
                }

                return -1;
            }
            
            private Comparer() { }
        }
        
        internal const string DefaultConfig = 
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
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

        private static readonly List<IConfigurator> Configurators = new List<IConfigurator>();
        private static Type[] _types;

        private static void FillTypes()
        {
            if(_types != null) return;
            _types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch
                    {
                        return ArrayEmpty<Type>.Instance;
                    }
                })
                .Where(type =>
                {
                    if (type == null) return false;
                    if (!typeof(IConfigurator).IsAssignableFrom(type)) return false;
                    if (type.ContainsGenericParameters || type.IsGenericType || type.IsGenericTypeDefinition) return false;

                    if (typeof(ScriptableObject).IsAssignableFrom(type)) return true;

                    if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;

                    if (type.GetCustomAttributes(typeof(ExcludeFromSearchAttribute), false).Length > 0) return false;

                    var constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(info => info.GetParameters().Length == 0);

                    return constructor != null;
                }).ToArray();
        }

        private static void FillDefaults()
        {
            Configurators.Add(FallbackConfigurator.Instance);
            FallbackConfigurator.Instance.OnChange += ReconfigureLoggers;
            Configurators.Add(ResourceConfigurator.Instance);
            ResourceConfigurator.Instance.OnChange += ReconfigureLoggers;
            Configurators.Add(AppDataPathConfigurator.Instance);
            AppDataPathConfigurator.Instance.OnChange += ReconfigureLoggers;
            Configurators.Add(AppPersistentDataPathConfigurator.Instance);
            AppPersistentDataPathConfigurator.Instance.OnChange += ReconfigureLoggers;
        }

        private static void FillReflections()
        {
            for (var i = 0; i <= _types.Length - 1; i++)
            {
                var type = _types[i];
                if (type == null) continue;
                if (typeof(UnityEngine.Object).IsAssignableFrom(type)) continue;
                var configurator = Activator.CreateInstance(type) as IConfigurator;
                if (configurator != null)
                {
                    Configurators.Add(configurator);
                    configurator.OnChange += ReconfigureLoggers;
                }
            }
        }
        
        private static void FillResources()
        {
            for (var i = 0; i <= _types.Length - 1; i++)
            {
                var type = _types[i];
                if (type == null) continue;
                if (!typeof(ScriptableObject).IsAssignableFrom(type)) continue;
                var resources = Resources.LoadAll("", type);
                for (var j = 0; j <= resources.Length - 1; j++)
                {
                    var resource = resources[j];
                    if (!resource) continue;
                    var configurator = resource as IConfigurator;
                    if (configurator != null)
                    {
                        Configurators.Add(configurator);
                        configurator.OnChange += ReconfigureLoggers;
                    }
                }
            }
        }
        
        private static void Check()
        {
            UnityDefaultLogHandler.UpdateLogHandler();
            if (Configurators.Count != 0) return;
            FillTypes();
            FillDefaults();
            FillReflections();
            FillResources();
        }

        public static void AddConfigurator(IConfigurator configurator)
        {
            Check();
            if(configurator == null) return;
            var idx = Configurators.IndexOf(configurator);
            if(idx >= 0) return;
            Configurators.Add(configurator);
            configurator.OnChange += ReconfigureLoggers;
        }
        
        public static void RemoveConfigurator(IConfigurator configurator)
        {
            Check();
            if(configurator == null) return;
            var idx = Configurators.IndexOf(configurator);
            if(idx < 0) return;
            Configurators.RemoveAt(idx);
            configurator.OnChange -= ReconfigureLoggers;
        }
        
        internal static void ReconfigureLoggers()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            Check();
            Configurators.Sort(Comparer.Instance);
            hierarchy.ResetConfiguration();
            for (var i = 0; i <= Configurators.Count - 1; i++)
            {
                var configurator = Configurators[i];
                if(configurator == null) return;
                try
                {
                    configurator.TryConfigure();
                    if(IsConfigured) return;
                }
                catch (Exception e)
                {
                    UnityDefaultLogHandler.DefaultUnityLogger.LogFormat(LogType.Error, null,
                        $"Error occured while '{configurator}' try configure. Exception: {e}");
                }
            }

            if (!hierarchy.Configured)
            {
                UnityDefaultLogHandler.DefaultUnityLogger.LogFormat(LogType.Warning, null,
                    "Log4net not configured... something goes wrong.");
            }
        }
        
        internal static void SaveDefaultConfig(string path)
        {
            if(string.IsNullOrEmpty(path)) return;
            var fullPath = Path.GetFullPath(path);
            try
            {
                var directory = Path.GetDirectoryName(fullPath);
                if(string.IsNullOrEmpty(directory)) return;
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(fullPath, DefaultConfig, Encoding.UTF8);
            }
            catch 
            {
                //
            }
        }
    }
}