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
        Assert.Contains("https://api.energidataservice.dk/dataset/Elspotprices", url);
        Assert.Contains("start=2025-10-11T00:00", url);
        Assert.Contains("end=2025-10-12T00:00", url);
        Assert.Contains("filter={\"PriceArea\":[\"DK1\"]}", url);
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
        var expectedStart = startDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");
        var expectedEnd = endDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");
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
        Assert.Contains("filter={\"PriceArea\":[\"SE3\"]}", url);
    }
}
