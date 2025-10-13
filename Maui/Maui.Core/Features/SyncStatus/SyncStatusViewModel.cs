using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Core.Shared.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Core.Features.SyncStatus
{
    public partial class SyncStatusViewModel: ObservableObject
    {
        private readonly PriceSyncService _priceSyncService;

        public SyncStatusViewModel(PriceSyncService priceSyncService)
        {
            _priceSyncService = priceSyncService ?? throw new ArgumentNullException(nameof(priceSyncService));
            _priceSyncService.PropertyChanged += OnPriceSyncServicePropertyChanged;
        }

        public bool IsSynching => _priceSyncService.IsSynching;
        public string SyncText => IsSynching ? "Syncing" : "Up-to-date";

        private void OnPriceSyncServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PriceSyncService.IsSynching))
            {
                OnPropertyChanged(nameof(IsSynching));
                OnPropertyChanged(nameof(SyncText));
            }
        }
    }
}
