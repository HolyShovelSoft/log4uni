using System;
using System.Xml;
using log4net.Config;

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
        
        public void TryConfigure()
        {
            var document = GetDocument();
            if (document == null) return;
            XmlConfigurator.Configure(document.DocumentElement);
        }
    }
}