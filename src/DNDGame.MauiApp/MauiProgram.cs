using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using DNDGame.MauiApp.Services;
using DNDGame.MauiApp.ViewModels;
using DNDGame.MauiApp.Data;
using DNDGame.MauiApp.Interfaces;
using DNDGame.Core.Interfaces;
using DNDGame.Application.Services;
using Plugin.LocalNotification;

namespace DNDGame.MauiApp;

public static class MauiProgram
{
    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
            });

        // Blazor WebView
        builder.Services.AddMauiBlazorWebView();

        // Database
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "dndgame.db");
        builder.Services.AddDbContext<LocalDatabaseContext>(options =>
            options.UseSqlite($"Filename={dbPath}"));

        // Core Services
        builder.Services.AddScoped<ICharacterService, CharacterService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
        builder.Services.AddScoped<IDiceRoller, DiceRollerService>();

        // Platform Services
        builder.Services.AddSingleton<Interfaces.INotificationService, NotificationService>();
        builder.Services.AddSingleton<Interfaces.IFileService, FileService>();
        builder.Services.AddSingleton<Interfaces.IConnectivityService, ConnectivityService>();
        builder.Services.AddSingleton<Interfaces.IOfflineSyncService, OfflineSyncService>();

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CharacterListViewModel>();
        builder.Services.AddTransient<CharacterDetailViewModel>();
        builder.Services.AddTransient<SessionListViewModel>();
        builder.Services.AddTransient<SessionDetailViewModel>();
        builder.Services.AddTransient<DiceRollerViewModel>();

        // Pages
        builder.Services.AddTransient<Pages.MainPage>();
        builder.Services.AddTransient<Pages.CharacterListPage>();
        builder.Services.AddTransient<Pages.CharacterDetailPage>();
        builder.Services.AddTransient<Pages.CharacterCreatePage>();
        builder.Services.AddTransient<Pages.SessionListPage>();
        builder.Services.AddTransient<Pages.SessionDetailPage>();
        builder.Services.AddTransient<Pages.SessionCreatePage>();
        builder.Services.AddTransient<Pages.DiceRollerPage>();

        // HTTP Client for API
        builder.Services.AddHttpClient("DNDGameAPI", client =>
        {
            client.BaseAddress = new Uri("https://api.dndgame.com/");
            client.DefaultRequestHeaders.Add("User-Agent", "DNDGame-MAUI/1.0");
        });

        // Navigation
        builder.Services.AddSingleton<INavigationService, NavigationService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
