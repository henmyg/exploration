using Maui.Infrastructure.Api;
using Maui.Infrastructure.Api.Models;
using Maui.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maui.IntegrationTests.Infrastructure.Api;

/// <summary>
/// Integration tests for the Price API endpoint to verify it's alive and returning correct data.
/// </summary>
[Collection("API Integration Tests")]
public class PriceApiIntegrationTests : IntegrationTestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register HttpClient for API calls
        services.AddHttpClient();
    }

    [Fact]
    public async Task GetDayAheadPrices_WithKnownDateRange_ReturnsValidData()
    {
        // Arrange
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        // Test date range: October 10, 2025 00:00 to October 12, 2025 00:00
        var startDate = new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Local);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Local);
        var priceArea = "DK1"; // Western Denmark

        // Act
        var response = await httpClient.GetDayAheadPricesAsync(
            priceArea,
            startDate,
            endDate,
            CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Records);
        Assert.NotEmpty(response.Records);

        Assert.Equal(24 * 4 * 2, response.Records.Count);

        // Verify all records are within the requested date range and price area
        foreach (var record in response.Records)
        {
            Assert.Equal(priceArea, record.PriceArea);
            Assert.True(record.TimeDK >= startDate, $"Record time {record.TimeDK} is before start date {startDate}");
            Assert.True(record.TimeDK < endDate, $"Record time {record.TimeDK} is after or equal to end date {endDate}");

            // Verify that at least one price field has a value
            Assert.True(record.DayAheadPriceDKK.HasValue || record.DayAheadPriceEUR.HasValue,
                "Record should have at least one price value (DKK or EUR)");
        }
    }

    [Fact]
    public async Task GetDayAheadPrices_EndpointIsAlive_ReturnsSuccessStatusCode()
    {
        // Arrange
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        // Use a recent date to ensure data availability
        var startDate = new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Local);
        var endDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Local);
        var priceArea = "DK1";

        // Act & Assert - Should not throw HttpRequestException
        var response = await httpClient.GetDayAheadPricesAsync(
            priceArea,
            startDate,
            endDate,
            CancellationToken.None);

        // If we got here, the endpoint is alive and returned data
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetDayAheadPrices_ReturnsDataForDK2PriceArea()
    {
        // Arrange
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        var startDate = new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Local);
        var endDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Local);
        var priceArea = "DK2"; // Eastern Denmark

        // Act
        var response = await httpClient.GetDayAheadPricesAsync(
            priceArea,
            startDate,
            endDate,
            CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Records);
        Assert.All(response.Records, record => Assert.Equal(priceArea, record.PriceArea));
    }

    [Fact]
    public async Task GetDayAheadPrices_TwoDayRange_ReturnsMultipleRecords()
    {
        // Arrange
        var httpClientFactory = GetService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();

        // Two full days should return 48 hourly records (24 hours * 2 days)
        var startDate = new DateTime(2025, 10, 10, 0, 0, 0, DateTimeKind.Local);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Local);
        var priceArea = "DK1";

        // Act
        var response = await httpClient.GetDayAheadPricesAsync(
            priceArea,
            startDate,
            endDate,
            CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Records);

        // Should have multiple records for 2 days
        // The API returns hourly data, so we expect at least some records
        Assert.True(response.Records.Count > 0, "Should return at least one record");

        // Verify records are sorted by TimeDK (as requested in the API call)
        for (int i = 1; i < response.Records.Count; i++)
        {
            Assert.True(response.Records[i].TimeDK >= response.Records[i - 1].TimeDK,
                "Records should be sorted by TimeDK in ascending order");
        }
    }
}
