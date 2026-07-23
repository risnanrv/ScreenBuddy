using System;
using ScreenBuddy.Domain.Models;

namespace ScreenBuddy.Application.Services
{
    /// <summary>
    /// Contract for managing application settings cache, validation, and notification.
    /// </summary>
    public interface ISettingsService
    {
        event EventHandler<AppSettings>? SettingsChanged;

        AppSettings CurrentSettings { get; }
        void UpdateSettings(AppSettings newSettings);
        void ReloadSettings();
    }
}
