using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.Core.Features.Settings.Configuration
{
    public partial class PriceAreaViewModel : ObservableObject, IPriceAreaViewModel
    {
        [ObservableProperty]
        private string _text = "Price area";

    }
}
