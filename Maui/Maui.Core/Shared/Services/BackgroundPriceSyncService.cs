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
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _syncTask;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    public BackgroundPriceSyncService(IPriceSyncService syncService, ITaskDelayer taskDelayer)
    {
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _taskDelayer = taskDelayer ?? throw new ArgumentNullException(nameof(taskDelayer));
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

        // Then sync every hour
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _taskDelayer.DelayAsync(TimeSpan.FromHours(1), cancellationToken);

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
