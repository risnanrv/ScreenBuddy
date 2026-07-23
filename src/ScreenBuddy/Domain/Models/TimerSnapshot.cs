using System;

namespace ScreenBuddy.Domain.Models
{
    /// <summary>
    /// Value object representing a point-in-time snapshot of the timer for persistence and sleep/wake reconciliation.
    /// </summary>
    public sealed record TimerSnapshot
    {
        public SessionState Phase { get; init; }
        public int RemainingSeconds { get; init; }
        public DateTimeOffset SnapshotUtc { get; init; }
        public int ConfiguredWorkSeconds { get; init; }
        public int ConfiguredBreakSeconds { get; init; }
    }
}
