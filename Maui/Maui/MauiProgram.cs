using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Maui.Core.Features;
using Maui.Core.Features.CurrentPrice;
using Maui.Core.Features.PriceGraph;
using Maui.Core.Features.Settings;
using Maui.Core.Features.Settings.Configuration;
using Maui.Core.Features.SyncStatus;
using Maui.Core.Shared.Repositories;
using Maui.Core.Shared.Services;
using Maui.Features;
using Maui.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseLiveCharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Shell
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

            // Register HttpClient
            builder.Services.AddSingleton<HttpClient>();

            // Register Shared Services
            builder.Services.AddSingleton<IPriceRepository, InMemoryPriceRepository>();
            builder.Services.AddSingleton<IPriceSyncService, PriceSyncService>();
            builder.Services.AddSingleton<ITaskDelayer, SystemTaskDelayer>();

            // Register time provider
            builder.Services.AddSingleton<Func<DateTime>>(sp => () => DateTime.UtcNow);

            // Register Background Services
            builder.Services.AddSingleton<BackgroundPriceSyncService>();

            // Register Features
            builder.Services.AddSingleton<CurrentPriceViewModel>();
            builder.Services.AddSingleton<SyncStatusViewModel>();
            builder.Services.AddSingleton<PriceGraphViewModel>();
            builder.Services.AddSingleton<SettingsViewModel>();
            builder.Services.AddSingleton<PriceAreaViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register routes
            Routing.RegisterRoute("settings/configuration", typeof(ConfigurationPage));

            var app = builder.Build();

            // Give access to services from anywhere
            ServiceLocator.Services = app.Services;
            
            return app;
        }
    }
}
