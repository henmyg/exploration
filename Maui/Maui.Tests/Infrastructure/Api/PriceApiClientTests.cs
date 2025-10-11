using System.Net;
using System.Text;
using Maui.Infrastructure.Api;
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
    public async Task GetDayAheadPricesAsync_ReturnsResponse()
    {
        // Arrange
        var json = File.ReadAllText("Infrastructure/Api/DayAheadPricesExample.json");

        var mockHandler = new MockHttpMessageHandler(_ => Task.FromResult(
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            })
        );

        var httpClient = new HttpClient(mockHandler);
        var startDate = new DateTime(2025, 10, 11, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 10, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var response = await httpClient.GetDayAheadPricesAsync("DK1", startDate, endDate);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(576, response.Records.Count);
    }
}
