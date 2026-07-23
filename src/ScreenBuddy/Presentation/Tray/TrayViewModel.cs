using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenBuddy.Application;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Presentation.Tray
{
    public partial class TrayViewModel : ObservableObject, IDisposable
    {
        private readonly ISessionCoordinator _sessionCoordinator;

        [ObservableProperty]
        private string _statusText = "ScreenBuddy — Ready";

        [ObservableProperty]
        private bool _canPause;

        [ObservableProperty]
        private bool _canResume;

        [ObservableProperty]
        private bool _canStart = true;

        [ObservableProperty]
        private bool _canReset;

        public event EventHandler? OpenSettingsRequested;
        public event EventHandler? ExitRequested;

        public TrayViewModel(ISessionCoordinator sessionCoordinator)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _sessionCoordinator.SessionStateChanged += OnSessionStateChanged;

            UpdateStateFlags(_sessionCoordinator.CurrentState);
        }

        private void OnSessionStateChanged(object? sender, SessionState state)
        {
            UpdateStateFlags(state);
        }

        public void UpdateRemainingTime(int remainingSeconds)
        {
            int minutes = remainingSeconds / 60;
            int seconds = remainingSeconds % 60;

            SessionState state = _sessionCoordinator.CurrentState;
            StatusText = state switch
            {
                SessionState.Working => $"Working — {minutes:D2}:{seconds:D2} remaining",
                SessionState.Paused => $"Paused — {minutes:D2}:{seconds:D2} remaining",
                SessionState.Break => $"On Break — {minutes:D2}:{seconds:D2} remaining",
                _ => "ScreenBuddy — Ready"
            };
        }

        private void UpdateStateFlags(SessionState state)
        {
            CanPause = state == SessionState.Working;
            CanResume = state == SessionState.Paused;
            CanStart = state is SessionState.Stopped or SessionState.Paused;
            CanReset = state is SessionState.Working or SessionState.Paused;

            StatusText = state switch
            {
                SessionState.Working => "Working",
                SessionState.Paused => "Paused",
                SessionState.Break => "On Break",
                _ => "ScreenBuddy — Ready"
            };
        }

        [RelayCommand]
        private void Start() => _sessionCoordinator.Send(SessionCommand.Start);

        [RelayCommand]
        private void Pause() => _sessionCoordinator.Send(SessionCommand.Pause);

        [RelayCommand]
        private void Resume() => _sessionCoordinator.Send(SessionCommand.Resume);

        [RelayCommand]
        private void Reset() => _sessionCoordinator.Send(SessionCommand.Reset);

        [RelayCommand]
        private void TogglePauseResume()
        {
            if (_sessionCoordinator.CurrentState == SessionState.Working)
            {
                Pause();
            }
            else if (_sessionCoordinator.CurrentState == SessionState.Paused)
            {
                Resume();
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            OpenSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Quit()
        {
            _sessionCoordinator.Send(SessionCommand.Quit);
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _sessionCoordinator.SessionStateChanged -= OnSessionStateChanged;
        }
    }
}
