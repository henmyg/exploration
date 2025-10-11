using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maui.Features.Counter
{
    public partial class CounterViewModel : ObservableObject
    {
        [ObservableProperty]
        private int count = 0;

        [ObservableProperty]
        private string counterText = "Click me";

        [RelayCommand]
        private void IncrementCounter()
        {
            Count++;

            if (Count == 1)
                CounterText = $"Clicked {Count} time";
            else
                CounterText = $"Clicked {Count} times";

            SemanticScreenReader.Announce(CounterText);
        }
    }
}
