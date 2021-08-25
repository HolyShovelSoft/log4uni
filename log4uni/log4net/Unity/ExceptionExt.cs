using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace log4net.Unity
{
    public static class ExceptionExt
    {
        private static void AddNestedType(StringBuilder sb, Type type)
        {
            if (type.IsNested)
            {
                AddNestedType(sb, type.DeclaringType);
                sb.Append("+");
            }

            sb.Append(type.Name);
        }

        public static string UnityMessageWithStack(this Exception exception, bool withFiles = true)
        {
            if (exception == null)
            {
                return "";
            }
            
            try
            {
                return UnityMessageWithStackInternal(exception, withFiles);
            }
            catch
            {
                return $"{exception.Message}\r\n{exception.StackTrace}";
            }
        }
        
        private static string UnityMessageWithStackInternal(this Exception exception, bool withFiles = true)
        {
            var trace = new System.Diagnostics.StackTrace(exception, withFiles);
            
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
                sb.Append(" (");
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
                var isAsset = false;
                if (!string.IsNullOrEmpty(file))
                {
                    if (Application.isEditor)
                    {
                        file = Path.GetFullPath(file);
                        var applicationDataPath = string.IsNullOrEmpty(UnityDefaultLogHandler.applicationDataPath) ? "Assets" : UnityDefaultLogHandler.applicationDataPath; 
                        applicationDataPath = Path.GetFullPath(applicationDataPath);
                        var dirName = Path.GetDirectoryName(applicationDataPath);
                        if (!string.IsNullOrEmpty(dirName))
                        {
                            var projectPath = Path.GetFullPath(dirName);
                            if (file.StartsWith(projectPath))
                            {
                                isAsset = true;
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
                        file = file.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                    }
                    
                    sb.Append("(at ");
                    if (isAsset && UnityDefaultLogHandler.unityVersion.Major >= 2020 && Application.isEditor)
                    {
                        sb.Append("<a href=\"");
                        sb.Append(file);
                        sb.Append("\" line=\"");
                        sb.Append(frame.GetFileLineNumber());
                        sb.Append("\">");
                    }
                    sb.Append(file);
                    sb.Append(":");
                    sb.Append(frame.GetFileLineNumber());
                    if (isAsset && UnityDefaultLogHandler.unityVersion.Major >= 2020 && Application.isEditor)
                    {
                        sb.Append("</a>");
                    }
                    sb.Append(")");    
                    
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}