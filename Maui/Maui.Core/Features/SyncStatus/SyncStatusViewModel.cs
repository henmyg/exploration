using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Shared.Services;
using System.ComponentModel;

namespace Maui.Core.Features.SyncStatus
{
    public partial class SyncStatusViewModel: ObservableObject
    {
        private readonly IPriceSyncService _priceSyncService;

        public SyncStatusViewModel(IPriceSyncService priceSyncService)
        {
            _priceSyncService = priceSyncService ?? throw new ArgumentNullException(nameof(priceSyncService));
            _priceSyncService.PropertyChanged += OnPriceSyncServicePropertyChanged;
        }

        public State ViewState => _priceSyncService.IsSynching ? State.Synching
            : _priceSyncService.SynchException is null ? State.UpToDate
            : State.Error;

        public bool IsSynching => ViewState == State.Synching;
        public bool IsUpToDate => ViewState == State.UpToDate;
        public bool IsError => ViewState == State.Error;

        public string SyncText => ViewState switch
        {
            State.Synching => "Synching...",
            State.UpToDate => "Up-to-date",
            State.Error => _priceSyncService.SynchException?.Message ?? "Unknown error",
            _ => throw new NotImplementedException()
        };

        private void OnPriceSyncServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PriceSyncService.IsSynching):
                case nameof(PriceSyncService.SynchException):
                    OnPropertyChanged(nameof(ViewState));
                    OnPropertyChanged(nameof(IsSynching));
                    OnPropertyChanged(nameof(IsUpToDate));
                    OnPropertyChanged(nameof(IsError));
                    OnPropertyChanged(nameof(SyncText));
                    break;
            }
        }

        public enum State
        {
            Synching, UpToDate, Error
        }
    }
}
