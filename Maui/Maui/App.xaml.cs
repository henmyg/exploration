using Maui.Core.Shared.Services;

namespace Maui
{
    public partial class App : Application
    {
        private readonly AppShell _appShell;

        public App(AppShell appShell)
        {
            InitializeComponent();
            _appShell = appShell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(_appShell);
        }

        protected override async void OnStart()
        {
            var services = Current?.Handler.MauiContext?.Services;
            var hostedService = services?.GetRequiredService<BackgroundPriceSyncService>() ?? throw new ArgumentNullException(nameof(BackgroundPriceSyncService));
            
            await hostedService.StartAsync(CancellationToken.None);
        }
    }
}