using System.Net.Http.Json;
using DoAnCSharp.Models;

namespace DoAnCSharp.Services;

public record AppConfig(string PaymentModel, int DailyFreeListens);

public class AdminSyncService
{
    // 🔧 ĐỔI URL NÀY sau khi deploy lên Railway/ngrok
    // Local (emulator):   http://10.0.2.2:5000
    // Local (điện thoại): http://192.168.x.x:5000  (cùng WiFi)
    // ngrok:              https://abc123.ngrok-free.app
    // Railway (vĩnh viễn): https://ten-app.railway.app
    // LƯU Ý: IP hiện tại được lấy động từ Preferences, giúp bạn đổi IP từ cài đặt App mà không cần build lại
    public string ServerUrl
    {
        get => Microsoft.Maui.Storage.Preferences.Default.Get("ServerIP", "http://10.0.2.2:5000");
        set => Microsoft.Maui.Storage.Preferences.Default.Set("ServerIP", value);
    }

    private static readonly HttpClient _http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private System.Threading.Timer? _heartbeatTimer;
    private string _userId = "guest";
    private readonly string _deviceId;

    public AdminSyncService()
    {
        _deviceId = GetOrCreateDeviceId();
    }

    // Gọi sau khi user nghe audio — fire and forget, không crash app nếu server tắt
    public async Task SyncPlayHistoryAsync(string userId, string poiName, DateTime playedAt)
    {
        try
        {
            var payload = new
            {
                UserId = userId,
                PoiName = poiName,
                PlayedAt = playedAt.ToString("o"),
                DeviceId = _deviceId
            };
            await _http.PostAsJsonAsync($"{ServerUrl}/api/sync/history", payload);
        }
        catch (Exception ex)
        {
            // Bỏ qua lỗi mạng — app vẫn hoạt động bình thường khi server tắt
            System.Diagnostics.Debug.WriteLine($"[SyncPlayHistory] Lỗi mạng: {ex.Message}");
        }
    }

    // Gọi khi người dùng mua gói thành công trên App
    public async Task SyncPaymentToServerAsync(string email, string fullName, string packageName, decimal amount)
    {
        try
        {
            var payload = new
            {
                Email = email,
                FullName = fullName,
                PackageName = packageName,
                Amount = amount
            };
            
            var response = await _http.PostAsJsonAsync($"{ServerUrl}/api/payments/sync-from-app", payload);
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[Payment Sync] Success");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Payment Sync] Error: {ex.Message}");
        }
    }

    // Bắt đầu gửi heartbeat mỗi 30 giây để server biết user đang online
    public void StartHeartbeat(string userId)
    {
        _userId = userId;
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = new System.Threading.Timer(
            async _ => await SendHeartbeatAsync(),
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(30)
        );
    }

    public void StopHeartbeat()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }

    private async Task SendHeartbeatAsync()
    {
        try
        {
            var payload = new 
            { 
                DeviceId = _deviceId, 
                UserId = _userId,
                DeviceName = Microsoft.Maui.Devices.DeviceInfo.Current.Name,
                DeviceModel = Microsoft.Maui.Devices.DeviceInfo.Current.Model,
                DeviceOS = Microsoft.Maui.Devices.DeviceInfo.Current.Platform.ToString()
            };
            var response = await _http.PostAsJsonAsync($"{ServerUrl}/api/devices/heartbeat", payload);
            
            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[Heartbeat] Success -> {ServerUrl}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Heartbeat] Failed: {response.StatusCode}");
            }
        }
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine($"[Heartbeat] Error connecting to {ServerUrl}: {ex.Message}");
        }
    }

    // Tải cấu hình thanh toán từ admin server (mô hình, lượt nghe miễn phí, gói cước)
    // Admin có thể chỉnh sửa các giá trị này trên web mà không cần build lại app
    public async Task<AppConfig?> FetchAppConfigAsync()
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = await _http.GetFromJsonAsync<AppConfig>($"{ServerUrl}/api/settings/app-config", options);
            System.Diagnostics.Debug.WriteLine($"[AppConfig] model={config?.PaymentModel}, freeListens={config?.DailyFreeListens}");
            return config;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppConfig] Offline or error: {ex.Message}");
            return null;
        }
    }

    // Tải danh sách quán ăn từ admin server để đồng bộ về máy
    // Trả về null nếu server tắt hoặc lỗi mạng — app vẫn dùng dữ liệu cục bộ
    public async Task<List<AudioPOI>?> FetchPOIsFromServerAsync()
    {
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pois = await _http.GetFromJsonAsync<List<AudioPOI>>($"{ServerUrl}/api/pois", options);
            System.Diagnostics.Debug.WriteLine($"[FetchPOIs] Downloaded {pois?.Count ?? 0} POIs from server");
            return pois;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[FetchPOIs] Offline or error: {ex.Message}");
            return null;
        }
    }

    // Tạo ID duy nhất cho thiết bị, lưu vào Preferences để giữ nguyên qua các lần mở app
    private static string GetOrCreateDeviceId()
    {
        var id = Microsoft.Maui.Storage.Preferences.Default.Get("DeviceId", "");
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            Microsoft.Maui.Storage.Preferences.Default.Set("DeviceId", id);
        }
        return id;
    }
}
