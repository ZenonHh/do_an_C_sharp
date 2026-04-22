using DoAnCSharp.Views;
using DoAnCSharp.Services;
using CommunityToolkit.Mvvm.Messaging;
using DoAnCSharp.Models;

namespace DoAnCSharp;

using System; // For Uri

public partial class App : Application
{
    private readonly ILanguageService _langService;
    private readonly DatabaseService _dbService;
    private readonly AdminSyncService _syncService;
    private bool _isInitialized = false;
    // Deep link that arrived before init finished — replayed after init completes
    private Uri? _pendingDeepLink;

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
        Task.Run(async () =>
        {
            try
            {
                if (_isInitialized) return;

                System.Diagnostics.Debug.WriteLine("=== Starting App Initialization ===");

                try
                {
                    await _langService.Initialize();
                    System.Diagnostics.Debug.WriteLine("✓ Language Service initialized");
                }
                catch (Exception langEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Language Service Error: {langEx}");
                }

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

                string userId = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "guest");
                _syncService.StartHeartbeat(userId);

                // Replay any deep link that arrived before init finished
                if (_pendingDeepLink != null)
                {
                    var uri = _pendingDeepLink;
                    _pendingDeepLink = null;
                    MainThread.BeginInvokeOnMainThread(async () => await NavigateToDeepLinkAsync(uri));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in App initialization: {ex}");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Lỗi Khởi Tạo",
                                $"Không thể tải dữ liệu: {ex.Message}", "OK");
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
    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        if (uri.Scheme != "vinhkhanhtour" || uri.Host != "play_audio") return;

        if (!_isInitialized)
        {
            // DB chưa seed xong — lưu lại, xử lý sau khi init hoàn tất
            _pendingDeepLink = uri;
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () => await NavigateToDeepLinkAsync(uri));
    }

    private async Task NavigateToDeepLinkAsync(Uri uri)
    {
        string? poiName = GetQueryParamFromUri(uri, "poi_name");
        if (string.IsNullOrEmpty(poiName)) return;

        // Truyền thẳng dữ liệu qua tham số của Shell thay vì dùng Messenger (tránh lỗi timing)
        var navigationParameter = new Dictionary<string, object>
        {
            { "poi_name", poiName }
        };
        await Shell.Current.GoToAsync("//MapTab", navigationParameter);
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