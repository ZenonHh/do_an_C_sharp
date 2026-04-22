using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;
using System;
using System.Threading.Tasks;

namespace DoAnCSharp.AdminWeb.Controllers;

/// <summary>
/// Quản lý QR code scanning - endpoint cho web public
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QRScansController : ControllerBase
{
    private readonly DatabaseService _db;
    private readonly ILogger<QRScansController> _logger;

    public QRScansController(DatabaseService db, ILogger<QRScansController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Xác minh mã QR, kiểm tra giới hạn quét của thiết bị và trả về thông tin quán ăn.
    /// </summary>
    /// <param name="qrCode">Mã QR được quét.</param>
    /// <param name="deviceId">ID duy nhất của thiết bị.</param>
    /// <returns>Thông tin quán ăn nếu hợp lệ, hoặc lỗi nếu vượt quá giới hạn.</returns>
    [HttpGet("verify")]
    public async Task<IActionResult> VerifyScan([FromQuery] string qrCode, [FromQuery] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(qrCode) || string.IsNullOrWhiteSpace(deviceId))
        {
            return BadRequest(new { error = "Yêu cầu phải có mã QR (qrCode) và ID thiết bị (deviceId)." });
        }

        try
        {
            // LƯU Ý: Bạn cần triển khai các phương thức này trong DatabaseService của WebAdmin:
            // - Task<DeviceScanLimit> GetDeviceScanLimitAsync(string deviceId)
            // - Task SaveDeviceScanLimitAsync(DeviceScanLimit limit) // (Hàm này sẽ tự động Insert hoặc Update)
            // - Task<AudioPOI> GetPOIByQRCodeAsync(string qrCode)

            var scanLimit = await _db.GetDeviceScanLimitAsync(deviceId) ?? new DeviceScanLimit
            {
                DeviceId = deviceId,
                LastResetDate = DateTime.UtcNow.Date
            };

            // Tự động reset lượt quét nếu đã sang ngày mới (tính theo giờ UTC)
            if (scanLimit.LastResetDate < DateTime.UtcNow.Date)
            {
                scanLimit.ScanCount = 0;
                scanLimit.LastResetDate = DateTime.UtcNow.Date;
            }

            // Kiểm tra nếu thiết bị đã hết lượt quét trong ngày
            if (scanLimit.ScanCount >= scanLimit.MaxScans)
            {
                // Trả về mã lỗi 429 Too Many Requests để frontend xử lý
                return StatusCode(429, new
                {
                    limitExceeded = true,
                    message = "Bạn đã hết lượt quét trong ngày. Vui lòng tải ứng dụng và đăng ký gói để sử dụng không giới hạn."
                });
            }

            // Tìm quán ăn tương ứng với mã QR
            var poi = await _db.GetPOIByQRCodeAsync(qrCode);
            if (poi == null)
            {
                return NotFound(new { error = "Mã QR không hợp lệ hoặc không tìm thấy địa điểm." });
            }

            // Nếu hợp lệ, tăng số lượt quét và lưu lại
            scanLimit.ScanCount++;
            await _db.SaveDeviceScanLimitAsync(scanLimit);

            // Trả về dữ liệu quán ăn và số lượt quét còn lại
            return Ok(new
            {
                limitExceeded = false,
                poi,
                scansRemaining = scanLimit.MaxScans - scanLimit.ScanCount,
                message = "Xác minh thành công."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xác minh mã QR cho thiết bị {DeviceId}", deviceId);
            return StatusCode(500, new { error = "Lỗi hệ thống, vui lòng thử lại sau." });
        }
    }

    /// <summary>
    /// Redirect khi quét QR từ camera điện thoại - endpoint không cần /api/
    /// </summary>
    [HttpGet("scan")]
    public async Task<IActionResult> ScanQR([FromQuery] string qrCode, [FromQuery] string deviceId = "mobile-camera")
    {
        if (string.IsNullOrWhiteSpace(qrCode))
        {
            return Redirect("/poi-public.html?error=invalid");
        }

        try
        {
            // Xác minh mã QR
            var scanLimit = await _db.GetDeviceScanLimitAsync(deviceId) ?? new DeviceScanLimit
            {
                DeviceId = deviceId,
                LastResetDate = DateTime.UtcNow.Date
            };

            if (scanLimit.LastResetDate < DateTime.UtcNow.Date)
            {
                scanLimit.ScanCount = 0;
                scanLimit.LastResetDate = DateTime.UtcNow.Date;
            }

            // Kiểm tra giới hạn
            if (scanLimit.ScanCount >= scanLimit.MaxScans)
            {
                return Redirect($"/poi-public.html?error=limit_exceeded");
            }

            // Tìm POI
            var poi = await _db.GetPOIByQRCodeAsync(qrCode);
            if (poi == null)
            {
                return Redirect($"/poi-public.html?error=poi_not_found");
            }

            // Tăng lượt quét
            scanLimit.ScanCount++;
            await _db.SaveDeviceScanLimitAsync(scanLimit);

            // Redirect tới trang hiển thị
            return Redirect($"/poi-public.html?poiId={poi.Id}&deviceId={Uri.EscapeDataString(deviceId)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi quét QR từ camera");
            return Redirect($"/poi-public.html?error=system_error");
        }
    }

    /// <summary>
    /// Route đặc biệt: /qr/{code} để handle camera quét trực tiếp
    /// VD: điện thoại quét QR → 192.168.1.100:5000/qr/POI_UA8AG0H2D
    /// </summary>
    [HttpGet]
    [Route("/qr/{code}")]
    public async Task<IActionResult> QuickScanQR(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Redirect("/poi-public.html?error=invalid");
        }

        // Lấy device ID từ request
        var deviceId = Request.Query["deviceId"].ToString();
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            // Tạo device ID từ IP address
            deviceId = $"device_{Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
        }

        try
        {
            // 🔥 NEW: Capture & Track Device Info
            await TrackDeviceInfoAsync(deviceId);

            // Xác minh mã QR
            var scanLimit = await _db.GetDeviceScanLimitAsync(deviceId) ?? new DeviceScanLimit
            {
                DeviceId = deviceId,
                LastResetDate = DateTime.UtcNow.Date
            };

            if (scanLimit.LastResetDate < DateTime.UtcNow.Date)
            {
                scanLimit.ScanCount = 0;
                scanLimit.LastResetDate = DateTime.UtcNow.Date;
            }

            // Kiểm tra giới hạn
            if (scanLimit.ScanCount >= scanLimit.MaxScans)
            {
                return Redirect($"/poi-public.html?error=limit_exceeded&code={code}");
            }

            // Tìm POI bằng cách search trực tiếp với full code (database stores with POI_ prefix)
            AudioPOI poi = null;
            poi = await _db.GetPOIByQRCodeAsync(code);

            if (poi == null)
            {
                _logger.LogWarning("POI không tìm thấy cho code: {QRCode}", code);
                return Redirect($"/poi-public.html?error=poi_not_found&code={Uri.EscapeDataString(code)}");
            }

            // Tăng lượt quét
            scanLimit.ScanCount++;
            await _db.SaveDeviceScanLimitAsync(scanLimit);

            _logger.LogInformation("Quét QR thành công: {QRCode} → POI {POIId} từ device {DeviceId}", code, poi.Id, deviceId);

            // Redirect tới trang hiển thị
            return Redirect($"/poi-public.html?poiId={poi.Id}&deviceId={Uri.EscapeDataString(deviceId)}&code={Uri.EscapeDataString(code)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi quét QR nhanh: {QRCode}", code);
            return Redirect($"/poi-public.html?error=system_error&code={Uri.EscapeDataString(code)}");
        }
    }

    /// <summary>
    /// 🔥 NEW: Track device info when QR is scanned
    /// Captures device information and updates online status
    /// </summary>
    private async Task TrackDeviceInfoAsync(string deviceId)
    {
        try
        {
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Extract device info from User-Agent
            var (deviceName, deviceModel, deviceOS) = ExtractDeviceInfo(userAgent);

            // Check if device already exists
            var existingDevice = await _db.GetUserDevicesAsync(1).ConfigureAwait(false); // Default user ID 1
            var device = existingDevice?.FirstOrDefault(d => d.DeviceId == deviceId);

            if (device == null)
            {
                // Create new device
                device = new UserDevice
                {
                    UserId = 1, // Default user for anonymous scans
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    DeviceModel = deviceModel,
                    DeviceOS = deviceOS,
                    AppVersion = "1.0.0",
                    IsOnline = true,
                    LastOnlineAt = DateTime.Now,
                    RegisteredAt = DateTime.Now,
                    IpAddress = ipAddress,
                    LocationInfo = GetLocationFromIP(ipAddress),
                    IsActive = true
                };
                await _db.InsertUserDeviceAsync(device).ConfigureAwait(false);
                _logger.LogInformation("✅ Device registered: {DeviceId} ({DeviceName})", deviceId, deviceName);
            }
            else
            {
                // Update existing device
                device.IsOnline = true;
                device.LastOnlineAt = DateTime.Now;
                device.IpAddress = ipAddress;
                await _db.UpdateUserDeviceAsync(device).ConfigureAwait(false);
                _logger.LogInformation("✅ Device updated online: {DeviceId}", deviceId);
            }

            // Update user status if linked
            await _db.SetUserOnlineAsync(1, true, $"{deviceName} ({deviceOS})", ipAddress).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Warning tracking device: {DeviceId}", deviceId);
            // Don't throw - device tracking should not block QR scan
        }
    }

    /// <summary>
    /// Extract device information from User-Agent header
    /// </summary>
    private (string deviceName, string deviceModel, string deviceOS) ExtractDeviceInfo(string userAgent)
    {
        string deviceName = "Unknown Device";
        string deviceModel = "Unknown";
        string deviceOS = "Unknown";

        if (string.IsNullOrEmpty(userAgent))
            return (deviceName, deviceModel, deviceOS);

        // Detect OS
        if (userAgent.Contains("Windows"))
        {
            deviceOS = "Windows";
            deviceName = "Windows PC";
        }
        else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
        {
            deviceOS = "iOS";
            deviceName = userAgent.Contains("iPhone") ? "iPhone" : "iPad";
            // Extract model
            if (userAgent.Contains("iPhone 14")) deviceModel = "iPhone 14";
            else if (userAgent.Contains("iPhone 13")) deviceModel = "iPhone 13";
            else if (userAgent.Contains("iPhone 12")) deviceModel = "iPhone 12";
            else if (userAgent.Contains("iPhone 11")) deviceModel = "iPhone 11";
            else deviceModel = "iPhone";
        }
        else if (userAgent.Contains("Android"))
        {
            deviceOS = "Android";
            deviceName = "Android Device";
            // Try to extract model
            var match = System.Text.RegularExpressions.Regex.Match(userAgent, @";\s+([^;]+)\s+Build");
            if (match.Success)
                deviceModel = match.Groups[1].Value;
            else
                deviceModel = "Android Phone";
        }
        else if (userAgent.Contains("Mac"))
        {
            deviceOS = "macOS";
            deviceName = "Mac";
        }
        else if (userAgent.Contains("Linux"))
        {
            deviceOS = "Linux";
            deviceName = "Linux Device";
        }

        return (deviceName, deviceModel, deviceOS);
    }

    /// <summary>
    /// Get location info from IP (simplified version)
    /// In production, use a proper GeoIP service
    /// </summary>
    private string GetLocationFromIP(string ipAddress)
    {
        // Simplified: just return IP for now
        // In production, call a GeoIP API
        return ipAddress switch
        {
            "127.0.0.1" => "Local",
            "::1" => "Local",
            _ => ipAddress
        };
    }

    /// <summary>
    /// Lấy danh sách giới hạn QR scan cho tất cả thiết bị (cho admin dashboard)
    /// </summary>
    [HttpGet("limits")]
    public async Task<ActionResult> GetQRLimits()
    {
        try
        {
            var limits = await _db.GetAllDeviceScanLimitsAsync();
            return Ok(limits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách QR limits");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 🔥 NEW: Get all online devices for dashboard
    /// </summary>
    [HttpGet("online-devices")]
    public async Task<ActionResult> GetOnlineDevices()
    {
        try
        {
            var onlineDevices = await _db.GetOnlineUsersAsync();
            return Ok(new
            {
                totalOnlineDevices = onlineDevices.Count,
                devices = onlineDevices.OrderByDescending(d => d.LastOnlineAt).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách devices online");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 🔥 NEW: Get dashboard statistics including online devices
    /// </summary>
    [HttpGet("dashboard-stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        try
        {
            var summary = await _db.GetDashboardSummaryAsync();
            var onlineDevices = await _db.GetOnlineUsersAsync();
            var qrActivity = await _db.GetQRActivityTodayAsync();

            return Ok(new
            {
                summary.TotalOnlineUsers,
                summary.TotalRegisteredUsers,
                summary.TotalPaidUsers,
                OnlineDevices = onlineDevices.Count,
                TodayQRScans = summary.TodayQRScans,
                QRActivity = new
                {
                    TotalScans = qrActivity.totalScans,
                    UniqueUsers = qrActivity.uniqueUsers,
                    TopPOIs = qrActivity.topPOIs
                },
                OnlineDevicesList = onlineDevices.OrderByDescending(d => d.LastOnlineAt).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy dashboard stats");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 🔥 NEW: Update device online status (heartbeat from client)
    /// Clients can send periodic heartbeat to keep device marked as online
    /// </summary>
    [HttpPost("device-heartbeat")]
    public async Task<ActionResult> DeviceHeartbeat([FromQuery] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { error = "deviceId is required" });

        try
        {
            var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Get all devices for default user (ID 1)
            var devices = await _db.GetUserDevicesAsync(1);
            var device = devices?.FirstOrDefault(d => d.DeviceId == deviceId);

            if (device != null)
            {
                device.IsOnline = true;
                device.LastOnlineAt = DateTime.Now;
                device.IpAddress = ipAddress;
                await _db.UpdateUserDeviceAsync(device);
                _logger.LogInformation("Device heartbeat: {DeviceId}", deviceId);
            }

            return Ok(new { message = "Device heartbeat recorded", timestamp = DateTime.Now });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi nhận device heartbeat: {DeviceId}", deviceId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 🔥 NEW: Mark device as offline when user leaves
    /// </summary>
    [HttpPost("device-offline")]
    public async Task<ActionResult> DeviceOffline([FromQuery] string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return BadRequest(new { error = "deviceId is required" });

        try
        {
            var devices = await _db.GetUserDevicesAsync(1);
            var device = devices?.FirstOrDefault(d => d.DeviceId == deviceId);

            if (device != null)
            {
                device.IsOnline = false;
                device.LastOnlineAt = DateTime.Now;
                await _db.UpdateUserDeviceAsync(device);
                _logger.LogInformation("Device marked offline: {DeviceId}", deviceId);
            }

            return Ok(new { message = "Device marked offline" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đánh dấu device offline: {DeviceId}", deviceId);
            return BadRequest(new { error = ex.Message });
        }
    }
}
