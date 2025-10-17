using Maui.Core.Features.CurrentPrice;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;

namespace Maui.AcceptanceTests.CurrentPrice;

/// <summary>
/// Acceptance tests for the Current Price feature
/// Tests the entire stack using real repository
/// </summary>
public class CurrentPriceTests
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
    public void CurrentPrice_UpdatesEvery15Minutes()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;

        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Assert timer was started
        Assert.True(fakeTimer.IsStarted);

        // Initial price should be loaded
        Assert.NotNull(viewModel.CurrentPrice);

        // Act - Simulate timer firing by triggering the callback
        fakeTimer.Trigger();

        // Assert - Price should still be available (even if same value)
        Assert.NotNull(viewModel.CurrentPrice);
        Assert.Equal(1, fakeTimer.TriggerCount);
    }

    [Theory]
    [InlineData(13, 37, 5, 7, 55)]   // 13:37:05 -> next quarter at 13:45:00 (7m 55s away)
    [InlineData(14, 0, 0, 15, 0)]    // 14:00:00 -> next quarter at 14:15:00 (15m away)
    [InlineData(14, 14, 59, 0, 1)]   // 14:14:59 -> next quarter at 14:15:00 (1s away)
    [InlineData(14, 15, 0, 15, 0)]   // 14:15:00 -> next quarter at 14:30:00 (15m away)
    [InlineData(14, 44, 30, 0, 30)]  // 14:44:30 -> next quarter at 14:45:00 (30s away)
    public void CurrentPrice_TimerSyncsToNextQuarterBoundary(
        int currentHour, int currentMinute, int currentSecond,
        int expectedMinutes, int expectedSeconds)
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, currentHour, currentMinute, currentSecond, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;

        // Act
        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Assert - Timer should be configured to fire at the next quarter boundary
        Assert.True(fakeTimer.IsStarted);
        var expectedDelay = TimeSpan.FromMinutes(expectedMinutes).Add(TimeSpan.FromSeconds(expectedSeconds));
        Assert.Equal(expectedDelay, fakeTimer.Delay);
    }

    [Fact]
    public void CurrentPrice_At1417_ShowsPriceFor1415Quarter()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;

        // Expected: Day 1 (today), 14:15 = 1 * 1000 + 14 * 100 + 15 = 2415
        var expectedPrice = 2415m;

        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Act
        var prices = repository.GetPrices("DK1", currentTimeUtc.AddDays(-1), currentTimeUtc.AddDays(1));
        var actualPrice = CurrentPriceOperations.GetCurrentPrice(prices, currentTimeUtc);

        // Assert
        Assert.NotNull(actualPrice);
        Assert.Equal(expectedPrice, actualPrice.PriceDkk);
    }

    [Fact]
    public void CurrentPrice_At1430_ShowsPriceFor1430Quarter()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 30, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;

        // Expected: Day 1 (today), 14:30 = 1 * 1000 + 14 * 100 + 30 = 2430
        var expectedPrice = 2430m;

        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Act
        var prices = repository.GetPrices("DK1", currentTimeUtc.AddDays(-1), currentTimeUtc.AddDays(1));
        var actualPrice = CurrentPriceOperations.GetCurrentPrice(prices, currentTimeUtc);

        // Assert
        Assert.NotNull(actualPrice);
        Assert.Equal(expectedPrice, actualPrice.PriceDkk);
    }

    [Fact]
    public void CurrentPrice_NoDataAvailable_ShowsPlaceholder()
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, 14, 17, 0, DateTimeKind.Utc);

        var repository = new InMemoryPriceRepository();
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;
        // Don't store any prices

        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Act
        var prices = repository.GetPrices("DK1", currentTimeUtc.AddDays(-1), currentTimeUtc.AddDays(1));
        var actualPrice = CurrentPriceOperations.GetCurrentPrice(prices, currentTimeUtc);

        // Assert
        Assert.Null(actualPrice);
    }

    [Theory]
    [InlineData(14, 0, 14, 0)]   // Exactly on the hour
    [InlineData(14, 7, 14, 0)]   // First quarter
    [InlineData(14, 15, 14, 15)] // Exactly on 15-min mark
    [InlineData(14, 22, 14, 15)] // Second quarter
    [InlineData(14, 30, 14, 30)] // Exactly on 30-min mark
    [InlineData(14, 37, 14, 30)] // Third quarter
    [InlineData(14, 45, 14, 45)] // Exactly on 45-min mark
    [InlineData(14, 52, 14, 45)] // Fourth quarter
    public void CurrentPrice_VariousTimes_ReturnsCorrectQuarter(
        int currentHour, int currentMinute,
        int expectedHour, int expectedMinute)
    {
        // Arrange
        var currentTimeUtc = new DateTime(2025, 10, 17, currentHour, currentMinute, 0, DateTimeKind.Utc);
        var repository = CreatePopulatedRepository(currentTimeUtc);
        var fakeTimer = new FakePeriodicTimer();
        Func<Infrastructure.Services.ITimer> timerFactory = () => fakeTimer;
        Func<DateTime> getUtcNow = () => currentTimeUtc;

        // Expected: Day 1 (today), expectedHour:expectedMinute
        var expectedPrice = 1 * 1000 + expectedHour * 100 + expectedMinute;

        var viewModel = new CurrentPriceViewModel(repository, timerFactory, getUtcNow);

        // Act
        var prices = repository.GetPrices("DK1", currentTimeUtc.AddDays(-1), currentTimeUtc.AddDays(1));
        var actualPrice = CurrentPriceOperations.GetCurrentPrice(prices, currentTimeUtc);

        // Assert
        Assert.NotNull(actualPrice);
        Assert.Equal(expectedPrice, actualPrice.PriceDkk);
    }
}
