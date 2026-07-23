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
        /// Pauses the active work session.
        /// </summary>
        Pause,

        /// <summary>
        /// Resumes a paused work session.
        /// </summary>
        Resume,

        /// <summary>
        /// Resets the work session back to full duration.
        /// </summary>
        Reset,

        /// <summary>
        /// Skips an active break session and immediately starts a new work session.
        /// </summary>
        SkipBreak,

        /// <summary>
        /// Gracefully shuts down the session.
        /// </summary>
        Quit
    }
}
