using Microsoft.Extensions.DependencyInjection;

namespace Maui.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for test fixtures that can be shared across multiple test classes.
/// Implements IDisposable for proper cleanup.
/// </summary>
public abstract class TestFixtureBase : IDisposable
{
    protected ServiceProvider ServiceProvider { get; private set; }

    protected TestFixtureBase()
    {
        ServiceProvider = BuildServiceProvider();
        OnFixtureSetup();
    }

    /// <summary>
    /// Override this method to configure services for the test fixture.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override in derived classes to add specific services
    }

    /// <summary>
    /// Called once after the fixture is constructed.
    /// Use this for one-time setup operations.
    /// </summary>
    protected virtual void OnFixtureSetup()
    {
        // Override in derived classes for custom setup
    }

    /// <summary>
    /// Called once before the fixture is disposed.
    /// Use this for one-time cleanup operations.
    /// </summary>
    protected virtual void OnFixtureTeardown()
    {
        // Override in derived classes for custom cleanup
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

    public void Dispose()
    {
        OnFixtureTeardown();
        ServiceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
