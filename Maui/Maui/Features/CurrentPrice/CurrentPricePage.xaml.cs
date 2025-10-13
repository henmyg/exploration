using Maui.Core.Features.CurrentPrice;

namespace Maui.Features.CurrentPrice;

public partial class CurrentPricePage : ContentPage
{
    public CurrentPricePage(CurrentPriceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
