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

                // Đồng bộ dữ liệu quán ăn từ admin server — cập nhật mọi thay đổi admin đã chỉnh sửa.
                // Fire-and-forget với try/catch: nếu server tắt thì app vẫn dùng dữ liệu cục bộ bình thường.
                try
                {
                    var serverPois = await _syncService.FetchPOIsFromServerAsync();
                    if (serverPois != null && serverPois.Count > 0)
                    {
                        await _dbService.SyncPOIsFromServerAsync(serverPois);
                        System.Diagnostics.Debug.WriteLine($"✓ Synced {serverPois.Count} POIs from admin server");
                    }
                }
                catch (Exception syncEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ POI sync skipped (server offline): {syncEx.Message}");
                }

                // Tải cấu hình thanh toán từ admin (mô hình, lượt miễn phí, gói cước).
                // Lưu vào Preferences để ScanQuotaService và PaymentViewModel đọc ngay khi cần.
                try
                {
                    var config = await _syncService.FetchAppConfigAsync();
                    if (config != null)
                    {
                        Microsoft.Maui.Storage.Preferences.Default.Set("daily_free_listens", config.DailyFreeListens);
                        Microsoft.Maui.Storage.Preferences.Default.Set("payment_model", config.PaymentModel);
                        System.Diagnostics.Debug.WriteLine($"✓ App config: model={config.PaymentModel}, freeListens={config.DailyFreeListens}");
                    }
                }
                catch (Exception cfgEx)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ App config fetch skipped (server offline): {cfgEx.Message}");
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
    // Deep link format: vinhkhanhtour://poi/{id}/play-audio
    protected override void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        System.Diagnostics.Debug.WriteLine($"🔗 Deep link received: {uri}");
        System.Diagnostics.Debug.WriteLine($"  - Scheme: {uri.Scheme}");
        System.Diagnostics.Debug.WriteLine($"  - Host: {uri.Host}");
        System.Diagnostics.Debug.WriteLine($"  - PathAndQuery: {uri.PathAndQuery}");
        System.Diagnostics.Debug.WriteLine($"  - AbsolutePath: {uri.AbsolutePath}");

        if (uri.Scheme != "vinhkhanhtour")
        {
            System.Diagnostics.Debug.WriteLine($"❌ Invalid scheme: {uri.Scheme}");
            return;
        }

        // Extract POI ID from path: vinhkhanhtour://poi/123/play-audio
        string path = uri.Host + uri.PathAndQuery;
        System.Diagnostics.Debug.WriteLine($"  - Parsed path: {path}");

        if (!path.Contains("poi/") || !path.Contains("play-audio"))
        {
            System.Diagnostics.Debug.WriteLine($"❌ Invalid path format: {path}");
            return;
        }

        if (!_isInitialized)
        {
            System.Diagnostics.Debug.WriteLine($"⏱️ App not initialized yet, saving deep link for later");
            // DB chưa seed xong — lưu lại, xử lý sau khi init hoàn tất
            _pendingDeepLink = uri;
            return;
        }

        System.Diagnostics.Debug.WriteLine($"✓ Deep link valid, navigating...");
        MainThread.BeginInvokeOnMainThread(async () => await NavigateToDeepLinkAsync(uri));
    }

    private async Task NavigateToDeepLinkAsync(Uri uri)
    {
        try
        {
            // Extract POI ID from path: vinhkhanhtour://poi/123/play-audio
            string path = uri.Host + uri.PathAndQuery; // e.g., "poi/123/play-audio"

            System.Diagnostics.Debug.WriteLine($"📍 Parsing deep link path: {path}");

            // Parse POI ID
            var pathSegments = path.Split('/');
            System.Diagnostics.Debug.WriteLine($"  Path segments: [{string.Join(", ", pathSegments)}]");

            if (pathSegments.Length < 2 || !int.TryParse(pathSegments[1], out int poiId))
            {
                System.Diagnostics.Debug.WriteLine($"❌ Invalid deep link format: {uri}");
                await Application.Current?.MainPage?.DisplayAlert("❌ Lỗi", "Đường dẫn không hợp lệ", "OK")!;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✓ Parsed POI ID: {poiId}");

            // ⚡ FORCE SYNC POIs từ server trước khi tìm POI
            // Đảm bảo nếu admin vừa thêm quán mới thì app sẽ có luôn
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Force syncing POIs from server before deep link navigation...");
                var serverPois = await _syncService.FetchPOIsFromServerAsync();
                if (serverPois != null && serverPois.Count > 0)
                {
                    await _dbService.SyncPOIsFromServerAsync(serverPois);
                    System.Diagnostics.Debug.WriteLine($"✓ Force sync complete: {serverPois.Count} POIs from server");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Server returned no POIs or is offline, using local data");
                }
            }
            catch (Exception syncEx)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Force sync failed (server offline?): {syncEx.Message}");
            }

            // Lấy POI từ database
            var poi = await _dbService.GetPOIByIdAsync(poiId);
            if (poi == null)
            {
                System.Diagnostics.Debug.WriteLine($"❌ POI not found in database: {poiId}");
                System.Diagnostics.Debug.WriteLine($"   Checking all POIs in database...");

                var allPois = await _dbService.GetPOIsAsync();
                System.Diagnostics.Debug.WriteLine($"   Total POIs in database: {allPois.Count}");
                if (allPois.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"   Available POI IDs: {string.Join(", ", allPois.Select(p => p.Id))}");
                }

                await Application.Current?.MainPage?.DisplayAlert(
                    "❌ Lỗi", 
                    $"Không tìm thấy quán ăn (ID: {poiId})\n\nCó thể quán này chưa được đồng bộ từ server.", 
                    "OK")!;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✓ POI found: {poi.Name} (ID: {poi.Id})");

            // 📍 Navigate to MapTab TRƯỚC để MapPage listener active
            await Shell.Current.GoToAsync("//MapTab");
            System.Diagnostics.Debug.WriteLine($"✓ Navigated to MapTab");

            // ⏱️ Delay 500ms để MapPage.OnAppearing() chạy xong
            await Task.Delay(500);

            // 📱 Gửi tín hiệu để MapPage tự động phát audio của POI này
            WeakReferenceMessenger.Default.Send(new PlayAudioMessage(poi.Name, poi.Id));
            System.Diagnostics.Debug.WriteLine($"✓ Sent PlayAudioMessage for POI: {poi.Name} (ID: {poi.Id})");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error handling deep link: {ex}");
            await Application.Current?.MainPage?.DisplayAlert("❌ Lỗi", $"Không thể mở deep link:\n{ex.Message}", "OK")!;
        }
    }
}