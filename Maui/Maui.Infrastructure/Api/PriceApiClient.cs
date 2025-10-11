using System.Text.Json;
using Maui.Infrastructure.Api.Models;

namespace Maui.Infrastructure.Api;

public class PriceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string BaseUrl = "https://api.energidataservice.dk/dataset/";
    private const string Dataset = "Elspotprices";

    public PriceApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false
        };
    }

    public async Task<DayAheadPricesResponse?> GetDayAheadPricesAsync(
        string priceArea,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var startUtc = startDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");
        var endUtc = endDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");

        var url = $"{BaseUrl}{Dataset}?start={startUtc}&end={endUtc}&filter={{\"PriceArea\":[\"{priceArea}\"]}}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<DayAheadPricesResponse>(
            stream,
            _jsonOptions,
            cancellationToken);
    }
}
