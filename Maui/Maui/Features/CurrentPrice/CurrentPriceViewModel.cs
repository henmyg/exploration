using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Shared;
using Maui.Shared.Repositories;

namespace Maui.Features.CurrentPrice;

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

    private Shared.Models.PriceRecord? GetCurrentPriceFromRepository()
    {
        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var dayEnd = dayStart.AddDays(1);

        // Get all prices for today and find the current one
        var prices = _priceRepository.GetPrices(PriceArea, dayStart, dayEnd);
        return prices.GetCurrentPrice(now);
    }
}
