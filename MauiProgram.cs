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

        // 注册服务（替换原来的 FoodService）
        builder.Services.AddSingleton<FoodDatabaseService>();
        builder.Services.AddSingleton<FoodService>();

        // 注册 ViewModels
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<AddItemViewModel>();
        builder.Services.AddTransient<DetailViewModel>();

        // 注册页面
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<AddItemPage>();
        builder.Services.AddTransient<DetailPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}