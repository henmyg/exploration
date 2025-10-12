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

    [ObservableProperty]
    private decimal? _currentPriceDKK;

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
        CurrentPriceDKK = null;

        try
        {
            // First, try to get from store
            var currentPrice = GetCurrentPriceFromStore();

            if (currentPrice == null)
            {
                // If not in store, sync from API
                await _syncService.SyncCurrentAndUpcomingPricesAsync(PriceArea);

                // Try to get from store again
                currentPrice = GetCurrentPriceFromStore();
            }

            if (currentPrice != null)
            {
                CurrentPriceDKK = currentPrice.PriceDkk;
            }
            else
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
        // When prices are updated in the store, refresh the current price
        var currentPrice = GetCurrentPriceFromStore();
        if (currentPrice != null)
        {
            CurrentPriceDKK = currentPrice.PriceDkk;
        }
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
