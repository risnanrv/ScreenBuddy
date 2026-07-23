using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using ScreenBuddy.Application;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using ScreenBuddy.Infrastructure.Platform;

namespace ScreenBuddy.Presentation.Overlay
{
    /// <summary>
    /// Manages per-monitor break overlay window lifecycle, Z-order enforcement, and display topology changes.
    /// </summary>
    public sealed class OverlayManager : IOverlayManager
    {
        private readonly ISessionCoordinator _sessionCoordinator;
        private readonly ITimerEngine _timerEngine;
        private readonly IDisplayMonitor _displayMonitor;

        private readonly List<BreakOverlayWindow> _activeWindows = new();
        private readonly DispatcherTimer _zOrderTimer;
        private BreakOverlayViewModel? _sharedViewModel;
        private BreakMessage? _currentMessage;

        public OverlayManager(
            ISessionCoordinator sessionCoordinator,
            ITimerEngine timerEngine,
            IDisplayMonitor displayMonitor)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _timerEngine = timerEngine ?? throw new ArgumentNullException(nameof(timerEngine));
            _displayMonitor = displayMonitor ?? throw new ArgumentNullException(nameof(displayMonitor));

            _sessionCoordinator.BreakStarted += OnBreakStarted;
            _sessionCoordinator.BreakEnded += OnBreakEnded;
            _timerEngine.TimerTick += OnTimerTick;
            _displayMonitor.DisplayConfigurationChanged += OnDisplayConfigurationChanged;

            _zOrderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _zOrderTimer.Tick += OnZOrderTimerTick;
        }

        public void ShowBreak(BreakMessage message)
        {
            _currentMessage = message;
            _sharedViewModel = new BreakOverlayViewModel(_sessionCoordinator);
            _sharedViewModel.SetMessage(message.Text);
            _sharedViewModel.UpdateCountdown(_timerEngine.RemainingSeconds);

            RecreateOverlayWindows();
            _zOrderTimer.Start();
        }

        public void HideBreak()
        {
            _zOrderTimer.Stop();

            List<BreakOverlayWindow> windowsToClose;
            lock (_activeWindows)
            {
                windowsToClose = new List<BreakOverlayWindow>(_activeWindows);
                _activeWindows.Clear();
            }

            Task.Run(async () =>
            {
                foreach (var window in windowsToClose)
                {
                    await window.Dispatcher.InvokeAsync(async () =>
                    {
                        await window.FadeOutAndCloseAsync();
                    });
                }
            });
        }

        public void UpdateRemainingTime(int remainingSeconds)
        {
            _sharedViewModel?.UpdateCountdown(remainingSeconds);
        }

        private void RecreateOverlayWindows()
        {
            lock (_activeWindows)
            {
                foreach (var win in _activeWindows)
                {
                    win.Close();
                }
                _activeWindows.Clear();

                if (_sharedViewModel == null)
                {
                    return;
                }

                var monitors = _displayMonitor.GetMonitors();
                foreach (var monitor in monitors)
                {
                    var window = new BreakOverlayWindow(_sharedViewModel)
                    {
                        WindowStartupLocation = System.Windows.WindowStartupLocation.Manual,
                        Left = monitor.Bounds.Left,
                        Top = monitor.Bounds.Top,
                        Width = monitor.Bounds.Width,
                        Height = monitor.Bounds.Height
                    };

                    _activeWindows.Add(window);
                    window.Show();
                }
            }
        }

        private void OnBreakStarted(object? sender, BreakMessage message)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ShowBreak(message);
            });
        }

        private void OnBreakEnded(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HideBreak();
            });
        }

        private void OnTimerTick(object? sender, int remainingSeconds)
        {
            if (_sessionCoordinator.CurrentState == SessionState.Break)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    UpdateRemainingTime(remainingSeconds);
                });
            }
        }

        private void OnDisplayConfigurationChanged(object? sender, EventArgs e)
        {
            if (_sessionCoordinator.CurrentState == SessionState.Break && _currentMessage != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RecreateOverlayWindows();
                });
            }
        }

        private void OnZOrderTimerTick(object? sender, EventArgs e)
        {
            lock (_activeWindows)
            {
                foreach (var win in _activeWindows)
                {
                    win.EnforceTopmostZOrder();
                }
            }
        }

        public void Dispose()
        {
            _sessionCoordinator.BreakStarted -= OnBreakStarted;
            _sessionCoordinator.BreakEnded -= OnBreakEnded;
            _timerEngine.TimerTick -= OnTimerTick;
            _displayMonitor.DisplayConfigurationChanged -= OnDisplayConfigurationChanged;
            _zOrderTimer.Stop();
            HideBreak();
        }
    }
}
