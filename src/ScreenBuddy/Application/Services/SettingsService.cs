using System;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Validation;

namespace ScreenBuddy.Application.Services
{
    /// <summary>
    /// Implements thread-safe application settings caching, validation, and persistence.
    /// </summary>
    public sealed class SettingsService : ISettingsService
    {
        private readonly IConfigPersister _configPersister;
        private readonly object _lock = new();
        private AppSettings _cachedSettings;

        public event EventHandler<AppSettings>? SettingsChanged;

        public AppSettings CurrentSettings
        {
            get
            {
                lock (_lock)
                {
                    return _cachedSettings;
                }
            }
        }

        public SettingsService(IConfigPersister configPersister)
        {
            _configPersister = configPersister ?? throw new ArgumentNullException(nameof(configPersister));
            _cachedSettings = LoadAndValidateInternal();
        }

        public void UpdateSettings(AppSettings newSettings)
        {
            ArgumentNullException.ThrowIfNull(newSettings);

            AppSettings validated = SettingsValidator.Validate(newSettings);
            lock (_lock)
            {
                _cachedSettings = validated;
                _configPersister.SaveConfig(validated);
            }

            SettingsChanged?.Invoke(this, validated);
        }

        public void ReloadSettings()
        {
            AppSettings reloaded = LoadAndValidateInternal();
            lock (_lock)
            {
                _cachedSettings = reloaded;
            }

            SettingsChanged?.Invoke(this, reloaded);
        }

        private AppSettings LoadAndValidateInternal()
        {
            try
            {
                AppSettings raw = _configPersister.LoadConfig();
                return SettingsValidator.Validate(raw);
            }
            catch
            {
                return AppSettings.Default;
            }
        }
    }
}
