using DoAnCSharp.Views;
using DoAnCSharp.Services;

namespace DoAnCSharp;

public partial class App : Application
{
    private readonly ILanguageService _langService;
    private readonly DatabaseService _dbService;
    private bool _isInitialized = false;

    public App(AuthPage authPage, ILanguageService langService, DatabaseService dbService)
    {
        InitializeComponent(); 
        
        _langService = langService;
        _dbService = dbService;

        // Trang khởi đầu là AuthPage (hiện lên ngay lập tức để không bị màn hình trắng)
        MainPage = new NavigationPage(authPage);
        
        // Khởi tạo các dịch vụ NGẦM ở Background Thread để không làm đơ App
        InitializeServicesInBackground();
    }

    private void InitializeServicesInBackground()
    {
        // Sử dụng Task.Run thay vì MainThread.BeginInvokeOnMainThread
        Task.Run(async () =>
        {
            try
            {
                if (_isInitialized) return;
                
                System.Diagnostics.Debug.WriteLine("=== Starting App Initialization ===");

                // 1. Khởi tạo ngôn ngữ 
                System.Diagnostics.Debug.WriteLine("Initializing Language Service...");
                try
                {
                    await _langService.Initialize();
                    System.Diagnostics.Debug.WriteLine("✓ Language Service initialized");
                }
                catch (Exception langEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Language Service Error: {langEx}");
                }

                // 2. Khởi tạo database (Chạy ngầm sẽ rất mượt, không giật UI)
                System.Diagnostics.Debug.WriteLine("Initializing Database...");
                try
                {
                    await _dbService.SeedDataAsync();
                    System.Diagnostics.Debug.WriteLine("✓ Database initialized with seed data");
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Database Init Error: {dbEx}");
                }

                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("=== App Initialization Complete ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in App initialization: {ex}");
                
                // Nếu có lỗi nặng cần báo cho người dùng, LÚC NÀY mới gọi MainThread
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Lỗi Khởi Tạo", 
                                $"Không thể tải dữ liệu: {ex.Message}", "OK");
                        }
                    }
                    catch { }
                });
            }
        });
    }
}