using NLog;

namespace REviewer.Modules.Common
{
    using NLog;

    namespace REviewer.Modules.Common
    {
        public static class Logger
        {
            public static NLog.Logger Logging { get; } = LogManager.GetCurrentClassLogger();
        }
    }
}
