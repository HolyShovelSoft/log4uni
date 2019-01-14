using System;

namespace log4net.Unity.Config
{
    public interface IConfigurator
    {
        int Order { get; }
        event Action OnChange;
        void TryConfigure();
    }
}