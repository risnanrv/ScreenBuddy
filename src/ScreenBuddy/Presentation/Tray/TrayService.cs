using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Infrastructure.Platform;
using ScreenBuddy.Presentation.Settings;

namespace ScreenBuddy.Presentation.Tray
{
    public sealed class TrayService : ITrayService
    {
        private readonly TrayViewModel _viewModel;
        private readonly ISettingsService _settingsService;
        private readonly IStartupRegistrar? _startupRegistrar;
        private TaskbarIcon? _taskbarIcon;

        public TrayService(
            TrayViewModel viewModel,
            ISettingsService settingsService,
            IStartupRegistrar? startupRegistrar = null)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _startupRegistrar = startupRegistrar;

            _viewModel.OpenSettingsRequested += OnOpenSettingsRequested;
            _viewModel.ExitRequested += OnExitRequested;
        }

        public void Initialize()
        {
            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "ScreenBuddy",
                Icon = SystemIcons.Application, // Fallback icon until customized .ico loaded
                DataContext = _viewModel
            };

            var contextMenu = new ContextMenu();

            var statusItem = new MenuItem
            {
                IsEnabled = false,
                Header = "ScreenBuddy — Ready"
            };
            // Bind status header to ViewModel StatusText
            statusItem.SetBinding(HeaderedItemsControl.HeaderProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.StatusText)));
            contextMenu.Items.Add(statusItem);

            contextMenu.Items.Add(new Separator());

            var startItem = new MenuItem { Header = "Start Work Session" };
            startItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.StartCommand)));
            contextMenu.Items.Add(startItem);

            var pauseItem = new MenuItem { Header = "Pause" };
            pauseItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.PauseCommand)));
            contextMenu.Items.Add(pauseItem);

            var resumeItem = new MenuItem { Header = "Resume" };
            resumeItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.ResumeCommand)));
            contextMenu.Items.Add(resumeItem);

            var resetItem = new MenuItem { Header = "Reset Session" };
            resetItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.ResetCommand)));
            contextMenu.Items.Add(resetItem);

            contextMenu.Items.Add(new Separator());

            var settingsItem = new MenuItem { Header = "Settings..." };
            settingsItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.OpenSettingsCommand)));
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new Separator());

            var quitItem = new MenuItem { Header = "Quit ScreenBuddy" };
            quitItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.QuitCommand)));
            contextMenu.Items.Add(quitItem);

            _taskbarIcon.ContextMenu = contextMenu;
            _taskbarIcon.LeftClickCommand = _viewModel.TogglePauseResumeCommand;
            _taskbarIcon.ForceCreate();
        }

        private void OnOpenSettingsRequested(object? sender, EventArgs e)
        {
            var settingsVm = new SettingsViewModel(_settingsService, _startupRegistrar);
            SettingsWindow.ShowSingleInstance(settingsVm);
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _viewModel.OpenSettingsRequested -= OnOpenSettingsRequested;
            _viewModel.ExitRequested -= OnExitRequested;
            _taskbarIcon?.Dispose();
        }
    }
}
