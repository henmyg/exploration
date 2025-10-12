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

```bash
# Run all integration tests
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj

# Run specific test class
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj --filter FullyQualifiedName~MyFeatureIntegrationTests

# Run tests with detailed output
dotnet test Maui.IntegrationTests/Maui.IntegrationTests.csproj --logger "console;verbosity=detailed"
```

## Best Practices

1. **Keep tests independent** - Each test should be able to run in isolation
2. **Use meaningful names** - Test names should describe what is being tested
3. **Clean up resources** - Use Dispose or cleanup methods to release resources
4. **Mock external dependencies** - Use test doubles for external APIs, databases, etc.
5. **Test realistic scenarios** - Integration tests should test real-world use cases
6. **Keep tests fast** - Avoid unnecessary delays or sleeps
