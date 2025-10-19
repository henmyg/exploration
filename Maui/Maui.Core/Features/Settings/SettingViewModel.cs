using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Core.Shared.Services;

namespace Maui.Core.Features.Settings
{
    public partial class SettingsViewModel : ObservableObject, ISettingsViewModel
    {
        private readonly INavigationService _navigation;

        public IAsyncRelayCommand GoToConfigurationCommand { get; }
        public IAsyncRelayCommand? GoToViewStatesCommand { get; }

        [ObservableProperty]
        private string _title = "Settings";

        public bool ShowDebugSection { get; } =
#if DEBUG
            true;
#else
            false;
#endif

        public SettingsViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            GoToConfigurationCommand = new AsyncRelayCommand(OnNavigateToConfiguration);

#if DEBUG
            GoToViewStatesCommand = new AsyncRelayCommand(OnNavigateToViewStates);
#endif
        }

        private async Task OnNavigateToConfiguration()
        {
            await _navigation.GoToAsync("settings/configuration");
        }

#if DEBUG
        private async Task OnNavigateToViewStates()
        {
            await _navigation.GoToAsync("debug/viewstates");
        }
#endif
    }
}
