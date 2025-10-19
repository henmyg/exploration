using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Core.Shared.Services;
using System.Windows.Input;

namespace Maui.Core.Features.Debug
{
    public partial class ViewStatesPageModel: ObservableObject
    {
        [ObservableProperty]
        private ICommand _goToSyncStatusCommand;

        [ObservableProperty]
        private ICommand _goToPriceGraphCommand;

        public ViewStatesPageModel(INavigationService navigationService)
        {
            _goToSyncStatusCommand = new RelayCommand(async () => await navigationService.GoToAsync("debug/viewstates/syncstatus"));
            _goToPriceGraphCommand = new RelayCommand(async () => await navigationService.GoToAsync("debug/viewstates/pricegraph"));
        }
    }
}
