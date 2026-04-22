using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Models;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DatabaseService _dbService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(DatabaseService dbService, ILogger<DevicesController> logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserDevice>>> GetAllDevices()
    {
        try
        {
            var devices = await _dbService.GetAllUserDevicesAsync();
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetAllDevices Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi lấy danh sách thiết bị" });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<UserDevice>>> GetUserDevices(int userId)
    {
        try
        {
            var devices = await _dbService.GetUserDevicesAsync(userId);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetUserDevices Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi lấy thiết bị người dùng" });
        }
    }

    [HttpGet("{deviceId}")]
    public async Task<ActionResult<UserDevice>> GetDevice(int deviceId)
    {
        try
        {
            var device = await _dbService.GetUserDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound(new { error = "Không tìm thấy thiết bị" });

            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetDevice Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi lấy thiết bị" });
        }
    }

    [HttpGet("{deviceId}/scanlimit")]
    public async Task<ActionResult<DeviceScanLimit>> GetDeviceScanLimit(string deviceId)
    {
        try
        {
            var limit = await _dbService.GetDeviceScanLimitAsync(deviceId);
            if (limit == null)
            {
                limit = new DeviceScanLimit
                {
                    DeviceId = deviceId,
                    ScanCount = 0,
                    MaxScans = 5,
                    LastResetDate = DateTime.UtcNow.Date
                };
            }
            return Ok(limit);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetDeviceScanLimit Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi lấy giới hạn quét" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserDevice>> RegisterDevice([FromBody] UserDevice device)
    {
        try
        {
            if (device == null)
                return BadRequest(new { error = "Dữ liệu thiết bị không hợp lệ" });

            device.RegisteredAt = DateTime.Now;
            device.LastOnlineAt = DateTime.Now;
            device.IsOnline = true;

            var id = await _dbService.InsertUserDeviceAsync(device);
            device.Id = id;

            return CreatedAtAction(nameof(GetDevice), new { deviceId = id }, device);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ RegisterDevice Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi đăng ký thiết bị" });
        }
    }

    [HttpPut("{deviceId}")]
    public async Task<IActionResult> UpdateDevice(int deviceId, [FromBody] UserDevice device)
    {
        try
        {
            var existing = await _dbService.GetUserDeviceByIdAsync(deviceId);
            if (existing == null)
                return NotFound(new { error = "Không tìm thấy thiết bị" });

            device.Id = deviceId;
            device.RegisteredAt = existing.RegisteredAt;
            device.LastOnlineAt = DateTime.Now;

            await _dbService.UpdateUserDeviceAsync(device);
            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ UpdateDevice Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi cập nhật thiết bị" });
        }
    }

    [HttpPut("{deviceId}/status")]
    public async Task<IActionResult> UpdateDeviceStatus(int deviceId, [FromBody] DeviceStatusUpdate status)
    {
        try
        {
            var device = await _dbService.GetUserDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound(new { error = "Không tìm thấy thiết bị" });

            device.IsOnline = status.IsOnline;
            device.LastOnlineAt = DateTime.Now;
            if (!string.IsNullOrEmpty(status.IpAddress))
                device.IpAddress = status.IpAddress;

            await _dbService.UpdateUserDeviceAsync(device);
            return Ok(device);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ UpdateDeviceStatus Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi cập nhật trạng thái" });
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchDevices(
        [FromQuery] string? query = "",
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 5,
        [FromQuery] string status = "all")
    {
        try
        {
            var allDevices = await _dbService.GetAllUserDevicesAsync();

            // Filter by status
            var filtered = status switch
            {
                "online" => allDevices.Where(d => d.IsOnline).ToList(),
                "offline" => allDevices.Where(d => !d.IsOnline).ToList(),
                _ => allDevices
            };

            // Server-side search (DeviceName, DeviceModel, IpAddress)
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(d =>
                    d.DeviceName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    d.DeviceModel.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    d.IpAddress.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Order by LastOnlineAt desc
            filtered = filtered.OrderByDescending(d => d.LastOnlineAt).ToList();

            var totalCount = filtered.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var paginated = filtered.Skip(page * pageSize).Take(pageSize).ToList();

            return Ok(new
            {
                devices = paginated,
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext = page < totalPages - 1,
                hasPrev = page > 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ SearchDevices Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi tìm kiếm thiết bị" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        try
        {
            var devices = await _dbService.GetAllUserDevicesAsync();
            return Ok(new
            {
                total = devices.Count,
                online = devices.Count(d => d.IsOnline),
                offline = devices.Count(d => !d.IsOnline),
                blocked = devices.Count(d => !d.IsActive)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetStats Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi lấy thống kê" });
        }
    }

    [HttpPost("status/online/increment")]
    public async Task<IActionResult> IncrementOnlineDevices()
    {
        try
        {
            var devices = await _dbService.GetAllUserDevicesAsync();
            var onlineCount = devices.Count(d => d.IsOnline);

            return Ok(new { 
                message = "Online devices count", 
                onlineCount = onlineCount + 1,
                timestamp = DateTime.Now 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ IncrementOnlineDevices Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi cập nhật online count" });
        }
    }

    [HttpPut("{deviceId}/toggle")]
    public async Task<IActionResult> ToggleDeviceActive(int deviceId, [FromBody] dynamic request)
    {
        try
        {
            var device = await _dbService.GetUserDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound(new { error = "Không tìm thấy thiết bị" });

            device.IsActive = request.isActive ?? !device.IsActive;
            await _dbService.UpdateUserDeviceAsync(device);

            return Ok(new { message = "Trạng thái thiết bị đã cập nhật", device = device });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ ToggleDeviceActive Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi cập nhật trạng thái" });
        }
    }

    [HttpDelete("{deviceId}")]
    public async Task<IActionResult> DeleteDevice(int deviceId)
    {
        try
        {
            var device = await _dbService.GetUserDeviceByIdAsync(deviceId);
            if (device == null)
                return NotFound(new { error = "Không tìm thấy thiết bị" });

            await _dbService.DeleteUserDeviceAsync(deviceId);
            return Ok(new { message = "Xóa thiết bị thành công" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ DeleteDevice Error: {ex.Message}");
            return StatusCode(500, new { error = "Lỗi khi xóa thiết bị" });
        }
    }

    // ===== REAL-TIME TRACKING ENDPOINTS =====

    [HttpPost("track")]
    public async Task<ActionResult<object>> TrackDevice([FromBody] DeviceTrackingRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DeviceId))
                return BadRequest(new { error = "DeviceId is required" });

            // Find or create device
            var devices = await _dbService.GetAllUserDevicesAsync();
            var device = devices.FirstOrDefault(d => d.DeviceId == request.DeviceId);

            if (device == null)
            {
                // Create new device
                device = new UserDevice
                {
                    DeviceId = request.DeviceId,
                    DeviceName = request.DeviceName ?? "Unknown Device",
                    DeviceModel = request.DeviceModel ?? "Unknown",
                    DeviceOS = request.DeviceOS ?? "Unknown",
                    AppVersion = request.AppVersion ?? "1.0.0",
                    IpAddress = request.IpAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = request.UserAgent ?? Request.Headers["User-Agent"].ToString(),
                    IsOnline = true,
                    IsActive = true,
                    RegisteredAt = DateTime.Now,
                    LastOnlineAt = DateTime.Now
                };

                await _dbService.InsertUserDeviceAsync(device);
                _logger.LogInformation($"✅ New device registered: {device.DeviceId}");
            }
            else
            {
                // Update existing device
                device.IsOnline = true;
                device.LastOnlineAt = DateTime.Now;

                // Update info if provided
                if (!string.IsNullOrEmpty(request.IpAddress))
                    device.IpAddress = request.IpAddress;
                if (!string.IsNullOrEmpty(request.AppVersion))
                    device.AppVersion = request.AppVersion;
                if (!string.IsNullOrEmpty(request.UserAgent))
                    device.UserAgent = request.UserAgent;

                await _dbService.UpdateUserDeviceAsync(device);
                _logger.LogInformation($"✅ Device updated: {device.DeviceId}");
            }

            // Get current stats
            var allDevices = await _dbService.GetAllUserDevicesAsync();
            var stats = new
            {
                total = allDevices.Count,
                online = allDevices.Count(d => d.IsOnline && 
                    (DateTime.Now - d.LastOnlineAt).TotalMinutes < 5),
                offline = allDevices.Count(d => !d.IsOnline || 
                    (DateTime.Now - d.LastOnlineAt).TotalMinutes >= 5)
            };

            return Ok(new
            {
                success = true,
                device = device,
                stats = stats,
                message = "Device tracked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ TrackDevice Error: {ex.Message}");
            return StatusCode(500, new { error = "Failed to track device" });
        }
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<object>> DeviceHeartbeat([FromBody] DeviceHeartbeatRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DeviceId))
                return BadRequest(new { error = "DeviceId is required" });

            var devices = await _dbService.GetAllUserDevicesAsync();
            var device = devices.FirstOrDefault(d => d.DeviceId == request.DeviceId);

            if (device == null)
                return NotFound(new { error = "Device not found" });

            // Update online status
            device.IsOnline = true;
            device.LastOnlineAt = DateTime.Now;
            await _dbService.UpdateUserDeviceAsync(device);

            return Ok(new
            {
                success = true,
                message = "Heartbeat received",
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ DeviceHeartbeat Error: {ex.Message}");
            return StatusCode(500, new { error = "Failed to process heartbeat" });
        }
    }

    [HttpPost("cleanup-offline")]
    public async Task<ActionResult<object>> CleanupOfflineDevices()
    {
        try
        {
            var devices = await _dbService.GetAllUserDevicesAsync();
            var offlineThreshold = DateTime.Now.AddMinutes(-5);

            var markedOffline = 0;
            foreach (var device in devices)
            {
                if (device.LastOnlineAt < offlineThreshold && device.IsOnline)
                {
                    device.IsOnline = false;
                    await _dbService.UpdateUserDeviceAsync(device);
                    markedOffline++;
                }
            }

            return Ok(new
            {
                success = true,
                markedOffline = markedOffline,
                message = $"Marked {markedOffline} devices as offline"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ CleanupOfflineDevices Error: {ex.Message}");
            return StatusCode(500, new { error = "Failed to cleanup offline devices" });
        }
    }
}

public class DeviceStatusUpdate
{
    public bool IsOnline { get; set; }
    public string? IpAddress { get; set; }
}

public class DeviceTrackingRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceOS { get; set; }
    public string? AppVersion { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class DeviceHeartbeatRequest
{
    public string DeviceId { get; set; } = string.Empty;
}
