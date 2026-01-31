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
using System.Windows.Media;
using System.Windows.Threading;
using REviewer.Core.Configuration;

namespace REviewer
{
    public partial class Overlay : Window, INotifyPropertyChanged
    {
        private readonly Process _targetProcess;
        private readonly DispatcherTimer _positionTimer = new();
        public readonly Config OverlayConfig;
        
        private int _cornerPositionValue;
        public int _cornerPosition
        {
            get => _cornerPositionValue;
            set
            {
                if (_cornerPositionValue != value)
                {
                    _cornerPositionValue = value;
                    if (IsLoaded)
                    {
                        UpdateOverlayMargins();
                    }
                }
            }
        }

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

        private double _overlayCanvasScale = 1.0; // Track the current scale of OverlayCanvas

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

            // Apply initial scaling from OverlayConfig
            ApplyScaling();
        }

        public void ApplyScaling()
        {
            double scalingFactor = Math.Clamp(OverlayConfig.scaling / 100.0, 1.0, 3.0);
            _overlayCanvasScale = scalingFactor;

            // Apply the scale to OverlayCanvas using RenderTransform
            if (MainOverlayGroupScale != null)
            {
                MainOverlayGroupScale.ScaleX = _overlayCanvasScale;
                MainOverlayGroupScale.ScaleY = _overlayCanvasScale;
            }
            if (OverlayEnemyGroupScale != null)
            {
                OverlayEnemyGroupScale.ScaleX = _overlayCanvasScale;
                OverlayEnemyGroupScale.ScaleY = _overlayCanvasScale;
            }

            // Force the OverlayCanvas to update its layout
            OverlayCanvas?.UpdateLayout();
        }

        private void UpdateOverlayCanvasLayout()
        {
            OverlayCanvas.UpdateLayout();
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

                double scaledOverlayWidth = OverlayGroup.ActualWidth * _overlayCanvasScale;
                double scaledOverlayHeight = OverlayGroup.ActualHeight * _overlayCanvasScale;

                // Corner positions: 0=BottomRight, 1=TopRight, 2=TopLeft, 3=BottomLeft
                double x = _cornerPosition switch
                {
                    0 => this.Width - scaledOverlayWidth - margin_x,  // Bottom-Right
                    1 => this.Width - scaledOverlayWidth - margin_x,  // Top-Right
                    2 => margin_x,                                     // Top-Left
                    3 => margin_x,                                     // Bottom-Left
                    _ => this.Width - scaledOverlayWidth - margin_x
                };

                double y = _cornerPosition switch
                {
                    0 => this.Height - scaledOverlayHeight - margin_y, // Bottom-Right
                    1 => margin_y,                                      // Top-Right
                    2 => margin_y,                                      // Top-Left
                    3 => this.Height - scaledOverlayHeight - margin_y, // Bottom-Left
                    _ => this.Height - scaledOverlayHeight - margin_y
                };

                Canvas.SetLeft(OverlayGroup, x);
                Canvas.SetTop(OverlayGroup, y);

                OverlayEnemyGroup.UpdateLayout();

                double enemyMarginX = 5;
                double enemyMarginY = 0;

                double scaledEnemyWidth = OverlayEnemyGroup.ActualWidth * _overlayCanvasScale;
                double scaledEnemyHeight = OverlayEnemyGroup.ActualHeight * _overlayCanvasScale;

                // Enemy tracker goes on the OPPOSITE horizontal side of main overlay
                // Main at right (0,1) -> Enemy at left; Main at left (2,3) -> Enemy at right
                double enemyX = _cornerPosition switch
                {
                    0 => enemyMarginX,                                    // Main Bottom-Right -> Enemy Left
                    1 => enemyMarginX,                                    // Main Top-Right -> Enemy Left
                    2 => this.Width - scaledEnemyWidth - enemyMarginX,   // Main Top-Left -> Enemy Right
                    3 => this.Width - scaledEnemyWidth - enemyMarginX,   // Main Bottom-Left -> Enemy Right
                    _ => enemyMarginX
                };
                // Enemy tracker stays on same vertical side as main overlay
                double enemyY = _cornerPosition switch
                {
                    0 => this.Height - scaledEnemyHeight - enemyMarginY, // Bottom-Right -> Bottom
                    1 => enemyMarginY,                                    // Top-Right -> Top
                    2 => enemyMarginY,                                    // Top-Left -> Top
                    3 => this.Height - scaledEnemyHeight - enemyMarginY, // Bottom-Left -> Bottom
                    _ => this.Height - scaledEnemyHeight - enemyMarginY
                };

                Canvas.SetLeft(OverlayEnemyGroup, enemyX);
                Canvas.SetTop(OverlayEnemyGroup, enemyY);
            }
        }

        private void UpdateOverlayMargins()
        {
            // Corner positions: 0=BottomRight, 1=TopRight, 2=TopLeft, 3=BottomLeft
            bool isLeft = _cornerPosition == 2 || _cornerPosition == 3;
            bool isTop = _cornerPosition == 1 || _cornerPosition == 2;

            // Swap order of children for left-side positions (icon first, then bars)
            if (MainOverlayStack != null && MainOverlayStack.Children.Count >= 2)
            {
                var timerBar = MainOverlayStack.Children[0];
                var weaponIcon = MainOverlayStack.Children[1];

                // Check current order and swap if needed
                bool currentlySwapped = MainOverlayStack.Children[0] == WeaponIconBorder;
                
                if (isLeft && !currentlySwapped)
                {
                    // Swap: Icon first, then timer bar
                    MainOverlayStack.Children.Clear();
                    MainOverlayStack.Children.Add(WeaponIconBorder);
                    MainOverlayStack.Children.Add(TweakedOverlay);
                    TweakedOverlay.Margin = new Thickness(6, 0, 0, 0);
                    WeaponIconBorder.Margin = new Thickness(0);
                }
                else if (!isLeft && currentlySwapped)
                {
                    // Swap back: Timer bar first, then icon
                    MainOverlayStack.Children.Clear();
                    MainOverlayStack.Children.Add(TweakedOverlay);
                    MainOverlayStack.Children.Add(WeaponIconBorder);
                    TweakedOverlay.Margin = new Thickness(0, 0, 6, 0);
                    WeaponIconBorder.Margin = new Thickness(0);
                }
            }

            // Adjust vertical alignment based on top/bottom
            if (isTop)
            {
                // Top corners: align timer bar with TOP of weapon icon
                TweakedOverlay.VerticalAlignment = VerticalAlignment.Top;
                WeaponIconBorder.VerticalAlignment = VerticalAlignment.Top;
            }
            else
            {
                // Bottom corners: align timer bar with BOTTOM of weapon icon
                TweakedOverlay.VerticalAlignment = VerticalAlignment.Bottom;
                WeaponIconBorder.VerticalAlignment = VerticalAlignment.Bottom;
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
