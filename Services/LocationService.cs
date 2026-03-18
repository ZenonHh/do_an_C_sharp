namespace DoAnCSharp.Services;

public interface ILocationService
{
    Task<bool> HandleLocationPermission();
    Task StartListening(Action<Location> onLocationChanged);
    void StopListening();
}

public class LocationService : ILocationService
{
    private bool _isListening;
    private readonly GeolocationRequest _request;

    public LocationService()
    {
        // Thiết kế request lấy vị trí với độ chính xác trung bình để tiết kiệm PIN/RAM
        _request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
    }

    public async Task<bool> HandleLocationPermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }
        return status == PermissionStatus.Granted;
    }

    // Đây là hàm thay thế cho StartListeningAsync bị lỗi
    public async Task StartListening(Action<Location> onLocationChanged)
    {
        if (_isListening) return;
        _isListening = true;

        // Vòng lặp chạy ngầm để lấy vị trí liên tục (Polling)
        while (_isListening)
        {
            try
            {
                var location = await Geolocation.Default.GetLocationAsync(_request);
                if (location != null)
                {
                    // Trả tọa độ về cho ViewModel xử lý Geofencing
                    onLocationChanged?.Invoke(location);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi GPS: {ex.Message}");
            }

            // Nghỉ 3 giây trước khi lấy vị trí tiếp theo (Tối ưu cho máy 8GB)
            await Task.Delay(3000);
        }
    }

    public void StopListening()
    {
        _isListening = false;
    }
}
