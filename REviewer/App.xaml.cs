using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace REviewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => HandleException(e.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");
            DispatcherUnhandledException += (sender, e) =>
            {
                HandleException(e.Exception, "Application.DispatcherUnhandledException");
                // Optionally set e.Handled = true to prevent app from closing immediately
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                HandleException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void HandleException(Exception? ex, string source)
        {
            try
            {
                // Create logs directory if it doesn't exist
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDir);

                var dumpPath = Path.Combine(logsDir, "dump.log");

                using var sw = new StreamWriter(dumpPath, append: true);
                sw.WriteLine(new string('=', 80));
                sw.WriteLine($"CRASH REPORT - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sw.WriteLine(new string('=', 80));
                sw.WriteLine();
                sw.WriteLine($"Source: {source}");
                sw.WriteLine($"Exception Type: {ex?.GetType().FullName}");
                sw.WriteLine($"Message: {ex?.Message}");
                sw.WriteLine();
                sw.WriteLine("--- STACK TRACE ---");
                sw.WriteLine(ex?.StackTrace);
                
                // Log inner exceptions
                var inner = ex?.InnerException;
                while (inner != null)
                {
                    sw.WriteLine();
                    sw.WriteLine("--- INNER EXCEPTION ---");
                    sw.WriteLine($"Type: {inner.GetType().FullName}");
                    sw.WriteLine($"Message: {inner.Message}");
                    sw.WriteLine(inner.StackTrace);
                    inner = inner.InnerException;
                }

                sw.WriteLine();
                sw.WriteLine($"OS: {Environment.OSVersion}");
                sw.WriteLine($".NET Version: {Environment.Version}");
                sw.WriteLine();
                sw.WriteLine(new string('=', 80));
                sw.WriteLine();

                // Show message box to user
                MessageBox.Show(
                    $"An unexpected error occurred and has been logged to:\n{dumpPath}\n\nError: {ex?.Message}",
                    "REviewer - Crash Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // Swallow exceptions in crash logger to avoid recursion
            }
        }
    }
}
