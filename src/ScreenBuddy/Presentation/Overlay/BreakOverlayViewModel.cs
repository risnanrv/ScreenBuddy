using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScreenBuddy.Application;

namespace ScreenBuddy.Presentation.Overlay
{
    public partial class BreakOverlayViewModel : ObservableObject
    {
        private readonly ISessionCoordinator _sessionCoordinator;

        [ObservableProperty]
        private string _messageText = "Rest is not the opposite of productivity. It is its fuel.";

        [ObservableProperty]
        private string _countdownText = "05:00";

        public BreakOverlayViewModel(ISessionCoordinator sessionCoordinator)
        {
            _sessionCoordinator = sessionCoordinator ?? throw new ArgumentNullException(nameof(sessionCoordinator));
        }

        public void SetMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                MessageText = message;
            }
        }

        public void UpdateCountdown(int remainingSeconds)
        {
            int minutes = Math.Max(0, remainingSeconds / 60);
            int seconds = Math.Max(0, remainingSeconds % 60);
            CountdownText = $"{minutes:D2}:{seconds:D2}";
        }

        [RelayCommand]
        private void Skip()
        {
            _sessionCoordinator.Send(SessionCommand.SkipBreak);
        }

        [RelayCommand]
        private void Snooze(string? minutesString)
        {
            int minutes = 5;
            if (int.TryParse(minutesString, out int parsed))
            {
                minutes = Math.Clamp(parsed, 1, 10);
            }
            _sessionCoordinator.Snooze(minutes);
        }

        [RelayCommand]
        private void Minimize()
        {
            _sessionCoordinator.Send(SessionCommand.MinimizeBreak);
        }

        [RelayCommand]
        private void EmergencyEscape()
        {
            _sessionCoordinator.Send(SessionCommand.EmergencyEscape);
        }
    }
}
