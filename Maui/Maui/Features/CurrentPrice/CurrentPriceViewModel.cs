using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Shared;
using Maui.Shared.Services;

namespace Maui.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject
{
    private readonly IPriceStore _priceStore;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    /// <summary>
    /// Computed property that calculates the current price on-demand from the store.
    /// </summary>
    public decimal? CurrentPriceDKK => GetCurrentPriceFromStore()?.PriceDkk;

    public CurrentPriceViewModel(IPriceStore priceStore)
    {
        _priceStore = priceStore ?? throw new ArgumentNullException(nameof(priceStore));

        // Subscribe to price updates
        _priceStore.PricesUpdated += OnPricesUpdated;
    }

    private void OnPricesUpdated(object? sender, EventArgs e)
    {
        // When prices are updated in the store, notify the UI to re-query the computed property
        OnPropertyChanged(nameof(CurrentPriceDKK));
    }

    private Shared.Models.PriceRecord? GetCurrentPriceFromStore()
    {
        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var dayEnd = dayStart.AddDays(1);

        // Get all prices for today and find the current one
        var prices = _priceStore.GetPrices(PriceArea, dayStart, dayEnd);
        return prices.GetCurrentPrice(now);
    }
}
