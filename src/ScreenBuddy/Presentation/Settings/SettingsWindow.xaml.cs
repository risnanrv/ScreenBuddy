using System;
using System.Windows;
using System.Windows.Input;

namespace ScreenBuddy.Presentation.Settings
{
    public partial class SettingsWindow : Window
    {
        private static SettingsWindow? _activeInstance;

        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.CloseRequested += (sender, args) => Close();
        }

        public static void ShowSingleInstance(SettingsViewModel viewModel)
        {
            if (_activeInstance != null && _activeInstance.IsLoaded)
            {
                _activeInstance.Activate();
                return;
            }

            _activeInstance = new SettingsWindow(viewModel);
            _activeInstance.Closed += (s, e) => _activeInstance = null;
            _activeInstance.Show();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
