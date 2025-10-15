# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

.NET MAUI electricity price optimizer with multi-project architecture. Targets: Android, iOS, macOS, Windows. Uses .NET 9.0.

**For detailed architecture**, see `Documentation/docs/architecture.md`.
**For architectural decisions**, see `Documentation/decisions/` - records the reasoning behind key project decisions.

## Build & Test

```bash
# Build solution
dotnet build Maui.sln

# Run tests (Linux-compatible)
dotnet test Maui/Maui.Tests
dotnet test Maui/Maui.IntegrationTests

# Run app (Windows)
dotnet run --project Maui/Maui/Maui.csproj -f net9.0-windows10.0.19041.0
```

## Project Structure

Three-layer architecture for Linux CI/CD compatibility:

1. **Maui.Infrastructure** - Repositories, services, API clients (Linux-compatible)
2. **Maui.Core** - ViewModels, business logic (Linux-compatible, uses CommunityToolkit.Mvvm)
3. **Maui** - XAML pages, platform-specific code (requires MAUI workloads)

```
Maui/
├── Maui/                      # UI layer (Windows-only)
│   └── Features/              # Feature pages (XAML)
├── Maui.Core/                 # Logic layer (Linux-compatible)
│   └── Features/              # ViewModels
├── Maui.Infrastructure/       # Data layer (Linux-compatible)
│   ├── Api/                   # API clients
│   ├── Repositories/          # Data storage
│   └── Services/              # Background services
├── Maui.Tests/                # Unit tests
├── Maui.IntegrationTests/     # Integration tests
└── Maui.Analyzers/            # Roslyn analyzer
```

## Architecture Patterns

### Multi-Project Vertical Slices

Features span three projects with parallel folder structure:

```
Maui/Features/MyFeature/
├── MyFeaturePage.xaml         # UI (Maui project)
└── MyFeaturePage.xaml.cs

Maui.Core/Features/MyFeature/
├── MyFeatureViewModel.cs      # Logic (Maui.Core project)
└── MyFeatureOperations.cs     # Static operations
```

### MVVM with CommunityToolkit.Mvvm

- **ViewModels**: In Maui.Core, use `[ObservableProperty]` and `[RelayCommand]`
- **Views**: In Maui, **prefer ContentView over ContentPage** for features (better composability)
  - XAML with data bindings, code-behind sets `BindingContext`
  - ContentPages compose multiple ContentViews together
- **DI**: Register in `MauiProgram.cs`, constructor injection throughout

### Static Operations Classes

**CRITICAL PATTERN**: Extract pure functions into companion static operations classes (same file).

```csharp
// Service/ViewModel with dependencies
public class MyService(IDependency dep)
{
    public async Task DoWork()
    {
        var data = await dep.GetDataAsync();
        var result = data.Transform(); // Pure logic → operations class
        await dep.SaveAsync(result);
    }
}

// Static operations with pure functions (extension methods preferred)
internal static class MyServiceOperations
{
    public static Result Transform(this Data data) => new Result { /* ... */ };
}
```

**When extractable:**
- Data transformations/mappings
- Calculations, formatting
- Date/time manipulations
- Validation rules
- Any logic without dependencies/side effects

**Reference**: `PriceSyncService.cs` and `PriceSyncOperations`

## Adding New Features

1. **Create ViewModels** in `Maui.Core/Features/MyFeature/`:
   - `MyFeatureViewModel.cs` (inherits `ObservableObject`)
   - `MyFeatureOperations.cs` (static operations if needed)

2. **Create ContentViews** in `Maui/Features/MyFeature/`:
   - `MyFeatureView.xaml` (**use ContentView, not ContentPage**)
   - `MyFeatureView.xaml.cs` (set `BindingContext` only)
   ```xml
   <ContentView xmlns="..." x:Class="..." x:DataType="...ViewModel">
       <!-- Feature UI -->
   </ContentView>
   ```

3. **Compose into pages** (if needed):
   ```xml
   <!-- MainPage.xaml -->
   <ContentPage>
       <myFeature:MyFeatureView BindingContext="{Binding MyFeature}" />
   </ContentPage>
   ```

4. **Register in `MauiProgram.cs`**:
   ```csharp
   builder.Services.AddSingleton<MyFeatureViewModel>();
   builder.Services.AddSingleton<MyFeatureView>();
   ```

## Key Conventions

- **Dependency flow**: Maui → Maui.Core → Maui.Infrastructure (never reverse)
- **ContentView over ContentPage**: Features are ContentViews, composed into ContentPages
- **Event-driven updates**: Repositories raise events, ViewModels subscribe
- **Extension methods**: Prefer fluent API style for operations
- **DI lifetimes**: Singletons for services/repos, transient for views
- **Testing**: Unit tests for Core/Infrastructure, integration tests for API

## CI/CD

- **Linux runners**: Test Maui.Core and Maui.Infrastructure (fast, cheap)
- **Windows runners**: Only when MAUI UI build required (slow, expensive)
- **Analyzer validation**: Warns if pure functions not extracted

## Decision Records

Architectural and technical decisions are documented in `Documentation/decisions/`. When making significant design choices:

1. **Create a decision record** following the template in `Documentation/decisions/README.md`
2. **Include**: Problem statement, chosen solution with rationale, impact analysis, alternative proposals with pros/cons
3. **Add to index**: Update the index in `Documentation/decisions/README.md`

**Important**:
- **Documentation files** (like `architecture.md`) are **snapshots of current implementation** - the "what" and "how"
- **Decision records** are a **historical trail** - capturing decisions made, overruled, or changed throughout the project lifecycle
- Keep docs free of detailed reasoning. The "why" goes in decision records, which are never deleted or overwritten - only added to
- This separation keeps documentation concise while maintaining a complete decision history
