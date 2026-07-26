using System;
using System.Windows;

namespace ScreenBuddy.Presentation.Main
{
    /// <summary>
    /// Implements centralized MainWindow lifecycle management.
    /// Ensures single-instance window activation, foregrounding, and tray restoration.
    /// </summary>
    public sealed class MainWindowService : IMainWindowService
    {
        private readonly Func<MainWindow> _windowFactory;
        private MainWindow? _mainWindow;

        public bool IsVisible => _mainWindow != null && _mainWindow.IsVisible;

        public MainWindowService(Func<MainWindow> windowFactory)
        {
            _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        }

        public void Show()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                EnsureWindowCreated();
                _mainWindow!.Show();
                _mainWindow.Activate();
            });
        }

        public void Hide()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow?.Hide();
            });
        }

        public void Restore()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                EnsureWindowCreated();
                if (_mainWindow!.WindowState == WindowState.Minimized)
                {
                    _mainWindow.WindowState = WindowState.Normal;
                }
                _mainWindow.Show();
                _mainWindow.Activate();
            });
        }

        public void BringToForeground()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Restore();
                _mainWindow?.Focus();
            });
        }

        private void EnsureWindowCreated()
        {
            if (_mainWindow == null)
            {
                _mainWindow = _windowFactory();
                _mainWindow.Closed += (sender, args) => _mainWindow = null;
            }
        }

        public void Dispose()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindow?.Close();
                _mainWindow = null;
            });
        }
    }
}
