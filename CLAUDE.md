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

### MVVM Pattern

This project uses the **Model-View-ViewModel (MVVM)** pattern with dependency injection:

1. **ViewModels/** - Contains all ViewModels
   - ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm)
   - Use `[ObservableProperty]` attribute for bindable properties (source generator creates the property automatically)
   - Use `[RelayCommand]` attribute for commands (source generator creates `{MethodName}Command` property)
   - ViewModels are registered in DI container as singletons or transients

2. **Views (Pages)** - XAML pages with minimal code-behind
   - Code-behind only sets `BindingContext` via constructor injection
   - XAML bindings connect to ViewModel properties and commands
   - Views are registered in DI container and injected with their ViewModels

3. **Dependency Injection** - Configured in `MauiProgram.cs`
   - ViewModels registered via `builder.Services.AddSingleton<T>()` or `AddTransient<T>()`
   - Views registered similarly and receive ViewModels through constructor injection
   - Shell and App also use constructor injection for their dependencies

### Application Entry Points

1. **MauiProgram.cs** - Application bootstrapping
   - Configures the MAUI app builder
   - Registers fonts (OpenSans-Regular, OpenSans-Semibold)
   - **Registers all ViewModels and Views in DI container**
   - Adds debug logging in DEBUG builds
   - Entry point: `MauiProgram.CreateMauiApp()`

2. **App.xaml.cs** - Application lifecycle
   - Receives `AppShell` via constructor injection
   - Sets `MainPage` to the injected AppShell

3. **AppShell.xaml** - Shell-based navigation container
   - Manages app navigation structure via routes
   - Uses `ContentTemplate="{DataTemplate local:PageName}"` for DI-compatible page creation
   - Currently contains MainPage route

4. **Views** - Content pages following MVVM
   - XAML defines UI with data bindings to ViewModel
   - Code-behind receives ViewModel via constructor and sets as `BindingContext`
   - Example: MainPage.xaml + MainPage.xaml.cs + MainPageViewModel

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

### Adding New Pages with MVVM

When creating a new page, follow this pattern:

1. Create ViewModel in `ViewModels/` folder:
   ```csharp
   public partial class MyPageViewModel : ObservableObject
   {
       [ObservableProperty]
       private string myProperty;

       [RelayCommand]
       private void MyAction() { /* logic */ }
   }
   ```

2. Register in `MauiProgram.cs`:
   ```csharp
   builder.Services.AddSingleton<MyPageViewModel>();
   builder.Services.AddSingleton<MyPage>();
   ```

3. Create View with XAML bindings:
   ```xml
   <ContentPage x:DataType="viewModels:MyPageViewModel">
       <Button Text="{Binding MyProperty}"
               Command="{Binding MyActionCommand}" />
   </ContentPage>
   ```

4. Inject ViewModel in code-behind:
   ```csharp
   public MyPage(MyPageViewModel viewModel)
   {
       InitializeComponent();
       BindingContext = viewModel;
   }
   ```
