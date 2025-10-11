namespace Maui.Features.Counter
{
    public partial class CounterPage : ContentPage
    {
        public CounterPage(CounterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
