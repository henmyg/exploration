using Maui.Features.PriceSync;
using Maui.Infrastructure.Api.Models;
using Xunit;

namespace Maui.Tests.Features.PriceSync;

public class PriceSyncOperationsTests
{
    [Fact]
    public void ToPriceRecord_ConvertsApiRecordCorrectly()
    {
        // Arrange
        var timeUtc = new DateTime(2025, 10, 11, 12, 0, 0, DateTimeKind.Utc);
        var timeDk = timeUtc.AddHours(1);
        var apiRecord = new DayAheadPriceRecord
        {
            TimeUTC = timeUtc,
            TimeDK = timeDk,
            PriceArea = "DK1",
            DayAheadPriceDKK = 123.45m,
            DayAheadPriceEUR = 16.54m
        };

        // Act
        var result = apiRecord.ToPriceRecord();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(timeUtc, result.TimeUtc);
        Assert.Equal(timeDk, result.TimeDk);
        Assert.Equal("DK1", result.PriceArea);
        Assert.Equal(123.45m, result.PriceDkk);
        Assert.Equal(16.54m, result.PriceEur);
    }

    [Fact]
    public void ToPriceRecord_HandlesNullPrices()
    {
        // Arrange
        var timeUtc = new DateTime(2025, 10, 11, 12, 0, 0, DateTimeKind.Utc);
        var apiRecord = new DayAheadPriceRecord
        {
            TimeUTC = timeUtc,
            TimeDK = timeUtc.AddHours(1),
            PriceArea = "DK1",
            DayAheadPriceDKK = null,
            DayAheadPriceEUR = null
        };

        // Act
        var result = apiRecord.ToPriceRecord();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PriceDkk);
        Assert.Null(result.PriceEur);
    }

    [Fact]
    public void ToPriceRecords_ConvertsMultipleRecords_AndPreservesOrder()
    {
        // Arrange
        var baseTime = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var apiRecords = new[]
        {
            new DayAheadPriceRecord
            {
                TimeUTC = baseTime,
                TimeDK = baseTime.AddHours(1),
                PriceArea = "DK1",
                DayAheadPriceDKK = 100m,
                DayAheadPriceEUR = 13.40m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = baseTime.AddHours(1),
                TimeDK = baseTime.AddHours(2),
                PriceArea = "DK1",
                DayAheadPriceDKK = 150m,
                DayAheadPriceEUR = 20.10m
            },
            new DayAheadPriceRecord
            {
                TimeUTC = baseTime.AddHours(2),
                TimeDK = baseTime.AddHours(3),
                PriceArea = "DK2",
                DayAheadPriceDKK = 200m,
                DayAheadPriceEUR = 26.80m
            }
        };

        // Act
        var result = apiRecords.ToPriceRecords().ToList();

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(baseTime, result[0].TimeUtc);
        Assert.Equal("DK1", result[0].PriceArea);
        Assert.Equal(100m, result[0].PriceDkk);

        Assert.Equal(baseTime.AddHours(1), result[1].TimeUtc);
        Assert.Equal("DK1", result[1].PriceArea);
        Assert.Equal(150m, result[1].PriceDkk);

        Assert.Equal(baseTime.AddHours(2), result[2].TimeUtc);
        Assert.Equal("DK2", result[2].PriceArea);
        Assert.Equal(200m, result[2].PriceDkk);
    }

    [Fact]
    public void ToPriceRecords_HandlesEmptyCollection()
    {
        // Arrange
        var apiRecords = Array.Empty<DayAheadPriceRecord>();

        // Act
        var result = apiRecords.ToPriceRecords().ToList();

        // Assert
        Assert.Empty(result);
    }
}
