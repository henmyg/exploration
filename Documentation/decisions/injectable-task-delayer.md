# Injectable Task Delayer Pattern

How should we handle async delays (`Task.Delay`, `Timer`, `PeriodicTimer`) to ensure time-based behavior remains testable without making tests slow or flaky?

# Decision

We've chosen **injectable ITaskDelayer abstraction** with Roslyn analyzer enforcement (MAUI003), because this approach makes all delay-based code testable without actually waiting for real time to pass, while maintaining the same async/await patterns as production code.

## Impact

- **Fast Tests**: Tests complete instantly without waiting for real delays
- **Determinism**: Tests can precisely control when delays complete
- **No Flakiness**: Tests don't depend on timing or thread scheduling
- **Testable Retry Logic**: Can verify retry behavior without waiting hours/minutes
- **Production Simplicity**: Production code uses simple `Task.Delay` wrapper
- **Cancellation Support**: Fully supports CancellationToken for graceful shutdown
- **Enforced**: Roslyn analyzer (MAUI003) prevents accidental use of Task.Delay at compile time

**Implementation pattern**:
```csharp
// Interface for delay abstraction
public interface ITaskDelayer
{
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);
}

// Production implementation using Task.Delay
public class SystemTaskDelayer : ITaskDelayer
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default)
    {
        return Task.Delay(delay, cancellationToken);
    }
}

// Service using delays
public class BackgroundPriceSyncService(ITaskDelayer taskDelayer)
{
    private async Task RunSyncLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextSync();
            await _taskDelayer.DelayAsync(delay, cancellationToken);

            await SyncPricesAsync();
        }
    }
}

// Test fake for controlling delays
public class FakeTaskDelayer : ITaskDelayer
{
    private TaskCompletionSource<bool>? _delayCompletionSource;

    public TimeSpan? RequestedDelay { get; private set; }
    public bool IsDelaying { get; private set; }

    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        RequestedDelay = delay;
        IsDelaying = true;
        _delayCompletionSource = new TaskCompletionSource<bool>();

        cancellationToken.Register(() =>
            _delayCompletionSource?.TrySetCanceled(cancellationToken));

        return _delayCompletionSource.Task;
    }

    public void CompleteDelay()
    {
        IsDelaying = false;
        _delayCompletionSource?.TrySetResult(true);
    }
}

// Test using fake delayer
[Fact]
public async Task BackgroundSync_RetriesEveryHour_UntilPricesReceived()
{
    var fakeDelayer = new FakeTaskDelayer();
    var service = new BackgroundPriceSyncService(syncService, fakeDelayer, repository);

    await service.StartAsync(CancellationToken.None);
    await Task.Delay(50); // Let async task start

    // Verify first delay is until 13:05
    Assert.Equal(expectedDelay, fakeDelayer.RequestedDelay);

    // Complete delay instantly (no real waiting)
    fakeDelayer.CompleteDelay();
    await Task.Delay(50);

    // Verify retry starts (1 hour delay)
    Assert.Equal(TimeSpan.FromHours(1), fakeDelayer.RequestedDelay);
}
```

**Analyzer enforcement**:
```csharp
// ❌ Compile error MAUI003
await Task.Delay(1000);

// ✅ Allowed: Inside SystemTaskDelayer implementation
public class SystemTaskDelayer : ITaskDelayer
{
    public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken); // OK
    }
}

// ✅ Correct: Use ITaskDelayer
await _taskDelayer.DelayAsync(TimeSpan.FromMinutes(5));
```

# Solution Proposals

## 1. Direct Task.Delay() Calls

Use `Task.Delay()` directly throughout the codebase.

### Pros/Cons

**Pros:**
- Simple and straightforward
- No abstractions needed
- Standard .NET async pattern
- Zero boilerplate

**Cons:**
- Tests must wait for real time to pass (slow)
- Cannot test retry logic without waiting hours
- Tests become flaky (timing-dependent)
- Cannot verify delay durations
- Cannot fast-forward time in tests
- Makes acceptance tests impractical for background services
- Hard to debug timing-related issues

## 2. Injectable ITaskDelayer (Chosen)

Abstract delays behind `ITaskDelayer` interface with production and test implementations.

### Pros/Cons

**Pros:**
- Tests run instantly (no real waiting)
- Can verify exact delay durations requested
- Tests are deterministic and reproducible
- Can test retry logic without waiting
- Full cancellation token support
- Simple abstraction - single method interface
- Production code is straightforward wrapper around Task.Delay
- FakeTaskDelayer allows precise control of when delays complete
- Easy to debug - can inspect delay state

**Cons:**
- Additional interface to maintain
- Need both production and test implementations
- One more dependency to inject
- Small learning curve for pattern

## 3. ITimer / PeriodicTimer Abstraction

Abstract `System.Threading.Timer` or `PeriodicTimer` behind interfaces.

### Pros/Cons

**Pros:**
- Testable timing behavior
- Built-in periodic execution support
- Can be mocked or faked

**Cons:**
- More complex API (callbacks, dispose patterns)
- Harder to use with async/await
- PeriodicTimer is .NET 6+ only
- Callback-based timers don't compose well with async code
- More difficult to reason about execution flow
- Additional complexity compared to Task.Delay pattern
- Harder to implement fake for testing

## 4. Virtual/Overridable Delay Methods

Make delay methods virtual and override in test subclasses.

### Pros/Cons

**Pros:**
- No interface needed
- Direct method calls

**Cons:**
- Requires inheritance (limits design)
- Services must be non-sealed
- Protected or public virtual methods (breaks encapsulation)
- Hard to mock with multiple services
- Not composable
- Goes against composition-over-inheritance principle
- Makes production code more complex

## 5. Time Providers with Virtual Time

Use time provider that controls both current time and delays (like NodaTime's `FakeClock`).

### Pros/Cons

**Pros:**
- Unified approach for both time and delays
- Can simulate complex timing scenarios
- Can fast-forward virtual time

**Cons:**
- Very complex abstraction
- Large external dependency (NodaTime)
- Steeper learning curve
- Over-engineered for simple delay scenarios
- Replaces DateTime types throughout codebase
- May conflict with injectable time provider pattern
- More moving parts to understand and maintain

## 6. Test-Specific Conditional Compilation

Use `#if DEBUG` or similar to skip delays in tests.

### Pros/Cons

**Pros:**
- No abstractions or DI
- Simple implementation

**Cons:**
- Cannot verify delay durations
- Cannot control when delays complete
- Production code polluted with test concerns
- Breaks separation of concerns
- Cannot test actual delay behavior
- Debug builds behave differently than release
- Anti-pattern - production code shouldn't know about tests
