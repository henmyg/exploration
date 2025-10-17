using System.Timers;
using Timer = System.Timers.Timer;

namespace Maui.Infrastructure.Services;

/// <summary>
/// Production implementation of IPeriodicTimer using System.Timers.Timer.
/// Simple one-shot timer wrapper.
/// </summary>
public class SystemTimer : ITimer
{
    private Timer? _timer;
    private Action? _callback;

    public Timer? Timer { get => _timer; set => _timer = value; }

    public void Start(TimeSpan delay, Action callback)
    {
        if (Timer != null)
            throw new InvalidOperationException("Timer is already started");

        _callback = callback ?? throw new ArgumentNullException(nameof(callback));

        Timer = new Timer(delay.TotalMilliseconds)
        {
            AutoReset = false
        };
        Timer.Elapsed += OnTimerElapsed;
        Timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        _callback?.Invoke();
    }

    public void Stop()
    {
        if (Timer != null)
        {
            Timer.Stop();
            Timer.Elapsed -= OnTimerElapsed;
        }
    }

    public void Dispose()
    {
        Stop();
        Timer?.Dispose();
        Timer = null;
        _callback = null;
    }
}
