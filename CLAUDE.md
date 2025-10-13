# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET MAUI (Multi-platform App UI) cross-platform application targeting:
- Android (net9.0-android, min SDK 21.0)
- iOS (net9.0-ios, min 15.0)
- macOS Catalyst (net9.0-maccatalyst, min 15.0)
- Windows (net9.0-windows10.0.19041.0, min 10.0.17763.0)
- Tizen (optional, currently commented out)

The project uses .NET 9.0 with nullable reference types enabled and implicit usings.

## Build and Run Commands

### Building the project
```bash
# Build for all target frameworks
dotnet build Maui/Maui.sln

# Build for a specific platform
dotnet build Maui/Maui.csproj -f net9.0-android
dotnet build Maui/Maui.csproj -f net9.0-ios
dotnet build Maui/Maui.csproj -f net9.0-windows10.0.19041.0
```

### Running the project
```bash
# Run on Windows
dotnet run --project Maui/Maui.csproj -f net9.0-windows10.0.19041.0

# Run on Android emulator/device
dotnet build Maui/Maui.csproj -f net9.0-android -t:Run

# Run on iOS simulator
dotnet build Maui/Maui.csproj -f net9.0-ios -t:Run
```

### Clean build artifacts
```bash
dotnet clean Maui/Maui.sln
```

## Architecture

### Vertical Slice Architecture

This project uses **Vertical Slice Architecture** where features are organized by business capability rather than technical layer. Each feature contains all the code needed for that functionality in a single folder.

**Features/** - Each feature is self-contained in its own folder:
- `Features/Counter/` - Counter feature example
  - `CounterPage.xaml` - UI definition
  - `CounterPage.xaml.cs` - Minimal code-behind (sets BindingContext)
  - `CounterViewModel.cs` - Business logic and state management

**Benefits of Vertical Slices:**
- Related code stays together (high cohesion)
- Easy to find everything for a feature
- Features can be added/removed independently
- Clear boundaries between features
- Reduces coupling between unrelated features

### MVVM Pattern Within Features

Each feature uses **MVVM (Model-View-ViewModel)** pattern with CommunityToolkit.Mvvm:

1. **ViewModel** - Business logic and state
   - Inherits from `ObservableObject` (CommunityToolkit.Mvvm)
   - Use `[ObservableProperty]` for bindable properties (source generator creates public property)
   - Use `[RelayCommand]` for commands (source generator creates `{MethodName}Command` property)
   - Registered in DI container as singleton or transient

2. **Page (View)** - UI presentation
   - XAML defines UI with data bindings to ViewModel
   - Code-behind only receives ViewModel via constructor and sets `BindingContext`
   - Registered in DI container

3. **Dependency Injection** - Configured in `MauiProgram.cs`
   - Each feature's ViewModel and Page registered together
   - Shell and App also use constructor injection

### Application Entry Points

1. **MauiProgram.cs** - Application bootstrapping
   - Configures the MAUI app builder
   - Registers fonts (OpenSans-Regular, OpenSans-Semibold)
   - **Registers all features (ViewModels and Pages) in DI container**
   - Adds debug logging in DEBUG builds
   - Entry point: `MauiProgram.CreateMauiApp()`

2. **App.xaml.cs** - Application lifecycle
   - Receives `AppShell` via constructor injection
   - Overrides `CreateWindow` to return window with injected AppShell

3. **AppShell.xaml** - Shell-based navigation container
   - Manages app navigation structure via routes
   - Uses `ContentTemplate="{DataTemplate featureName:PageName}"` for DI-compatible page creation
   - Currently contains CounterPage route

### Platform-Specific Code

Platform-specific implementations are located in `Maui/Maui/Platforms/`:
- **Android/** - Android-specific initialization (MainActivity, MainApplication)
- **iOS/** - iOS-specific initialization (AppDelegate, Program)
- **MacCatalyst/** - macOS Catalyst initialization
- **Windows/** - Windows-specific initialization (App.xaml.cs)
- **Tizen/** - Tizen-specific initialization (currently unused)

Each platform has its own entry points that eventually call `MauiProgram.CreateMauiApp()`.

### Resources Structure

- **Resources/AppIcon/** - Application icon assets
- **Resources/Splash/** - Splash screen assets
- **Resources/Images/** - Image assets
- **Resources/Fonts/** - Custom fonts
- **Resources/Styles/** - XAML style definitions (Colors.xaml, Styles.xaml)
- **Resources/Raw/** - Raw asset files

### Project Configuration

- **ApplicationId**: `com.companyname.maui`
- **ApplicationTitle**: Maui
- **Version**: 1.0 (Display), 1 (Build)
- **WindowsPackageType**: None (unpackaged for development)
- Build artifacts are in `bin/` and `obj/` directories (git-ignored)

## Key Dependencies

- Microsoft.Maui.Controls (version managed by MauiVersion property)
- Microsoft.Extensions.Logging.Debug 9.0.5
- CommunityToolkit.Mvvm 8.4.0 (provides MVVM source generators and base classes)

## Development Notes

- The project uses **MVVM pattern** with CommunityToolkit.Mvvm source generators
- Implicit usings are enabled, so common namespaces are automatically imported
- MAUI uses single-project structure (SingleProject=true) for managing all platforms
- For Windows development, use the "Windows Machine" launch profile defined in Properties/launchSettings.json

### Static Operations Pattern Enforcement

**IMPORTANT**: This codebase follows a pattern where pure functions (logic without dependencies or side effects) should be extracted into companion static operations classes.

**When to proactively check:**
- After implementing new features or services
- When the user asks you to review code quality
- Periodically when making significant changes to ViewModels or Services

**How to check:**
1. Search for ViewModels in `Maui/Features/`
2. Search for Services in `Maui/Shared/Services/`
3. Look for pure logic that could be extracted:
   - Data transformations and mappings
   - Calculations and formatting
   - Date/time manipulations
   - Validation rules
   - Any logic that doesn't use class fields/properties or have side effects

**Pattern reference**: See `PriceSyncService.cs` and `PriceSyncOperations` for the reference implementation.

**Detailed documentation**: See `docs/architecture.md` section "Static Operations Classes" for complete pattern description.

If you find extractable logic, inform the user and offer to extract it.

### Adding New Features

When creating a new feature, follow the vertical slice pattern:

1. Create feature folder in `Features/`:
   ```
   Features/
     MyFeature/
       MyFeaturePage.xaml
       MyFeaturePage.xaml.cs
       MyFeatureViewModel.cs
   ```

2. Create ViewModel with business logic:
   ```csharp
   namespace Maui.Features.MyFeature
   {
       public partial class MyFeatureViewModel : ObservableObject
       {
           [ObservableProperty]
           private string myProperty;

           [RelayCommand]
           private void MyAction() { /* logic */ }
       }
   }
   ```

3. Create Page XAML with bindings:
   ```xml
   <ContentPage xmlns:myFeature="clr-namespace:Maui.Features.MyFeature"
                x:Class="Maui.Features.MyFeature.MyFeaturePage"
                x:DataType="myFeature:MyFeatureViewModel">
       <Button Text="{Binding MyProperty}"
               Command="{Binding MyActionCommand}" />
   </ContentPage>
   ```

4. Create Page code-behind with ViewModel injection:
   ```csharp
   namespace Maui.Features.MyFeature
   {
       public partial class MyFeaturePage : ContentPage
       {
           public MyFeaturePage(MyFeatureViewModel viewModel)
           {
               InitializeComponent();
               BindingContext = viewModel;
           }
       }
   }
   ```

5. Register in `MauiProgram.cs`:
   ```csharp
   using Maui.Features.MyFeature;

   // In CreateMauiApp():
   builder.Services.AddSingleton<MyFeatureViewModel>();
   builder.Services.AddSingleton<MyFeaturePage>();
   ```

6. Add route in `AppShell.xaml`:
   ```xml
   <Shell xmlns:myFeature="clr-namespace:Maui.Features.MyFeature">
       <ShellContent Title="My Feature"
                     ContentTemplate="{DataTemplate myFeature:MyFeaturePage}"
                     Route="MyFeaturePage" />
   </Shell>
   ```
