using Maui.Core.Features.Counter;

namespace Maui.Features.Counter
{
    public partial class CounterPage : ContentPage
    {
        public CounterPage(CounterViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;

            // Wire up MAUI-specific accessibility
            viewModel.CounterTextChanged += (sender, text) =>
            {
                SemanticScreenReader.Announce(text);
            };
        }
    }
}
