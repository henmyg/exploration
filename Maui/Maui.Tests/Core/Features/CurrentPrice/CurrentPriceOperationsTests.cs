using Maui.Core.Features.CurrentPrice;
using Maui.Core.Shared.Models;
using Xunit;

namespace Maui.Tests.Core.Features.CurrentPrice;

public class CurrentPriceOperationsTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]           // Start of day
    [InlineData(14, 30, 45, 0)]        // Middle of day
    [InlineData(23, 59, 59, 999)]      // End of day
    public void GetTodayDateRange_ReturnsCorrectRange(int hour, int minute, int second, int millisecond)
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, hour, minute, second, millisecond, DateTimeKind.Utc);

        // Act
        var (dayStart, dayEnd) = now.GetTodayDateRange();

        // Assert
        Assert.Equal(new DateTime(2025, 10, 17, 0, 0, 0, DateTimeKind.Utc), dayStart);
        Assert.Equal(new DateTime(2025, 10, 18, 0, 0, 0, DateTimeKind.Utc), dayEnd);
    }

    [Theory]
    [InlineData(14, 15, 0, 0, 15, 0, 0, 0)]      // Exactly on quarter boundary → 15 minutes
    [InlineData(14, 7, 30, 500, 7, 29, 500, 0)]  // Middle of quarter → 7m 29.5s
    [InlineData(14, 14, 59, 999, 0, 0, 1, 0)]    // Just before quarter → 1ms
    [InlineData(14, 0, 30, 0, 14, 30, 0, 0)]     // At :00 minute → delay to :15
    [InlineData(14, 30, 15, 250, 14, 44, 750, 0)] // At :30 minute → delay to :45
    [InlineData(14, 45, 10, 0, 14, 50, 0, 0)]    // At :45 minute → delay to next hour :00
    [InlineData(14, 0, 0, 0, 15, 0, 0, 0)]       // At :00:00.000 exactly → 15 minutes
    [InlineData(14, 44, 0, 0, 1, 0, 0, 0)]       // At :44:00.000 → 1 minute to :45
    public void CalculateDelayUntilNextQuarter_ReturnsCorrectDelay(
        int hour, int minute, int second, int millisecond,
        int expectedMinutes, int expectedSeconds, int expectedMilliseconds, int _)
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, hour, minute, second, millisecond, DateTimeKind.Utc);
        var expected = TimeSpan.FromMinutes(expectedMinutes)
            + TimeSpan.FromSeconds(expectedSeconds)
            + TimeSpan.FromMilliseconds(expectedMilliseconds);

        // Act
        var delay = CurrentPriceOperations.CalculateDelayUntilNextQuarter(now);

        // Assert
        Assert.Equal(expected, delay);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsNull_WhenNoPrices()
    {
        // Arrange
        var prices = Array.Empty<PriceRecord>();
        var now = new DateTime(2025, 10, 17, 14, 15, 0, DateTimeKind.Utc);

        // Act
        var result = prices.GetCurrentPrice(now);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsNull_WhenAllPricesInFuture()
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, 14, 0, 0, DateTimeKind.Utc);
        var prices = new[]
        {
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(1), PriceDkk = 100m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(2), PriceDkk = 200m }
        };

        // Act
        var result = prices.GetCurrentPrice(now);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsMostRecentPrice_BeforeOrAtCurrentTime()
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, 14, 30, 0, DateTimeKind.Utc);
        var prices = new[]
        {
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(-2), PriceDkk = 100m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(-1), PriceDkk = 200m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now, PriceDkk = 300m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(1), PriceDkk = 400m }
        };

        // Act
        var result = prices.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(300m, result.PriceDkk);
        Assert.Equal(now, result.TimeUtc);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsExactMatch_WhenTimeMatches()
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, 14, 15, 0, DateTimeKind.Utc);
        var prices = new[]
        {
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddMinutes(-15), PriceDkk = 100m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now, PriceDkk = 200m }
        };

        // Act
        var result = prices.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200m, result.PriceDkk);
        Assert.Equal(now, result.TimeUtc);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsMostRecent_WhenMultiplePricesBeforeNow()
    {
        // Arrange
        var now = new DateTime(2025, 10, 17, 14, 45, 0, DateTimeKind.Utc);
        var prices = new[]
        {
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(-3), PriceDkk = 100m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(-1), PriceDkk = 300m },
            new PriceRecord { PriceArea = "DK1", TimeUtc = now.AddHours(-2), PriceDkk = 200m }
        };

        // Act
        var result = prices.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(300m, result.PriceDkk);
        Assert.Equal(now.AddHours(-1), result.TimeUtc);
    }
}
