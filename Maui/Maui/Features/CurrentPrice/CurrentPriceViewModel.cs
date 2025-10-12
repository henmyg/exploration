using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Features.PriceSync;
using Maui.Shared;
using Maui.Shared.Services;

namespace Maui.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject
{
    private readonly IPriceStore _priceStore;
    private readonly PriceSyncService _syncService;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    /// <summary>
    /// Computed property that calculates the current price on-demand from the store.
    /// </summary>
    public decimal? CurrentPriceDKK => GetCurrentPriceFromStore()?.PriceDkk;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CurrentPriceViewModel(IPriceStore priceStore, PriceSyncService syncService)
    {
        _priceStore = priceStore ?? throw new ArgumentNullException(nameof(priceStore));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

        // Subscribe to price updates
        _priceStore.PricesUpdated += OnPricesUpdated;
    }

    [RelayCommand]
    private async Task LoadCurrentPriceAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // First, check if we have current price in store
            if (CurrentPriceDKK == null)
            {
                // If not in store, sync from API
                // The PricesUpdated event will automatically notify CurrentPriceDKK changed
                await _syncService.SyncCurrentAndUpcomingPricesAsync(PriceArea);
            }

            // Check if we now have a price
            if (CurrentPriceDKK == null)
            {
                ErrorMessage = "No current price available";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading price: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
