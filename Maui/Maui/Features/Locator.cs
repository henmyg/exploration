namespace Maui.Features
{
    public static class ServiceLocator
    {
        public static IServiceProvider Services { get; set; } = null!;
    }

    [ContentProperty(nameof(ViewModelType))]
    public class LocatorExtension : IMarkupExtension
    {
        // The ViewModel type you want to resolve
        public Type? ViewModelType { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ViewModelType == null)
                throw new InvalidOperationException("ViewModelType must be set.");

            // Resolve the ViewModel from MAUI DI container
            return ViewModelType.IsInterface
                ? ServiceLocator.Services.GetRequiredService(ViewModelType)
                : ActivatorUtilities.GetServiceOrCreateInstance(ServiceLocator.Services, ViewModelType);
        }
    }
}
