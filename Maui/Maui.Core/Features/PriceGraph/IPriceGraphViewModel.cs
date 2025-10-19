using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Core.Features.PriceGraph;

/// <summary>
/// Interface for PriceGraph feature ViewModel
/// </summary>
public interface IPriceGraphViewModel : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Chart data points in format required by LiveCharts
    /// </summary>
    DateTimePoint[] ChartData { get; }

    /// <summary>
    /// Current time in ticks for chart "now" indicator
    /// </summary>
    long Now { get; }

    /// <summary>
    /// Formatted time range string for display
    /// </summary>
    string TimeRange { get; }

    /// <summary>
    /// Formatter function for X-axis labels
    /// </summary>
    Func<DateTime, string> XAxisFormatter { get; }
}
