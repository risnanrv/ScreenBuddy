using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenBuddy.Application;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using ScreenBuddy.Infrastructure.Platform;
using ScreenBuddy.Presentation.Settings;

namespace ScreenBuddy.Presentation.Main
{
    public partial class MainWindowViewModel : ObservableObject, IDisposable
    {
        private readonly ISessionCoordinator _sessionCoordinator;
        private readonly ITimerEngine _timerEngine;
        private readonly ISettingsService _settingsService;
        private readonly IStartupRegistrar? _startupRegistrar;

        [ObservableProperty]
        private SessionState _currentState = SessionState.Stopped;

        [ObservableProperty]
        private string _statusHeader = "ScreenBuddy";

        [ObservableProperty]
        private string _statusSubtitle = "Your silent desktop companion for healthier screen time.";

        [ObservableProperty]
        private string _countdownText = "25:00";

        [ObservableProperty]
        private int _workDurationMinutes = 25;

        [ObservableProperty]
        private int _breakDurationMinutes = 5;

        [ObservableProperty]
        private bool _isStateStopped = true;

        [ObservableProperty]
        private bool _isStateWorking;

        [ObservableProperty]
        private bool _isStatePaused;

        [ObservableProperty]
        private bool _isStateBreak;

        [ObservableProperty]
        private bool _isStateSnoozed;

        public event EventHandler? RequestCloseToTray;

        public MainWindowViewModel(
            ISessionCoordinator sessionCoordinator,
            ITimerEngine timerEngine,
            ISettingsService settingsService,
            IStartupRegistrar? startupRegistrar = null)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _timerEngine = timerEngine ?? throw new ArgumentNullException(nameof(timerEngine));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _startupRegistrar = startupRegistrar;

            _workDurationMinutes = _settingsService.CurrentSettings.WorkDurationMinutes;
            _breakDurationMinutes = _settingsService.CurrentSettings.BreakDurationMinutes;

            _sessionCoordinator.SessionStateChanged += OnSessionStateChanged;
            _timerEngine.TimerTick += OnTimerTick;

            UpdateStateView(_sessionCoordinator.CurrentState);
        }

        private void OnSessionStateChanged(object? sender, SessionState newState)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateStateView(newState);
            });
        }

        private void OnTimerTick(object? sender, int remainingSeconds)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateRemainingTime(remainingSeconds);
            });
        }

        public void UpdateRemainingTime(int remainingSeconds)
        {
            int minutes = Math.Max(0, remainingSeconds / 60);
            int seconds = Math.Max(0, remainingSeconds % 60);
            CountdownText = $"{minutes:D2}:{seconds:D2}";
        }

        private void UpdateStateView(SessionState state)
        {
            CurrentState = state;

            IsStateStopped = state == SessionState.Stopped;
            IsStateWorking = state == SessionState.Working;
            IsStatePaused = state == SessionState.Paused;
            IsStateBreak = state == SessionState.Break;
            IsStateSnoozed = state == SessionState.Snoozed;

            switch (state)
            {
                case SessionState.Stopped:
                    StatusHeader = "ScreenBuddy";
                    StatusSubtitle = "Your silent desktop companion for healthier screen time.";
                    break;

                case SessionState.Working:
                    StatusHeader = "Work Session";
                    StatusSubtitle = "Focus time in progress";
                    UpdateRemainingTime(_timerEngine.RemainingSeconds);
                    break;

                case SessionState.Paused:
                    StatusHeader = "Session Paused";
                    StatusSubtitle = "Timer is currently paused";
                    UpdateRemainingTime(_timerEngine.RemainingSeconds);
                    break;

                case SessionState.Break:
                    StatusHeader = "Break Time";
                    StatusSubtitle = "Step away and rest your eyes";
                    UpdateRemainingTime(_timerEngine.RemainingSeconds);
                    break;

                case SessionState.Snoozed:
                    StatusHeader = "Break Snoozed";
                    StatusSubtitle = "Break postponed — starting soon";
                    UpdateRemainingTime(_timerEngine.RemainingSeconds);
                    break;
            }
        }

        partial void OnWorkDurationMinutesChanged(int value)
        {
            SaveSettings();
        }

        partial void OnBreakDurationMinutesChanged(int value)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            int validWork = Math.Clamp(WorkDurationMinutes, 1, 120);
            int validBreak = Math.Clamp(BreakDurationMinutes, 1, 60);

            var updated = new AppSettings
            {
                WorkDurationMinutes = validWork,
                BreakDurationMinutes = validBreak,
                LaunchOnStartup = _settingsService.CurrentSettings.LaunchOnStartup,
                ConfigVersion = _settingsService.CurrentSettings.ConfigVersion
            };

            _settingsService.UpdateSettings(updated);
        }

        [RelayCommand]
        private void StartSession()
        {
            SaveSettings();
            _sessionCoordinator.Send(SessionCommand.Start);
        }

        [RelayCommand]
        private void PauseSession()
        {
            _sessionCoordinator.Send(SessionCommand.Pause);
        }

        [RelayCommand]
        private void ResumeSession()
        {
            _sessionCoordinator.Send(SessionCommand.Resume);
        }

        [RelayCommand]
        private void StopSession()
        {
            _sessionCoordinator.Send(SessionCommand.Stop);
        }

        [RelayCommand]
        private void SkipBreak()
        {
            _sessionCoordinator.Send(SessionCommand.SkipBreak);
        }

        [RelayCommand]
        private void SnoozeBreak(string? minutes)
        {
            int m = 5;
            if (int.TryParse(minutes, out int parsed))
            {
                m = Math.Clamp(parsed, 1, 10);
            }
            _sessionCoordinator.Snooze(m);
        }

        [RelayCommand]
        private void NotNow()
        {
            RequestCloseToTray?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void OpenSettings()
        {
            var vm = new SettingsViewModel(_settingsService, _startupRegistrar);
            SettingsWindow.ShowSingleInstance(vm);
        }

        [RelayCommand]
        private void Quit()
        {
            _sessionCoordinator.Send(SessionCommand.Quit);
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _sessionCoordinator.SessionStateChanged -= OnSessionStateChanged;
            _timerEngine.TimerTick -= OnTimerTick;
        }
    }
}
