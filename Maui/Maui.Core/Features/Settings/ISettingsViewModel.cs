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
    /// Command to navigate to configuration page
    /// </summary>
    IAsyncRelayCommand GoToConfigurationCommand { get; }
}
