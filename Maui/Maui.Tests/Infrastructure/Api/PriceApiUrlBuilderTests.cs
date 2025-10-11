using Maui.Infrastructure.Api;
using Xunit;

namespace Maui.Tests.Infrastructure.Api;

public class PriceApiUrlBuilderTests
{
    [Fact]
    public void BuildDayAheadPricesUrl_ConstructsCorrectUrl()
    {
        // Arrange
        var startDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var url = PriceApiUrlBuilder.BuildDayAheadPricesUrl("DK1", startDate, endDate);
        // Assert
        Assert.Equal(
            "https://api.energidataservice.dk/dataset/DayAheadPrices?offset=0&start=2025-10-11T02%3a00&end=2025-10-12T02%3a00&filter=%7b%22PriceArea%22%3a%5b%22DK1%22%5d%7d&sort=TimeDK+ASC",
            url);
    }

    [Fact]
    public void BuildDayAheadPricesUrl_ConvertsToUtc()
    {
        // Arrange
        var startDate = new DateTime(2025, 10, 11, 14, 30, 0, DateTimeKind.Local);
        var endDate = new DateTime(2025, 10, 12, 14, 30, 0, DateTimeKind.Local);

        // Act
        var url = PriceApiUrlBuilder.BuildDayAheadPricesUrl("DK2", startDate, endDate);

        // Assert
        var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var startDanish = TimeZoneInfo.ConvertTime(startDate, danishTimeZone);
        var endDanish = TimeZoneInfo.ConvertTime(endDate, danishTimeZone);

        var expectedStart = startDanish.ToString("yyyy-MM-dd'T'HH':'mm").Replace(":", "%3a");
        var expectedEnd = endDanish.ToString("yyyy-MM-dd'T'HH':'mm").Replace(":", "%3a");
        Assert.Contains($"start={expectedStart}", url);
        Assert.Contains($"end={expectedEnd}", url);
    }

    [Fact]
    public void BuildDayAheadPricesUrl_IncludesPriceArea()
    {
        // Arrange
        var startDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var url = PriceApiUrlBuilder.BuildDayAheadPricesUrl("SE3", startDate, endDate);

        // Assert
        Assert.Contains("filter=%7b%22PriceArea%22%3a%5b%22SE3%22%5d%7d", url);
    }
}
