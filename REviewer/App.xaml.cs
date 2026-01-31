using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using REviewer.Core.Memory;
using REviewer.Services;
using REviewer.Services.Game;
using REviewer.Services.Processes;
using REviewer.Services.Configuration;
using REviewer.Services.Timer;
using REviewer.Services.Inventory;
using REviewer.Services.Challenge;
using REviewer.Services.Save;
using REviewer.Services.Enemy;
using REviewer.ViewModels;

namespace REviewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Set up Exception Handling e.g.
            this.DispatcherUnhandledException += (s, e) => HandleException(e.Exception, "Dispatcher");
            AppDomain.CurrentDomain.UnhandledException += (s, e) => HandleException(e.ExceptionObject as Exception, "AppDomain");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Core
            services.AddSingleton<IMemoryMonitor, MemoryMonitor>();

            // Services
            services.AddSingleton<IGameStateService, GameStateService>();
            services.AddSingleton<ITimerService, TimerService>();
            services.AddSingleton<IInventoryService, InventoryService>();
            services.AddSingleton<IChallengeService, ChallengeService>();
            services.AddSingleton<ISaveService, SaveService>();
            services.AddSingleton<IEnemyTrackingService, EnemyTrackingService>();
            services.AddSingleton<GameMemoryService>();
            services.AddSingleton<ProcessWatcherService>();
            services.AddSingleton<ConfigurationService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<SRTViewModel>();
            services.AddTransient<TrackerViewModel>();
            services.AddTransient<OverlayViewModel>();
            
            // Windows
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = ServiceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();
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
                System.Windows.MessageBox.Show(
                    $"An unexpected error occurred and has been logged to:\n{dumpPath}\n\nError: {ex?.Message}",
                    "REviewer - Crash Report",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            catch
            {
                // Swallow exceptions in crash logger to avoid recursion
            }
        }
    }
}
