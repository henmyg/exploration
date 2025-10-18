# Injectable Time Provider Pattern

How should we handle current time access (`DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`) to ensure code remains testable and deterministic?

# Decision

We've chosen **injectable time provider via `Func<DateTime>`** with Roslyn analyzer enforcement (MAUI002), because this approach makes all time-dependent code testable while maintaining simplicity and allowing reasonable default values.

## Impact

- **Testability**: All time-dependent logic can be tested with controlled time values
- **Determinism**: Tests produce consistent results regardless of when they run
- **Debugging**: Easier to reproduce time-based bugs by controlling the clock
- **Enforced**: Roslyn analyzer (MAUI002) prevents accidental use of static DateTime properties at compile time
- **Flexibility**: Tests can simulate time progression, time zones, and edge cases (midnight, year boundaries, etc.)

**Implementation pattern**:
```csharp
// Service/ViewModel with injected time provider
public class PriceGraphViewModel(
    IPriceRepository priceRepository,
    Func<DateTime>? getNow = null)
{
    private readonly Func<DateTime> _getNow = getNow ?? (() => DateTime.Now);

    public void LoadPrices()
    {
        var now = _getNow(); // Use injected time
        var (start, end) = GetDateRange(now);
        // ...
    }
}

// Pure operations accept DateTime as parameter
public static class PriceGraphOperations
{
    public static (DateTime start, DateTime end) GetTodayAndTomorrowRange(DateTime now)
    {
        var startOfToday = now.Date;
        var endOfTomorrow = startOfToday.AddDays(2);
        return (startOfToday.ToUniversalTime(), endOfTomorrow.ToUniversalTime());
    }
}

// Test with controlled time
[Fact]
public void Test_WithSpecificTime()
{
    var fixedTime = new DateTime(2025, 10, 17, 14, 30, 0);
    Func<DateTime> getNow = () => fixedTime;

    var viewModel = new PriceGraphViewModel(repository, getNow);
    // Test behavior is now deterministic
}
```

**Analyzer enforcement**:
```csharp
// ❌ Compile error MAUI002
var now = DateTime.Now;

// ✅ Allowed: constructor default with null-coalescing
_getNow = getNow ?? (() => DateTime.Now);

// ✅ Allowed: method parameter default with null-coalescing
var effectiveTime = timestamp ?? DateTime.UtcNow;
```

# Solution Proposals

## 1. Static DateTime.Now/UtcNow/Today (Current .NET Default)

Use static `DateTime.Now`, `DateTime.UtcNow`, and `DateTime.Today` directly throughout the codebase.

### Pros/Cons

**Pros:**
- Simple and straightforward
- No additional code or abstractions
- Standard .NET approach
- Zero boilerplate

**Cons:**
- Impossible to test time-dependent logic deterministically
- Tests can pass/fail based on when they run (time of day, date, timezone)
- Cannot simulate edge cases (midnight, year boundaries, DST transitions)
- Makes debugging time-based bugs difficult
- Tests become flaky and non-reproducible
- Cannot fast-forward or rewind time in tests

## 2. Injectable Time Provider via Func<DateTime> (Chosen)

Inject `Func<DateTime>` in constructors with null-coalescing default, and pass `DateTime` values to pure operations.

### Pros/Cons

**Pros:**
- Fully testable - tests control the clock
- Deterministic tests that always produce same results
- Simple pattern - just a function parameter
- No external dependencies or frameworks
- Analyzer enforcement prevents violations
- Acceptable default pattern: `getNow ?? (() => DateTime.Now)`
- Pure operations receive explicit DateTime values
- Easy to simulate time progression in tests
- Clear separation: services get `Func<DateTime>`, operations get `DateTime`

**Cons:**
- Requires discipline to inject consistently
- Additional parameter in constructors
- Need to call `_getNow()` instead of `DateTime.Now`
- Small learning curve for team

## 3. ISystemClock / TimeProvider Abstraction

Create an `ISystemClock` or `ITimeProvider` interface with a concrete implementation that returns current time.

### Pros/Cons

**Pros:**
- Very testable via mocking or fake implementations
- Clear abstraction boundary
- Can add additional time-related methods (timers, delays, etc.)
- .NET 8+ has `TimeProvider` class built-in

**Cons:**
- More complex - requires interface + implementation + DI registration
- Over-engineering for simple time access
- More boilerplate code
- Another dependency to inject
- Still need analyzer to prevent direct DateTime usage
- Heavier solution than needed for most cases

## 4. Ambient Context / Static Time Provider

Use a static `Clock.Current` or `TimeProvider.Current` that can be swapped out in tests.

### Pros/Cons

**Pros:**
- No constructor parameters needed
- Can be changed globally for testing
- Minimal code changes

**Cons:**
- Global mutable state is problematic
- Tests can affect each other (shared state)
- Not thread-safe without careful implementation
- Harder to test parallel scenarios
- Hidden dependency - not obvious from constructor
- Can forget to reset between tests
- Generally considered an anti-pattern

## 5. Method-Level Time Parameters

Pass `DateTime` as method parameters everywhere it's needed.

### Pros/Cons

**Pros:**
- Very explicit - clear where time is used
- No stored dependencies
- Trivial to test individual methods

**Cons:**
- Time parameter pollution throughout entire codebase
- Every method needs time parameter even if just passing it down
- Breaks encapsulation - callers need to know implementation details
- Verbose and repetitive
- Difficult to maintain consistency across call chains
