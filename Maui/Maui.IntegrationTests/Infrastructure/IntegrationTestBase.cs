using Microsoft.Extensions.DependencyInjection;

namespace Maui.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests providing common setup and utilities.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; private set; }

    protected IntegrationTestBase()
    {
        ServiceProvider = BuildServiceProvider();
    }

    /// <summary>
    /// Override this method to configure services for your integration tests.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Default empty configuration
        // Override in derived classes to add specific services
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    protected T? GetOptionalService<T>()
    {
        return ServiceProvider.GetService<T>();
    }

    public virtual void Dispose()
    {
        ServiceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
