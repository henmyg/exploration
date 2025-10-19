using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Maui.Core.Features.Settings;

/// <summary>
/// Interface for Settings feature ViewModel
/// </summary>
public interface ISettingsViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Title for the settings page
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Whether to show the debug section
    /// </summary>
    bool ShowDebugSection { get; }

    /// <summary>
    /// Command to navigate to configuration page
    /// </summary>
    IAsyncRelayCommand GoToConfigurationCommand { get; }

    /// <summary>
    /// Command to navigate to view states page (only available when ShowDebugSection is true)
    /// </summary>
    IAsyncRelayCommand? GoToViewStatesCommand { get; }
}
