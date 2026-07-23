using System;
using System.Windows;
using System.Windows.Input;

namespace ScreenBuddy.Presentation.FirstRun
{
    public partial class FirstRunWindow : Window
    {
        public FirstRunWindow(FirstRunViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.Completed += (sender, args) => Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
