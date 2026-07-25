using System;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Domain.Timer
{
    /// <summary>
    /// Contract for the dual-clock timer engine.
    /// </summary>
    public interface ITimerEngine : IDisposable
    {
        event EventHandler<int>? TimerTick;
        event EventHandler? WorkTimerExpired;
        event EventHandler? BreakTimerExpired;
        event EventHandler? SnoozeTimerExpired;
        event EventHandler<TimerSnapshot>? PersistStateRequested;

        SessionState CurrentPhase { get; }
        int RemainingSeconds { get; }
        bool IsRunning { get; }

        void Start(int workDurationSeconds);
        void Pause();
        void Resume();
        void Reset(int workDurationSeconds);
        void BeginBreak(int breakDurationSeconds);
        void EndBreak();
        void Snooze(int snoozeDurationSeconds);
        TimerSnapshot CreateSnapshot();
        void ReconcileSleepGap(TimerSnapshot lastSnapshot);
    }
}
