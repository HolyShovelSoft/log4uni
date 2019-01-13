using UnityEngine;

namespace log4net.Unity.Config
{
    [ExcludeFromSearch]
    internal class AppPersistentDataPathConfigurator: PathConfigurator
    {
        public static readonly AppPersistentDataPathConfigurator Instance = new AppPersistentDataPathConfigurator();
        
        public override int Order => int.MaxValue - 3;

        protected override string Path => Application.persistentDataPath;
        
        private AppPersistentDataPathConfigurator() { }
    }
}