using System;
using Microsoft.Extensions.Logging;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Messages;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;

namespace ScreenBuddy.Application
{
    /// <summary>
    /// Coordinates session state transitions, timer engine events, message rotation, and settings.
    /// Implements the central application state machine specified in Phase 2 §6.
    /// </summary>
    public sealed class SessionCoordinator : ISessionCoordinator
    {
        private readonly ITimerEngine _timerEngine;
        private readonly ISettingsService _settingsService;
        private readonly IMessageLibrary _messageLibrary;
        private readonly ILogger<SessionCoordinator>? _logger;
        private readonly object _lock = new();

        public event EventHandler<SessionState>? SessionStateChanged;
        public event EventHandler<BreakMessage>? BreakStarted;
        public event EventHandler? BreakEnded;

        public SessionState CurrentState => _timerEngine.CurrentPhase;
        public AppSettings CurrentSettings => _settingsService.CurrentSettings;

        public SessionCoordinator(
            ITimerEngine timerEngine,
            ISettingsService settingsService,
            IMessageLibrary messageLibrary,
            ILogger<SessionCoordinator>? logger = null)
        {
            _timerEngine = timerEngine ?? throw new ArgumentNullException(nameof(timerEngine));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _messageLibrary = messageLibrary ?? throw new ArgumentNullException(nameof(messageLibrary));
            _logger = logger;

            _timerEngine.WorkTimerExpired += OnWorkTimerExpired;
            _timerEngine.BreakTimerExpired += OnBreakTimerExpired;
        }

        public bool Send(SessionCommand command)
        {
            lock (_lock)
            {
                SessionState currentState = _timerEngine.CurrentPhase;
                _logger?.LogInformation("SessionCoordinator received command {Command} in state {State}", command, currentState);

                switch (command)
                {
                    case SessionCommand.Start:
                        if (currentState is SessionState.Stopped or SessionState.Paused)
                        {
                            StartWorkSessionInternal();
                            return true;
                        }
                        break;

                    case SessionCommand.Pause:
                        if (currentState == SessionState.Working)
                        {
                            _timerEngine.Pause();
                            OnSessionStateChanged(SessionState.Paused);
                            return true;
                        }
                        break;

                    case SessionCommand.Resume:
                        if (currentState == SessionState.Paused)
                        {
                            _timerEngine.Resume();
                            OnSessionStateChanged(SessionState.Working);
                            return true;
                        }
                        break;

                    case SessionCommand.Reset:
                        if (currentState is SessionState.Working or SessionState.Paused)
                        {
                            StartWorkSessionInternal();
                            return true;
                        }
                        break;

                    case SessionCommand.SkipBreak:
                        if (currentState == SessionState.Break)
                        {
                            _timerEngine.EndBreak();
                            OnBreakEnded();
                            StartWorkSessionInternal();
                            return true;
                        }
                        break;

                    case SessionCommand.Quit:
                        _timerEngine.EndBreak();
                        OnSessionStateChanged(SessionState.Stopped);
                        return true;
                }

                _logger?.LogWarning("Invalid command {Command} for current state {State}", command, currentState);
                return false;
            }
        }

        public void HandleWakeFromSleep(TimerSnapshot? lastSnapshot)
        {
            if (lastSnapshot == null)
            {
                return;
            }

            lock (_lock)
            {
                _logger?.LogInformation("Reconciling sleep gap for phase {Phase}", lastSnapshot.Phase);
                _timerEngine.ReconcileSleepGap(lastSnapshot);
                OnSessionStateChanged(_timerEngine.CurrentPhase);
            }
        }

        private void StartWorkSessionInternal()
        {
            int workSecs = CurrentSettings.WorkDurationMinutes * 60;
            _timerEngine.Start(workSecs);
            OnSessionStateChanged(SessionState.Working);
        }

        private void OnWorkTimerExpired(object? sender, EventArgs e)
        {
            BreakMessage message;
            lock (_lock)
            {
                message = _messageLibrary.GetNextMessage();
                int breakSecs = CurrentSettings.BreakDurationMinutes * 60;
                _timerEngine.BeginBreak(breakSecs);
                OnSessionStateChanged(SessionState.Break);
            }

            BreakStarted?.Invoke(this, message);
        }

        private void OnBreakTimerExpired(object? sender, EventArgs e)
        {
            lock (_lock)
            {
                _timerEngine.EndBreak();
                OnBreakEnded();
                StartWorkSessionInternal();
            }
        }

        private void OnSessionStateChanged(SessionState newState)
        {
            SessionStateChanged?.Invoke(this, newState);
        }

        private void OnBreakEnded()
        {
            BreakEnded?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _timerEngine.WorkTimerExpired -= OnWorkTimerExpired;
            _timerEngine.BreakTimerExpired -= OnBreakTimerExpired;
            _timerEngine.Dispose();
        }
    }
}
