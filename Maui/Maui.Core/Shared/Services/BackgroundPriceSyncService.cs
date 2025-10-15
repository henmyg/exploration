using Microsoft.Extensions.Hosting;

namespace Maui.Core.Shared.Services;

/// <summary>
/// Background service that automatically syncs prices at regular intervals.
/// </summary>
public class BackgroundPriceSyncService(IPriceSyncService syncService) : IHostedService, IDisposable
{
    private readonly IPriceSyncService _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
    private Timer? _timer;
    private const string PriceArea = "DK1"; // Hardcoded for now, will be configurable later

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Sync immediately on startup, then every hour
        _timer = new Timer(
            callback: async _ => await SyncPricesAsync(),
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromHours(1)
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
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
        _timer?.Dispose();
        _timer = null;
        GC.SuppressFinalize(this);
    }
}
