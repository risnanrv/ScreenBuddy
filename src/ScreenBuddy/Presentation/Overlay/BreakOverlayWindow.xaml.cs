using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using ScreenBuddy.Infrastructure.Platform;

namespace ScreenBuddy.Presentation.Overlay
{
    public partial class BreakOverlayWindow : Window
    {
        private DispatcherTimer? _escTimer;
        private DateTime _escPressedStart;

        public BreakOverlayWindow(BreakOverlayViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            EnforceTopmostZOrder();
            Focus();
        }

        public void EnforceTopmostZOrder()
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    NativeMethods.SetWindowPos(
                        hwnd,
                        NativeMethods.HWND_TOPMOST,
                        0, 0, 0, 0,
                        NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
                }
            }
            catch
            {
                // Fail-safe
            }
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Intercept Alt+F4 (System key F4)
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                e.Handled = true;

                if (_escTimer == null)
                {
                    _escPressedStart = DateTime.UtcNow;
                    _escTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(100)
                    };
                    _escTimer.Tick += OnEscTimerTick;
                    _escTimer.Start();
                }
            }
        }

        private void OnWindowPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                StopEscTimer();
            }
        }

        private void OnEscTimerTick(object? sender, EventArgs e)
        {
            if ((DateTime.UtcNow - _escPressedStart).TotalSeconds >= 2.5)
            {
                StopEscTimer();
                if (DataContext is BreakOverlayViewModel vm)
                {
                    vm.EmergencyEscapeCommand.Execute(null);
                }
            }
        }

        private void StopEscTimer()
        {
            if (_escTimer != null)
            {
                _escTimer.Stop();
                _escTimer.Tick -= OnEscTimerTick;
                _escTimer = null;
            }
        }
    }
}
