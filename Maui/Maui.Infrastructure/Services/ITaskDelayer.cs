namespace Maui.Infrastructure.Services;

/// <summary>
/// Abstraction for async delay operations.
/// Enables testable time-based behavior without depending on actual Task.Delay.
/// </summary>
public interface ITaskDelayer
{
    /// <summary>
    /// Delays execution for the specified duration.
    /// </summary>
    /// <param name="delay">The duration to delay</param>
    /// <param name="cancellationToken">Token to cancel the delay</param>
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}
