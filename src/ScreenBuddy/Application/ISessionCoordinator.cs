using System;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Application
{
    /// <summary>
    /// Contract for managing application session state transitions and coordinating domain timer & UI events.
    /// </summary>
    public interface ISessionCoordinator : IDisposable
    {
        event EventHandler<SessionState>? SessionStateChanged;
        event EventHandler<BreakMessage>? BreakStarted;
        event EventHandler? BreakEnded;

        SessionState CurrentState { get; }
        AppSettings CurrentSettings { get; }

        bool Send(SessionCommand command);
        void HandleWakeFromSleep(TimerSnapshot? lastSnapshot);
    }
}
