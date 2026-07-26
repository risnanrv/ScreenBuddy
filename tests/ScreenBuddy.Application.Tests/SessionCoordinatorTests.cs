using System;
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
        public void Stop_WhenWorking_StopsTimerAndTransitionsToStopped()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Working);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool success = coordinator.Send(SessionCommand.Stop);

            success.Should().BeTrue();
            _timerEngine.Received(1).Stop();
        }

        [Fact]
        public void Stop_WhenInBreak_EndsBreakAndStopsTimer()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Break);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool breakEndedFired = false;
            coordinator.BreakEnded += (sender, args) => breakEndedFired = true;

            bool success = coordinator.Send(SessionCommand.Stop);

            success.Should().BeTrue();
            _timerEngine.Received(1).Stop();
            breakEndedFired.Should().BeTrue();
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
        public void Snooze_WhenInBreak_SnoozesForRequestedDuration()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Break);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool breakEndedFired = false;
            coordinator.BreakEnded += (sender, args) => breakEndedFired = true;

            bool success = coordinator.Snooze(3);

            success.Should().BeTrue();
            _timerEngine.Received(1).Snooze(180);
            breakEndedFired.Should().BeTrue();
        }

        [Fact]
        public void MinimizeBreak_WhenInBreak_FiresBreakEndedWithoutEndingTimer()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Break);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool breakEndedFired = false;
            coordinator.BreakEnded += (sender, args) => breakEndedFired = true;

            bool success = coordinator.Send(SessionCommand.MinimizeBreak);

            success.Should().BeTrue();
            breakEndedFired.Should().BeTrue();
            _timerEngine.DidNotReceive().EndBreak();
        }

        [Fact]
        public void EmergencyEscape_WhenInBreak_HidesOverlaysAndPauses()
        {
            _timerEngine.CurrentPhase.Returns(SessionState.Break);
            using var coordinator = new SessionCoordinator(_timerEngine, _settingsService, _messageLibrary);

            bool breakEndedFired = false;
            coordinator.BreakEnded += (sender, args) => breakEndedFired = true;

            bool success = coordinator.Send(SessionCommand.EmergencyEscape);

            success.Should().BeTrue();
            _timerEngine.Received(1).EndBreak();
            _timerEngine.Received(1).Pause();
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

        [Fact]
        public void WorkTimerExpired_TriggersBreakStartedAndStartsBreakInTimerEngine()
        {
            using var realTimer = new TimerEngine();
            using var coordinator = new SessionCoordinator(realTimer, _settingsService, _messageLibrary);

            bool breakStartedFired = false;
            coordinator.BreakStarted += (sender, message) => breakStartedFired = true;

            realTimer.Start(1);
            System.Threading.Thread.Sleep(1500);

            coordinator.CurrentState.Should().Be(SessionState.Break);
            breakStartedFired.Should().BeTrue();
        }

        [Fact]
        public void BreakTimerExpired_TransitionsBackToWorking_WithoutDuplicateBreakRestart()
        {
            using var realTimer = new TimerEngine();
            using var coordinator = new SessionCoordinator(realTimer, _settingsService, _messageLibrary);

            realTimer.BeginBreak(1);
            System.Threading.Thread.Sleep(1500);

            coordinator.CurrentState.Should().Be(SessionState.Working);
        }
    }
}
