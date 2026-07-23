using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Infrastructure.Platform;

namespace ScreenBuddy.Presentation.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IStartupRegistrar? _startupRegistrar;

        [ObservableProperty]
        private int _workDurationMinutes;

        [ObservableProperty]
        private int _breakDurationMinutes;

        [ObservableProperty]
        private bool _launchOnStartup;

        public event EventHandler? CloseRequested;

        public SettingsViewModel(ISettingsService settingsService, IStartupRegistrar? startupRegistrar = null)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _startupRegistrar = startupRegistrar;

            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            AppSettings current = _settingsService.CurrentSettings;
            WorkDurationMinutes = current.WorkDurationMinutes;
            BreakDurationMinutes = current.BreakDurationMinutes;
            LaunchOnStartup = _startupRegistrar?.IsEnabled() ?? current.LaunchOnStartup;
        }

        [RelayCommand]
        private void Save()
        {
            var updated = new AppSettings
            {
                WorkDurationMinutes = WorkDurationMinutes,
                BreakDurationMinutes = BreakDurationMinutes,
                LaunchOnStartup = LaunchOnStartup
            };

            _settingsService.UpdateSettings(updated);

            if (_startupRegistrar != null)
            {
                if (LaunchOnStartup)
                {
                    _startupRegistrar.Enable();
                }
                else
                {
                    _startupRegistrar.Disable();
                }
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
