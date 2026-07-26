using System;
using FluentAssertions;
using NSubstitute;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using ScreenBuddy.Presentation.Main;
using Xunit;

namespace ScreenBuddy.Application.Tests
{
    public class MainWindowViewModelTests
    {
        private readonly ISessionCoordinator _sessionCoordinator = Substitute.For<ISessionCoordinator>();
        private readonly ITimerEngine _timerEngine = Substitute.For<ITimerEngine>();
        private readonly ISettingsService _settingsService = Substitute.For<ISettingsService>();

        public MainWindowViewModelTests()
        {
            _settingsService.CurrentSettings.Returns(AppSettings.Default);
            _sessionCoordinator.CurrentState.Returns(SessionState.Stopped);
            _timerEngine.RemainingSeconds.Returns(1500);
        }

        [Fact]
        public void InitialState_IsStopped()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.CurrentState.Should().Be(SessionState.Stopped);
            vm.IsStateStopped.Should().BeTrue();
            vm.IsStateWorking.Should().BeFalse();
            vm.StatusHeader.Should().Be("ScreenBuddy");
        }

        [Fact]
        public void UpdateStateView_Working_SetsFlagsCorrectly()
        {
            _sessionCoordinator.CurrentState.Returns(SessionState.Working);
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.IsStateWorking.Should().BeTrue();
            vm.IsStateStopped.Should().BeFalse();
            vm.StatusHeader.Should().Be("Work Session");
        }

        [Fact]
        public void StartSessionCommand_SendsStartCommandToCoordinator()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.StartSessionCommand.Execute(null);

            _sessionCoordinator.Received(1).Send(SessionCommand.Start);
        }

        [Fact]
        public void PauseSessionCommand_SendsPauseCommandToCoordinator()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.PauseSessionCommand.Execute(null);

            _sessionCoordinator.Received(1).Send(SessionCommand.Pause);
        }

        [Fact]
        public void StopSessionCommand_SendsStopCommandToCoordinator()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.StopSessionCommand.Execute(null);

            _sessionCoordinator.Received(1).Send(SessionCommand.Stop);
        }

        [Fact]
        public void SnoozeBreakCommand_CallsCoordinatorSnooze()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.SnoozeBreakCommand.Execute("5");

            _sessionCoordinator.Received(1).Snooze(5);
        }

        [Fact]
        public void UpdateRemainingTime_FormatsMinutesAndSecondsCorrectly()
        {
            using var vm = new MainWindowViewModel(_sessionCoordinator, _timerEngine, _settingsService);

            vm.UpdateRemainingTime(125);

            vm.CountdownText.Should().Be("02:05");
        }
    }
}
