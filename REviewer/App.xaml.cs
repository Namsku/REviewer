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
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            DumpException(e.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            DumpException(e.Exception, "Application.DispatcherUnhandledException");
            // Optionally set e.Handled = true to prevent app from closing immediately
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            DumpException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        }

        private void DumpException(Exception? ex, string source)
        {
            try
            {
                var dumpPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"CrashDump_REviewer_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                using var sw = new StreamWriter(dumpPath);
                sw.WriteLine($"[{DateTime.Now}] Exception Source: {source}");
                sw.WriteLine(ex?.ToString());

                // Dump MainWindow state if available
                if (Current?.MainWindow is REviewer.MainWindow mw)
                {
                    sw.WriteLine("\n--- MainWindow State ---");
                    var state = JsonConvert.SerializeObject(mw, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                            Error = (sender, args) => { args.ErrorContext.Handled = true; }
                        });
                    sw.WriteLine(state);
                }
            }
            catch
            {
                // Swallow exceptions in crash logger to avoid recursion
            }
        }
    }

}
