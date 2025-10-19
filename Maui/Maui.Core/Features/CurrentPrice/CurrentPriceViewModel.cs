using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;

namespace Maui.Core.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject, ICurrentPriceViewModel
{
    private readonly IPriceRepository _priceRepository;
    private readonly ITaskDelayer _taskDelayer;
    private readonly Func<DateTime> _getUtcNow;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _updateTask;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    [ObservableProperty]
    private PriceRecord? _currentPrice;

    /// <summary>
    /// Convenience property for accessing just the price value.
    /// </summary>
    public decimal? CurrentPriceDKK => CurrentPrice?.PriceDkk;

    partial void OnCurrentPriceChanged(PriceRecord? value)
    {
        OnPropertyChanged(nameof(CurrentPriceDKK));
    }

    public CurrentPriceViewModel(
        IPriceRepository priceRepository,
        ITaskDelayer taskDelayer,
        Func<DateTime> getUtcNow)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _taskDelayer = taskDelayer ?? throw new ArgumentNullException(nameof(taskDelayer));
        _getUtcNow = getUtcNow ?? throw new ArgumentNullException(nameof(getUtcNow));

        // Subscribe to price updates from repository
        _priceRepository.PricesUpdated += OnPricesUpdated;

        // Initial load
        RefreshCurrentPrice();

        // Start the update loop
        _updateTask = RunUpdateLoopAsync(_cancellationTokenSource.Token);
    }

    private void OnPricesUpdated(object? sender, EventArgs e)
    {
        // When prices are updated in the repository, refresh current price
        RefreshCurrentPrice();
    }

    private void RefreshCurrentPrice()
    {
        var now = _getUtcNow();
        var (dayStart, dayEnd) = now.GetTodayDateRange();

        // Get all prices for today and find the current one
        var prices = _priceRepository.GetPrices(PriceArea, dayStart, dayEnd);
        CurrentPrice = prices.GetCurrentPrice(now);
    }

    private async Task RunUpdateLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var now = _getUtcNow();
                var delay = CurrentPriceOperations.CalculateDelayUntilNextQuarter(now);
                await _taskDelayer.DelayAsync(delay, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    RefreshCurrentPrice();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disposed
                break;
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _priceRepository.PricesUpdated -= OnPricesUpdated;
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
    /// Calculates the time delay until the next 15-minute quarter boundary.
    /// Quarter boundaries are: 00, 15, 30, 45 minutes past the hour.
    /// </summary>
    public static TimeSpan CalculateDelayUntilNextQuarter(DateTime now)
    {
        var currentMinute = now.Minute;
        var currentSecond = now.Second;
        var currentMillisecond = now.Millisecond;

        // Calculate minutes until next quarter (0, 15, 30, 45)
        var minutesUntilNextQuarter = 15 - (currentMinute % 15);

        // If we're exactly on a quarter boundary, next quarter is 15 minutes away
        if (minutesUntilNextQuarter == 15 && currentSecond == 0 && currentMillisecond == 0)
        {
            return TimeSpan.FromMinutes(15);
        }

        // Calculate total delay including seconds and milliseconds
        var totalSeconds = (minutesUntilNextQuarter * 60) - currentSecond;
        var totalMilliseconds = (totalSeconds * 1000) - currentMillisecond;

        return TimeSpan.FromMilliseconds(totalMilliseconds);
    }

    /// <summary>
    /// Gets the current price from price records.
    /// </summary>
    /// <param name="records">Price records to search</param>
    /// <param name="nowUtc">Current UTC time (optional, defaults to DateTime.UtcNow)</param>
    public static PriceRecord? GetCurrentPrice(
        this IEnumerable<PriceRecord> records,
        DateTime? nowUtc = null
    ) => records
        .Where(r => r.TimeUtc <= (nowUtc ?? DateTime.UtcNow))
        .OrderByDescending(r => r.TimeUtc)
        .FirstOrDefault();
}
