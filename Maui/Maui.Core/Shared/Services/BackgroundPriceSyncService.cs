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
    private readonly Func<DateTime> _getNow;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _syncTask;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    public BackgroundPriceSyncService(
        IPriceSyncService syncService,
        ITaskDelayer taskDelayer,
        Func<DateTime>? getNow = null)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _taskDelayer = taskDelayer ?? throw new ArgumentNullException(nameof(taskDelayer));
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

        // Then sync at 13:05 daily
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var now = _getNow();
                var delay = BackgroundPriceSyncOperations.CalculateDelayUntilNext1305(now);
                await _taskDelayer.DelayAsync(delay, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    await SyncPricesAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
        }
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
internal static class BackgroundPriceSyncOperations
{
    /// <summary>
    /// Calculates the delay until the next occurrence of 13:05.
    /// If current time is before 13:05 today, returns delay until today at 13:05.
    /// If current time is at or after 13:05 today, returns delay until tomorrow at 13:05.
    /// </summary>
    public static TimeSpan CalculateDelayUntilNext1305(DateTime now)
    {
        var target1305Today = new DateTime(now.Year, now.Month, now.Day, 13, 5, 0, now.Kind);

        // If we haven't reached 13:05 today yet, wait until today at 13:05
        if (now < target1305Today)
        {
            return target1305Today - now;
        }

        // Otherwise, wait until tomorrow at 13:05
        var target1305Tomorrow = target1305Today.AddDays(1);
        return target1305Tomorrow - now;
    }
}
