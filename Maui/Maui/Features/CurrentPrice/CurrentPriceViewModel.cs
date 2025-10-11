using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Infrastructure.Api;

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
            var hourStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var hourEnd = hourStart.AddHours(1);

            var response = await _httpClient.GetDayAheadPricesAsync("DK1", hourStart, hourEnd);

            if (response?.Records.Count > 0)
            {
                CurrentPriceDKK = response.Records[0].DayAheadPriceDKK;
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
