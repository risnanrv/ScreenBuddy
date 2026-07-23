using System;

namespace ScreenBuddy.Domain.Timer
{
    /// <summary>
    /// Abstraction for system time getter, enabling deterministic testing.
    /// </summary>
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }

    /// <summary>
    /// Real system clock adapter.
    /// </summary>
    public sealed class ClockAdapter : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
