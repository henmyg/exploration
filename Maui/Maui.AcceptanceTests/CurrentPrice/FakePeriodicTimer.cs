using Maui.Core.Shared.Services;
using Maui.Infrastructure.Services;

namespace Maui.AcceptanceTests.CurrentPrice;

/// <summary>
/// Fake implementation of IPeriodicTimer for testing.
/// Allows tests to manually trigger callbacks without waiting for real time to pass.
/// </summary>
public class FakePeriodicTimer : Infrastructure.Services.ITimer
{
    private Action? _callback;
    private bool _isStarted;

    public TimeSpan? Delay { get; private set; }
    public bool IsStarted => _isStarted;
    public int TriggerCount { get; private set; }

    public void Start(TimeSpan delay, Action callback)
    {
        if (_isStarted)
            throw new InvalidOperationException("Timer is already started");

        Delay = delay;
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        _isStarted = true;
    }

    public void Stop()
    {
        _isStarted = false;
    }

    /// <summary>
    /// Manually triggers the timer callback for testing purposes.
    /// </summary>
    public void Trigger()
    {
        if (!_isStarted)
            throw new InvalidOperationException("Timer is not started");

        TriggerCount++;
        _callback?.Invoke();
    }

    public void Dispose()
    {
        Stop();
        _callback = null;
    }
}
