# Three-Layer Project Architecture for Cross-Platform CI/CD

How should we structure the .NET MAUI project to enable fast, cost-effective CI/CD testing while maintaining clear separation of concerns?

# Decision

We've chosen a **three-layer project architecture** (Maui.Infrastructure → Maui.Core → Maui) with MAUI dependencies only in the top layer, because this enables Linux-compatible testing for business logic and infrastructure, dramatically reducing CI/CD costs and execution time.

## Impact

- **CI/CD Performance**: Business logic tests run on Linux (fast, cheap) instead of Windows (slow, expensive)
- **Development Speed**: Core and Infrastructure tests execute in seconds instead of minutes
- **Cost Efficiency**: Linux runners are free for public repos and significantly cheaper for private repos
- **Test Coverage**: Developers can run comprehensive tests locally without MAUI workloads installed
- **Maintenance**: Clear separation of concerns makes refactoring and testing easier

**Implementation:**

1. **Maui.Infrastructure** (bottom layer)
   - API clients, repositories, services
   - No project dependencies
   - Linux-compatible (no MAUI, no CommunityToolkit.Mvvm)

2. **Maui.Core** (middle layer)
   - ViewModels, business logic, app-specific services
   - Depends only on Maui.Infrastructure
   - Uses CommunityToolkit.Mvvm (Linux-compatible)
   - No MAUI UI dependencies

3. **Maui** (top layer)
   - XAML pages, platform-specific code
   - Depends on both Core and Infrastructure
   - Requires MAUI workloads (Windows/macOS runners for CI/CD)

# Solution Proposals

## 1. Single MAUI Project

All code (UI, business logic, infrastructure) in one MAUI project.

### Pros/Cons

**Pros:**
- Simple - everything in one place
- No need to manage project references
- Standard .NET MAUI template structure

**Cons:**
- All tests require Windows runners (slow, expensive)
- Can't test business logic without MAUI workloads installed
- Tight coupling between UI and business logic
- Longer CI/CD pipeline execution (5-10 minutes vs seconds)
- Higher CI/CD costs for private repositories
- Developers need full MAUI setup to run tests

## 2. Three-Layer Architecture (Chosen)

Split into Infrastructure (data), Core (business logic), and Maui (UI) projects, with MAUI dependencies only in the top layer.

### Pros/Cons

**Pros:**
- Business logic tests run on Linux (fast: ~30 seconds vs 5+ minutes)
- Linux runners are free (public) or cheaper (private)
- Core/Infrastructure testable without MAUI installation
- Clear separation of concerns
- Forces proper dependency management
- Easier to maintain and refactor
- Can run most tests in IDE without platform-specific setup

**Cons:**
- More projects to manage (3 vs 1)
- Need to understand dependency flow
- Slightly more initial setup time
- Must be disciplined about not adding MAUI dependencies to lower layers

## 3. Two-Layer Architecture (UI + Business)

Split into UI layer (Maui) and Business layer (everything else combined).

### Pros/Cons

**Pros:**
- Simpler than three layers
- Business logic still testable on Linux
- Clear UI vs logic separation

**Cons:**
- Infrastructure and business logic mixed together
- Repository and service boundaries less clear
- Harder to reason about data flow
- API clients mixed with ViewModels
- Less flexibility for future architectural changes

## 4. Platform-Specific Class Libraries

Keep MAUI project but extract testable logic into standard .NET class libraries.

### Pros/Cons

**Pros:**
- Libraries testable on Linux
- Can keep familiar MAUI project structure

**Cons:**
- Doesn't enforce architectural boundaries
- Easy to accidentally add MAUI dependencies to libraries
- ViewModels still in MAUI project (can't test on Linux with CommunityToolkit.Mvvm)
- Unclear where to put business logic vs infrastructure
- Doesn't solve the CommunityToolkit.Mvvm testing issue
