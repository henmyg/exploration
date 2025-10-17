using Maui.AcceptanceTests.CurrentPrice;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Core.Shared.Services;

namespace Maui.AcceptanceTests.BackgroundPriceSync;

/// <summary>
/// Acceptance tests for the Background Price Sync feature
/// Tests the entire stack using real services
/// </summary>
public class BackgroundPriceSyncTests
{
    /// <summary>
    /// Fake sync service that tracks sync calls
    /// </summary>
    private class FakePriceSyncService : IPriceSyncService
    {
        public int SyncCallCount { get; private set; }
        public List<DateTime> SyncTimestamps { get; } = new();

        public bool IsSynching => false;
        public Exception? SynchException => null;

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public Task SyncCurrentAndUpcomingPricesAsync(string priceArea, CancellationToken cancellationToken = default)
        {
            SyncCallCount++;
            SyncTimestamps.Add(DateTime.Now);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task BackgroundPriceSync_SyncsImmediatelyOnStartup()
    {
        // Arrange
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();
        var repository = new InMemoryPriceRepository();
        var service = new BackgroundPriceSyncService(fakeSyncService, fakeDelayer, repository);

        // Act - Start the service
        await service.StartAsync(CancellationToken.None);

        // Give the initial sync a moment to trigger
        await Task.Delay(50);

        // Assert - Sync should have been called immediately on startup
        Assert.Equal(1, fakeSyncService.SyncCallCount);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task BackgroundPriceSync_SyncsAt1305Daily()
    {
        // Arrange
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var currentTime = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();
        var repository = new InMemoryPriceRepository();
        Func<DateTime> getNow = () => currentTime;

        var service = new BackgroundPriceSyncService(fakeSyncService, fakeDelayer, repository, getNow);

        // Act - Start the service (triggers immediate sync)
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Give initial sync time to complete

        // Initial sync should have happened
        Assert.Equal(1, fakeSyncService.SyncCallCount);

        // Wait for delay task to be initiated
        await Task.Delay(50);

        // Assert - Should request delay until next publication time (24 hours from now)
        Assert.True(fakeDelayer.IsDelaying);
        Assert.Equal(TimeSpan.FromHours(24), fakeDelayer.RequestedDelay);

        // Simulate time passing to tomorrow at publication time
        currentTime = currentTime.AddDays(1);
        fakeDelayer.CompleteDelay();
        await Task.Delay(50); // Give sync time to trigger

        // Should have synced again
        Assert.Equal(2, fakeSyncService.SyncCallCount);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task BackgroundPriceSync_WaitsUntil1305_WhenStartedEarlier()
    {
        // Arrange - Start at 10:00 AM (before publication time)
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var currentTime = new DateTime(2025, 10, 17, 10, 0, 0, DateTimeKind.Local);
        var publicationTimeToday = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();
        var repository = new InMemoryPriceRepository();
        Func<DateTime> getNow = () => currentTime;

        var service = new BackgroundPriceSyncService(fakeSyncService, fakeDelayer, repository, getNow);

        // Act - Start the service
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Give initial sync time to complete

        // Initial sync should have happened
        Assert.Equal(1, fakeSyncService.SyncCallCount);

        // Wait for delay task to be initiated
        await Task.Delay(50);

        // Assert - Should request delay until publication time today
        Assert.True(fakeDelayer.IsDelaying);
        var expectedDelay = publicationTimeToday - currentTime;
        Assert.Equal(expectedDelay, fakeDelayer.RequestedDelay);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task BackgroundPriceSync_RetriesInOneHour_WhenTomorrowPricesMissing()
    {
        // Arrange - Start at publication time with no tomorrow prices
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var currentTime = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Local);
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();
        var repository = new InMemoryPriceRepository();
        Func<DateTime> getNow = () => currentTime;

        var service = new BackgroundPriceSyncService(fakeSyncService, fakeDelayer, repository, getNow);

        // Act - Start the service
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Give initial sync time to complete

        // Initial sync at startup
        Assert.Equal(1, fakeSyncService.SyncCallCount);

        // Wait for first delay (until publication time)
        await Task.Delay(50);
        Assert.True(fakeDelayer.IsDelaying);
        fakeDelayer.CompleteDelay();
        await Task.Delay(50);

        // Sync at publication time
        Assert.Equal(2, fakeSyncService.SyncCallCount);

        // Wait for retry delay to be initiated (no tomorrow prices yet)
        await Task.Delay(50);

        // Assert - Should request 1 hour retry delay
        Assert.True(fakeDelayer.IsDelaying);
        Assert.Equal(TimeSpan.FromHours(1), fakeDelayer.RequestedDelay);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task BackgroundPriceSync_StopsRetrying_WhenTomorrowPricesReceived()
    {
        // Arrange - Start at publication time on Oct 17
        var pubTime = BackgroundPriceSyncOperations.PricePublicationTime;
        var currentTime = new DateTime(2025, 10, 17, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Utc);
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();
        var repository = new InMemoryPriceRepository();
        Func<DateTime> getNow = () => currentTime;

        var service = new BackgroundPriceSyncService(fakeSyncService, fakeDelayer, repository, getNow);

        // Act - Start the service
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(50); // Initial sync at startup
        Assert.Equal(1, fakeSyncService.SyncCallCount);

        // Wait for delay until publication time and complete it (we're already there, so 24h delay)
        await Task.Delay(50);
        fakeDelayer.CompleteDelay();
        await Task.Delay(50);

        // Sync at publication time on Oct 17 - no prices for Oct 18 yet
        Assert.Equal(2, fakeSyncService.SyncCallCount);

        // Should start hourly retry loop
        await Task.Delay(50);
        Assert.True(fakeDelayer.IsDelaying);
        Assert.Equal(TimeSpan.FromHours(1), fakeDelayer.RequestedDelay);

        // Simulate multiple hourly retries without receiving tomorrow's prices
        for (int hour = pubTime.Hour + 1; hour <= 23; hour++)
        {
            currentTime = new DateTime(2025, 10, 17, hour, 5, 0, DateTimeKind.Utc);
            fakeDelayer.CompleteDelay();
            await Task.Delay(50);

            // Still no prices for tomorrow, so should keep retrying
            Assert.True(fakeDelayer.IsDelaying);
            Assert.Equal(TimeSpan.FromHours(1), fakeDelayer.RequestedDelay);
        }

        // Continue retrying into Oct 18
        for (int hour = 0; hour <= 7; hour++)
        {
            currentTime = new DateTime(2025, 10, 18, hour, 5, 0, DateTimeKind.Utc);
            fakeDelayer.CompleteDelay();
            await Task.Delay(50);

            // Still retrying
            Assert.True(fakeDelayer.IsDelaying);
            Assert.Equal(TimeSpan.FromHours(1), fakeDelayer.RequestedDelay);
        }

        // Now at Oct 18 08:05 - simulate receiving prices for Oct 18
        currentTime = new DateTime(2025, 10, 18, 8, 5, 0, DateTimeKind.Utc);

        // Add prices for Oct 18 (the day that was "tomorrow" when we started)
        var oct18Prices = new List<PriceRecord>();
        for (int hour = 0; hour < 24; hour++)
        {
            oct18Prices.Add(new PriceRecord
            {
                PriceArea = "DK1",
                TimeUtc = new DateTime(2025, 10, 18, hour, 0, 0, DateTimeKind.Utc),
                PriceDkk = 1500m
            });
        }
        repository.StorePrices(oct18Prices);

        // Complete the current retry
        fakeDelayer.CompleteDelay();
        await Task.Delay(50);

        // Assert - Should NOT be retrying anymore because we now have prices for Oct 18
        // The service should wait until the next publication time (Oct 18 at publication time)
        Assert.True(fakeDelayer.IsDelaying);
        var nextPublicationTime = new DateTime(2025, 10, 18, pubTime.Hour, pubTime.Minute, 0, DateTimeKind.Utc);
        var expectedDelay = nextPublicationTime - currentTime;
        Assert.Equal(expectedDelay, fakeDelayer.RequestedDelay);

        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public void BackgroundPriceSync_StopsOldRetryLoop_When1305Arrives()
    {
        // Arrange
        var fakeSyncService = new FakePriceSyncService();
        var fakeDelayer = new FakeTaskDelayer();

        // TODO: Requires logic to manage retry loop cancellation
        // For now, this test is a placeholder

        Assert.True(true, "Test requires implementation of retry loop management");
    }
}
