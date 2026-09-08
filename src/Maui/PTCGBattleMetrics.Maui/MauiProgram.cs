using Microsoft.Extensions.Logging;
using PTCGBattleMetrics.Application.Services;
using PTCGBattleMetrics.Maui.Services.Data;
using PTCGBattleMetrics.Maui.Services.Settings;
using PTCGBattleMetrics.Maui.ViewModels;
using PTCGBattleMetrics.Maui.Views;

namespace PTCGBattleMetrics.Maui;

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
            })
            .RegisterAppServices()
            .RegisterViewModels()
            .RegisterViews();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
    {
        // Enterprise Pattern: Settings & Preferences (Chapter 8)
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

        // Application Domain Services (Clean Architecture)
        builder.Services.AddSingleton<IMetricsService, MetricsService>();

        // Resilient HTTP Client
        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<ISettingsService>();
            var endpoint = settings.ApiEndpointBase.TrimEnd('/') + "/";
            var handler = new HttpClientHandler();
            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
            if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
            {
                client.BaseAddress = uri;
            }
            return client;
        });

        // Enterprise Pattern: Data Access & Offline-First Cache (Chapter 10)
        builder.Services.AddSingleton<IMauiBattleMetricsService, MauiBattleMetricsService>();

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        // Enterprise Pattern: MVVM CommunityToolkit ViewModels (Chapter 3)
        builder.Services.AddTransient<FastInputViewModel>();
        builder.Services.AddTransient<MatchupMatrixViewModel>();
        builder.Services.AddTransient<DecksViewModel>();
        builder.Services.AddTransient<TournamentsViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        return builder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        // Enterprise Pattern: Shell & Page DI Constructor Injection (Chapter 4)
        builder.Services.AddTransient<FastInputPage>();
        builder.Services.AddTransient<MatchupMatrixPage>();
        builder.Services.AddTransient<DecksPage>();
        builder.Services.AddTransient<TournamentsPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<SettingsPage>();

        return builder;
    }
}
