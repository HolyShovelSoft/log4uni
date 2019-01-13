//using System;
//using System.Linq;
//using System.Text;
//using log4net.Repository.Hierarchy;
//using UnityEngine;
//
//namespace log4net.Unity
//{
//    public abstract class Configurator: ScriptableObject, IConfigurator 
//    {
//        public abstract ConfiguratorTarget Target { get; }
//        public abstract void Configure();
//
//        internal static void ConfigureLoggers()
//        {
//            var configurators = Resources.LoadAll<Configurator>("");
//            foreach (var configurator in configurators)
//            {
//                if (configurator && TryConfigure(configurator))
//                {
//                    return;
//                }
//            }
//
//            TryConfigure(DefaultConfigurator.Instance);
//        }
//        
//        public static bool TryConfigure(IConfigurator configurator)
//        {
//            UnityDefaultLogHandler.UpdateLogHandler();
//            var unityHandler = UnityDefaultLogHandler.unityLogHandler;
//            
//            if (configurator == null) return false;
//            if (Application.isEditor && (configurator.Target & ConfiguratorTarget.Editor) != ConfiguratorTarget.Editor)
//                return false;
//            if (!Application.isEditor && (configurator.Target & ConfiguratorTarget.Runtime) != ConfiguratorTarget.Runtime)
//                return false;
//            
//            var hierarchy = (Hierarchy)LogManager.GetRepository();
//            hierarchy.ResetConfiguration();
//
//            try
//            {
//                configurator.Configure();
//            }
//            catch (Exception e)
//            {
//                unityHandler.LogException(e, null);
//                return false;
//            }
//            
//            
//            if (hierarchy.Configured) return true;
//            
//            var sb = new StringBuilder();
//            foreach (var message in hierarchy.ConfigurationMessages.Cast<Util.LogLog>())
//            {
//                if (!string.IsNullOrEmpty(message.Message))
//                {
//                    sb.AppendLine(message.Message);
//                }
//
//                if (message.Exception != null)
//                {
//                    sb.AppendLine($"  exception: {message.Exception}");
//                }
//            }
//
//            var messages = sb.ToString().Trim();
//
//            unityHandler.LogFormat(LogType.Error, null,
//                !string.IsNullOrEmpty(messages)
//                    ? $"Configuration of log4net failed with configurator: {configurator}. Configuration Messages:\r\n{messages}"
//                    : $"Configuration of log4net failed with configurator: {configurator}.");
//
//            return false;
//        }
//    }
//}