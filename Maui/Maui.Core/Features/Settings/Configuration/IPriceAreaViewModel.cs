using System.ComponentModel;

namespace Maui.Core.Features.Settings.Configuration;

/// <summary>
/// Interface for PriceArea configuration ViewModel
/// </summary>
public interface IPriceAreaViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Display text for price area
    /// </summary>
    string Text { get; }
}
