using System;
using System.Xml;
using log4net.Config;
using log4net.Repository.Hierarchy;

namespace log4net.Unity.Config
{
    public abstract class BaseDefaultConfigurator: IConfigurator 
    {
        public abstract int Order { get; }
        public event Action OnChange;
        protected abstract XmlDocument GetDocument();
        
        internal void CallChange()
        {
            OnChange?.Invoke();
        }
        
        public bool TryConfigure()
        {
            var document = GetDocument();
            if (document == null) return false;
            XmlConfigurator.Configure(document.DocumentElement);
            var hierarchy = (Hierarchy)LogManager.GetRepository();
            return hierarchy.Configured;
        }
    }
}