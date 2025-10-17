using Maui.Core.Shared.Services;
using Xunit;

namespace Maui.Tests.Core.Shared.Services;

public class BackgroundPriceSyncOperationsTests
{
    [Theory]
    [InlineData(10, 0, 3, 5)]    // 10:00:00 → wait 3h 5m until 13:05 today
    [InlineData(13, 0, 0, 5)]    // 13:00:00 → wait 5m until 13:05 today
    [InlineData(13, 4, 0, 1)]    // 13:04:00 → wait 1m until 13:05 today
    [InlineData(13, 5, 24, 0)]   // 13:05:00 → wait 24h until 13:05 tomorrow
    [InlineData(13, 6, 23, 59)]  // 13:06:00 → wait 23h 59m until 13:05 tomorrow
    [InlineData(20, 0, 17, 5)]   // 20:00:00 → wait 17h 5m until 13:05 tomorrow
    [InlineData(0, 0, 13, 5)]    // 00:00:00 → wait 13h 5m until 13:05 today
    public void CalculateDelayUntilNext1305_ReturnsCorrectDelay(
        int currentHour, int currentMinute,
        int expectedHours, int expectedMinutes)
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, currentHour, currentMinute, 0, DateTimeKind.Local);
        var expected = TimeSpan.FromHours(expectedHours)
            .Add(TimeSpan.FromMinutes(expectedMinutes))
            .Add(TimeSpan.FromSeconds(0));

        // Act
        var actual = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateDelayUntilNext1305_BeforeMidnight_WaitsUntilNext1305()
    {
        // Arrange - 23:59 should wait until 13:05 tomorrow
        var now = new DateTime(2025, 10, 17, 23, 59, 0, DateTimeKind.Local);

        // Act
        var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert - Should be ~13h 6m
        Assert.Equal(TimeSpan.FromHours(13).Add(TimeSpan.FromMinutes(6)), delay);
    }

    [Fact]
    public void CalculateDelayUntilNext1305_ExactlyAt1305_WaitsUntilTomorrow()
    {
        // Arrange - exactly at 13:05:00
        var now = new DateTime(2025, 10, 17, 13, 5, 0, DateTimeKind.Local);

        // Act
        var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert - Should wait exactly 24 hours
        Assert.Equal(TimeSpan.FromHours(24), delay);
    }
}
