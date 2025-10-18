# Maui.Analyzers

Custom Roslyn analyzers for enforcing architectural patterns and testability conventions in the Maui project.

## Analyzers

### MAUI001: Pure Function Extraction

**Rule**: Extract pure functions from services/ViewModels into companion static operations classes.

**Purpose**: Maximize testability by separating stateless logic from stateful dependencies.

**Decision Record**: [Separation of Stateful Services/ViewModels and Pure Function Libraries](../../Documentation/decisions/pure-function-operations.md)

**Examples**:
```csharp
// ❌ Error: Pure function mixed with stateful service
public class PriceSyncService
{
    private decimal ConvertEurToDkk(decimal eur) // Pure function
    {
        return eur * 7.45m;
    }
}

// ✅ Correct: Pure function in operations class
public class PriceSyncService { }

public static class PriceSyncOperations
{
    public static decimal ConvertEurToDkk(decimal eur)
    {
        return eur * 7.45m;
    }
}
```

### MAUI002: Injectable Time Provider

**Rule**: Do not use `DateTime.Now`, `DateTime.UtcNow`, or `DateTime.Today` directly. Use injected `Func<DateTime>` instead.

**Purpose**: Make time-dependent code testable with deterministic time values.

**Decision Record**: [Injectable Time Provider Pattern](../../Documentation/decisions/injectable-time-provider.md)

**Examples**:
```csharp
// ❌ Error: Direct DateTime usage
public class MyService
{
    public void DoWork()
    {
        var now = DateTime.Now;
    }
}

// ✅ Correct: Injected time provider
public class MyService
{
    private readonly Func<DateTime> _getNow;

    public MyService(Func<DateTime>? getNow = null)
    {
        _getNow = getNow ?? (() => DateTime.Now); // Allowed pattern
    }

    public void DoWork()
    {
        var now = _getNow();
    }
}
```

**Allowed patterns**:
- Constructor defaults: `_getNow = getNow ?? (() => DateTime.Now)`
- Null-coalescing in methods: `var time = timestamp ?? DateTime.UtcNow`

### MAUI003: Injectable Task Delayer

**Rule**: Do not use `Task.Delay()` directly. Use injected `ITaskDelayer` instead.

**Purpose**: Make delay-based code testable without waiting for real time to pass.

**Decision Record**: [Injectable Task Delayer Pattern](../../Documentation/decisions/injectable-task-delayer.md)

**Examples**:
```csharp
// ❌ Error: Direct Task.Delay usage
public class MyService
{
    public async Task DoWork()
    {
        await Task.Delay(TimeSpan.FromMinutes(5));
    }
}

// ✅ Correct: Injected ITaskDelayer
public class MyService
{
    private readonly ITaskDelayer _taskDelayer;

    public MyService(ITaskDelayer taskDelayer)
    {
        _taskDelayer = taskDelayer;
    }

    public async Task DoWork()
    {
        await _taskDelayer.DelayAsync(TimeSpan.FromMinutes(5));
    }
}
```

**Allowed location**:
- Inside `SystemTaskDelayer` class (the ITaskDelayer implementation)

## Testing

Integration tests for all analyzers are located in `Maui.IntegrationTests/Analyzers/`:
- `PureFunctionAnalyzerTests.cs`
- `DateTimeNowAnalyzerTests.cs`
- `TaskDelayAnalyzerTests.cs`

Run tests:
```bash
dotnet test Maui/Maui.IntegrationTests --filter "FullyQualifiedName~Analyzers"
```

## Benefits

1. **Compile-Time Enforcement**: Violations are caught during development, not in code review
2. **Consistency**: Ensures patterns are followed uniformly across the codebase
3. **Testability**: Makes code testable by enforcing dependency injection
4. **Fast Tests**: Enables instant test execution without waiting for delays
5. **Determinism**: Tests produce consistent results regardless of execution time
6. **Documentation**: Analyzers serve as living documentation of architectural patterns
