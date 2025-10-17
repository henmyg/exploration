using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;
using Microsoft.Extensions.Hosting;

namespace Maui.Core.Shared.Services;

/// <summary>
/// Background service that automatically syncs prices at regular intervals.
/// </summary>
public class BackgroundPriceSyncService : IHostedService, IDisposable
{
    private readonly IPriceSyncService _syncService;
    private readonly ITaskDelayer _taskDelayer;
    private readonly IPriceRepository _priceRepository;
    private readonly Func<DateTime> _getNow;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _syncTask;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    public BackgroundPriceSyncService(
        IPriceSyncService syncService,
        ITaskDelayer taskDelayer,
        IPriceRepository priceRepository,
        Func<DateTime>? getNow = null)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _taskDelayer = taskDelayer ?? throw new ArgumentNullException(nameof(taskDelayer));
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _getNow = getNow ?? (() => DateTime.Now);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Start the sync loop
        _syncTask = RunSyncLoopAsync(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private async Task RunSyncLoopAsync(CancellationToken cancellationToken)
    {
        // Sync immediately on startup
        await SyncPricesAsync();

        // Then sync at the daily price publication time
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Wait until price publication time
                var now = _getNow();
                var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);
                await _taskDelayer.DelayAsync(delay, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    await SyncPricesAsync();

                    // Inner retry loop - keep trying hourly until we get tomorrow's prices
                    // Stop if we're too close to next publication time to avoid overlap
                    while (!cancellationToken.IsCancellationRequested &&
                           !HasTomorrowPrices() &&
                           !BackgroundPriceSyncOperations.IsTooCloseTo1305(_getNow()))
                    {
                        await _taskDelayer.DelayAsync(TimeSpan.FromHours(1), cancellationToken);

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await SyncPricesAsync();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
        }
    }

    private bool HasTomorrowPrices()
    {
        var expectedDate = BackgroundPriceSyncOperations.GetExpectedPriceDate(_getNow());
        var prices = _priceRepository.GetPrices(PriceArea, expectedDate, expectedDate.AddMinutes(1));
        return prices.Any();
    }

    private async Task SyncPricesAsync()
    {
        try
        {
            await _syncService.SyncCurrentAndUpcomingPricesAsync(PriceArea);
        }
        catch (Exception ex)
        {
            // Log error but don't crash the app
            // In production, use proper logging (ILogger)
            System.Diagnostics.Debug.WriteLine($"Background sync failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Pure functions for background price sync operations.
/// </summary>
public static class BackgroundPriceSyncOperations
{
    /// <summary>
    /// The time when tomorrow's electricity prices are published (13:05 local time).
    /// This is when Energi Data Service releases the next day's prices.
    /// </summary>
    public static readonly TimeOnly PricePublicationTime = new(13, 5);

    /// <summary>
    /// Calculates the delay until the next occurrence of the price publication time.
    /// If current time is before the publication time today, returns delay until today.
    /// If current time is at or after the publication time today, returns delay until tomorrow.
    /// </summary>
    public static TimeSpan CalculateDelayUntilNext1305(DateTime now)
    {
        var target1305Today = new DateTime(now.Year, now.Month, now.Day,
            PricePublicationTime.Hour, PricePublicationTime.Minute, 0, now.Kind);

        // If we haven't reached 13:05 today yet, wait until today at 13:05
        if (now < target1305Today)
        {
            return target1305Today - now;
        }

        // Otherwise, wait until tomorrow at 13:05
        var target1305Tomorrow = target1305Today.AddDays(1);
        return target1305Tomorrow - now;
    }

    /// <summary>
    /// Determines which day's prices we should expect to have based on current time.
    /// Before publication time: Should have today's prices (released yesterday)
    /// After publication time: Should have tomorrow's prices (just released)
    /// Returns the target date at 11:00 AM UTC to avoid midnight edge cases.
    /// </summary>
    public static DateTime GetExpectedPriceDate(DateTime now)
    {
        var publicationTimeToday = new DateTime(now.Year, now.Month, now.Day,
            PricePublicationTime.Hour, PricePublicationTime.Minute, 0, now.Kind);

        // Before publication time: We should have today's prices (released yesterday)
        if (now < publicationTimeToday)
        {
            return new DateTime(now.Year, now.Month, now.Day, 11, 0, 0, DateTimeKind.Utc);
        }

        // After publication time: We should have tomorrow's prices (just released)
        var tomorrow = now.Date.AddDays(1);
        return new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 11, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Checks if current time is too close (less than 1 hour) to the next price publication time.
    /// Used to avoid starting hourly retries that would overlap with the next scheduled sync.
    /// </summary>
    public static bool IsTooCloseTo1305(DateTime now)
    {
        return CalculateDelayUntilNext1305(now) < TimeSpan.FromHours(1);
    }
}
