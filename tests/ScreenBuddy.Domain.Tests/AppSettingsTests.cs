using FluentAssertions;
using ScreenBuddy.Domain.Models;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class AppSettingsTests
    {
        [Fact]
        public void DefaultSettings_ShouldHaveCorrectDefaults()
        {
            var settings = AppSettings.Default;

            settings.WorkDurationMinutes.Should().Be(25);
            settings.BreakDurationMinutes.Should().Be(5);
            settings.LaunchOnStartup.Should().BeTrue();
            settings.ConfigVersion.Should().Be(1);
        }

        [Fact]
        public void WithExpression_ShouldCopyAndModify()
        {
            var initial = AppSettings.Default;
            var updated = initial with { WorkDurationMinutes = 50, BreakDurationMinutes = 10 };

            updated.WorkDurationMinutes.Should().Be(50);
            updated.BreakDurationMinutes.Should().Be(10);
            updated.LaunchOnStartup.Should().BeTrue();

            // Initial unchanged
            initial.WorkDurationMinutes.Should().Be(25);
        }
    }
}
