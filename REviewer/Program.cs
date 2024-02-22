

using REviewer.Modules.Common.REviewer.Modules.Common;

namespace REviewer
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        static readonly Mutex Mutex = new(true, "{F67946B5-6320-46FF-A9CA-62C818B500EA}");
        
        [STAThread]
        static void Main()
        {
            if (Mutex.WaitOne(TimeSpan.Zero, true))
            {
                Logger.Logging.Info("Starting application");
                ApplicationConfiguration.Initialize();
                Application.Run(new Modules.Forms.MainWindow());
                Mutex.ReleaseMutex();
            }
            else
            {
                // only one is allowed
                MessageBox.Show("Another instance of the application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}