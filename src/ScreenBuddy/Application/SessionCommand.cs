namespace ScreenBuddy.Application
{
    /// <summary>
    /// Commands that trigger session state machine transitions.
    /// </summary>
    public enum SessionCommand
    {
        /// <summary>
        /// Starts a fresh work session from configured duration.
        /// </summary>
        Start,

        /// <summary>
        /// Pauses the active session.
        /// </summary>
        Pause,

        /// <summary>
        /// Resumes a paused session.
        /// </summary>
        Resume,

        /// <summary>
        /// Stops the session, cancels all active timers/breaks, and transitions to Stopped state.
        /// </summary>
        Stop,

        /// <summary>
        /// Resets the work session back to full duration.
        /// </summary>
        Reset,

        /// <summary>
        /// Skips an active break session and immediately starts a new work session.
        /// </summary>
        SkipBreak,

        /// <summary>
        /// Postpones (snoozes) an active break for a specified duration in minutes.
        /// </summary>
        SnoozeBreak,

        /// <summary>
        /// Hides the break overlay windows while allowing the break countdown to continue in the background.
        /// </summary>
        MinimizeBreak,

        /// <summary>
        /// Emergency override that immediately dismisses all overlays and transitions to Paused state.
        /// </summary>
        EmergencyEscape,

        /// <summary>
        /// Gracefully shuts down the session.
        /// </summary>
        Quit
    }
}
