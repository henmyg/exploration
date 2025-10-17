using Maui.Core.Features.CurrentPrice;
using Maui.Core.Shared.Models;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;
using Moq;
using Xunit;

namespace Maui.Tests.Core.Features.CurrentPrice;

public class CurrentPriceViewModelTests
{
    [Fact]
    public void CurrentPriceDKK_NotifiesPropertyChanged_WhenCurrentPriceChanges()
    {
        // Arrange
        var mockRepository = new Mock<IPriceRepository>();
        var mockDelayer = new Mock<ITaskDelayer>();
        var currentTime = new DateTime(2025, 10, 17, 14, 15, 0, DateTimeKind.Utc);
        DateTime getUtcNow() => currentTime;

        // Setup delayer to return a task that never completes (background loop waits forever)
        var delayTaskCompletionSource = new TaskCompletionSource<bool>();
        mockDelayer.Setup(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(delayTaskCompletionSource.Task);

        // Setup initial empty prices
        mockRepository.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns([]);

        var viewModel = new CurrentPriceViewModel(mockRepository.Object, mockDelayer.Object, getUtcNow);

        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Initial state: no price
        Assert.Null(viewModel.CurrentPriceDKK);
        changedProperties.Clear();

        // Setup new price data
        var newPrice = new PriceRecord
        {
            PriceArea = "DK1",
            TimeUtc = currentTime,
            PriceDkk = 2415m
        };
        mockRepository.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns([newPrice]);

        // Act - Trigger the PricesUpdated event to simulate repository update
        mockRepository.Raise(r => r.PricesUpdated += null, EventArgs.Empty);

        // Assert - Both CurrentPrice and CurrentPriceDKK should notify property changed
        Assert.Contains(nameof(viewModel.CurrentPrice), changedProperties);
        Assert.Contains(nameof(viewModel.CurrentPriceDKK), changedProperties);
        Assert.Equal(2415m, viewModel.CurrentPriceDKK);

        viewModel.Dispose();
    }

    [Fact]
    public void CurrentPriceDKK_NotifiesPropertyChanged_WhenPriceValueChanges()
    {
        // Arrange
        var mockRepository = new Mock<IPriceRepository>();
        var mockDelayer = new Mock<ITaskDelayer>();
        var currentTime = new DateTime(2025, 10, 17, 14, 15, 0, DateTimeKind.Utc);
        DateTime getUtcNow() => currentTime;

        // Setup delayer to return a task that never completes (background loop waits forever)
        var delayTaskCompletionSource = new TaskCompletionSource<bool>();
        mockDelayer.Setup(d => d.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(delayTaskCompletionSource.Task);

        // Setup initial price
        var initialPrice = new PriceRecord
        {
            PriceArea = "DK1",
            TimeUtc = currentTime,
            PriceDkk = 2415m
        };
        mockRepository.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns([initialPrice]);

        var viewModel = new CurrentPriceViewModel(mockRepository.Object, mockDelayer.Object, getUtcNow);
        Assert.Equal(2415m, viewModel.CurrentPriceDKK);

        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Setup updated price
        var updatedPrice = new PriceRecord
        {
            PriceArea = "DK1",
            TimeUtc = currentTime,
            PriceDkk = 2500m
        };
        mockRepository.Setup(r => r.GetPrices(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns([updatedPrice]);

        // Act - Trigger the PricesUpdated event
        mockRepository.Raise(r => r.PricesUpdated += null, EventArgs.Empty);

        // Assert
        Assert.Contains(nameof(viewModel.CurrentPrice), changedProperties);
        Assert.Contains(nameof(viewModel.CurrentPriceDKK), changedProperties);
        Assert.Equal(2500m, viewModel.CurrentPriceDKK);

        viewModel.Dispose();
    }
}
