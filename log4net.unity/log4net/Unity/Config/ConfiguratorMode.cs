using System;

namespace log4net.Unity.Config
{
    [Flags]
    public enum ConfiguratorMode
    {
        Editor = 1,
        Runtime = 2
    }
}