//using System.Xml;
//
//namespace log4net.Unity
//{
//    public sealed class DefaultConfigurator: IConfigurator
//    {
//        public static readonly DefaultConfigurator Instance = new DefaultConfigurator();
//        
//        internal const string DefaultConfig = 
//            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
//            "<log4net>\r\n" +
//            "	<appender name=\"unityConsole\" type=\"log4net.Unity.UnityDefaultLogAppender\">\r\n" +
//            "		<layout type=\"log4net.Layout.PatternLayout\">\r\n" +
//            "			<conversionPattern value=\"[%thread][%level][%logger] %message\"/>\r\n" +
//            "		</layout>\r\n" +
//            "	</appender>\r\n" +
//            "	<root>\r\n" +
//            "		<level value=\"INFO\"/>\r\n" +
//            "		<appender-ref ref=\"unityConsole\"/>\r\n" +
//            "	</root>\r\n" +
//            "</log4net>";
//        
//        ConfiguratorTarget IConfigurator.Target => ConfiguratorTarget.Editor | ConfiguratorTarget.Runtime;
//
//        void IConfigurator.Configure()
//        {
//            var doc = new XmlDocument();
//            doc.LoadXml(DefaultConfig);
//            Config.XmlConfigurator.Configure(doc.DocumentElement);
//        }
//        
//        private DefaultConfigurator(){}
//    }
//}