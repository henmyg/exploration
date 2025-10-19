using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Core.Shared.Services;

namespace Maui.Core.Features.Settings
{
    public partial class SettingsViewModel : ObservableObject, ISettingsViewModel
    {
        private readonly INavigationService _navigation;

        public IAsyncRelayCommand GoToConfigurationCommand { get; }

        [ObservableProperty]
        private string _title = "Settings page";

        public SettingsViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            GoToConfigurationCommand = new AsyncRelayCommand(OnNavigateToConfiguration);
        }

        private async Task OnNavigateToConfiguration()
        {
            Console.WriteLine("Nav to config!");
            await _navigation.GoToAsync("settings/configuration");
        }
    }
}
