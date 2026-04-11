using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using DoAnCSharp.Services;
using DoAnCSharp.Views;
using DoAnCSharp.ViewModels;
using SkiaSharp.Views.Maui.Controls.Hosting;
using ZXing.Net.Maui.Controls;

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
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        try
        {
            // 1. ĐĂNG KÝ DỊCH VỤ (Singleton cho các bộ não dùng chung)
            System.Diagnostics.Debug.WriteLine("Registering Services...");
            
            builder.Services.AddSingleton<ILanguageService, LanguageService>();
            System.Diagnostics.Debug.WriteLine("✓ LanguageService registered");
            
            builder.Services.AddSingleton<DatabaseService>();
            System.Diagnostics.Debug.WriteLine("✓ DatabaseService registered");
            
            builder.Services.AddSingleton<LocationService>(); 
            System.Diagnostics.Debug.WriteLine("✓ LocationService registered");
            
            builder.Services.AddSingleton<IAuthService, AuthService>();
            System.Diagnostics.Debug.WriteLine("✓ AuthService registered");

            // 2. ĐĂNG KÝ VIEWMODELS
            builder.Services.AddTransient<AuthViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ AuthViewModel registered");
            
            builder.Services.AddTransient<RegisterViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ RegisterViewModel registered");
            
            builder.Services.AddSingleton<HomeViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ HomeViewModel registered");
            
            builder.Services.AddSingleton<MapViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ MapViewModel registered");
            
            builder.Services.AddTransient<ProfileViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ ProfileViewModel registered");
            
            builder.Services.AddTransient<HistoryViewModel>();
            System.Diagnostics.Debug.WriteLine("✓ HistoryViewModel registered");

            // 3. ĐĂNG KÝ CÁC TRANG 
            builder.Services.AddTransient<AuthPage>();
            System.Diagnostics.Debug.WriteLine("✓ AuthPage registered");
            
            builder.Services.AddTransient<RegisterPage>();
            System.Diagnostics.Debug.WriteLine("✓ RegisterPage registered");
            
            builder.Services.AddSingleton<HomePage>();
            System.Diagnostics.Debug.WriteLine("✓ HomePage registered");
            
            builder.Services.AddSingleton<MapPage>();
            System.Diagnostics.Debug.WriteLine("✓ MapPage registered");
            
            builder.Services.AddTransient<ProfilePage>(); 
            System.Diagnostics.Debug.WriteLine("✓ ProfilePage registered");
            
            builder.Services.AddTransient<ScanQRPage>();
            System.Diagnostics.Debug.WriteLine("✓ ScanQRPage registered");
            
            builder.Services.AddTransient<HistoryPage>();
            System.Diagnostics.Debug.WriteLine("✓ HistoryPage registered");
            
            // AppShell nên là Singleton để giữ trạng thái Tab
            builder.Services.AddSingleton<AppShell>();
            System.Diagnostics.Debug.WriteLine("✓ AppShell registered");
            
            builder.Services.AddTransient<EditProfilePage>();
            System.Diagnostics.Debug.WriteLine("✓ EditProfilePage registered");
            
            builder.Services.AddTransient<ForgotPasswordPage>();
            System.Diagnostics.Debug.WriteLine("✓ ForgotPasswordPage registered");

            System.Diagnostics.Debug.WriteLine("=== All services registered successfully ===");
            
            return builder.Build();
        }
        catch (Exception ex)
        {
            // Ghi lỗi chi tiết ra Terminal
            System.Diagnostics.Debug.WriteLine("CRITICAL ERROR in MauiProgram: " + ex.Message);
            System.Diagnostics.Debug.WriteLine("FULL EXCEPTION: " + ex.ToString());
            System.Console.WriteLine("CRITICAL ERROR: " + ex.ToString());
            throw; 
        }
    }
}