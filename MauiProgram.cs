using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using DoAnCSharp.Services;
using DoAnCSharp.Views;
using DoAnCSharp.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace DoAnCSharp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // Dòng này cực kỳ quan trọng để chạy bản đồ OpenStreetMap
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. ĐĂNG KÝ CÁC DỊCH VỤ (Services)
        builder.Services.AddSingleton<ILanguageService, LanguageService>();
        builder.Services.AddSingleton<IPoiRepository, PoiRepository>();
        builder.Services.AddSingleton<LocationService>(); // Tên phải đúng với class của bạn

        // 2. ĐĂNG KÝ VIEWMODELS (Quan trọng: Giúp App không bị văng khi mở trang Bản đồ)
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<IGeofenceService, GeofenceService>();
        
        builder.Services.AddTransient<AuthViewModel>(); // Rất quan trọng
        builder.Services.AddSingleton<MapViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();
        // 3. ĐĂNG KÝ CÁC TRANG (Pages)
        builder.Services.AddTransient<Views.AuthPage>(); // Rất quan trọng để chạy Bước 1
        builder.Services.AddSingleton<Views.HomePage>();
        builder.Services.AddSingleton<Views.MapPage>();
        builder.Services.AddSingleton<AppShell>();

        //builder.Services.AddSingleton<HomePage>();
        //builder.Services.AddSingleton<MapPage>();
        //builder.Services.AddSingleton<ProfilePage>();
        //builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
#if DEBUG
        //builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}