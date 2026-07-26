using System;
using System.Drawing;
using System.Windows.Controls;
using H.NotifyIcon;
using H.NotifyIcon.Core;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Infrastructure.Platform;
using ScreenBuddy.Presentation.Main;

namespace ScreenBuddy.Presentation.Tray
{
    public sealed class TrayService : ITrayService
    {
        private readonly TrayViewModel _viewModel;
        private readonly IMainWindowService _mainWindowService;
        private TaskbarIcon? _taskbarIcon;

        public TrayService(
            TrayViewModel viewModel,
            IMainWindowService mainWindowService)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _mainWindowService = mainWindowService ?? throw new ArgumentNullException(nameof(mainWindowService));

            _viewModel.OpenMainWindowRequested += OnOpenMainWindowRequested;
            _viewModel.ExitRequested += OnExitRequested;
        }

        public void Initialize()
        {
            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = "ScreenBuddy",
                Icon = SystemIcons.Application,
                DataContext = _viewModel
            };

            var contextMenu = new ContextMenu();

            // 1. Open ScreenBuddy (Default bold menu item)
            var openItem = new MenuItem
            {
                Header = "Open ScreenBuddy",
                FontWeight = System.Windows.FontWeights.Bold
            };
            openItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.OpenMainWindowCommand)));
            contextMenu.Items.Add(openItem);

            contextMenu.Items.Add(new Separator());

            // 2. Contextual Pause / Resume
            var toggleItem = new MenuItem { Header = "Pause / Resume" };
            toggleItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.TogglePauseResumeCommand)));
            contextMenu.Items.Add(toggleItem);

            contextMenu.Items.Add(new Separator());

            // 3. Quit
            var quitItem = new MenuItem { Header = "Quit ScreenBuddy" };
            quitItem.SetBinding(MenuItem.CommandProperty, new System.Windows.Data.Binding(nameof(TrayViewModel.QuitCommand)));
            contextMenu.Items.Add(quitItem);

            _taskbarIcon.ContextMenu = contextMenu;
            _taskbarIcon.LeftClickCommand = _viewModel.OpenMainWindowCommand;
            _taskbarIcon.DoubleClickCommand = _viewModel.OpenMainWindowCommand;
            _taskbarIcon.ForceCreate();
        }

        private void OnOpenMainWindowRequested(object? sender, EventArgs e)
        {
            _mainWindowService.BringToForeground();
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _viewModel.OpenMainWindowRequested -= OnOpenMainWindowRequested;
            _viewModel.ExitRequested -= OnExitRequested;
            _taskbarIcon?.Dispose();
        }
    }
}
