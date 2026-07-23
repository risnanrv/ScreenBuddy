namespace ScreenBuddy.Domain.Models
{
    /// <summary>
    /// Represents the high-level operational state of ScreenBuddy session.
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// Application timer is stopped or uninitialized.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Work countdown timer is active (invisible in system tray).
        /// </summary>
        Working = 1,

        /// <summary>
        /// Work session is manually paused by the user.
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Break period is active; fullscreen overlay is displayed.
        /// </summary>
        Break = 3
    }
}
