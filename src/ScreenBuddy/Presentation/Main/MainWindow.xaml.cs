using System;
using System.Windows;
using System.Windows.Input;

namespace ScreenBuddy.Presentation.Main
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.RequestCloseToTray += (sender, args) => HideToTray();
            MouseDown += OnWindowMouseDown;
        }

        private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        public void HideToTray()
        {
            Hide();
        }
    }
}
