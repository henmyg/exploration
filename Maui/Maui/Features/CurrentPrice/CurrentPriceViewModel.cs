using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Infrastructure.Api;
using Maui.Shared;

namespace Maui.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private decimal? _currentPriceDKK;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public CurrentPriceViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [RelayCommand]
    private async Task LoadCurrentPriceAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        CurrentPriceDKK = null;

        try
        {
            var now = DateTime.UtcNow;
            var dayStart = now.Date;
            var dayEnd = dayStart.AddDays(1);

            var response = await _httpClient.GetDayAheadPricesAsync("DK1", dayStart, dayEnd);

            if (response?.Records != null)
            {
                var currentPrice = PriceHelper.GetCurrentPrice(response.Records);
                if (currentPrice != null)
                {
                    CurrentPriceDKK = currentPrice.DayAheadPriceDKK;
                }
                else
                {
                    ErrorMessage = "No current price available";
                }
            }
            else
            {
                ErrorMessage = "No price data available";
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
}
