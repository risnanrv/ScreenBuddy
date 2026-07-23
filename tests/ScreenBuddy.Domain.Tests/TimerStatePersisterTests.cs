using System;
using System.IO;
using FluentAssertions;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Infrastructure.Persistence;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class TimerStatePersisterTests : IDisposable
    {
        private readonly string _tempFile;

        public TimerStatePersisterTests()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), $"screenbuddy_test_timerstate_{Guid.NewGuid():N}.json");
        }

        [Fact]
        public void Read_FileDoesNotExist_ReturnsNull()
        {
            var persister = new TimerStatePersister(_tempFile);
            var snapshot = persister.Read();

            snapshot.Should().BeNull();
        }

        [Fact]
        public void Write_And_Read_RoundTripsSuccessfully()
        {
            var persister = new TimerStatePersister(_tempFile);
            var snapshot = new TimerSnapshot
            {
                Phase = SessionState.Working,
                RemainingSeconds = 600,
                SnapshotUtc = DateTimeOffset.UtcNow,
                ConfiguredWorkSeconds = 1500,
                ConfiguredBreakSeconds = 300
            };

            persister.Write(snapshot);
            var readBack = persister.Read();

            readBack.Should().NotBeNull();
            readBack!.Phase.Should().Be(SessionState.Working);
            readBack.RemainingSeconds.Should().Be(600);
            readBack.ConfiguredWorkSeconds.Should().Be(1500);
        }

        [Fact]
        public void Delete_RemovesFile()
        {
            var persister = new TimerStatePersister(_tempFile);
            var snapshot = new TimerSnapshot { Phase = SessionState.Working, RemainingSeconds = 100 };
            persister.Write(snapshot);

            File.Exists(_tempFile).Should().BeTrue();

            persister.Delete();
            File.Exists(_tempFile).Should().BeFalse();
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
        }
    }
}
