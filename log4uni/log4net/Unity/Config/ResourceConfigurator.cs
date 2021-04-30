using System.Linq;
using System.Xml;
using UnityEngine;

namespace log4net.Unity.Config
{
    [ExcludeFromSearch]
    internal class ResourceConfigurator: BaseDefaultConfigurator
    {
        public static readonly ResourceConfigurator Instance = new ResourceConfigurator();
        
        public override int Order => int.MaxValue - 1;

        protected override XmlDocument GetDocument()
        {
            var assets = Resources.LoadAll<TextAsset>($"log4net.{(Application.isEditor ? "editor" : "runtime")}").ToList();
            assets.AddRange(Resources.LoadAll<TextAsset>("log4net"));
            
            for (var i = 0; i <= assets.Count - 1; i++)
            {
                var asset = assets[i];
                var doc = new XmlDocument();
                try
                {
                    doc.LoadXml(asset.text);
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
        
        private ResourceConfigurator() {} 
    }
}