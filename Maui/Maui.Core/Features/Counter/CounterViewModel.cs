using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maui.Core.Features.Counter;

public partial class CounterViewModel : ObservableObject
{
    [ObservableProperty]
    private int count = 0;

    [ObservableProperty]
    private string counterText = "Click me";

    /// <summary>
    /// Event raised when the counter is incremented to notify the UI for accessibility announcements.
    /// </summary>
    public event EventHandler<string>? CounterTextChanged;

    [RelayCommand]
    private void IncrementCounter()
    {
        Count++;

        if (Count == 1)
            CounterText = $"Clicked {Count} time";
        else
            CounterText = $"Clicked {Count} times";

        // Raise event for UI to handle accessibility
        CounterTextChanged?.Invoke(this, CounterText);
    }
}
