# Separation of Stateful Services/ViewModels and Pure Function Libraries

How should we organize business logic that involves both stateful operations (API calls, repository access, observable properties) and pure transformations (data mapping, calculations, formatting)?

# Decision

We've chosen **companion static operations classes** placed in the same file as their stateful counterparts, because this approach maximizes testability while keeping related code together and maintaining clarity about dependencies.

## Impact

- **Testability**: Pure functions are trivially testable without mocking or dependency injection
- **Clarity**: Static operations classes make it explicit which logic has dependencies vs which is pure
- **Performance**: Pure functions enable easier optimization, caching, and parallelization
- **Maintainability**: Business logic transformations are isolated and reusable
- **Analyzer Support**: Custom Roslyn analyzer can enforce this pattern

**Implementation pattern**:
```csharp
// Stateful service with dependencies
public class PriceSyncService(HttpClient httpClient, IPriceRepository repository)
{
    public async Task SyncPricesAsync(...)
    {
        var apiData = await httpClient.GetDayAheadPricesAsync(...);
        var records = apiData.Records.ToPriceRecords(); // Pure operation
        repository.StorePrices(records);
    }
}

// Pure operations as extension methods (same file)
public static class PriceSyncOperations
{
    public static PriceRecord ToPriceRecord(this DayAheadPriceRecord apiRecord)
    {
        return new PriceRecord { /* mapping */ };
    }
}
```

# Solution Proposals

## 1. Keep All Logic in Services/ViewModels

All business logic, including transformations, remains in the service or ViewModel class.

### Pros/Cons

**Pros:**
- Simple - everything in one place
- No need to decide what to extract
- Fewer files/classes to navigate

**Cons:**
- Difficult to test pure logic without mocking dependencies
- Harder to reuse transformations across services
- Business logic mixed with orchestration code
- Can't easily optimize or cache pure calculations
- Violates Single Responsibility Principle

## 2. Separate Pure Function Libraries (Chosen)

Extract pure functions into companion static operations classes (same file), using extension methods for fluent API style.

### Pros/Cons

**Pros:**
- Pure functions are trivially testable (no mocks needed)
- Clear separation between stateful and stateless logic
- Reusable across services/ViewModels
- Easier to reason about and optimize
- Extension methods enable fluent API patterns
- Same-file placement keeps related code together
- Can be enforced via Roslyn analyzer

**Cons:**
- Requires discipline to identify extractable logic
- Slightly more verbose (separate class declaration)
- Need to learn/follow the pattern consistently

## 3. Separate Service Layer with Interfaces

Create separate service layer with interfaces for all business logic, injecting them as dependencies.

### Pros/Cons

**Pros:**
- Very testable via mocking
- Clear abstraction boundaries
- Follows traditional DI patterns

**Cons:**
- Over-engineering for pure functions
- Adds unnecessary interfaces and DI registrations
- Testing still requires mocks for simple transformations
- More boilerplate code
- Obscures which logic is pure vs stateful

## 4. Utility/Helper Classes in Shared Folder

Move pure functions to shared utility/helper classes in a separate folder structure.

### Pros/Cons

**Pros:**
- Centralized location for shared utilities
- Clear that they're reusable

**Cons:**
- Loses context - disconnected from services that use them
- Harder to navigate between related code
- Encourages overly generic code
- Risk of creating "god utility classes"
- Not discoverable when reading the service code
