namespace ScreenBuddy.Domain.Models
{
    /// <summary>
    /// Immutable record representing user-configured application settings.
    /// </summary>
    public sealed record AppSettings
    {
        public int WorkDurationMinutes { get; init; } = 25;
        public int BreakDurationMinutes { get; init; } = 5;
        public bool LaunchOnStartup { get; init; } = true;
        public int ConfigVersion { get; init; } = 1;

        public static AppSettings Default => new AppSettings();
    }
}
