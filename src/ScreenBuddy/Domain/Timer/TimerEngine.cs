using System;
using System.Diagnostics;
using System.Threading;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Domain.Timer
{
    /// <summary>
    /// Implements the hybrid Stopwatch + DateTimeOffset dual-clock timer engine.
    /// Thread-safe and resilient to system sleep/wake and clock adjustments.
    /// </summary>
    public sealed class TimerEngine : ITimerEngine
    {
        private readonly IClock _clock;
        private readonly object _lock = new();

        private System.Threading.Timer? _tickTimer;
        private System.Threading.Timer? _persistTimer;
        private Stopwatch _stopwatch = new();

        private SessionState _currentPhase = SessionState.Stopped;
        private int _configuredWorkSeconds;
        private int _configuredBreakSeconds;
        private int _totalTargetSeconds;
        private int _elapsedBeforePause;
        private int _lastReportedRemaining;

        public event EventHandler<int>? TimerTick;
        public event EventHandler? WorkTimerExpired;
        public event EventHandler? BreakTimerExpired;
        public event EventHandler<TimerSnapshot>? PersistStateRequested;

        public SessionState CurrentPhase
        {
            get { lock (_lock) return _currentPhase; }
        }

        public bool IsRunning
        {
            get { lock (_lock) return _stopwatch.IsRunning; }
        }

        public int RemainingSeconds
        {
            get
            {
                lock (_lock)
                {
                    return CalculateRemainingSeconds();
                }
            }
        }

        public TimerEngine(IClock? clock = null)
        {
            _clock = clock ?? new ClockAdapter();
        }

        public void Start(int workDurationSeconds)
        {
            lock (_lock)
            {
                StopTimersInternal();

                _configuredWorkSeconds = Math.Max(1, workDurationSeconds);
                _totalTargetSeconds = _configuredWorkSeconds;
                _elapsedBeforePause = 0;
                _currentPhase = SessionState.Working;
                _lastReportedRemaining = _totalTargetSeconds;

                _stopwatch.Restart();
                StartTimersInternal();
            }

            OnTimerTick(_totalTargetSeconds);
            OnPersistRequested();
        }

        public void Pause()
        {
            lock (_lock)
            {
                if (_currentPhase != SessionState.Working || !_stopwatch.IsRunning)
                {
                    return;
                }

                _elapsedBeforePause += (int)_stopwatch.Elapsed.TotalSeconds;
                _stopwatch.Stop();
                _currentPhase = SessionState.Paused;
                StopTimersInternal();
            }

            OnPersistRequested();
        }

        public void Resume()
        {
            lock (_lock)
            {
                if (_currentPhase != SessionState.Paused)
                {
                    return;
                }

                _currentPhase = SessionState.Working;
                _stopwatch.Restart();
                StartTimersInternal();
            }

            OnPersistRequested();
        }

        public void Reset(int workDurationSeconds)
        {
            Start(workDurationSeconds);
        }

        public void BeginBreak(int breakDurationSeconds)
        {
            lock (_lock)
            {
                StopTimersInternal();

                _configuredBreakSeconds = Math.Max(1, breakDurationSeconds);
                _totalTargetSeconds = _configuredBreakSeconds;
                _elapsedBeforePause = 0;
                _currentPhase = SessionState.Break;
                _lastReportedRemaining = _totalTargetSeconds;

                _stopwatch.Restart();
                StartTimersInternal();
            }

            OnTimerTick(_totalTargetSeconds);
            OnPersistRequested();
        }

        public void EndBreak()
        {
            lock (_lock)
            {
                StopTimersInternal();
                _stopwatch.Reset();
                _currentPhase = SessionState.Stopped;
                _elapsedBeforePause = 0;
            }
        }

        public TimerSnapshot CreateSnapshot()
        {
            lock (_lock)
            {
                return new TimerSnapshot
                {
                    Phase = _currentPhase,
                    RemainingSeconds = CalculateRemainingSeconds(),
                    SnapshotUtc = _clock.UtcNow,
                    ConfiguredWorkSeconds = _configuredWorkSeconds,
                    ConfiguredBreakSeconds = _configuredBreakSeconds
                };
            }
        }

        public void ReconcileSleepGap(TimerSnapshot lastSnapshot)
        {
            ArgumentNullException.ThrowIfNull(lastSnapshot);

            bool expireWork = false;
            bool expireBreak = false;
            int newRemaining = 0;

            lock (_lock)
            {
                if (lastSnapshot.Phase is SessionState.Stopped or SessionState.Paused)
                {
                    return;
                }

                int elapsedWallSeconds = (int)(_clock.UtcNow - lastSnapshot.SnapshotUtc).TotalSeconds;
                if (elapsedWallSeconds < 0)
                {
                    elapsedWallSeconds = 0;
                }

                int adjustedRemaining = lastSnapshot.RemainingSeconds - elapsedWallSeconds;

                _configuredWorkSeconds = lastSnapshot.ConfiguredWorkSeconds;
                _configuredBreakSeconds = lastSnapshot.ConfiguredBreakSeconds;
                _currentPhase = lastSnapshot.Phase;

                if (adjustedRemaining <= 0)
                {
                    if (lastSnapshot.Phase == SessionState.Working)
                    {
                        expireWork = true;
                    }
                    else if (lastSnapshot.Phase == SessionState.Break)
                    {
                        expireBreak = true;
                    }

                    StopTimersInternal();
                    _stopwatch.Reset();
                }
                else
                {
                    _totalTargetSeconds = lastSnapshot.Phase == SessionState.Working ? _configuredWorkSeconds : _configuredBreakSeconds;
                    _elapsedBeforePause = _totalTargetSeconds - adjustedRemaining;
                    newRemaining = adjustedRemaining;

                    _stopwatch.Restart();
                    StartTimersInternal();
                }
            }

            if (expireWork)
            {
                OnWorkTimerExpired();
            }
            else if (expireBreak)
            {
                OnBreakTimerExpired();
            }
            else
            {
                OnTimerTick(newRemaining);
            }
        }

        private int CalculateRemainingSeconds()
        {
            if (_currentPhase == SessionState.Stopped)
            {
                return 0;
            }

            int elapsedSeconds = _elapsedBeforePause + (_stopwatch.IsRunning ? (int)_stopwatch.Elapsed.TotalSeconds : 0);
            int remaining = _totalTargetSeconds - elapsedSeconds;
            return Math.Max(0, remaining);
        }

        private void OnTickCallback(object? state)
        {
            bool workExpired = false;
            bool breakExpired = false;
            int remaining = 0;

            lock (_lock)
            {
                if (!_stopwatch.IsRunning)
                {
                    return;
                }

                remaining = CalculateRemainingSeconds();
                _lastReportedRemaining = remaining;

                if (remaining <= 0)
                {
                    StopTimersInternal();
                    _stopwatch.Reset();

                    if (_currentPhase == SessionState.Working)
                    {
                        workExpired = true;
                    }
                    else if (_currentPhase == SessionState.Break)
                    {
                        breakExpired = true;
                    }
                }
            }

            OnTimerTick(remaining);

            if (workExpired)
            {
                OnWorkTimerExpired();
            }
            else if (breakExpired)
            {
                OnBreakTimerExpired();
            }
        }

        private void OnPersistCallback(object? state)
        {
            OnPersistRequested();
        }

        private void StartTimersInternal()
        {
            _tickTimer = new System.Threading.Timer(OnTickCallback, null, 1000, 1000);
            _persistTimer = new System.Threading.Timer(OnPersistCallback, null, 30000, 30000);
        }

        private void StopTimersInternal()
        {
            _tickTimer?.Dispose();
            _tickTimer = null;

            _persistTimer?.Dispose();
            _persistTimer = null;

            _stopwatch.Stop();
        }

        private void OnTimerTick(int remaining)
        {
            TimerTick?.Invoke(this, remaining);
        }

        private void OnWorkTimerExpired()
        {
            WorkTimerExpired?.Invoke(this, EventArgs.Empty);
        }

        private void OnBreakTimerExpired()
        {
            BreakTimerExpired?.Invoke(this, EventArgs.Empty);
        }

        private void OnPersistRequested()
        {
            PersistStateRequested?.Invoke(this, CreateSnapshot());
        }

        public void Dispose()
        {
            lock (_lock)
            {
                StopTimersInternal();
            }
        }
    }
}
