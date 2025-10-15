using Maui.Core.Features.PriceGraph;
using Maui.Core.Shared.Models;
using Xunit;

namespace Maui.Tests.Core.Features.PriceGraph;

public class PriceGraphOperationsTests
{
    [Fact]
    public void GetTodayAndTomorrowRange_ReturnsRangeStartingAtMidnightToday()
    {
        // Act
        var (startUtc, endUtc) = PriceGraphOperations.GetTodayAndTomorrowRange();

        // Assert
        var nowLocal = DateTime.Now;
        var expectedStartLocal = nowLocal.Date;
        var expectedEndLocal = expectedStartLocal.AddDays(2);

        Assert.Equal(expectedStartLocal, startUtc.ToLocalTime());
        Assert.Equal(expectedEndLocal, endUtc.ToLocalTime());
    }

    [Fact]
    public void GetTodayAndTomorrowRange_Returns48HourRange()
    {
        // Act
        var (startUtc, endUtc) = PriceGraphOperations.GetTodayAndTomorrowRange();

        // Assert
        var duration = endUtc - startUtc;
        Assert.Equal(TimeSpan.FromHours(48), duration);
    }

    [Fact]
    public void ToPricePoints_FiltersOutNullPrices()
    {
        // Arrange
        var priceRecords = new List<PriceRecord>
        {
            new() { TimeUtc = DateTime.UtcNow, PriceArea = "DK1", PriceDkk = 100m, PriceEur = 13m },
            new() { TimeUtc = DateTime.UtcNow.AddHours(1), PriceArea = "DK1", PriceDkk = null, PriceEur = null },
            new() { TimeUtc = DateTime.UtcNow.AddHours(2), PriceArea = "DK1", PriceDkk = 200m, PriceEur = 27m }
        };

        // Act
        var result = priceRecords.ToPricePoints();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ToPricePoints_ConvertsTimesToLocal()
    {
        // Arrange
        var utcTime = new DateTime(2025, 10, 15, 12, 0, 0, DateTimeKind.Utc);
        var priceRecords = new List<PriceRecord>
        {
            new() { TimeUtc = utcTime, PriceArea = "DK1", PriceDkk = 100m, PriceEur = 13m }
        };

        // Act
        var result = priceRecords.ToPricePoints();

        // Assert
        Assert.Single(result);
        Assert.Equal(utcTime.ToLocalTime(), result[0].Time);
    }

    [Fact]
    public void ToPricePoints_OrdersByTime()
    {
        // Arrange
        var priceRecords = new List<PriceRecord>
        {
            new() { TimeUtc = DateTime.UtcNow.AddHours(2), PriceArea = "DK1", PriceDkk = 300m, PriceEur = 40m },
            new() { TimeUtc = DateTime.UtcNow, PriceArea = "DK1", PriceDkk = 100m, PriceEur = 13m },
            new() { TimeUtc = DateTime.UtcNow.AddHours(1), PriceArea = "DK1", PriceDkk = 200m, PriceEur = 27m }
        };

        // Act
        var result = priceRecords.ToPricePoints();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(100m, result[0].PriceDkk);
        Assert.Equal(200m, result[1].PriceDkk);
        Assert.Equal(300m, result[2].PriceDkk);
    }

    [Fact]
    public void ToPricePoints_PreservesPriceDkkValues()
    {
        // Arrange
        var priceRecords = new List<PriceRecord>
        {
            new() { TimeUtc = DateTime.UtcNow, PriceArea = "DK1", PriceDkk = 123.45m, PriceEur = 16m }
        };

        // Act
        var result = priceRecords.ToPricePoints();

        // Assert
        Assert.Single(result);
        Assert.Equal(123.45m, result[0].PriceDkk);
    }

    [Fact]
    public void ToPricePoints_ReturnsObservableCollection()
    {
        // Arrange
        var priceRecords = new List<PriceRecord>
        {
            new() { TimeUtc = DateTime.UtcNow, PriceArea = "DK1", PriceDkk = 100m, PriceEur = 13m }
        };

        // Act
        var result = priceRecords.ToPricePoints();

        // Assert
        Assert.IsType<System.Collections.ObjectModel.ObservableCollection<PricePoint>>(result);
    }

    [Fact]
    public void FormatTimeRange_FormatsCorrectly()
    {
        // Arrange
        var startUtc = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = new DateTime(2025, 10, 14, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = PriceGraphOperations.FormatTimeRange(startUtc, endUtc);

        // Assert
        var startLocal = startUtc.ToLocalTime();
        var endLocal = endUtc.ToLocalTime();
        var expected = $"{startLocal:dd/MM HH:mm} - {endLocal:dd/MM HH:mm}";
        Assert.Equal(expected, result);
    }
}
