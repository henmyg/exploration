namespace Maui.Infrastructure.Services;

/// <summary>
/// Production implementation of ITaskDelayer using Task.Delay.
/// </summary>
public class SystemTaskDelayer : ITaskDelayer
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delay, cancellationToken);
    }
}
