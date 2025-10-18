using Maui.AcceptanceTests.CurrentPrice;
using Maui.Core.Features.PriceGraph;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;

namespace Maui.AcceptanceTests.PriceGraph;

/// <summary>
/// Acceptance tests for the Price Graph feature
/// Tests the entire stack using real repository and ViewModel
/// </summary>
public class PriceGraphTests
{
    /// <summary>
    /// Creates a repository populated with price data for every 15-minute quarter
    /// from yesterday until the day after tomorrow.
    /// Price formula: dayIndex * 1000 + hour * 100 + minutes
    /// Example: Day 1, 14:15 = 1000 + 1400 + 15 = 2415
    /// </summary>
    private static InMemoryPriceRepository CreatePopulatedRepository(DateTime referenceDateUtc)
    {
        var repository = new InMemoryPriceRepository();
        var prices = new List<PriceRecord>();

        // Generate prices for 4 days: yesterday, today, tomorrow, day after tomorrow
        for (int dayOffset = -1; dayOffset <= 2; dayOffset++)
        {
            var dateUtc = referenceDateUtc.Date.AddDays(dayOffset);
            var dayIndex = dayOffset + 1; // -1 becomes 0, 0 becomes 1, 1 becomes 2, 2 becomes 3

            // Generate prices for every 15-minute quarter
            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15)
                {
                    var timeUtc = new DateTime(dateUtc.Year, dateUtc.Month, dateUtc.Day, hour, minute, 0, DateTimeKind.Utc);
                    var price = dayIndex * 1000 + hour * 100 + minute;

                    prices.Add(new PriceRecord
                    {
                        PriceArea = "DK1",
                        TimeUtc = timeUtc,
                        PriceDkk = price
                    });
                }
            }
        }

        repository.StorePrices(prices);
        return repository;
    }

    [Fact]
    public void PriceGraph_ShowsAllAvailablePricesFromTodayAndTomorrow()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeDelayer = new FakeTaskDelayer();

        // Act - Create ViewModel which loads prices from repository
        var viewModel = new PriceGraphViewModel(repository, fakeDelayer);

        // Assert - ChartData should have all prices from today (96 quarters) and tomorrow (96 quarters)
        Assert.Equal(192, viewModel.ChartData.Length);

        // Verify first and last prices span two days (today and tomorrow)
        var firstTime = viewModel.ChartData[0].DateTime;
        var lastTime = viewModel.ChartData[^1].DateTime;

        // Should span from 00:00 today to 23:45 tomorrow (approximately 48 hours minus 15 minutes)
        var timeSpan = lastTime - firstTime;
        Assert.Equal(TimeSpan.FromHours(47).Add(TimeSpan.FromMinutes(45)), timeSpan);

        // Verify first price is at midnight (00:00)
        Assert.Equal(0, firstTime.Hour);
        Assert.Equal(0, firstTime.Minute);

        // Verify last price is at 23:45
        Assert.Equal(23, lastTime.Hour);
        Assert.Equal(45, lastTime.Minute);

        viewModel.Dispose();
    }

    [Fact]
    public void PriceGraph_Displays15MinuteIntervalPrices()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeDelayer = new FakeTaskDelayer();

        // Act - Create ViewModel which loads prices
        var viewModel = new PriceGraphViewModel(repository, fakeDelayer);

        // Assert - ChartData should have data for today and tomorrow (192 total)
        Assert.Equal(192, viewModel.ChartData.Length);

        // Verify each price is exactly 15 minutes apart
        for (int i = 1; i < viewModel.ChartData.Length; i++)
        {
            var timeDiff = viewModel.ChartData[i].DateTime - viewModel.ChartData[i - 1].DateTime;
            Assert.Equal(TimeSpan.FromMinutes(15), timeDiff);
        }

        // Verify prices are at quarter boundaries (00, 15, 30, 45)
        foreach (var dataPoint in viewModel.ChartData)
        {
            Assert.True(dataPoint.DateTime.Minute % 15 == 0,
                $"Price at {dataPoint.DateTime} is not on a 15-minute boundary");
        }

        viewModel.Dispose();
    }

    [Fact]
    public void PriceGraph_UpdatesWhenNewPriceDataAvailable()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);
        var currentTimeLocal = currentTimeUtc.ToLocalTime();
        var repository = new InMemoryPriceRepository();
        var fakeDelayer = new FakeTaskDelayer();
        Func<DateTime> getNow = () => currentTimeLocal;

        var todayStart = currentTimeUtc.Date;

        // Create ViewModel with empty repository
        var viewModel = new PriceGraphViewModel(repository, fakeDelayer, getNow);

        // Initially, ChartData should be empty
        Assert.Empty(viewModel.ChartData);

        // Act - Add new price data (triggers PricesUpdated event)
        var newPrices = new List<PriceRecord>
        {
            new() { PriceArea = "DK1", TimeUtc = todayStart, PriceDkk = 1000m },
            new() { PriceArea = "DK1", TimeUtc = todayStart.AddMinutes(15), PriceDkk = 1015m }
        };
        repository.StorePrices(newPrices);

        // Assert - ViewModel ChartData should now have the new prices
        Assert.Equal(2, viewModel.ChartData.Length);
        Assert.Equal(1000.0, viewModel.ChartData[0].Value);
        Assert.Equal(1015.0, viewModel.ChartData[1].Value);

        viewModel.Dispose();
    }

    [Fact]
    public void PriceGraph_ShowsNowMarker()
    {
        // Arrange
        var currentTime = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Local);
        var repository = CreatePopulatedRepository(currentTime.ToUniversalTime());
        var fakeDelayer = new FakeTaskDelayer();
        Func<DateTime> getNow = () => currentTime;

        // Act - Create ViewModel which sets Now property
        var viewModel = new PriceGraphViewModel(repository, fakeDelayer, getNow);

        // Assert - Now property should be set to the current time
        Assert.Equal(currentTime.Ticks, viewModel.Now);

        viewModel.Dispose();
    }

    [Theory]
    [InlineData(14, 17)] // 14:17 - in the middle of a quarter
    [InlineData(14, 15)] // 14:15 - exactly on a quarter boundary
    [InlineData(14, 29)] // 14:29 - just before next quarter
    [InlineData(0, 0)]   // 00:00 - start of day
    [InlineData(23, 59)] // 23:59 - end of day
    public async Task PriceGraph_NowMarkerPosition_UpdatesEveryMinute(int hour, int minute)
    {
        // Arrange
        var initialTime = new DateTime(2025, 10, 17, hour, minute, 0, DateTimeKind.Local);
        var currentTime = initialTime;
        var repository = CreatePopulatedRepository(initialTime.ToUniversalTime());
        var fakeDelayer = new FakeTaskDelayer();
        Func<DateTime> getNow = () => currentTime;

        // Act - Create ViewModel
        var viewModel = new PriceGraphViewModel(repository, fakeDelayer, getNow);

        // Initial Now should be set to initialTime
        Assert.Equal(initialTime.Ticks, viewModel.Now);

        // Wait for the delay task to be initiated (1 minute update loop)
        await Task.Delay(50);

        // Assert - Delay should be requested for 1 minute
        Assert.True(fakeDelayer.IsDelaying);
        Assert.Equal(TimeSpan.FromMinutes(1), fakeDelayer.RequestedDelay);

        // Simulate 1 minute passing - update time
        currentTime = initialTime.AddMinutes(1);
        fakeDelayer.CompleteDelay();
        await Task.Delay(50); // Give update time to process

        // Now property should have been updated to the new time
        Assert.Equal(currentTime.Ticks, viewModel.Now);

        viewModel.Dispose();
    }
}
