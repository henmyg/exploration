using Maui.Core.Features.SyncStatus;
using Maui.Core.Shared.Services;
using Moq;
using System.ComponentModel;
using Xunit;

namespace Maui.Tests;


public class SyncStatusViewModelTests
{
    [Theory]
    [InlineData(true, null, SyncStatusViewModel.State.Synching, "Synching...")]
    [InlineData(false, null, SyncStatusViewModel.State.UpToDate, "Up-to-date")]
    [InlineData(false, "Something went wrong", SyncStatusViewModel.State.Error, "Something went wrong")]
    [InlineData(true, "Something went wrong", SyncStatusViewModel.State.Synching, "Synching...")]
    public void ViewState_ReflectsSyncService(
        bool isSynching,
        string? errorMessage,
        SyncStatusViewModel.State expectedState,
        string expectedSyncText)
    {
        // Arrange
        var service = new Mock<IPriceSyncService>();
        service.Setup(s => s.IsSynching).Returns(isSynching);
        service.Setup(s => s.SynchException).Returns(
            errorMessage != null ? new Exception(errorMessage) : null);

        // Act
        var viewModel = new SyncStatusViewModel(service.Object);

        // Assert
        Assert.Equal(expectedState, viewModel.ViewState);
        Assert.Equal(expectedState == SyncStatusViewModel.State.UpToDate, viewModel.IsUpToDate);
        Assert.Equal(expectedState == SyncStatusViewModel.State.Error, viewModel.IsError);
        Assert.Equal(expectedState == SyncStatusViewModel.State.Synching, viewModel.IsSynching);
        Assert.Equal(expectedSyncText, viewModel.SyncText);
    }

    [Fact]
    public void PropertyChange_IsCorrectlyRaised()
    {
        void AssertAllPropertiesRaised(SyncStatusViewModel viewModel, IEnumerable<string?> changedProperties)
        {
            Assert.Contains(nameof(viewModel.IsSynching), changedProperties);
            Assert.Contains(nameof(viewModel.IsError), changedProperties);
            Assert.Contains(nameof(viewModel.IsUpToDate), changedProperties);
            Assert.Contains(nameof(viewModel.SyncText), changedProperties);
            Assert.Contains(nameof(viewModel.ViewState), changedProperties);
        }

        // Arrange
        var service = new Mock<IPriceSyncService>();
        var viewModel = new SyncStatusViewModel(service.Object);
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act 1
        service.Raise(m => m.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPriceSyncService.IsSynching)));

        // Assert 1
        AssertAllPropertiesRaised(viewModel, changedProperties);

        // Act 2
        changedProperties.Clear();
        service.Raise(m => m.PropertyChanged += null, new PropertyChangedEventArgs(nameof(IPriceSyncService.SynchException)));

        // Assert 2
        AssertAllPropertiesRaised(viewModel, changedProperties);
    }
}
