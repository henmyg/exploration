# ServiceLocator Pattern for ViewModel Injection in ContentViews

How should ViewModels be injected into ContentView components to maintain feature independence and portability while respecting XAML's parameterless constructor constraint?

# Decision

We've chosen to use the **ServiceLocator pattern via XAML markup extension** for injecting ViewModels into ContentView components, because this enables features to be truly self-contained vertical slices that can be easily moved, reused, and reorganized without coupling to parent pages.

## Impact

**Enables:**
- **Feature Portability**: ContentViews are completely self-contained and can be moved between pages without refactoring
- **True Vertical Slices**: Each feature owns its ViewModel resolution, maintaining independence
- **Rapid UI Reorganization**: Moving features around the app is as simple as moving XAML elements
- **Reusability**: Features can be used in multiple pages simultaneously without code duplication
- **Clear Ownership**: Each feature explicitly declares its ViewModel dependency in XAML

**Trade-offs:**
- **Service Locator Anti-Pattern**: Dependencies are hidden and resolved at runtime rather than compile-time
- **Runtime Errors**: Missing ViewModel registrations are only discovered when the view is instantiated
- **Testing Complexity**: Views are coupled to the DI container, requiring ServiceLocator setup in tests
- **Hidden Dependencies**: Can't see what services a ViewModel needs by looking at the View

**Mitigations:**
- Startup validation ensures all ViewModels are registered (fail-fast at app launch)
- `x:DataType` provides compile-time binding validation for ViewModel properties
- Clear documentation and consistent pattern usage across all features
- Decision record explains the rationale for future maintainers

**Implementation Pattern:**
```xml
<!-- CurrentPriceView.xaml -->
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:locator="clr-namespace:Maui.Features"
             xmlns:vm="clr-namespace:Maui.Core.Features.CurrentPrice;assembly=Maui.Core"
             x:Class="Maui.Features.CurrentPrice.CurrentPriceView"
             x:DataType="vm:CurrentPriceViewModel">
    <ContentView.BindingContext>
        <locator:Locator ViewModelType="{x:Type vm:CurrentPriceViewModel}" />
    </ContentView.BindingContext>
    <VerticalStackLayout>
        <!-- Feature UI bindings validated at compile-time via x:DataType -->
    </VerticalStackLayout>
</ContentView>
```

```csharp
// Locator.cs - ServiceLocator implementation
public static class ServiceLocator
{
    public static IServiceProvider Services { get; set; } = null!;
}

[ContentProperty(nameof(ViewModelType))]
public class LocatorExtension : IMarkupExtension
{
    public Type? ViewModelType { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (ViewModelType == null)
            throw new InvalidOperationException("ViewModelType must be set.");

        return ServiceLocator.Services.GetRequiredService(ViewModelType);
    }
}
```

```csharp
// MauiProgram.cs - Registration and validation
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();

    // Register ViewModels
    builder.Services.AddSingleton<CurrentPriceViewModel>();
    builder.Services.AddSingleton<SyncStatusViewModel>();
    // ... other ViewModels

    var app = builder.Build();
    ServiceLocator.Services = app.Services;

    return app;
}
```

**Usage in Pages:**
```xml
<!-- MainPage.xaml - Features are self-contained -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:currentPrice="clr-namespace:Maui.Features.CurrentPrice"
             xmlns:syncStatus="clr-namespace:Maui.Features.SyncStatus"
             x:Class="Maui.MainPage">
    <VerticalStackLayout>
        <!-- Each view resolves its own ViewModel - no parent BindingContext needed -->
        <syncStatus:SyncStatusView />
        <currentPrice:CurrentPriceView />
    </VerticalStackLayout>
</ContentPage>
```

# Solution Proposals

## 1. Constructor Injection (Traditional DI)

Inject ViewModels via constructor parameters in the View's code-behind.

### Pros/Cons

**Pros:**
- Standard .NET DI pattern
- Explicit dependencies visible in constructor
- Compile-time safety - missing registrations fail at app startup
- Easy unit testing with mock dependencies
- No coupling to DI container
- Compiler assistance and IntelliSense support

**Cons:**
- **XAML incompatible**: XAML requires parameterless constructors for views instantiated in markup
- Doesn't work for ContentViews declared in XAML (only for programmatically created views)
- Would require code-behind instantiation of all views
- Loses declarative XAML benefits

**Why Rejected:**
XAML fundamentally cannot pass constructor parameters, making this pattern impossible for views declared in markup. This is a framework constraint, not a design choice.

## 2. Inherited BindingContext (Parent Injection)

Child ContentViews inherit their BindingContext from parent pages or containers.

**Implementation:**
```csharp
// MainViewModel contains all child ViewModels
public class MainViewModel : ObservableObject
{
    public CurrentPriceViewModel CurrentPrice { get; }
    public SyncStatusViewModel SyncStatus { get; }

    public MainViewModel(IPriceRepository repo, IPriceSyncService sync)
    {
        CurrentPrice = new CurrentPriceViewModel(repo);
        SyncStatus = new SyncStatusViewModel(sync);
    }
}
```

```xml
<!-- Parent sets BindingContext for children -->
<ContentPage BindingContext="{Binding MainViewModel}">
    <VerticalStackLayout>
        <syncStatus:SyncStatusView BindingContext="{Binding SyncStatus}" />
        <currentPrice:CurrentPriceView BindingContext="{Binding CurrentPrice}" />
    </VerticalStackLayout>
</ContentPage>
```

### Pros/Cons

**Pros:**
- No ServiceLocator anti-pattern
- ViewModel creation uses proper constructor injection
- Explicit dependency graph in parent ViewModel
- Standard MVVM pattern
- Testability of parent ViewModel is excellent

**Cons:**
- **Feature coupling**: Features become tightly coupled to specific parent pages
- **Lost portability**: Moving `CurrentPriceView` to `SettingsPage` requires refactoring `MainViewModel` and creating/updating `SettingsViewModel`
- **Not vertical slices**: Features are not self-contained - they depend on parent structure
- **Code duplication**: Reusing a feature in multiple pages requires duplicating ViewModel creation logic
- **Rigid structure**: UI reorganization requires ViewModel refactoring
- **Property paths**: Bindings must know parent structure (e.g., `{Binding Parent.SomeProperty}`)

**Why Rejected:**
Defeats the primary architectural goal of portable, self-contained vertical slices. Features should be movable without cascading refactoring across ViewModels.

## 3. SubViewModel Pattern (Manual Instantiation)

Parent ViewModel manually instantiates child ViewModels in its constructor, passing dependencies.

**Implementation:**
```csharp
public class MainViewModel : ObservableObject
{
    public CurrentPriceViewModel CurrentPrice { get; }

    public MainViewModel(IPriceRepository repo, Func<DateTime> timeProvider)
    {
        // Manual instantiation - lose DI benefits
        CurrentPrice = new CurrentPriceViewModel(repo, timeProvider);
    }
}
```

### Pros/Cons

**Pros:**
- Parent ViewModel uses constructor injection
- No ServiceLocator in Views
- Sub-ViewModels get their dependencies

**Cons:**
- Same coupling issues as Inherited BindingContext
- **Manual dependency passing**: Must manually thread all dependencies through parent to children
- **Breaks DI chain**: Child ViewModels aren't in DI container, can't resolve their own dependencies
- **Maintenance burden**: Adding a dependency to `CurrentPriceViewModel` requires updating `MainViewModel` constructor
- **Testing complexity**: Must mock and pass all child dependencies through parent
- **Not scalable**: Deep nesting creates dependency passing nightmare

**Why Rejected:**
Creates tight coupling and breaks the DI chain. Features are still not independent or easily movable.

## 4. BindableProperties Instead of ViewModels

Treat ContentViews as custom controls with explicit `BindableProperty` declarations instead of ViewModels.

**Implementation:**
```csharp
public partial class CurrentPriceView : ContentView
{
    public static readonly BindableProperty CurrentPriceProperty =
        BindableProperty.Create(nameof(CurrentPrice), typeof(decimal), typeof(CurrentPriceView));

    public decimal CurrentPrice
    {
        get => (decimal)GetValue(CurrentPriceProperty);
        set => SetValue(CurrentPriceProperty, value);
    }

    // Many more properties...
}
```

### Pros/Cons

**Pros:**
- Standard pattern for reusable MAUI controls
- Properties are explicit and discoverable
- No ViewModel needed for simple displays
- Good for truly stateless UI components

**Cons:**
- **Too verbose for complex features**: Each property requires BindableProperty boilerplate
- **No business logic encapsulation**: Where does feature logic live?
- **Command handling**: Difficult to implement commands and complex interactions
- **State management**: No good place for feature state
- **Testing**: Can't test business logic separately from UI
- **Not MVVM**: Loses separation of concerns

**Why Rejected:**
Only suitable for simple, stateless UI components. Our features have business logic, commands, and state that belong in ViewModels.

## 5. ServiceLocator Pattern (Chosen)

Use a ServiceLocator with XAML markup extension to resolve ViewModels from the DI container.

**Implementation:** See Decision section above.

### Pros/Cons

**Pros:**
- **True feature independence**: Each ContentView is self-contained
- **Easy portability**: Features can move between pages without refactoring
- **Vertical slice architecture**: Features own their ViewModel resolution
- **XAML compatible**: Works with XAML's parameterless constructor requirement
- **Rapid reorganization**: UI changes don't require ViewModel changes
- **Reusability**: Features can appear in multiple pages simultaneously
- **DI benefits for ViewModels**: ViewModels still use constructor injection for their dependencies

**Cons:**
- **ServiceLocator anti-pattern**: Dependencies hidden, resolved at runtime
- **Runtime errors**: Missing registrations discovered late
- **Testing coupling**: Views coupled to DI container
- **Hidden dependencies**: Can't see ViewModel dependencies from View signature

**Why Chosen:**
Balances MAUI's technical constraints (XAML parameterless constructors) with architectural goals (portable vertical slices). The anti-pattern trade-off is acceptable given the mitigations (startup validation, x:DataType) and the significant composability benefits.

## 6. IPlatformApplication.Current.Services (Manual Resolution)

Manually resolve ViewModels in code-behind using MAUI's static service provider.

**Implementation:**
```csharp
public partial class CurrentPriceView : ContentView
{
    public CurrentPriceView()
    {
        InitializeComponent();
        BindingContext = IPlatformApplication.Current.Services
            .GetService<CurrentPriceViewModel>();
    }
}
```

### Pros/Cons

**Pros:**
- Same portability benefits as ServiceLocator
- No custom markup extension needed
- Uses MAUI's built-in APIs

**Cons:**
- Same anti-pattern issues as ServiceLocator
- Less declarative (code-behind vs XAML)
- Couples to MAUI platform APIs
- Harder to see ViewModel dependency in XAML
- More boilerplate in code-behind

**Why Rejected:**
Functionally equivalent to ServiceLocator but less declarative. The custom XAML markup extension makes the ViewModel dependency more visible and keeps code-behind minimal.

---

## Related Decisions

- [Vertical Slice Architecture with Parallel Folders](vertical-slice-architecture.md) - Features are organized as independent vertical slices
- [ContentView Over ContentPage for Features](contentview-over-contentpage.md) - Features are implemented as composable ContentViews
- [Three-Layer Project Architecture](three-layer-architecture.md) - ViewModels in Maui.Core, Views in Maui

## References

- [.NET MAUI Dependency Injection](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection)
- [Service Locator Anti-Pattern Discussion](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/)
- [MAUI ContentView DI Discussion](https://github.com/dotnet/maui/discussions/8363)
