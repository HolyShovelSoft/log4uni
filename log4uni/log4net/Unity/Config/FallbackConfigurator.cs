using System.Xml;

namespace log4net.Unity.Config
{
    [ExcludeFromSearch]
    internal class FallbackConfigurator: BaseDefaultConfigurator
    {
        public static readonly FallbackConfigurator Instance = new FallbackConfigurator();
        
        public override int Order => int.MaxValue;

        protected override XmlDocument GetDocument()
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(ConfigProcessor.DEFAULT_CONFIG);
                return xmlDoc;
            }
            catch
            {
                return null;
            }
        }
        
        private FallbackConfigurator() { }
    }
}