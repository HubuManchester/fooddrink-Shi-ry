using Microsoft.Extensions.Logging;
using FoodApp.Services;
using FoodApp.ViewModels;
using FoodApp.Views;
using CommunityToolkit.Maui;

namespace FoodApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<FoodDatabaseService>();
        builder.Services.AddSingleton<FoodService>();

        // registrate ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<AddItemViewModel>();
        builder.Services.AddTransient<DetailViewModel>();

        // registration
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddItemPage>();
        builder.Services.AddTransient<DetailPage>();
        builder.Services.AddTransient<EditItemPage>();
        builder.Services.AddTransient<EditItemViewModel>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<FavoritesViewModel>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}