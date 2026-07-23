using FluentAssertions;
using NSubstitute;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Messages;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using Xunit;

namespace ScreenBuddy.Application.Tests
{
    public class SessionCoordinatorTests
    {
        private readonly ITimerEngine _timerEngine = Substitute.For<ITimerEngine>();
        private readonly ISettingsService _settingsService = Substitute.For<ISettingsService>();
        private readonly IMessageLibrary _messageLibrary = Substitute.For<IMessageLibrary>();

        public SessionCoordinatorTests()
        {
            _settingsService.CurrentSettings.Returns(AppSettings.Default);
            _messageLibrary.GetNextMessage().Returns(new BreakMessage("Rest up!", 0));
        }

        [Fact]
        public void Start_WhenStopped_StartsTimerAndTransitionsToWorking()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Stopped);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool success = coordinator.Send(SessionCommand.Start);

            success.Should().BeTrue();
            _timerEngine.Received(1).Start(1500);
        }

        [Fact]
        public void Pause_WhenWorking_PausesTimer()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Working);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool success = coordinator.Send(SessionCommand.Pause);

            success.Should().BeTrue();
            _timerEngine.Received(1).Pause();
        }

        [Fact]
        public void Resume_WhenPaused_ResumesTimer()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Paused);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool success = coordinator.Send(SessionCommand.Resume);

            success.Should().BeTrue();
            _timerEngine.Received(1).Resume();
        }

        [Fact]
        public void SkipBreak_WhenInBreak_EndsBreakAndStartsWork()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Break);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool breakEndedFired = false;
            coordinator.BreakEnded += (sender, args) => breakEndedFired = true;

            bool success = coordinator.Send(SessionCommand.SkipBreak);

            success.Should().BeTrue();
            _timerEngine.Received(1).EndBreak();
            _timerEngine.Received(1).Start(1500);
            breakEndedFired.Should().BeTrue();
        }

        [Fact]
        public void Quit_TransitionsToStopped()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Working);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool success = coordinator.Send(SessionCommand.Quit);

            success.Should().BeTrue();
            _timerEngine.Received(1).EndBreak();
        }
    }
}
