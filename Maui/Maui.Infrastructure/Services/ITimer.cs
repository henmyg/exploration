namespace Maui.Infrastructure.Services;

/// <summary>
/// Abstraction for one-shot timer operations.
/// Enables testable time-based behavior without depending on actual timers.
/// The timer fires once after the specified delay.
/// </summary>
public interface ITimer : IDisposable
{
    /// <summary>
    /// Starts the timer to fire once after the specified delay.
    /// </summary>
    /// <param name="delay">The delay before the callback fires</param>
    /// <param name="callback">The action to execute when the timer fires</param>
    void Start(TimeSpan delay, Action callback);

    /// <summary>
    /// Stops the timer.
    /// </summary>
    void Stop();
}
