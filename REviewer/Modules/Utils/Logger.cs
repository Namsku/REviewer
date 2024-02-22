using NLog;

namespace REviewer.Modules.Utils
{
        public static class Logger
        {
            public static NLog.Logger Logging { get; } = LogManager.GetCurrentClassLogger();
        }
}
