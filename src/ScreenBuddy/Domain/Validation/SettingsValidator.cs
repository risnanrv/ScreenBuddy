using System;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Domain.Validation
{
    /// <summary>
    /// Validates and clamps AppSettings values to allowable domain bounds.
    /// </summary>
    public static class SettingsValidator
    {
        public const int MinWorkDurationMinutes = 1;
        public const int MaxWorkDurationMinutes = 120;
        public const int MinBreakDurationMinutes = 1;
        public const int MaxBreakDurationMinutes = 60;

        public static AppSettings Validate(AppSettings? settings)
        {
            if (settings == null)
            {
                return AppSettings.Default;
            }

            int work = Math.Clamp(settings.WorkDurationMinutes, MinWorkDurationMinutes, MaxWorkDurationMinutes);
            int breakDur = Math.Clamp(settings.BreakDurationMinutes, MinBreakDurationMinutes, MaxBreakDurationMinutes);

            return settings with
            {
                WorkDurationMinutes = work,
                BreakDurationMinutes = breakDur
            };
        }
    }
}
