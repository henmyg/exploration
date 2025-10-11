namespace Maui.Features.CurrentPrice;

public partial class CurrentPricePage : ContentPage
{
    private readonly CurrentPriceViewModel _viewModel;

    public CurrentPricePage(CurrentPriceViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCurrentPriceCommand.ExecuteAsync(null);
    }
}
