using Maui.Infrastructure.Services;

namespace Maui.AcceptanceTests.CurrentPrice;

/// <summary>
/// Fake implementation of ITaskDelayer for testing.
/// Allows tests to manually trigger delays without waiting for real time to pass.
/// </summary>
public class FakeTaskDelayer : ITaskDelayer
{
    private TaskCompletionSource<bool>? _delayCompletionSource;
    private CancellationToken _cancellationToken;

    public TimeSpan? RequestedDelay { get; private set; }
    public bool IsDelaying { get; private set; }
    public int DelayCallCount { get; private set; }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        RequestedDelay = delay;
        IsDelaying = true;
        DelayCallCount++;
        _cancellationToken = cancellationToken;
        _delayCompletionSource = new TaskCompletionSource<bool>();

        // Register cancellation
        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                _delayCompletionSource?.TrySetCanceled(cancellationToken);
            });
        }

        return _delayCompletionSource.Task;
    }

    /// <summary>
    /// Manually completes the current delay, allowing the test to simulate time passing.
    /// </summary>
    public void CompleteDelay()
    {
        if (_delayCompletionSource == null)
            throw new InvalidOperationException("No delay is currently in progress");

        IsDelaying = false;
        _delayCompletionSource.TrySetResult(true);
    }
}
