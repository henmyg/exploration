using Maui.Shared;
using Maui.Shared.Models;
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
            new PriceRecord
            {
                TimeUtc = now.AddHours(-5),
                TimeDk = now.AddHours(-5).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 100m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(-4),
                TimeDk = now.AddHours(-4).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 150m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(-3),
                TimeDk = now.AddHours(-3).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 175m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(-2),
                TimeDk = now.AddHours(-2).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 225m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(-1),
                TimeDk = now.AddHours(-1).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 200m  // This is the correct answer
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(1),
                TimeDk = now.AddHours(1).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 250m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(2),
                TimeDk = now.AddHours(2).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 300m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(3),
                TimeDk = now.AddHours(3).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 350m
            }
        };

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200m, result.PriceDkk);
        Assert.Equal(now.AddHours(-1), result.TimeUtc);
    }

    [Fact]
    public void GetCurrentPrice_ReturnsNullWhenNoRecordsBeforeNow()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var records = new[]
        {
            new PriceRecord
            {
                TimeUtc = now.AddHours(1),
                TimeDk = now.AddHours(1).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 100m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(2),
                TimeDk = now.AddHours(2).AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 200m
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
        var records = Array.Empty<PriceRecord>();

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
            new PriceRecord
            {
                TimeUtc = now,
                TimeDk = now.AddHours(1),
                PriceArea = "DK1",
                PriceDkk = 100m
            },
            new PriceRecord
            {
                TimeUtc = now.AddHours(1),
                TimeDk = now.AddHours(2),
                PriceArea = "DK1",
                PriceDkk = 200m
            }
        };

        // Act
        var result = records.GetCurrentPrice(now);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100m, result.PriceDkk);
    }
}
