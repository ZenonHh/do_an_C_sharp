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
            // 🔥 Theo dõi và lưu thông tin thiết bị khi App gọi API Verify QR
            await TrackDeviceInfoAsync(deviceId);

            var (isAllowed, scanLimit, message) = await CheckScanLimitAsync(deviceId);
            if (!isAllowed)
            {
                // Trả về mã lỗi 429 Too Many Requests để frontend xử lý
                return StatusCode(429, new
                {
                    limitExceeded = true,
                    message
                });
            }

            // 🔥 FIX: App gửi lên nguyên chuỗi URL chứa mã QR, ta cần tách lấy đúng phần mã code (VD: POI_12345)
            string codeToSearch = SanitizeQRCode(qrCode);

            // Tìm quán ăn tương ứng với mã QR
            var poi = await _db.GetPOIByQRCodeAsync(codeToSearch);
            if (poi == null)
            {
                return NotFound(new { error = "Mã QR không hợp lệ hoặc không tìm thấy địa điểm." });
            }

            // Nếu hợp lệ, tăng số lượt quét và lưu lại
            scanLimit!.ScanCount++;
            await _db.SaveDeviceScanLimitAsync(scanLimit);

            // Trả về dữ liệu quán ăn và số lượt quét còn lại
            return Ok(new
            {
                limitExceeded = false,
                poi,
                scansRemaining = scanLimit.MaxScans - scanLimit.ScanCount, // Đã tăng ở trên
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
            var (isAllowed, scanLimit, _) = await CheckScanLimitAsync(deviceId);
            if (!isAllowed)
            {
                return Redirect($"/poi-public.html?error=limit_exceeded");
            }

            // 🔥 FIX: Xử lý chuỗi qrCode nếu nó chứa full URL
            string codeToSearch = SanitizeQRCode(qrCode);

            // Tìm POI
            var poi = await _db.GetPOIByQRCodeAsync(codeToSearch);
            if (poi == null)
            {
                return Redirect($"/poi-public.html?error=poi_not_found");
            }

            // Tăng lượt quét
            scanLimit!.ScanCount++;
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
    /// Cập nhật số lượt nghe trực tiếp trên trang danh sách mà không cần tải lại trang
    /// </summary>
    [HttpPost("track-listen")]
    public async Task<IActionResult> TrackListen([FromQuery] string deviceId, [FromQuery] int poiId = 0, [FromQuery] string poiName = "")
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return BadRequest();
        try
        {
            var scanLimit = await _db.GetDeviceScanLimitAsync(deviceId);

            // Device came in via FoodStreet QR (which returns early before persisting the limit),
            // so the record may not exist yet — create it on the first listen call.
            if (scanLimit == null)
            {
                scanLimit = new DeviceScanLimit
                {
                    DeviceId = deviceId,
                    ScanCount = 0,
                    MaxScans = 5,
                    LastResetDate = DateTime.Now.Date,
                    CreatedAt = DateTime.Now
                };
            }

            if (scanLimit.ScanCount < scanLimit.MaxScans)
            {
                scanLimit.ScanCount++;
                await _db.SaveDeviceScanLimitAsync(scanLimit);

                if (poiId > 0 || !string.IsNullOrWhiteSpace(poiName))
                {
                    await _db.InsertPlayHistoryAsync(new PlayHistory
                    {
                        UserId = 0,
                        POIId = poiId,
                        POIName = poiName,
                        PlayedAt = DateTime.Now,
                        Source = "web"
                    });
                }

                return Ok(new { success = true, scans = scanLimit.ScanCount });
            }
            return BadRequest(new { error = "Đã vượt quá giới hạn nghe" });
        }
        catch { return StatusCode(500); }
    }

    /// <summary>
    /// Backward-compat route: /POI_XXXX (QR codes stored without /qr/ prefix)
    /// </summary>
    [HttpGet]
    [Route("/{code:regex(^POI_\\w+$)}")]
    public Task<IActionResult> LegacyPOIRoute(string code) => QuickScanQR(code);

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
            // Lấy từ Cookie hoặc tạo mới (Tránh lỗi giới hạn nhầm lượt quét của người khác khi dùng chung Wifi/4G)
            if (Request.Cookies.TryGetValue("vkt_device_id", out var cookieDeviceId) && !string.IsNullOrEmpty(cookieDeviceId))
            {
                deviceId = cookieDeviceId;
            }
            else 
            {
                deviceId = $"device_web_{Guid.NewGuid():N}";
                Response.Cookies.Append("vkt_device_id", deviceId, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }
        }

        try
        {
            // 🔥 NEW: Capture & Track Device Info
            await TrackDeviceInfoAsync(deviceId);

            // Lấy hoặc tạo mới giới hạn nghe cho thiết bị
            var (isAllowed, scanLimit, _) = await CheckScanLimitAsync(deviceId);

            string codeToSearch = SanitizeQRCode(code);

            // Xử lý QR phố ẩm thực: redirect đến trang danh sách
            if (codeToSearch.StartsWith("FOODSTREET", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect($"/poi-public.html?deviceId={Uri.EscapeDataString(deviceId)}");
            }

            // Kiểm tra giới hạn (Chỉ áp dụng khi người dùng quét xem chi tiết một quán)
            // isAllowed đã được kiểm tra ở trên
            if (!isAllowed)
            {
                return Content(BuildLimitExceededHtml(), "text/html", System.Text.Encoding.UTF8);
            }

            // Tìm POI bằng cách search trực tiếp với full code (database stores with POI_ prefix)
            var poi = await _db.GetPOIByQRCodeAsync(codeToSearch);

            if (poi == null)
            {
                _logger.LogWarning("POI không tìm thấy cho code: {QRCode}", code);
                return Redirect($"/poi-public.html?error=poi_not_found&code={Uri.EscapeDataString(code)}");
            }

            // Kiểm tra xem người dùng có đi từ danh sách tổng (FoodStreet) vào không
            bool fromList = Request.Query["fromList"] == "1";
            string ttsText = System.Web.HttpUtility.JavaScriptStringEncode(poi.Description ?? poi.Name ?? "");

            // Tăng lượt quét và lưu lại
            scanLimit!.ScanCount++;
            await _db.SaveDeviceScanLimitAsync(scanLimit);

            // Record listen in play history (fire-and-forget, non-blocking)
            _ = _db.InsertPlayHistoryAsync(new PlayHistory
            {
                UserId = 0,
                POIId = poi.Id,
                POIName = poi.Name ?? "",
                PlayedAt = DateTime.Now,
                Source = "web"
            });

            _logger.LogInformation("Quét QR thành công: {QRCode} → POI {POIId} từ device {DeviceId}. Lượt quét: {ScanCount}/{MaxScans}", code, poi.Id, deviceId, scanLimit.ScanCount, scanLimit.MaxScans);

            // Redirect to POI detail page
            return Redirect($"/poi-detail.html?poiId={poi.Id}");
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
                    AppVersion = "web-scan", // Phân biệt với app
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
    /// Tách lấy mã code (VD: POI_12345) từ một chuỗi URL hoặc code đầy đủ.
    /// </summary>
    private static string SanitizeQRCode(string qrCode)
    {
        if (string.IsNullOrWhiteSpace(qrCode))
        {
            return string.Empty;
        }

        if (qrCode.Contains("/qr/"))
        {
            return qrCode.Substring(qrCode.LastIndexOf("/qr/") + 4);
        }
        
        // Xử lý trường hợp code không chứa URL nhưng có thể có các query params khác
        int queryIndex = qrCode.IndexOf('?');
        if (queryIndex != -1)
        {
            qrCode = qrCode.Substring(0, queryIndex);
        }

        return qrCode;
    }

    /// <summary>
    /// Kiểm tra giới hạn lượt quét của một thiết bị và reset nếu cần.
    /// </summary>
    /// <returns>Tuple chứa (bool isAllowed, DeviceScanLimit? limit, string message)</returns>
    private async Task<(bool isAllowed, DeviceScanLimit? limit, string message)> CheckScanLimitAsync(string deviceId)
    {
        var scanLimit = await _db.GetDeviceScanLimitAsync(deviceId) ?? new DeviceScanLimit
        {
            DeviceId = deviceId,
            ScanCount = 0,
            MaxScans = 5, // Giá trị mặc định cho thiết bị mới
            LastResetDate = DateTime.Now.Date,
            CreatedAt = DateTime.Now
        };

        if (scanLimit.LastResetDate < DateTime.Now.Date)
        {
            scanLimit.ScanCount = 0;
            scanLimit.LastResetDate = DateTime.Now.Date;
            await _db.SaveDeviceScanLimitAsync(scanLimit); // Lưu lại ngày reset mới
        }

        if (scanLimit.ScanCount >= scanLimit.MaxScans)
        {
            return (false, scanLimit, "Bạn đã hết lượt nghe trong ngày. Vui lòng tải ứng dụng và đăng ký gói để nghe không giới hạn.");
        }

        return (true, scanLimit, "OK");
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
            var devices = await _db.GetAllUserDevicesAsync();

            var formattedLimits = limits.Select(limit => 
            {
                var device = devices.FirstOrDefault(d => d.DeviceId == limit.DeviceId);
                int remaining = limit.MaxScans - limit.ScanCount;
                return new 
                {
                    id = limit.Id,
                    deviceId = limit.DeviceId,
                    deviceName = device?.DeviceName ?? "Khách vãng lai (Web)",
                    deviceOS = device?.DeviceOS ?? "N/A",
                    scanCount = limit.ScanCount,
                    maxScans = limit.MaxScans,
                    remainingScans = remaining < 0 ? 0 : remaining,
                    lastResetDate = limit.LastResetDate,
                    createdAt = limit.CreatedAt
                };
            }).OrderByDescending(x => x.lastResetDate).ToList();

            return Ok(formattedLimits);
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

            // Lọc: Chỉ lấy máy là Mobile/Tablet VÀ vừa gửi heartbeat trong vòng 35s qua
            var realOnlineDevices = onlineDevices.Where(d => 
                (DateTime.Now - d.LastOnlineAt).TotalSeconds <= 35 &&
                d.DeviceOS != "Windows" && d.DeviceOS != "macOS" && d.DeviceOS != "Linux").ToList();

            return Ok(new
            {
                totalOnlineDevices = realOnlineDevices.Count,
                devices = realOnlineDevices.OrderByDescending(d => d.LastOnlineAt).ToList()
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

            // Lọc: Chỉ đếm các máy là App thực sự đang mở
            var realOnlineDevices = onlineDevices.Where(d => 
                (DateTime.Now - d.LastOnlineAt).TotalSeconds <= 35 &&
                d.DeviceOS != "Windows" && d.DeviceOS != "macOS" && d.DeviceOS != "Linux").ToList();

            return Ok(new
            {
                TotalOnlineUsers = realOnlineDevices.Count, // Ép ghi đè số đếm chính xác
                summary.TotalRegisteredUsers,
                summary.TotalPaidUsers,
                OnlineDevices = realOnlineDevices.Count,
                TodayQRScans = 0, // Xóa thống kê lượt nghe hôm nay
                QRActivity = new
                {
                    TotalScans = 0, // Xóa tổng lượt quét
                    UniqueUsers = 0,
                    TopPOIs = new List<object>() // Ẩn danh sách quán ăn
                },
                OnlineDevicesList = realOnlineDevices.OrderByDescending(d => d.LastOnlineAt).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy dashboard stats");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Trang web hiển thị danh sách toàn bộ quán ăn khi khách quét QR phố ẩm thực bằng camera điện thoại.
    /// Khuyến khích người dùng tải app để xem toàn bộ danh sách.
    /// </summary>
    private static string BuildFoodStreetHtml(List<AudioPOI> pois, Dictionary<int, string> mainImages, string baseUrl, DeviceScanLimit scanLimit, string deviceId)
    {
        // Resolve image URL: prefer admin-uploaded image, fallback to static asset
        string ResolveImageUrl(AudioPOI p) =>
            mainImages.TryGetValue(p.Id, out var uploaded) && !string.IsNullOrEmpty(uploaded)
                ? uploaded
                : (string.IsNullOrWhiteSpace(p.ImageAsset) ? "" : $"/images/restaurants/{p.ImageAsset}");

        // Serialize all POI data as JSON — keeps Vietnamese text out of HTML onclick attributes
        var poisJson = System.Text.Json.JsonSerializer.Serialize(pois.Select(p => new
        {
            id = p.Id,
            name = p.Name ?? "",
            desc = p.Description ?? p.Name ?? "",
            audioUrl = p.AudioUrl ?? "",
            imageUrl = ResolveImageUrl(p),
            lat = p.Lat,
            lng = p.Lng
        }));

        var cards = new System.Text.StringBuilder();
        foreach (var poi in pois)
        {
            string category = poi.Priority switch
            {
                1 => "🐚 Ốc",
                2 => "🔥 Nướng & Lẩu",
                _ => "🍡 Ăn vặt"
            };
            string categoryColor = poi.Priority switch
            {
                1 => "#0ea5e9",
                2 => "#f97316",
                _ => "#a855f7"
            };

            string imageUrl = ResolveImageUrl(poi);
            string mapsUrl = (poi.Lat != 0 && poi.Lng != 0)
                ? $"https://www.google.com/maps/search/?api=1&query={poi.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{poi.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                : $"https://www.google.com/maps/search/?api=1&query={System.Net.WebUtility.UrlEncode(poi.Address + " " + poi.Name)}";

            string imageHtml = !string.IsNullOrEmpty(imageUrl)
                ? $"<img src='{imageUrl}' class='card-img' alt='{System.Net.WebUtility.HtmlEncode(poi.Name)}' onerror=\"this.style.display='none'\">"
                : "<div class='card-img-placeholder'>🍲</div>";

            string addressHtml = !string.IsNullOrWhiteSpace(poi.Address)
                ? $"<a href='{mapsUrl}' target='_blank' class='card-address'><span style='font-size:14px'>📍</span> <span>{System.Net.WebUtility.HtmlEncode(poi.Address)}</span></a>"
                : "";

            string descHtml = !string.IsNullOrWhiteSpace(poi.Description)
                ? $"<p class='card-desc'>{System.Net.WebUtility.HtmlEncode(poi.Description)}</p>"
                : "";

            cards.Append($@"
        <div class='card'>
            <div class='card-img-wrap'>
                <div class='cat-badge'><span>{category}</span></div>
                {imageHtml}
            </div>
            <div class='card-content'>
                <h3 class='card-title'>{System.Net.WebUtility.HtmlEncode(poi.Name)}</h3>
                {addressHtml}
                {descHtml}
                <div id='player-{poi.Id}' class='player-wrap' style='display:none;'></div>
                <button id='btn-{poi.Id}' class='btn-play' onclick=""playPOI({poi.Id})"">🔊 Phát Thuyết Minh</button>
            </div>
        </div>");
        }

        return $@"<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Phố Ẩm Thực Vĩnh Khánh</title>
    <style>
        :root {{ --primary: #e74c3c; --bg: #f8fafc; --card: #ffffff; }}
        * {{ box-sizing: border-box; margin: 0; padding: 0; }}
        body {{
            font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: var(--bg);
            min-height: 100vh;
            color: #1e293b;
            padding-bottom: 40px;
        }}
        #content {{ display: none; }}
        #loading {{ display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; }}
        .spinner {{
            width: 44px; height: 44px; border: 4px solid #fee2e2; border-top-color: #e74c3c; border-radius: 50%; animation: spin 1s linear infinite;
        }}
        @keyframes spin {{ to {{ transform: rotate(360deg); }} }}

        /* ─── Header ─── */
        .hero {{
            background: linear-gradient(135deg, #e74c3c 0%, #f97316 100%);
            color: white;
            padding: 50px 20px 70px;
            text-align: center;
            border-radius: 0 0 32px 32px;
            box-shadow: 0 10px 20px -5px rgba(231,76,60,0.3);
            margin-bottom: -40px;
        }}
        .hero h1 {{ font-size: 28px; font-weight: 800; margin: 0 0 12px; letter-spacing: -0.5px; }}
        .hero p {{ font-size: 15px; opacity: 0.9; margin: 0 0 20px; line-height: 1.5; }}
        .badge-quota {{
            background: rgba(0,0,0,0.2); padding: 8px 20px; border-radius: 20px; font-size: 14px; font-weight: 600; display: inline-flex; align-items: center; gap: 6px; backdrop-filter: blur(4px);
        }}

        /* ─── List ─── */
        .container {{ max-width: 520px; margin: 0 auto; padding: 0 16px; position: relative; z-index: 10; }}

        /* ─── Card ─── */
        .card {{
            background: var(--card); border-radius: 24px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05), 0 2px 4px -2px rgba(0,0,0,0.05); margin-bottom: 20px; overflow: hidden; border: 1px solid #f1f5f9; transition: transform 0.2s;
        }}
        .card:hover {{ transform: translateY(-2px); }}
        .card-img-wrap {{ position: relative; height: 180px; background: #e2e8f0; }}
        .card-img {{ width: 100%; height: 100%; object-fit: cover; display: block; }}
        .card-img-placeholder {{ width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; font-size: 48px; background: linear-gradient(135deg, #fce7f3, #fed7aa); }}
        .cat-badge {{ position: absolute; top: 16px; left: 16px; background: rgba(255,255,255,0.95); color: #000; padding: 6px 12px; border-radius: 12px; font-size: 12px; font-weight: 700; backdrop-filter: blur(4px); box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .card-content {{ padding: 20px; }}
        .card-title {{ font-size: 20px; font-weight: 800; margin: 0 0 10px; line-height: 1.3; color: #0f172a; }}
        .card-address {{ display: inline-flex; align-items: flex-start; gap: 6px; font-size: 13px; color: var(--primary); text-decoration: none; font-weight: 600; margin-bottom: 12px; line-height: 1.4; }}
        .card-desc {{ color: #475569; font-size: 14px; line-height: 1.6; margin: 0 0 20px; }}
        
        .player-wrap {{ background: #f8fafc; border-radius: 16px; padding: 12px; margin-bottom: 16px; border: 1px solid #e2e8f0; }}
        .player-wrap audio {{ width: 100%; height: 40px; outline: none; border-radius: 8px; }}
        
        .btn-play {{
            background: var(--primary); color: white; border: none; padding: 14px; width: 100%; border-radius: 14px; font-size: 16px; font-weight: 700; cursor: pointer; display: flex; justify-content: center; align-items: center; gap: 8px; box-shadow: 0 4px 12px rgba(231,76,60,0.2);
        }}

        /* ─── Footer ─── */
        .footer-cta {{ margin-top: 40px; text-align: center; background: white; padding: 32px 20px; border-radius: 24px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
        .footer-cta h3 {{ margin: 0 0 8px; font-size: 20px; font-weight: 800; color: #0f172a; }}
        .footer-cta p {{ margin: 0 0 20px; color: #64748b; font-size: 14px; line-height: 1.5; }}
        .btn-dark {{ background: #0f172a; color: white; text-decoration: none; padding: 14px 24px; border-radius: 14px; font-weight: 700; display: inline-flex; align-items: center; gap: 8px; width: 100%; justify-content: center; }}
    </style>
</head>
<body>
    <div id='loading'>
        <div class='spinner'></div>
        <p style='margin-top: 16px; color: #475569; font-weight: 600;'>Đang mở ứng dụng...</p>
    </div>

    <div id='content'>
        <div class='hero'>
            <h1>🍜 Phố Ẩm Thực Vĩnh Khánh</h1>
            <p>Trải nghiệm văn hóa ẩm thực Sài Gòn độc đáo. Chạm để nghe thuyết minh miễn phí ngay tại đây.</p>
            <div class='badge-quota'>🔊 Còn <span id='scan-count'>{scanLimit.ScanCount}</span> / {scanLimit.MaxScans} lượt nghe</div>
        </div>

        <div class='container'>
            {cards}

            <div class='footer-cta'>
                <h3>Khám phá Bản đồ Ẩm thực</h3>
                <p>Tải ứng dụng Vĩnh Khánh Food Tour để định vị quán ngon, quét QR nhanh và nghe thuyết minh không giới hạn!</p>
                <a href='/api/download/app-apk' class='btn-dark'>⬇️ Tải App Miễn Phí</a>
            </div>
        </div>
    </div>

    <div id='limit-modal' class='modal-overlay'>
        <div class='modal-box'>
            <div style='font-size:54px; margin-bottom:16px; line-height:1;'>🛑</div>
            <div style='font-size:22px; font-weight:800; color:#ef4444; margin-bottom:12px;'>Đã Hết Lượt Nghe!</div>
            <div style='font-size:15px; color:#64748b; margin-bottom:24px; line-height:1.6;'>Bạn đã sử dụng hết {scanLimit.MaxScans} lượt miễn phí hôm nay. Cài đặt App để tiếp tục nghe không giới hạn nhé!</div>
            <a href='/api/download/app-apk' class='btn-dark' style='background:#ef4444;'>⬇️ Tải Ứng Dụng Ngay</a>
            <button onclick='document.getElementById(""limit-modal"").style.display=""none""' style='background:none; border:none; color:#94a3b8; margin-top:16px; font-size:15px; font-weight:600; cursor:pointer; width:100%; padding:8px;'>Để sau</button>
        </div>
    </div>

    <script>
        var appOpened = false;
        var appDeepLink = 'vinhkhanhtour://foodstreet';

        document.addEventListener('visibilitychange', function () {{
            if (document.hidden) appOpened = true;
        }});
        window.addEventListener('blur', function () {{
            appOpened = true;
        }});

        // 1. Thử mở app bằng Deep Link
        setTimeout(function () {{
            window.location.href = appDeepLink;
        }}, 50);

        // 2. Nếu chưa cài app thì sau 2.5s sẽ tắt màn hình Loading và hiện danh sách quán trên Web
        setTimeout(function () {{
            if (!appOpened) {{
                document.getElementById('loading').style.display = 'none';
                document.getElementById('content').style.display = 'block';
            }}
        }}, 2500);

        var currentScans = {scanLimit.ScanCount};
        var maxScans = {scanLimit.MaxScans};
        var deviceId = '{deviceId}';
        var currentAudio = null;
        var isTTSPlaying = false;
        var currentTTSId = null;

        // POI data injected server-side as JSON — no encoding issues with Vietnamese text
        var poisData = {poisJson};

        function playPOI(id) {{
            var poi = poisData.find(function(p) {{ return p.id === id; }});
            if (!poi) return;
            playAudio(id, poi.audioUrl, poi.desc);
        }}

        function playAudio(id, audioSrc, ttsText) {{
            if (currentScans >= maxScans) {{
                document.getElementById('limit-modal').style.display = 'flex';
                return;
            }}

            if (currentAudio) {{ currentAudio.pause(); currentAudio = null; }}
            if (isTTSPlaying) {{ window.speechSynthesis.cancel(); isTTSPlaying = false; }}

            // Ẩn tất cả player đang mở, hiện lại nút bấm ở các thẻ khác
            document.querySelectorAll('.player-wrap').forEach(function(el) {{ el.style.display = 'none'; }});
            document.querySelectorAll('.btn-play').forEach(function(el) {{ el.style.display = 'flex'; }});

            var playerDiv = document.getElementById('player-' + id);
            var btn = document.getElementById('btn-' + id);

            btn.style.display = 'none';
            playerDiv.style.display = 'block';
            currentScans++;
            document.getElementById('scan-count').innerText = currentScans;

            if (audioSrc) {{
                playerDiv.innerHTML = '<div style=""font-size: 14px; font-weight: 700; margin-bottom: 12px; color: #e74c3c;"">🎧 Đang phát âm thanh...</div>'
                    + '<audio controls autoplay><source src=""' + audioSrc + '"" type=""audio/mpeg""></audio>';
                currentAudio = playerDiv.querySelector('audio');
            }} else {{
                currentTTSId = id;
                playerDiv.innerHTML = '<div style=""font-size: 14px; font-weight: 700; margin-bottom: 12px; color: #f97316;"">🤖 Đang phát AI...</div>'
                    + '<button onclick=""stopTTS()"" class=""btn-play"" style=""background:#f97316;"">⏸️ Dừng phát AI</button>';
                var utterance = new SpeechSynthesisUtterance(ttsText);
                utterance.lang = 'vi-VN';
                utterance.onend = function() {{
                    isTTSPlaying = false;
                    playerDiv.innerHTML = '<div style=""font-size: 14px; font-weight: 700; margin-bottom: 12px; color: #e74c3c;"">🎧 Thuyết Minh AI</div>'
                        + '<button onclick=""replayTTS(' + id + ')"" class=""btn-play"">▶️ Nghe lại AI</button>';
                }};
                window.speechSynthesis.speak(utterance);
                isTTSPlaying = true;
            }}

            // Bắn tín hiệu về server để tăng lượt nghe và ghi lịch sử
            var _poi = poisData.find(function(p) {{ return p.id === id; }});
            var _poiName = _poi ? encodeURIComponent(_poi.name) : '';
            fetch('/api/qrscans/track-listen?deviceId=' + deviceId + '&poiId=' + id + '&poiName=' + _poiName, {{ method: 'POST' }});
        }}

        function stopTTS() {{
            if (isTTSPlaying) {{ window.speechSynthesis.cancel(); isTTSPlaying = false; }}
        }}

        function replayTTS(id) {{
            var poi = poisData.find(function(p) {{ return p.id === id; }});
            if (!poi) return;
            if (isTTSPlaying) {{ window.speechSynthesis.cancel(); isTTSPlaying = false; }}
            var playerDiv = document.getElementById('player-' + id);
            playerDiv.innerHTML = '<div style=""font-size: 14px; font-weight: 700; margin-bottom: 12px; color: #f97316;"">🤖 Đang phát AI...</div>'
                + '<button onclick=""stopTTS()"" class=""btn-play"" style=""background:#f97316;"">⏸️ Dừng phát AI</button>';
            var utterance = new SpeechSynthesisUtterance(poi.desc);
            utterance.lang = 'vi-VN';
            utterance.onend = function() {{
                isTTSPlaying = false;
                playerDiv.innerHTML = '<div style=""font-size: 14px; font-weight: 700; margin-bottom: 12px; color: #e74c3c;"">🎧 Thuyết Minh AI</div>'
                    + '<button onclick=""replayTTS(' + id + ')"" class=""btn-play"">▶️ Nghe lại AI</button>';
            }};
            window.speechSynthesis.speak(utterance);
            isTTSPlaying = true;
        }}
    </script>
</body>
</html>";
    }

    private static string BuildLimitExceededHtml()
    {
        string downloadUrl = "/api/download/app-apk";
        return $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Đã hết lượt nghe miễn phí</title>
    <style>
        body {{
            font-family: system-ui, -apple-system, sans-serif; background: #f8fafc; 
            display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0; padding: 20px; color: #0f172a;
        }}
        .card {{
            background: white; border-radius: 24px; padding: 40px 24px; max-width: 400px; width: 100%; 
            text-align: center; box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1); border: 1px solid #f1f5f9;
        }}
        .icon {{ 
            width: 80px; height: 80px; background: #fee2e2; border-radius: 50%; display: flex; 
            align-items: center; justify-content: center; font-size: 40px; margin: 0 auto 24px; box-shadow: 0 0 0 10px #fef2f2; 
        }}
        .title {{ font-size: 24px; font-weight: 800; margin: 0 0 12px; color: #ef4444; }}
        .desc {{ font-size: 15px; color: #64748b; line-height: 1.6; margin: 0 0 32px; }}
        .btn {{ display: inline-block; background: #0f172a; color: white; text-decoration: none; padding: 16px 24px; border-radius: 16px; font-weight: 700; width: 100%; box-sizing: border-box; }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='icon'>🎧</div>
        <h1 class='title'>Đã Hết Lượt Miễn Phí</h1>
        <p class='desc'>Bạn đã sử dụng hết 5 lượt nghe thử. Tải ngay ứng dụng Vĩnh Khánh Food Tour để tiếp tục khám phá bản đồ ẩm thực không giới hạn!</p>
        <a href='{downloadUrl}' class='btn'>⬇️ Tải App Ngay</a>
    </div>
</body>
</html>";
    }
}
