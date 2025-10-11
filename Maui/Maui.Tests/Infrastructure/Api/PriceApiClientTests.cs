using System.Net;
using System.Text;
using System.Text.Json;
using Maui.Infrastructure.Api;
using Maui.Infrastructure.Api.Models;
using Xunit;

namespace Maui.Tests.Infrastructure.Api;

public class PriceApiClientTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }

    [Fact]
    public async Task GetDayAheadPricesAsync_DeserializesResponse()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(_ =>
        {
            var json = """
            {
                "records": [
                    {
                        "TimeUTC": "2025-10-11T12:00:00",
                        "TimeDK": "2025-10-11T14:00:00",
                        "PriceArea": "DK1",
                        "DayAheadPriceEUR": 50.10,
                        "DayAheadPriceDKK": 373.75
                    },
                    {
                        "TimeUTC": "2025-10-11T13:00:00",
                        "TimeDK": "2025-10-11T15:00:00",
                        "PriceArea": "DK1",
                        "DayAheadPriceEUR": 52.30,
                        "DayAheadPriceDKK": 390.20
                    }
                ]
            }
            """;
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        var httpClient = new HttpClient(mockHandler);

        var startDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await PriceApiClient.GetDayAheadPricesAsync(httpClient, "DK1", startDate, endDate);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Records.Count);
        Assert.Equal("DK1", response.Records[0].PriceArea);
        Assert.Equal(50.10m, response.Records[0].DayAheadPriceEUR);
        Assert.Equal(373.75m, response.Records[0].DayAheadPriceDKK);
    }

    [Fact]
    public async Task ExtensionMethod_CallsStaticMethod()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler(_ =>
        {
            var json = """
            {
                "records": [
                    {
                        "TimeUTC": "2025-10-11T12:00:00",
                        "TimeDK": "2025-10-11T14:00:00",
                        "PriceArea": "DK2",
                        "DayAheadPriceEUR": 45.00,
                        "DayAheadPriceDKK": 335.50
                    }
                ]
            }
            """;
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        });

        var httpClient = new HttpClient(mockHandler);

        var startDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await httpClient.GetDayAheadPricesAsync("DK2", startDate, endDate);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Records);
        Assert.Equal("DK2", response.Records[0].PriceArea);
        Assert.Equal(45.00m, response.Records[0].DayAheadPriceEUR);
    }
}
