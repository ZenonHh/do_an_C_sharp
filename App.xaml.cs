using DoAnCSharp.Views;
using DoAnCSharp.Services;

namespace DoAnCSharp;

using System; // For Uri

public partial class App : Application
{
    private readonly ILanguageService _langService;
    private readonly DatabaseService _dbService;
    private readonly AdminSyncService _syncService;
    private bool _isInitialized = false;

    public App(AppShell appShell, ILanguageService langService, DatabaseService dbService, AdminSyncService syncService)
    {
        InitializeComponent();

        _langService = langService;
        _dbService = dbService;
        _syncService = syncService;

        MainPage = appShell;

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

                // Bắt đầu gửi heartbeat để server biết thiết bị này đang online
                string userId = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "guest");
                _syncService.StartHeartbeat(userId);
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

    // Bắt sự kiện vòng đời của ứng dụng (Khi thu nhỏ, mở lại hoặc tắt hẳn app)
    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Resumed += (s, e) =>
        {
            // Khi người dùng mở lại app từ background, tiếp tục báo Online
            string userId = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "guest");
            _syncService.StartHeartbeat(userId);
            System.Diagnostics.Debug.WriteLine("App Resumed - Heartbeat Started");
        };

        window.Deactivated += (s, e) =>
        {
            // Khi ẩn app xuống nền hoặc thoát app, dừng báo Online
            // Server sẽ tự động cho offline sau khoảng 30s do không nhận được heartbeat
            _syncService.StopHeartbeat(); 
            
            System.Diagnostics.Debug.WriteLine("App Deactivated/Closed - Heartbeat Stopped");
        };

        return window;
    }

    // Xử lý Deep Link khi ứng dụng được mở từ bên ngoài
    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        // Kiểm tra scheme và host của deep link
        if (uri.Scheme == "vinhkhanhtour" && uri.Host == "play_audio")
        {
            string? poiName = null; // Sử dụng string? để xử lý nullability
            if (!string.IsNullOrEmpty(uri.Query))
                poiName = GetQueryParamFromUri(uri, "poi_name");
            
            if (!string.IsNullOrEmpty(poiName))
            {
                // Điều hướng đến MapTab và truyền poi_name như một query attribute
                // MapPage (đã implement IQueryAttributable) sẽ xử lý tham số này.
                await Shell.Current.GoToAsync($"//MapTab?poi_name={Uri.EscapeDataString(poiName)}");
            }
        }
    }

    // Helper method to parse query parameters from a Uri object
    private string? GetQueryParamFromUri(Uri uri, string paramName) // Kiểu trả về là string?
    {
        if (uri == null || string.IsNullOrEmpty(uri.Query))
        {
            return null;
        }

        string query = uri.Query.TrimStart('?');
        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=');
            if (parts.Length == 2 && Uri.UnescapeDataString(parts[0]) == paramName)
                return Uri.UnescapeDataString(parts[1]);
        }
        return null;
    }
}