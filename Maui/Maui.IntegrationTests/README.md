# Maui Integration Tests

This project contains integration tests for the Maui application. Integration tests verify that multiple components work together correctly, unlike unit tests which test components in isolation.

## Structure

### Infrastructure/

Base classes and utilities for writing integration tests:

- **IntegrationTestBase.cs** - Base class for integration tests with DI support
- **TestFixtureBase.cs** - Base class for test fixtures that can be shared across test classes
- **TestCollectionDefinitions.cs** - xUnit collection definitions for organizing tests

## Writing Integration Tests

### Basic Integration Test

Inherit from `IntegrationTestBase` and override `ConfigureServices` to set up dependencies:

```csharp
using Maui.IntegrationTests.Infrastructure;

namespace Maui.IntegrationTests.Features;

public class MyFeatureIntegrationTests : IntegrationTestBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register services needed for your tests
        services.AddSingleton<IMyService, MyService>();
    }

    [Fact]
    public void Test_Feature_Works_EndToEnd()
    {
        // Arrange
        var service = GetService<IMyService>();

        // Act
        var result = service.DoSomething();

        // Assert
        Assert.NotNull(result);
    }
}
```

### Using Test Fixtures

For tests that need to share expensive setup across multiple test classes:

```csharp
[Collection("API Integration Tests")]
public class MyApiTests
{
    private readonly ApiTestFixture _fixture;

    public MyApiTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test_Api_Call()
    {
        // Use shared fixture resources
        var service = _fixture.GetService<IApiService>();

        // Test logic
    }
}
```

## Test Collections

Tests are organized into collections defined in `TestCollectionDefinitions.cs`:

- **API Integration Tests** - For tests involving HTTP APIs
- **Database Integration Tests** - For tests involving database operations

Tests in the same collection run sequentially and can share fixtures. Tests in different collections run in parallel.

## Running Tests

### Run Only Integration Tests (Explicitly)

Integration tests are marked with `[Trait("Category", "Integration")]` to separate them from unit tests:

```bash
# Run ONLY integration tests
dotnet test --filter "Category=Integration"

# Run integration tests from the integration test project
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj
```

### Exclude Integration Tests from Normal Test Runs

When running all tests in the solution, exclude integration tests:

```bash
# Run all tests EXCEPT integration tests
dotnet test --filter "Category!=Integration"

# Run only unit tests (assuming your unit tests don't have the Integration trait)
dotnet test Maui.Tests/Maui.Tests.csproj
```

### Other Useful Commands

```bash
# Run specific test class
dotnet test --filter "FullyQualifiedName~PriceApiIntegrationTests"

# Run tests with detailed output
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj --logger "console;verbosity=detailed"

# List all integration tests without running them
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj --list-tests
```

### Visual Studio Configuration

A `.runsettings` file has been created in the solution root that automatically excludes integration tests when you click "Run All Tests" in Visual Studio.

**Visual Studio will automatically:**
- Exclude integration tests when using "Run All Tests" or Test Explorer
- Apply the filter `Category!=Integration` by default

**To run integration tests in Visual Studio:**
1. Open Test Explorer (Test > Test Explorer)
2. Right-click on the `Maui.IntegrationTests` project
3. Select "Run Tests"

**OR manually configure the filter:**
1. Go to Test > Configure Run Settings > Select Solution Wide runsettings File
2. Select the `.runsettings` file in the solution root (if not auto-detected)

**To temporarily include integration tests:**
- In Test Explorer, use the filter box and enter `Trait:Category=Integration`

## Best Practices

1. **Keep tests independent** - Each test should be able to run in isolation
2. **Use meaningful names** - Test names should describe what is being tested
3. **Clean up resources** - Use Dispose or cleanup methods to release resources
4. **Mock external dependencies** - Use test doubles for external APIs, databases, etc.
5. **Test realistic scenarios** - Integration tests should test real-world use cases
6. **Keep tests fast** - Avoid unnecessary delays or sleeps
