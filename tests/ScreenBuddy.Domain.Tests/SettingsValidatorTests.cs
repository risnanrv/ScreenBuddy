using FluentAssertions;
using ScreenBuddy.Domain.Models;
using ScreenBuddy.Domain.Validation;
using Xunit;

namespace ScreenBuddy.Domain.Tests
{
    public class SettingsValidatorTests
    {
        [Fact]
        public void Validate_NullSettings_ReturnsDefault()
        {
            var result = SettingsValidator.Validate(null);
            result.Should().BeEquivalentTo(AppSettings.Default);
        }

        [Theory]
        [InlineData(0, 1)]      // Below min -> clamped to min
        [InlineData(-5, 1)]     // Negative -> clamped to min
        [InlineData(1, 1)]      // Min boundary -> preserved
        [InlineData(25, 25)]    // Normal -> preserved
        [InlineData(120, 120)]  // Max boundary -> preserved
        [InlineData(150, 120)]  // Above max -> clamped to max
        public void Validate_WorkDuration_ClampsCorrectly(int input, int expected)
        {
            var settings = new AppSettings { WorkDurationMinutes = input };
            var validated = SettingsValidator.Validate(settings);
            validated.WorkDurationMinutes.Should().Be(expected);
        }

        [Theory]
        [InlineData(0, 1)]     // Below min -> clamped to min
        [InlineData(-1, 1)]    // Negative -> clamped to min
        [InlineData(1, 1)]     // Min boundary -> preserved
        [InlineData(5, 5)]     // Normal -> preserved
        [InlineData(60, 60)]   // Max boundary -> preserved
        [InlineData(90, 60)]   // Above max -> clamped to max
        public void Validate_BreakDuration_ClampsCorrectly(int input, int expected)
        {
            var settings = new AppSettings { BreakDurationMinutes = input };
            var validated = SettingsValidator.Validate(settings);
            validated.BreakDurationMinutes.Should().Be(expected);
        }
    }
}
