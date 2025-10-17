using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;

namespace Maui.Core.Features.CurrentPrice;

public partial class CurrentPriceViewModel : ObservableObject, IDisposable
{
    private readonly IPriceRepository _priceRepository;
    private readonly Func<Infrastructure.Services.ITimer> _timerFactory;
    private Infrastructure.Services.ITimer? _updateTimer;
    private readonly Func<DateTime> _getUtcNow;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    [ObservableProperty]
    private PriceRecord? _currentPrice;

    /// <summary>
    /// Convenience property for accessing just the price value.
    /// </summary>
    public decimal? CurrentPriceDKK => CurrentPrice?.PriceDkk;

    public CurrentPriceViewModel(
        IPriceRepository priceRepository,
        Func<Infrastructure.Services.ITimer> timerFactory,
        Func<DateTime> getUtcNow)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _timerFactory = timerFactory ?? throw new ArgumentNullException(nameof(timerFactory));
        _getUtcNow = getUtcNow ?? throw new ArgumentNullException(nameof(getUtcNow));

        // Subscribe to price updates from repository
        _priceRepository.PricesUpdated += OnPricesUpdated;

        // Initial load
        RefreshCurrentPrice();

        // Schedule first update at next quarter boundary
        ScheduleNextUpdate();
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

    private void ScheduleNextUpdate()
    {
        // Dispose old timer and create a new one for each reschedule
        _updateTimer?.Dispose();
        _updateTimer = _timerFactory();

        var now = _getUtcNow();
        var delay = CurrentPriceOperations.CalculateDelayUntilNextQuarter(now);
        _updateTimer.Start(delay, OnTimerTick);
    }

    private void OnTimerTick()
    {
        RefreshCurrentPrice();

        // Reschedule for next quarter
        ScheduleNextUpdate();
    }

    public void Dispose()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
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
    public static PriceRecord? GetCurrentPrice(
        this IEnumerable<PriceRecord> records,
        DateTime? now = null
    ) => records
        .Where(r => r.TimeUtc <= (now ?? DateTime.UtcNow))
        .OrderByDescending(r => r.TimeUtc)
        .FirstOrDefault();
}
