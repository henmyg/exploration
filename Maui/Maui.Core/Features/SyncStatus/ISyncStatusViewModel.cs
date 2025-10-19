using System.ComponentModel;
using static Maui.Core.Features.SyncStatus.SyncStatusViewModel;

namespace Maui.Core.Features.SyncStatus;

/// <summary>
/// Interface for SyncStatus feature ViewModel
/// </summary>
public interface ISyncStatusViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Current state of the sync process
    /// </summary>
    State ViewState { get; }

    /// <summary>
    /// Whether sync is currently in progress
    /// </summary>
    bool IsSynching { get; }

    /// <summary>
    /// Whether sync is up-to-date and not in error
    /// </summary>
    bool IsUpToDate { get; }

    /// <summary>
    /// Whether there's a sync error
    /// </summary>
    bool IsError { get; }

    /// <summary>
    /// Text to display based on current sync state
    /// </summary>
    string SyncText { get; }
}
