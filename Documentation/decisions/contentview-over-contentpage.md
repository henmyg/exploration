# ContentView Over ContentPage for Features

How should we structure MAUI feature UI components to maximize reusability and composability?

# Decision

We've chosen to implement features as **ContentView components** rather than ContentPage, because this enables better composition, reusability, and flexibility in how features are combined and displayed.

## Impact

- **Composability**: Multiple features can be combined on a single page
- **Reusability**: Feature views can be embedded in different page layouts
- **Flexibility**: Easy to rearrange or reorganize features without refactoring
- **Testability**: Smaller, focused UI components are easier to test
- **Separation**: Each feature manages its own layout independently
- **Navigation**: Pages control navigation, features don't need to be aware of navigation context

**Implementation Pattern:**
```xml
<!-- Feature as ContentView -->
<ContentView xmlns="..."
             xmlns:locator="clr-namespace:Maui.Features"
             xmlns:vm="clr-namespace:Maui.Core.Features.CurrentPrice;assembly=Maui.Core"
             x:Class="Maui.Features.CurrentPrice.CurrentPriceView"
             x:DataType="vm:CurrentPriceViewModel">
    <ContentView.BindingContext>
        <locator:Locator ViewModelType="{x:Type vm:CurrentPriceViewModel}" />
    </ContentView.BindingContext>
    <VerticalStackLayout>
        <!-- Feature UI -->
    </VerticalStackLayout>
</ContentView>

<!-- Composed into Page -->
<ContentPage xmlns="..." x:Class="Maui.MainPage">
    <VerticalStackLayout>
        <syncStatus:SyncStatusView />
        <currentPrice:CurrentPriceView />
    </VerticalStackLayout>
</ContentPage>
```

**Note:** See [ServiceLocator Pattern for ViewModel Injection](service-locator-for-viewmodel-injection.md) for details on how ViewModels are injected into ContentViews.

# Solution Proposals

## 1. Features as ContentPage

Implement each feature as its own ContentPage with navigation between features.

### Pros/Cons

**Pros:**
- Standard MAUI pattern
- Built-in navigation support
- Each feature is a full-screen experience
- Clear feature boundaries

**Cons:**
- Can't combine multiple features on one screen
- Navigation overhead for simple features
- Harder to create dashboard-style layouts
- Each feature must be navigated to separately
- More complex navigation code
- Can't easily show multiple features simultaneously

## 2. Features as ContentView (Chosen)

Implement features as ContentView components that are composed into ContentPages.

### Pros/Cons

**Pros:**
- Multiple features on one screen
- Reusable across different pages
- Easy to rearrange layout
- Smaller, focused components
- Better testability
- Flexible composition
- Natural dashboard patterns
- Features don't need navigation awareness

**Cons:**
- ContentPages required for composition
- Slightly more initial setup
- Must be disciplined about feature boundaries
- ViewModel injection requires ServiceLocator pattern (see related decision)

## 3. Features as UserControl (Legacy Pattern)

Use traditional UserControl approach from WPF/WinForms.

### Pros/Cons

**Pros:**
- Familiar to WPF/WinForms developers
- Encapsulated components

**Cons:**
- Not the modern MAUI pattern
- ContentView is the MAUI equivalent
- Less documentation and examples
- Doesn't follow MAUI best practices

## 4. Features as DataTemplates

Define features as DataTemplates selected by a ContentPresenter.

### Pros/Cons

**Pros:**
- Dynamic feature selection
- Clean MVVM pattern
- Easy to add/remove features at runtime

**Cons:**
- Overly complex for static feature layouts
- Harder to reason about structure
- More difficult to maintain
- Overkill for simple compositions
- DataTemplate syntax is more verbose
- Not intuitive for developers unfamiliar with pattern
