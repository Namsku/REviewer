using NLog;

namespace REviewer.Modules.Utils
{
    public static class Logger
    {
        public static NLog.Logger Instance { get; } = LogManager.GetCurrentClassLogger();
    }
}
