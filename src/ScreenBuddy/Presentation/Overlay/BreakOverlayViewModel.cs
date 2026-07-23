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
        private async Task SkipAsync()
        {
            // Phase 3 §10 IP-005: 100ms intentional delay to prevent accidental click-through skips
            await Task.Delay(100);
            _sessionCoordinator.Send(SessionCommand.SkipBreak);
        }
    }
}
