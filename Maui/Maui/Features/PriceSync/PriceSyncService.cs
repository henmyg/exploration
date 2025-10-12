using Maui.Infrastructure.Api;
using Maui.Shared.Models;
using Maui.Shared.Services;

namespace Maui.Features.PriceSync;

/// <summary>
/// Service responsible for syncing price data from the API to the in-memory store.
/// </summary>
public class PriceSyncService
{
    private readonly HttpClient _httpClient;
    private readonly IPriceStore _priceStore;

    public PriceSyncService(HttpClient httpClient, IPriceStore priceStore)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _priceStore = priceStore ?? throw new ArgumentNullException(nameof(priceStore));
    }

    /// <summary>
    /// Syncs prices for the specified price area and date range.
    /// </summary>
    public async Task SyncPricesAsync(string priceArea, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(priceArea))
            throw new ArgumentException("Price area cannot be null or empty", nameof(priceArea));

        // Fetch prices from API
        var response = await _httpClient.GetDayAheadPricesAsync(
            priceArea,
            startDate,
            endDate,
            cancellationToken);

        if (response?.Records == null || response.Records.Count == 0)
            return;

        // Convert API records to app-specific records and store them
        _priceStore.StorePrices(response.Records.ToPriceRecords());
    }

    /// <summary>
    /// Syncs prices for today and tomorrow for the specified price area.
    /// </summary>
    public async Task SyncCurrentAndUpcomingPricesAsync(string priceArea, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startDate = now.Date;
        var endDate = startDate.AddDays(2); // Today and tomorrow

        await SyncPricesAsync(priceArea, startDate, endDate, cancellationToken);
    }
}

/// <summary>
/// Pure functions for mapping between API models and app models.
/// </summary>
internal static class PriceSyncOperations
{
    /// <summary>
    /// Converts a single API price record to an app-specific price record.
    /// </summary>
    public static PriceRecord ToPriceRecord(this Infrastructure.Api.Models.DayAheadPriceRecord apiRecord)
    {
        return new PriceRecord
        {
            TimeUtc = apiRecord.TimeUTC,
            TimeDk = apiRecord.TimeDK,
            PriceArea = apiRecord.PriceArea,
            PriceDkk = apiRecord.DayAheadPriceDKK,
            PriceEur = apiRecord.DayAheadPriceEUR
        };
    }

    /// <summary>
    /// Converts API price records to app-specific price records.
    /// </summary>
    public static IEnumerable<PriceRecord> ToPriceRecords(this IEnumerable<Infrastructure.Api.Models.DayAheadPriceRecord> apiRecords)
    {
        return apiRecords.Select(r => r.ToPriceRecord());
    }
}
