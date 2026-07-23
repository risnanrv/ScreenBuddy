using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenBuddy.Presentation.Common
{
    public partial class NumericInputBox : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(NumericInputBox),
                new FrameworkPropertyMetadata(25, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                nameof(MinValue),
                typeof(int),
                typeof(NumericInputBox),
                new PropertyMetadata(1));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                nameof(MaxValue),
                typeof(int),
                typeof(NumericInputBox),
                new PropertyMetadata(120));

        public static readonly DependencyProperty SuffixProperty =
            DependencyProperty.Register(
                nameof(Suffix),
                typeof(string),
                typeof(NumericInputBox),
                new PropertyMetadata("minutes", OnSuffixChanged));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public string Suffix
        {
            get => (string)GetValue(SuffixProperty);
            set => SetValue(SuffixProperty, value);
        }

        public NumericInputBox()
        {
            InitializeComponent();
            ValueTextBox.Text = Value.ToString();
            SuffixTextBlock.Text = Suffix;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericInputBox control && e.NewValue is int val)
            {
                control.ValueTextBox.Text = val.ToString();
            }
        }

        private static void OnSuffixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericInputBox control && e.NewValue is string s)
            {
                control.SuffixTextBlock.Text = s;
            }
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            InputBorder.BorderBrush = (Brush)FindResource("color-border-focus");
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            ValidateAndCommit();
            InputBorder.BorderBrush = (Brush)FindResource("color-border-subtle");
        }

        private void OnTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                Value = Math.Clamp(Value + 1, MinValue, MaxValue);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                Value = Math.Clamp(Value - 1, MinValue, MaxValue);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                ValidateAndCommit();
                e.Handled = true;
            }
        }

        private void ValidateAndCommit()
        {
            if (int.TryParse(ValueTextBox.Text, out int parsed))
            {
                int clamped = Math.Clamp(parsed, MinValue, MaxValue);
                Value = clamped;
                ValueTextBox.Text = clamped.ToString();
            }
            else
            {
                ValueTextBox.Text = Value.ToString();
            }
        }
    }
}
