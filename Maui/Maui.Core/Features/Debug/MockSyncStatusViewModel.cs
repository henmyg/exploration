using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Features.SyncStatus;
using System.Data;
using static Maui.Core.Features.SyncStatus.SyncStatusViewModel;

namespace Maui.Core.Features.Debug;

/// <summary>
/// Mock implementation of ISyncStatusViewModel for testing different states
/// </summary>
public partial class MockSyncStatusViewModel : ObservableObject, ISyncStatusViewModel, IPreviewView<State>
{
    [ObservableProperty]
    private State _viewState;

    [ObservableProperty]
    private string _syncText = string.Empty;

    public bool IsSynching => ViewState == State.Synching;
    public bool IsUpToDate => ViewState == State.UpToDate;
    public bool IsError => ViewState == State.Error;

    public MockSyncStatusViewModel()
    {
        // Start with Synching state
        ViewState = State.Synching;
        UpdateSyncText();
    }

    public IReadOnlyList<ViewStateOption<State>> States => [
        new("Synching", State.Synching),
        new("Up-to-date", State.UpToDate),
        new("Error", State.Error)
        ];

    partial void OnViewStateChanged(State value)
    {
        OnPropertyChanged(nameof(IsSynching));
        OnPropertyChanged(nameof(IsUpToDate));
        OnPropertyChanged(nameof(IsError));
        UpdateSyncText();
    }

    private void UpdateSyncText()
    {
        SyncText = ViewState switch
        {
            State.Synching => "Synching...",
            State.UpToDate => "Up-to-date",
            State.Error => "Failed to sync: Connection timeout",
            _ => "Unknown state"
        };
    }

    public void SetState(State state)
    {
        ViewState = state;
    }
}
