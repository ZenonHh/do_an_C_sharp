using Microsoft.AspNetCore.Mvc;
using DoAnCSharp.AdminWeb.Services;

namespace DoAnCSharp.AdminWeb.Controllers;

/// <summary>
/// API endpoints để monitor và quản lý hàng đợi QR scan
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QRQueueController : ControllerBase
{
    private readonly QRQueueService _queueService;
    private readonly ILogger<QRQueueController> _logger;

    public QRQueueController(QRQueueService queueService, ILogger<QRQueueController> logger)
    {
        _queueService = queueService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy thống kê hàng đợi real-time
    /// </summary>
    [HttpGet("stats")]
    public ActionResult<object> GetQueueStats()
    {
        try
        {
            var stats = _queueService.GetQueueStats();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue stats");
            return StatusCode(500, new { error = "Lỗi khi lấy thống kê hàng đợi" });
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái request của device
    /// </summary>
    [HttpGet("status/{deviceId}")]
    public ActionResult<object> GetRequestStatus(string deviceId)
    {
        try
        {
            var status = _queueService.GetRequestStatus(deviceId);

            if (status == null)
            {
                return NotFound(new { message = "Không tìm thấy request của device này" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request status for device {DeviceId}", deviceId);
            return StatusCode(500, new { error = "Lỗi khi kiểm tra trạng thái" });
        }
    }

    /// <summary>
    /// Test endpoint để thêm request vào hàng đợi.
    /// Request tự động hoàn thành sau completionMs ms, giúp demo thấy vòng đời đầy đủ.
    /// </summary>
    [HttpPost("test/enqueue")]
    public async Task<ActionResult<object>> TestEnqueue(
        [FromQuery] string? deviceId = null,
        [FromQuery] string? qrCode   = null,
        [FromQuery] bool   isPaid    = false,
        [FromQuery] int    completionMs = 4000)   // default: tự hoàn thành sau 4 giây
    {
        try
        {
            deviceId ??= $"test_device_{Guid.NewGuid().ToString("N")[..8]}";
            qrCode   ??= $"POI_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            var result = await _queueService.EnqueueRequestAsync(deviceId, qrCode, isPaid);

            // Auto-complete in background so the slot frees up and queued items advance
            if (result.Success && result.Status is "processing" or "queued")
            {
                var capturedDeviceId = deviceId;
                var delayMs = Math.Clamp(completionMs, 500, 30_000);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(delayMs);
                    _queueService.CompleteRequest(capturedDeviceId);
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test enqueue");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint để đánh dấu request hoàn thành
    /// </summary>
    [HttpPost("test/complete")]
    public ActionResult<object> TestComplete([FromQuery] string deviceId)
    {
        try
        {
            _queueService.CompleteRequest(deviceId);
            return Ok(new { message = $"Request của {deviceId} đã được đánh dấu hoàn thành" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test complete");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách requests đang xử lý
    /// </summary>
    [HttpGet("active")]
    public ActionResult<object> GetActiveRequests()
    {
        try
        {
            var stats = _queueService.GetQueueStats();
            return Ok(new
            {
                count = stats.ActiveRequests,
                maxConcurrent = stats.MaxConcurrentRequests,
                requests = stats.ActiveRequestsList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active requests");
            return StatusCode(500, new { error = "Lỗi khi lấy danh sách requests" });
        }
    }

    /// <summary>
    /// Lấy danh sách requests đang chờ trong queue
    /// </summary>
    [HttpGet("queued")]
    public ActionResult<object> GetQueuedRequests()
    {
        try
        {
            var stats = _queueService.GetQueueStats();
            return Ok(new
            {
                count = stats.QueuedRequests,
                maxQueueSize = stats.MaxQueueSize,
                requests = stats.QueuedRequestsList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queued requests");
            return StatusCode(500, new { error = "Lỗi khi lấy danh sách hàng đợi" });
        }
    }

    /// <summary>
    /// Xóa toàn bộ hàng đợi (admin reset)
    /// </summary>
    [HttpPost("clear")]
    public ActionResult<object> ClearQueue()
    {
        try
        {
            _queueService.ClearQueue();
            return Ok(new { message = "Hàng đợi đã được xóa sạch." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing queue");
            return StatusCode(500, new { error = "Lỗi khi xóa hàng đợi" });
        }
    }

    /// <summary>
    /// Lấy metrics tổng quan
    /// </summary>
    [HttpGet("metrics")]
    public ActionResult<object> GetMetrics()
    {
        try
        {
            var stats = _queueService.GetQueueStats();

            return Ok(new
            {
                current = new
                {
                    activeRequests = stats.ActiveRequests,
                    queuedRequests = stats.QueuedRequests,
                    utilizationPercent = (double)stats.ActiveRequests / stats.MaxConcurrentRequests * 100,
                    queueUtilizationPercent = (double)stats.QueuedRequests / stats.MaxQueueSize * 100
                },
                lifetime = new
                {
                    totalProcessed = stats.TotalProcessed,
                    totalQueued = stats.TotalQueued,
                    totalRejected = stats.TotalRejected,
                    averageProcessingRate = stats.AverageProcessingRate,
                    uptimeSeconds = stats.ServiceUptimeSeconds,
                    uptimeFormatted = TimeSpan.FromSeconds(stats.ServiceUptimeSeconds).ToString(@"hh\:mm\:ss")
                },
                capacity = new
                {
                    maxConcurrentRequests = stats.MaxConcurrentRequests,
                    maxQueueSize = stats.MaxQueueSize,
                    availableSlots = stats.MaxConcurrentRequests - stats.ActiveRequests,
                    availableQueueSlots = stats.MaxQueueSize - stats.QueuedRequests
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metrics");
            return StatusCode(500, new { error = "Lỗi khi lấy metrics" });
        }
    }
}
