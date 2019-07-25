using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using log4net.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace log4net.Unity
{
    public class UnityDefaultLogHandler: ILogHandler
    {
        public static ILogger DefaultUnityLogger { get; private set; }

        internal static ILogHandler unityLogHandler;
        private static ILogHandler _log4NetLogHandler;
        internal static string applicationDataPath;
        
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool IsTypedLogging { get; set; }
        
        internal static void UpdateLogHandler()
        {
            if (unityLogHandler != null) return;
            unityLogHandler = Debug.unityLogger.logHandler;
            DefaultUnityLogger = new Logger(unityLogHandler);
            if (_log4NetLogHandler == null) _log4NetLogHandler = new UnityDefaultLogHandler();
            Debug.unityLogger.logHandler = _log4NetLogHandler;

            LogLog.LogReceived += (source, args) =>
            {
                var prefix = args?.LogLog.Prefix ?? "log4net: ";
                if (prefix.ToLower().Contains("warn"))
                {
                    unityLogHandler.LogFormat(LogType.Warning, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
                else if (prefix.ToLower().Contains("error"))
                {
                    unityLogHandler.LogFormat(LogType.Error, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
                else
                {
                    unityLogHandler.LogFormat(LogType.Log, null, $"{prefix}{args?.LogLog?.Message}{(args?.LogLog?.Exception != null ? " Exception: " + args.LogLog.Exception.ToString() : "")}");
                }
            };
        }
        
        private static readonly ILog CommonLogger = LogManager.GetLogger("Unity");
        private static readonly Dictionary<Type, ILog> Loggers = new Dictionary<Type, ILog>();

        private static ILog GetLogger(Object context)
        {
            if (!context || !IsTypedLogging) return CommonLogger;
            var type = context.GetType();
            ILog typedLog;
            if (Loggers.TryGetValue(type, out typedLog) && typedLog != null) return typedLog;
            typedLog = LogManager.GetLogger(type);
            Loggers[type] = typedLog;

            return typedLog;

        }
        
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var logger = GetLogger(context);
            LogMethod? method = null;
            
            switch (logType)
            {
                case LogType.Assert:
                case LogType.Exception:
                case LogType.Error:
                {
                    method = logger.Error();
                }
                    break;
                case LogType.Warning:
                {
                    method = logger.Warn();
                }
                    break;
                case LogType.Log:
                {
                    method = logger.Info();
                }
                    break;
            }
            if (args?.Length > 0)
            {
                method?.CallFormat(format, args);    
            }
            else
            {
                method?.Call(format);
            }
        }

        void AddNestedType(StringBuilder sb, Type type)
        {
            if (type.IsNested)
            {
                AddNestedType(sb, type.DeclaringType);
                sb.Append("+");
            }

            sb.Append(type.Name);
        }
        
        public void LogException(Exception exception, Object context)
        {
            var logger = GetLogger(context);
            if(exception == null) return;

            var trace = new System.Diagnostics.StackTrace(exception, true);

            var sb = new StringBuilder();

            sb.AppendLine(exception.Message);

            var count = trace.FrameCount;

            var upBorder = count - 64;
            var downBorder = 64;
            var itWasInBorder = false;

            for (var i = 0; i <= trace.FrameCount - 1; i++)
            {
                if (i > downBorder && i < upBorder)
                {
                    if (!itWasInBorder)
                    {
                        sb.AppendLine("-------- Cut very long stack --------");
                        itWasInBorder = true;
                    }
                    continue;
                }

                var frame = trace.GetFrame(i);

                var method = frame.GetMethod();

                var type = method?.DeclaringType;
                if (type != null)
                {
                    var ns = type.Namespace;
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.Append(ns);
                        sb.Append(".");
                    }

                    AddNestedType(sb, type);
                }
                sb.Append(":");
                sb.Append(method?.Name);
                sb.Append("(");
                var args = method?.GetParameters();
                for (var j = 0; j <= args?.Length - 1; j++)
                {
                    var arg = args[j];
                    sb.Append(arg.ParameterType.Name);
                    if (j != args?.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");

                var file = frame.GetFileName();
                if (!string.IsNullOrEmpty(file))
                {
                    if (Application.isEditor)
                    {
                        
                        file = Path.GetFullPath(file);
                        applicationDataPath = Path.GetFullPath(applicationDataPath);
                        var dirName = Path.GetDirectoryName(applicationDataPath);
                        if (!string.IsNullOrEmpty(dirName))
                        {
                            var projectPath = Path.GetFullPath(dirName);
                            if (file.StartsWith(projectPath))
                            {
                                file = file.Remove(0, projectPath.Length);
                                if (file.Length > 0)
                                {
                                    if (file[0] == Path.PathSeparator || file[0] == Path.DirectorySeparatorChar ||
                                        file[0] == Path.AltDirectorySeparatorChar)
                                    {
                                        file = file.Remove(0, 1);
                                    }
                                }
                            }
                        }
                    }
                    
                    sb.Append(" (at ");
                    sb.Append(file);
                    sb.Append(":");
                    sb.Append(frame.GetFileLineNumber());
                    sb.Append(")");    
                    
                }
                sb.AppendLine();
            }

            logger.Fatal()?.Call(sb.ToString());
        }

        private UnityDefaultLogHandler(){ }
    }
}