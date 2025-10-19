using Maui.Core.Shared.Models;
using System.ComponentModel;

namespace Maui.Core.Features.CurrentPrice;

/// <summary>
/// Interface for CurrentPrice feature ViewModel
/// </summary>
public interface ICurrentPriceViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// The current price record containing all price information
    /// </summary>
    PriceRecord? CurrentPrice { get; }

    /// <summary>
    /// Convenience property for accessing just the price value in DKK
    /// </summary>
    decimal? CurrentPriceDKK { get; }
}
