using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Maui.IntegrationTests.Infrastructure;

/// <summary>
/// Defines test collections for xUnit.
/// Tests in the same collection run sequentially and can share a fixture.
/// Tests in different collections can run in parallel.
/// </summary>

/// <summary>
/// Collection for API integration tests that may need to share resources.
/// </summary>
[CollectionDefinition("API Integration Tests")]
public class ApiIntegrationTestCollection : ICollectionFixture<ApiTestFixture>
{
}

/// <summary>
/// Collection for database integration tests that may need to share resources.
/// </summary>
[CollectionDefinition("Database Integration Tests")]
public class DatabaseIntegrationTestCollection : ICollectionFixture<DatabaseTestFixture>
{
}

/// <summary>
/// Base fixture for API integration tests.
/// </summary>
public class ApiTestFixture : TestFixtureBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Configure API-specific services here
        // Example: HTTP clients, API mocks, etc.
    }
}

/// <summary>
/// Base fixture for database integration tests.
/// </summary>
public class DatabaseTestFixture : TestFixtureBase
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Configure database-specific services here
        // Example: test database connections, migrations, etc.
    }
}
