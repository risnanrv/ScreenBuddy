using FluentAssertions;
using NSubstitute;
using ScreenBuddy.Application.Services;
using ScreenBuddy.Domain.Models;
using Xunit;

namespace ScreenBuddy.Application.Tests
{
    public class SettingsServiceTests
    {
        private readonly IConfigPersister _configPersister = Substitute.For<IConfigPersister>();

        [Fact]
        public void Constructor_LoadsAndValidatesConfig()
        {
            _configPersister.LoadConfig().Returns(new AppSettings { WorkDurationMinutes = 30 });

            var service = new SettingsService(_configPersister);

            service.CurrentSettings.WorkDurationMinutes.Should().Be(30);
        }

        [Fact]
        public void UpdateSettings_ValidatesSavesAndFiresEvent()
        {
            _configPersister.LoadConfig().Returns(AppSettings.Default);
            var service = new SettingsService(_configPersister);

            bool eventFired = false;
            AppSettings? updatedReceived = null;
            service.SettingsChanged += (sender, settings) =>
            {
                eventFired = true;
                updatedReceived = settings;
            };

            var newSettings = new AppSettings { WorkDurationMinutes = 45, BreakDurationMinutes = 10 };
            service.UpdateSettings(newSettings);

            service.CurrentSettings.WorkDurationMinutes.Should().Be(45);
            service.CurrentSettings.BreakDurationMinutes.Should().Be(10);
            _configPersister.Received(1).SaveConfig(Arg.Is<AppSettings>(s => s.WorkDurationMinutes == 45));
            eventFired.Should().BeTrue();
            updatedReceived?.WorkDurationMinutes.Should().Be(45);
        }

        [Fact]
        public void UpdateSettings_ClampsInvalidValues()
        {
            _configPersister.LoadConfig().Returns(AppSettings.Default);
            var service = new SettingsService(_configPersister);

            var invalid = new AppSettings { WorkDurationMinutes = 500, BreakDurationMinutes = -10 };
            service.UpdateSettings(invalid);

            service.CurrentSettings.WorkDurationMinutes.Should().Be(120); // Max clamped
            service.CurrentSettings.BreakDurationMinutes.Should().Be(1);   // Min clamped
        }
    }
}
