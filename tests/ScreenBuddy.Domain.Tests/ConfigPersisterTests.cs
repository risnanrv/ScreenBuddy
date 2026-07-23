using System;
using System.IO;
using FluentAssertions;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Infrastructure.Persistence;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class ConfigPersisterTests : IDisposable
    {
        private readonly string _tempFile;

        public ConfigPersisterTests()
        {
            _tempFile = Path.Combine(Path.GetTempPath(), $"screenbuddy_test_config_{Guid.NewGuid():N}.json");
        }

        [Fact]
        public void LoadConfig_FileDoesNotExist_ReturnsDefaultSettings()
        {
            var persister = new ConfigPersister(_tempFile);
            var loaded = persister.LoadConfig();

            loaded.Should().BeEquivalentTo(AppSettings.Default);
        }

        [Fact]
        public void SaveConfig_And_LoadConfig_RoundTripsSuccessfully()
        {
            var persister = new ConfigPersister(_tempFile);
            var original = new AppSettings
            {
                WorkDurationMinutes = 45,
                BreakDurationMinutes = 10,
                LaunchOnStartup = false,
                ConfigVersion = 1
            };

            persister.SaveConfig(original);
            var loaded = persister.LoadConfig();

            loaded.Should().BeEquivalentTo(original);
        }

        public void Dispose()
        {
            if (File.Exists(_tempFile))
            {
                File.Delete(_tempFile);
            }
            string tmp = _tempFile + ".tmp";
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }
    }
}
