using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScreenBuddy.Presentation.Common
{
    public partial class ToggleSwitch : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public ToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState(false);
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitch control)
            {
                control.UpdateVisualState(true);
            }
        }

        private void OnGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }

        private void UpdateVisualState(bool useAnimation)
        {
            double targetLeft = IsChecked ? 21.0 : 3.0;
            Brush trackBrush = IsChecked ? (Brush)FindResource("color-accent-bg") : (Brush)FindResource("color-base-700");
            Brush thumbBrush = IsChecked ? (Brush)FindResource("color-accent-400") : (Brush)FindResource("color-text-secondary");

            TrackBorder.Background = trackBrush;
            ThumbBorder.Background = thumbBrush;

            if (useAnimation)
            {
                var animation = new DoubleAnimation
                {
                    To = targetLeft,
                    Duration = TimeSpan.FromMilliseconds(150),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                ThumbBorder.BeginAnimation(Canvas.LeftProperty, animation);
            }
            else
            {
                Canvas.SetLeft(ThumbBorder, targetLeft);
            }
        }
    }
}
