using Maui.Infrastructure.Api.Models;
using Maui.Shared;
using Xunit;

namespace Maui.Tests.Shared;

public class PriceHelperTests
{
    [Fact]
    public void GetCurrentPrice_ReturnsNewestRecordBeforeNow()
    {
        // Arrange
        var now = new DateTime(2025, 10, 11, 12, 0, 0, DateTimeKind.Utc);
        var records = new[]
        {
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(-5),
                PriceArea = "DK1",
                DayAheadPriceDKK = 100m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(-4),
                PriceArea = "DK1",
                DayAheadPriceDKK = 150m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(-3),
                PriceArea = "DK1",
                DayAheadPriceDKK = 175m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(-2),
                PriceArea = "DK1",
                DayAheadPriceDKK = 225m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(-1),
                PriceArea = "DK1",
                DayAheadPriceDKK = 200m  // This is the correct answer
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(1),
                PriceArea = "DK1",
                DayAheadPriceDKK = 250m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(2),
                PriceArea = "DK1",
                DayAheadPriceDKK = 300m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(3),
                PriceArea = "DK1",
                DayAheadPriceDKK = 350m
            }
        };

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200m, result.DayAheadPriceDKK);
        Assert.Equal(now.AddHours(-1), result.TimeUTC);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsNullWhenNoRecordsBeforeNow()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var records = new[]
        {
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(1),
                PriceArea = "DK1",
                DayAheadPriceDKK = 100m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(2),
                PriceArea = "DK1",
                DayAheadPriceDKK = 200m
            }
        };

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsNullForEmptyList()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var records = Array.Empty<DayAheadPriceRecord>();

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetCurrentPrice_HandlesRecordAtExactCurrentTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var records = new[]
        {
            new DayAheadPriceRecord
            {
                TimeUTC = now,
                PriceArea = "DK1",
                DayAheadPriceDKK = 100m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = now.AddHours(1),
                PriceArea = "DK1",
                DayAheadPriceDKK = 200m
            }
        };

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100m, result.DayAheadPriceDKK);
    }
}
