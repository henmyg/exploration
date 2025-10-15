using CommunityToolkit.Maui;
using LiveChartsCore.SkiaSharpView.Maui;
using Maui.Core.Features;
using Maui.Core.Features.CurrentPrice;
using Maui.Core.Features.PriceGraph;
using Maui.Core.Features.SyncStatus;
using Maui.Core.Shared.Repositories;
using Maui.Core.Shared.Services;
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

            // Register HttpClient
            builder.Services.AddSingleton<HttpClient>();

            // Register Shared Services
            builder.Services.AddSingleton<IPriceRepository, InMemoryPriceRepository>();
            builder.Services.AddSingleton<IPriceSyncService, PriceSyncService>();

            // Register Background Services
            builder.Services.AddSingleton<BackgroundPriceSyncService>();

            // Register Features
            builder.Services.AddSingleton<CurrentPriceViewModel>();
            builder.Services.AddSingleton<SyncStatusViewModel>();
            builder.Services.AddSingleton<PriceGraphViewModel>();

            builder.Services.AddSingleton<MainPageModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
