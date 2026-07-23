using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using ScreenBuddy.Infrastructure.Platform;

namespace ScreenBuddy.Presentation.Overlay
{
    public partial class BreakOverlayWindow : Window
    {
        private bool _isClosing;

        public BreakOverlayWindow(BreakOverlayViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Opacity = 0;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            EnforceTopmostZOrder();

            if (SystemParameters.ClientAreaAnimation)
            {
                var fadeIn = (Storyboard)FindResource("FadeInStoryboard");
                fadeIn.Begin(this);
            }
            else
            {
                Opacity = 1.0;
            }
        }

        public async Task FadeOutAndCloseAsync()
        {
            if (_isClosing)
            {
                return;
            }
            _isClosing = true;

            if (SystemParameters.ClientAreaAnimation)
            {
                var tcs = new TaskCompletionSource();
                var fadeOut = (Storyboard)FindResource("FadeOutStoryboard");
                EventHandler? completedHandler = null;
                completedHandler = (s, e) =>
                {
                    fadeOut.Completed -= completedHandler;
                    tcs.SetResult();
                };
                fadeOut.Completed += completedHandler;
                fadeOut.Begin(this);

                await tcs.Task;
            }

            Close();
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
            // Intercept Alt+F4 (System key F4) and Escape
            if ((e.Key == Key.System && e.SystemKey == Key.F4) || e.Key == Key.Escape)
            {
                e.Handled = true;
            }
        }
    }
}
