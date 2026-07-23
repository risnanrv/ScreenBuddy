using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenBuddy.Application;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Infrastructure.Platform;

namespace ScreenBuddy.Presentation.FirstRun
{
    public partial class FirstRunViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly ISessionCoordinator _sessionCoordinator;
        private readonly IStartupRegistrar? _startupRegistrar;

        [ObservableProperty]
        private int _workDurationMinutes = 25;

        [ObservableProperty]
        private int _breakDurationMinutes = 5;

        [ObservableProperty]
        private bool _launchOnStartup = true;

        public event EventHandler? Completed;

        public FirstRunViewModel(
            ISettingsService settingsService,
            ISessionCoordinator sessionCoordinator,
            IStartupRegistrar? startupRegistrar = null)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
            _startupRegistrar = startupRegistrar;
        }

        [RelayCommand]
        private void Start()
        {
            var initialSettings = new AppSettings
            {
                WorkDurationMinutes = WorkDurationMinutes,
                BreakDurationMinutes = BreakDurationMinutes,
                LaunchOnStartup = LaunchOnStartup
            };

            _settingsService.UpdateSettings(initialSettings);

            if (LaunchOnStartup)
            {
                _startupRegistrar?.Enable();
            }

            _sessionCoordinator.Send(SessionCommand.Start);
            Completed?.Invoke(this, EventArgs.Empty);
        }
    }
}
