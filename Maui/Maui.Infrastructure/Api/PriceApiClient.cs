using System.Text.Json;
using Maui.Infrastructure.Api.Models;

namespace Maui.Infrastructure.Api;

public static class PriceApiClient
{
    public static async Task<DayAheadPricesResponse?> GetDayAheadPricesAsync(
        this HttpClient httpClient,
        string priceArea,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var url = PriceApiUrlBuilder.BuildDayAheadPricesUrl(priceArea, startDate, endDate);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        return await JsonSerializer.DeserializeAsync<DayAheadPricesResponse>(
            stream,
            cancellationToken: cancellationToken);
    }
}
