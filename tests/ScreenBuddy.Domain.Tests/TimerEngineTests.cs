using System;
using FluentAssertions;
using NSubstitute;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Timer;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class TimerEngineTests
    {
        private readonly IClock _clock = Substitute.For<IClock>();

        public TimerEngineTests()
        {
            _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Start_SetsWorkingPhaseAndRemainingSeconds()
        {
            using var engine = new TimerEngine(_clock);
            engine.Start(1500); // 25 min

            engine.CurrentPhase.Should().Be(SessionState.Working);
            engine.RemainingSeconds.Should().Be(1500);
            engine.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void Pause_ChangesStateToPaused()
        {
            using var engine = new TimerEngine(_clock);
            engine.Start(1500);
            engine.Pause();

            engine.CurrentPhase.Should().Be(SessionState.Paused);
            engine.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void Resume_ChangesStateBackToWorking()
        {
            using var engine = new TimerEngine(_clock);
            engine.Start(1500);
            engine.Pause();
            engine.Resume();

            engine.CurrentPhase.Should().Be(SessionState.Working);
            engine.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void BeginBreak_SetsBreakPhaseAndDuration()
        {
            using var engine = new TimerEngine(_clock);
            engine.BeginBreak(300); // 5 min

            engine.CurrentPhase.Should().Be(SessionState.Break);
            engine.RemainingSeconds.Should().Be(300);
            engine.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void ReconcileSleepGap_GapShorterThanRemaining_AdjustsRemaining()
        {
            var baseTime = DateTimeOffset.UtcNow;
            _clock.UtcNow.Returns(baseTime.AddSeconds(100)); // 100s later

            using var engine = new TimerEngine(_clock);
            var snapshot = new TimerSnapshot
            {
                Phase = SessionState.Working,
                RemainingSeconds = 300, // 300s remaining when snapshot taken
                SnapshotUtc = baseTime,
                ConfiguredWorkSeconds = 1500,
                ConfiguredBreakSeconds = 300
            };

            engine.ReconcileSleepGap(snapshot);

            engine.CurrentPhase.Should().Be(SessionState.Working);
            engine.RemainingSeconds.Should().Be(200); // 300 - 100 = 200s
        }

        [Fact]
        public void ReconcileSleepGap_GapLongerThanRemaining_FiresExpirationEvent()
        {
            var baseTime = DateTimeOffset.UtcNow;
            _clock.UtcNow.Returns(baseTime.AddSeconds(400)); // 400s later (longer than 300s remaining)

            using var engine = new TimerEngine(_clock);
            bool eventFired = false;
            engine.WorkTimerExpired += (sender, args) => eventFired = true;

            var snapshot = new TimerSnapshot
            {
                Phase = SessionState.Working,
                RemainingSeconds = 300,
                SnapshotUtc = baseTime,
                ConfiguredWorkSeconds = 1500,
                ConfiguredBreakSeconds = 300
            };

            engine.ReconcileSleepGap(snapshot);

            eventFired.Should().BeTrue();
            engine.CurrentPhase.Should().Be(SessionState.Stopped);
        }
    }
}
