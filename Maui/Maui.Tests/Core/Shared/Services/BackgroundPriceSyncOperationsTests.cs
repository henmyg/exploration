using Maui.Core.Shared.Services;
using Xunit;

namespace Maui.Tests.Core.Shared.Services;

public class BackgroundPriceSyncOperationsTests
{
    [Theory]
    [InlineData(10, 0)]    // 10:00:00 → wait until publication time today
    [InlineData(13, 0)]    // 1 hour before publication time
    [InlineData(13, 4)]    // 1 minute before publication time
    [InlineData(13, 6)]    // 1 minute after publication time → wait until tomorrow
    [InlineData(20, 0)]    // Evening → wait until publication time tomorrow
    [InlineData(0, 0)]     // Midnight → wait until publication time today
    public void CalculateDelayUntilNext1305_ReturnsCorrectDelay(int currentHour, int currentMinute)
    {
        // Arrange
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, currentHour, currentMinute, 0, DateTimeKind.Local);
        var publicationTimeToday = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);

        // Calculate expected delay
        TimeSpan expected;
        if (now < publicationTimeToday)
        {
            // Before publication time today
            expected = publicationTimeToday - now;
        }
        else if (now == publicationTimeToday)
        {
            // Exactly at publication time → wait 24h
            expected = TimeSpan.FromHours(24);
        }
        else
        {
            // After publication time → wait until tomorrow
            var publicationTimeTomorrow = publicationTimeToday.AddDays(1);
            expected = publicationTimeTomorrow - now;
        }

        // Act
        var actual = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateDelayUntilNext1305_BeforeMidnight_WaitsUntilNextPublicationTime()
    {
        // Arrange - 23:59 should wait until publication time tomorrow
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, 23, 59, 0, DateTimeKind.Local);
        var publicationTimeTomorrow = new DateTime(2025, 10, 18, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);

        // Act
        var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert
        var expected = publicationTimeTomorrow - now;
        Assert.Equal(expected, delay);
    }

    [Fact]
    public void CalculateDelayUntilNext1305_ExactlyAtPublicationTime_WaitsUntilTomorrow()
    {
        // Arrange - exactly at publication time
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);

        // Act
        var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);

        // Assert - Should wait exactly 24 hours
        Assert.Equal(TimeSpan.FromHours(24), delay);
    }

    [Theory]
    [InlineData(0, 0)]      // Midnight - before publication time
    [InlineData(10, 30)]    // Morning - before publication time
    [InlineData(13, 4)]     // Just before publication time
    [InlineData(13, 6)]     // Just after publication time
    [InlineData(20, 0)]     // Evening - after publication time
    [InlineData(23, 59)]    // Just before midnight - after publication time
    public void GetExpectedPriceDate_ReturnsCorrectDateBasedOnTime(int hour, int minute)
    {
        // Arrange
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, hour, minute, 0, DateTimeKind.Local);
        var publicationTimeToday = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);

        // Determine expected day based on publication time
        int expectedDay = now < publicationTimeToday ? 17 : 18;
        var expectedDate = new DateTime(2025, 10, expectedDay, 11, 0, 0, DateTimeKind.Utc);

        // Act
        var result = BackgroundPriceSyncOperations.GetExpectedPriceDate(now);

        // Assert
        Assert.Equal(expectedDate, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
        Assert.Equal(11, result.Hour);
        Assert.Equal(0, result.Minute);
        Assert.Equal(0, result.Second);
    }

    [Fact]
    public void GetExpectedPriceDate_BeforePublicationTime_ReturnsTodayAt1100UTC()
    {
        // Arrange - Time before publication time on Oct 17
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, pubTime.Hour - 1, 0, 0, DateTimeKind.Local);

        // Act
        var result = BackgroundPriceSyncOperations.GetExpectedPriceDate(now);

        // Assert - Should expect today's prices (Oct 17 at 11:00 UTC)
        Assert.Equal(new DateTime(2025, 10, 17, 11, 0, 0, DateTimeKind.Utc), result);
    }

    [Fact]
    public void GetExpectedPriceDate_AfterPublicationTime_ReturnsTomorrowAt1100UTC()
    {
        // Arrange - Time after publication time on Oct 17
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var now = new DateTime(2025, 10, 17, pubTime.Hour + 1, 0, 0, DateTimeKind.Local);

        // Act
        var result = BackgroundPriceSyncOperations.GetExpectedPriceDate(now);

        // Assert - Should expect tomorrow's prices (Oct 18 at 11:00 UTC)
        Assert.Equal(new DateTime(2025, 10, 18, 11, 0, 0, DateTimeKind.Utc), result);
    }

    [Theory]
    [InlineData(-61, false)]  // 61 minutes before publication time - not too close
    [InlineData(-60, false)]  // 60 minutes before publication time - not too close
    [InlineData(-59, true)]   // 59 minutes before publication time - too close
    [InlineData(-5, true)]    // 5 minutes before publication time - too close
    [InlineData(-1, true)]    // 1 minute before publication time - too close
    [InlineData(0, false)]    // Exactly at publication time - not too close (24h away from next)
    [InlineData(1, false)]    // 1 minute after publication time - not too close
    [InlineData(120, false)]  // 2 hours after publication time - not too close
    public void IsTooCloseTo1305_ReturnsCorrectValue(int minutesFromPublicationTime, bool expectedTooClose)
    {
        // Arrange
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var publicationDateTime = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);
        var now = publicationDateTime.AddMinutes(minutesFromPublicationTime);

        // Act
        var result = BackgroundPriceSyncOperations.IsTooCloseTo1305(now);

        // Assert
        Assert.Equal(expectedTooClose, result);
    }
}
