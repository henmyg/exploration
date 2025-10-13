using Microsoft.Extensions.Logging;
using Maui.Core.Features.Counter;
using Maui.Core.Features.CurrentPrice;
using Maui.Features.Counter;
using Maui.Features.CurrentPrice;
using Maui.Core.Shared.Services;
using Maui.Core.Shared.Repositories;

namespace Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Shell
            builder.Services.AddSingleton<AppShell>();

            // Register HttpClient
            builder.Services.AddSingleton<HttpClient>();

            // Register Shared Services
            builder.Services.AddSingleton<IPriceRepository, InMemoryPriceRepository>();
            builder.Services.AddSingleton<PriceSyncService>();

            // Register Background Services
            builder.Services.AddSingleton<BackgroundPriceSyncService>();

            // Register Features
            builder.Services.AddSingleton<CounterViewModel>();
            builder.Services.AddSingleton<CounterPage>();

            builder.Services.AddSingleton<CurrentPriceViewModel>();
            builder.Services.AddSingleton<CurrentPricePage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
