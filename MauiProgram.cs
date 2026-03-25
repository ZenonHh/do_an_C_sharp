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
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 1. ĐĂNG KÝ DỊCH VỤ
        builder.Services.AddSingleton<ILanguageService, LanguageService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<LocationService>(); 
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // 2. ĐĂNG KÝ VIEWMODELS
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<MapViewModel>();

        // 3. ĐĂNG KÝ CÁC TRANG (Đã XÓA SẠCH chữ "Views." và thêm ProfilePage)
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<MapPage>();
        builder.Services.AddTransient<ProfilePage>(); 
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
builder.Services.AddTransient<Views.RegisterPage>();

#if DEBUG
        //builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}