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

        public event EventHandler? OpenMainWindowRequested;
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
            int minutes = Math.Max(0, remainingSeconds / 60);
            int seconds = Math.Max(0, remainingSeconds % 60);

            SessionState state = _sessionCoordinator.CurrentState;
            StatusText = state switch
            {
                SessionState.Working => $"Working — {minutes:D2}:{seconds:D2} remaining",
                SessionState.Paused => "ScreenBuddy is Paused",
                SessionState.Snoozed => $"Snoozed — {minutes:D2}:{seconds:D2} remaining",
                SessionState.Break => $"On Break — {minutes:D2}:{seconds:D2} remaining",
                _ => "ScreenBuddy — Ready"
            };
        }

        private void UpdateStateFlags(SessionState state)
        {
            CanPause = state is SessionState.Working or SessionState.Break or SessionState.Snoozed;
            CanResume = state == SessionState.Paused;

            StatusText = state switch
            {
                SessionState.Working => "Working",
                SessionState.Paused => "ScreenBuddy is Paused",
                SessionState.Snoozed => "Break Snoozed",
                SessionState.Break => "On Break",
                _ => "ScreenBuddy — Ready"
            };
        }

        [RelayCommand]
        private void OpenMainWindow()
        {
            OpenMainWindowRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void TogglePauseResume()
        {
            if (_sessionCoordinator.CurrentState is SessionState.Working or SessionState.Break or SessionState.Snoozed)
            {
                _sessionCoordinator.Send(SessionCommand.Pause);
            }
            else if (_sessionCoordinator.CurrentState == SessionState.Paused)
            {
                _sessionCoordinator.Send(SessionCommand.Resume);
            }
            else
            {
                OpenMainWindowRequested?.Invoke(this, EventArgs.Empty);
            }
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
