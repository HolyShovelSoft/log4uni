using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace log4net.Unity.Config
{
    internal abstract class PathConfigurator: BaseDefaultConfigurator
    {
        private string[] filePaths;

        internal string[] FilePaths
        {
            get
            {
                if (filePaths != null) return filePaths;
                
                var path = Path;
                if (string.IsNullOrEmpty(path)) return null;
                var fullPath = System.IO.Path.GetFullPath(path);
                filePaths = new[]
                {
                    System.IO.Path.Combine(fullPath, $"log4net.{(Application.isEditor ? "editor" : "runtime")}.xml"),
                    System.IO.Path.Combine(fullPath, $"log4net.{(Application.isEditor ? "editor" : "runtime")}.config"),
                    System.IO.Path.Combine(fullPath, $"log4net.{(Application.isEditor ? "editor" : "runtime")}.txt"),
                    System.IO.Path.Combine(fullPath, "log4net.xml"),
                    System.IO.Path.Combine(fullPath, "log4net.config"),
                    System.IO.Path.Combine(fullPath, "log4net.txt")
                };

                return filePaths;
            }
        }
        
        protected abstract string Path { get; }

        protected override XmlDocument GetDocument()
        {
            var files = FilePaths;

            for (var i = 0; i <= files.Length - 1; i++)
            {
                var file = files[i];
                try
                {
                    if(!File.Exists(file)) continue;
                    var text = File.ReadAllText(file, Encoding.UTF8);
                    var doc = new XmlDocument();
                    doc.LoadXml(text);
                    if(doc.DocumentElement?.Name != "log4net") continue;
                    return doc;
                }
                catch 
                {
                    //
                }
            }

            return null;
        }
    }
}