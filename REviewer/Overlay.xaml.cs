using REviewer.Modules.RE.Common;
using REviewer.Modules.Utils;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Threading;

namespace REviewer
{
    public partial class Overlay : Window, INotifyPropertyChanged
    {
        private readonly Process _targetProcess;
        private readonly DispatcherTimer _positionTimer = new();
        public readonly Config OverlayConfig;
        public int _cornerPosition;

        private int _overlayFontSize = 16; 
        public int OverlayFontSize
        {
            get => _overlayFontSize;
            set
            {
                _overlayFontSize = value;
                OnPropertyChanged(nameof(OverlayFontSize));
            }
        }

        private RootObject? _gameData; // Reference to RootObject

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public Overlay(Process targetProcess, Config overlayConfig, RootObject gameData)
        {
            InitializeComponent();
            DataContext = this;
            _targetProcess = targetProcess;
            _cornerPosition = overlayConfig._position;
            OverlayFontSize = overlayConfig._size;
            OverlayConfig = overlayConfig;
            _gameData = gameData; // Initialize RootObject reference

            _positionTimer.Interval = TimeSpan.FromMilliseconds(100);
            _positionTimer.Tick += PositionTimer_Tick;

            DataContext = this._gameData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_targetProcess == null || _targetProcess.MainWindowHandle == IntPtr.Zero)
            {
                MessageBox.Show("Target process not found.");
                Close();
                return;
            }

            MakeClickThrough();
            _positionTimer.Start();

            // Update margins based on _cornerPosition
            UpdateOverlayMargins();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Ensure proper cleanup when the Overlay window is closed
            _positionTimer?.Stop();
            Logger.Instance.Info("Overlay window has been closed.");
        }

        private void PositionTimer_Tick(object? sender, EventArgs e)
        {
            if (_targetProcess.HasExited || _targetProcess.MainWindowHandle == IntPtr.Zero)
            {
                this.Hide();
                return;
            }

            IntPtr hwnd = _targetProcess.MainWindowHandle;

            // Check if the game window is minimized
            if (IsIconic(hwnd))
            {
                this.Hide();
                return;
            }

            // Check if the game window is the foreground window
            IntPtr foreground = GetForegroundWindow();
            if (foreground != hwnd)
            {
                this.Hide();
                return;
            }

            // Otherwise, show and update overlay position
            if (!this.IsVisible)
                this.Show();

            if (GetClientRect(hwnd, out RECT rect))
            {
                POINT topLeft = new POINT { X = 0, Y = 0 };
                ClientToScreen(hwnd, ref topLeft);

                this.Left = ConvertPixelsToDIPsX(topLeft.X);
                this.Top = ConvertPixelsToDIPsY(topLeft.Y);
                this.Width = ConvertPixelsToDIPsX(rect.Right - rect.Left);
                this.Height = ConvertPixelsToDIPsY(rect.Bottom - rect.Top);

                OverlayCanvas.UpdateLayout();
                OverlayGroup.UpdateLayout();

                double margin_x = 5;
                double margin_y = 0;

                double x = _cornerPosition switch
                {
                    0 => this.Width - OverlayGroup.ActualWidth - margin_x, // bottom right
                    1 => this.Width - OverlayGroup.ActualWidth - margin_x, // top right
                    _ => this.Width - OverlayGroup.ActualWidth - margin_x
                };

                double y = _cornerPosition switch
                {
                    0 => this.Height - OverlayGroup.ActualHeight - margin_y, // bottom right
                    1 => margin_y,                                           // top right
                    _ => this.Height - OverlayGroup.ActualHeight - margin_y
                };

                Canvas.SetLeft(OverlayGroup, x);
                Canvas.SetTop(OverlayGroup, y);

                OverlayEnemyGroup.UpdateLayout();

                double enemyMarginX = 5;
                double enemyMarginY = 0;

                double enemyX, enemyY;

                switch (_cornerPosition)
                {
                    case 0: // bottom right
                        enemyX = enemyMarginX; // bottom left
                        enemyY = this.Height - OverlayEnemyGroup.ActualHeight - enemyMarginY;
                        break;
                    case 1: // top right
                        enemyX = enemyMarginX; // top left
                        enemyY = enemyMarginY;
                        break;
                    default: // fallback to bottom left
                        enemyX = enemyMarginX;
                        enemyY = this.Height - OverlayEnemyGroup.ActualHeight - enemyMarginY;
                        break;
                }

                Canvas.SetLeft(OverlayEnemyGroup, enemyX);
                Canvas.SetTop(OverlayEnemyGroup, enemyY);
            }
        }

        private void UpdateOverlayMargins()
        {
            if (_cornerPosition == 0)
            {
                TweakedOverlay.Margin = new Thickness(0, 0, 8, -24);
            }
            else if (_cornerPosition == 1)
            {
                TweakedOverlay.Margin = new Thickness(0, 0, 8, 24);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd); // Checks if minimized

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow(); // Checks if active

        private void MakeClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, style | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT);
        }

        private double ConvertPixelsToDIPsX(double pixels)
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget?.TransformFromDevice.M11 * pixels ?? pixels;
        }

        private double ConvertPixelsToDIPsY(double pixels)
        {
            var source = PresentationSource.FromVisual(this);
            return source?.CompositionTarget?.TransformFromDevice.M22 * pixels ?? pixels;
        }

        #region Win32 Interop

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_TRANSPARENT = 0x00000020;

        #endregion
    }
}
