using Microsoft.Extensions.Logging;
using RentalApp.ViewModels;
using RentalApp.Database.Data;
using RentalApp.Views;
using System.Diagnostics;
using RentalApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace RentalApp;

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

        // Database context is NOT needed for MAUI app anymore
        // All database operations go through the API
        // builder.Services.AddDbContext<AppDbContext>();

        // Authentication Service - Using HTTP API with JWT tokens
        // All database calls go through the API
        builder.Services.AddSingleton<IAuthenticationService, HttpAuthenticationService>();

        // Core services
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // HTTP Client for API services
        builder.Services.AddHttpClient();

        // API services
        builder.Services.AddSingleton<IItemService, ItemService>();
        builder.Services.AddSingleton<IRentalService, RentalService>();
        builder.Services.AddSingleton<IRatingService, RatingService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();

        // Marketplace ViewModels and Pages
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<NearbyItemsViewModel>();
        builder.Services.AddTransient<NearbyItemsPage>();
        builder.Services.AddTransient<CreateItemViewModel>();
        builder.Services.AddTransient<CreateItemPage>();
        builder.Services.AddTransient<MyItemsViewModel>();
        builder.Services.AddTransient<MyItemsPage>();
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}