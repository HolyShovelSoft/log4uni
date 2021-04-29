using UnityEngine;

namespace log4net.Unity.Config
{
    [ExcludeFromSearch]
    internal class AppDataPathConfigurator: PathConfigurator
    {
        public static readonly AppDataPathConfigurator Instance = new AppDataPathConfigurator();
        
        public override int Order => int.MaxValue - 2;

        protected override string Path => Application.dataPath;
        
        private AppDataPathConfigurator() { }
    }
}