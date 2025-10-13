using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure;

namespace Maui.Core.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject
{
    private readonly IPriceRepository _priceRepository;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    /// <summary>
    /// Computed property that calculates the current price on-demand from the repository.
    /// </summary>
    public decimal? CurrentPriceDKK => GetCurrentPriceFromRepository()?.PriceDkk;

    public CurrentPriceViewModel(IPriceRepository priceRepository)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));

        // Subscribe to price updates
        _priceRepository.PricesUpdated += OnPricesUpdated;
    }

    private void OnPricesUpdated(object? sender, EventArgs e)
    {
        // When prices are updated in the repository, notify the UI to re-query the computed property
        OnPropertyChanged(nameof(CurrentPriceDKK));
    }

    private PriceRecord? GetCurrentPriceFromRepository()
    {
        var now = DateTime.UtcNow;
        var (dayStart, dayEnd) = now.GetTodayDateRange();

        // Get all prices for today and find the current one
        var prices = _priceRepository.GetPrices(PriceArea, dayStart, dayEnd);
        return prices.GetCurrentPrice(now);
    }
}

public static class CurrentPriceOperations
{
    public static (DateTime dayStart, DateTime dayEnd) GetTodayDateRange(this DateTime now)
    {
        var dayStart = now.Date;
        var dayEnd = dayStart.AddDays(1);
        return (dayStart, dayEnd);
    }

    /// <summary>
    /// Gets the current price from price records.
    /// </summary>
    public static PriceRecord? GetCurrentPrice(
        this IEnumerable<PriceRecord> records,
        DateTime? now = null
    ) => records
        .Where(r => r.TimeUtc <= (now ?? DateTime.UtcNow))
        .OrderByDescending(r => r.TimeUtc)
        .FirstOrDefault();
}
