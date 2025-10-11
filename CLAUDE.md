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

### Application Entry Points

1. **MauiProgram.cs** - Application bootstrapping
   - Configures the MAUI app builder
   - Registers fonts (OpenSans-Regular, OpenSans-Semibold)
   - Adds debug logging in DEBUG builds
   - Entry point: `MauiProgram.CreateMauiApp()`

2. **App.xaml.cs** - Application lifecycle
   - Creates the main application window with AppShell as root

3. **AppShell.xaml** - Shell-based navigation container
   - Manages app navigation structure
   - Currently contains MainPage route

4. **MainPage.xaml** - Main content page
   - Example counter button implementation

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

## Development Notes

- The project uses XAML for UI with code-behind C# files
- Implicit usings are enabled, so common namespaces are automatically imported
- MAUI uses single-project structure (SingleProject=true) for managing all platforms
- For Windows development, use the "Windows Machine" launch profile defined in Properties/launchSettings.json
