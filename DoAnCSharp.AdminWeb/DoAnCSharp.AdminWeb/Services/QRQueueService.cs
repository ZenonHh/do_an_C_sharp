using System.Collections.Concurrent;
using DoAnCSharp.AdminWeb.Models;

namespace DoAnCSharp.AdminWeb.Services;

/// <summary>
/// Service quản lý hàng đợi QR scan với rate limiting và priority queue
/// </summary>
public class QRQueueService
{
    private readonly ILogger<QRQueueService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // Concurrent queue for pending requests
    private readonly ConcurrentQueue<QRQueueItem> _pendingQueue = new();

    // Active processing requests (deviceId -> QRQueueItem)
    private readonly ConcurrentDictionary<string, QRQueueItem> _activeRequests = new();

    // Configuration
    private const int MAX_CONCURRENT_REQUESTS = 10; // Số request xử lý đồng thời tối đa
    private const int MAX_QUEUE_SIZE = 100; // Kích thước hàng đợi tối đa
    private const int REQUEST_TIMEOUT_SECONDS = 30; // Timeout cho mỗi request

    // Statistics
    private long _totalProcessed = 0;
    private long _totalQueued = 0;
    private long _totalRejected = 0;
    private DateTime _serviceStartTime = DateTime.Now;

    public QRQueueService(ILogger<QRQueueService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        // Start background worker to process queue
        Task.Run(ProcessQueueAsync);

        // Start cleanup worker to remove stale requests
        Task.Run(CleanupStaleRequestsAsync);
    }

    /// <summary>
    /// Thêm request vào hàng đợi
    /// </summary>
    public async Task<QRQueueResult> EnqueueRequestAsync(string deviceId, string qrCode, bool isPaidUser = false)
    {
        try
        {
            // Check if device already has active request
            if (_activeRequests.ContainsKey(deviceId))
            {
                return new QRQueueResult
                {
                    Success = false,
                    Status = "already_processing",
                    Message = "Bạn đang có request đang xử lý. Vui lòng đợi.",
                    QueuePosition = 0
                };
            }

            // Check if device already in queue
            if (_pendingQueue.Any(q => q.DeviceId == deviceId))
            {
                var position = _pendingQueue.ToList().FindIndex(q => q.DeviceId == deviceId) + 1;
                return new QRQueueResult
                {
                    Success = false,
                    Status = "already_queued",
                    Message = $"Bạn đã có trong hàng đợi. Vị trí: {position}",
                    QueuePosition = position
                };
            }

            // Check queue size limit
            if (_pendingQueue.Count >= MAX_QUEUE_SIZE)
            {
                Interlocked.Increment(ref _totalRejected);
                return new QRQueueResult
                {
                    Success = false,
                    Status = "queue_full",
                    Message = "Hệ thống đang quá tải. Vui lòng thử lại sau.",
                    QueuePosition = -1
                };
            }

            // Check if can process immediately
            if (_activeRequests.Count < MAX_CONCURRENT_REQUESTS)
            {
                var item = new QRQueueItem
                {
                    Id = Guid.NewGuid().ToString(),
                    DeviceId = deviceId,
                    QRCode = qrCode,
                    IsPaidUser = isPaidUser,
                    EnqueuedAt = DateTime.Now,
                    Status = "processing"
                };

                if (_activeRequests.TryAdd(deviceId, item))
                {
                    _logger.LogInformation($"✅ Request processing immediately: {deviceId} -> {qrCode}");
                    return new QRQueueResult
                    {
                        Success = true,
                        Status = "processing",
                        Message = "Đang xử lý request của bạn...",
                        QueuePosition = 0,
                        QueueItemId = item.Id
                    };
                }
            }

            // Add to queue with priority
            var queueItem = new QRQueueItem
            {
                Id = Guid.NewGuid().ToString(),
                DeviceId = deviceId,
                QRCode = qrCode,
                IsPaidUser = isPaidUser,
                EnqueuedAt = DateTime.Now,
                Priority = isPaidUser ? 10 : 1, // Paid users get higher priority
                Status = "queued"
            };

            _pendingQueue.Enqueue(queueItem);
            Interlocked.Increment(ref _totalQueued);

            var queuePosition = _pendingQueue.Count;
            _logger.LogInformation($"📥 Request queued: {deviceId} -> {qrCode} (Position: {queuePosition}, Paid: {isPaidUser})");

            return new QRQueueResult
            {
                Success = true,
                Status = "queued",
                Message = $"Bạn đang ở vị trí {queuePosition} trong hàng đợi.",
                QueuePosition = queuePosition,
                QueueItemId = queueItem.Id,
                EstimatedWaitSeconds = queuePosition * 3 // Estimate 3s per request
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing request");
            return new QRQueueResult
            {
                Success = false,
                Status = "error",
                Message = "Lỗi hệ thống. Vui lòng thử lại."
            };
        }
    }

    /// <summary>
    /// Đánh dấu request đã hoàn thành
    /// </summary>
    public void CompleteRequest(string deviceId)
    {
        if (_activeRequests.TryRemove(deviceId, out var item))
        {
            item.Status = "completed";
            item.CompletedAt = DateTime.Now;
            Interlocked.Increment(ref _totalProcessed);

            var duration = (item.CompletedAt.Value - (item.ProcessingStartedAt ?? item.EnqueuedAt)).TotalSeconds;
            _logger.LogInformation($"✅ Request completed: {deviceId} (Duration: {duration:F2}s)");
        }
    }

    /// <summary>
    /// Background worker xử lý hàng đợi
    /// </summary>
    private async Task ProcessQueueAsync()
    {
        _logger.LogInformation("🚀 Queue processor started");

        while (true)
        {
            try
            {
                // Check if we can process more requests
                if (_activeRequests.Count < MAX_CONCURRENT_REQUESTS && _pendingQueue.TryDequeue(out var item))
                {
                    // Sort by priority before processing (paid users first)
                    var sortedQueue = _pendingQueue.OrderByDescending(q => q.Priority).ToList();
                    _pendingQueue.Clear();

                    foreach (var q in sortedQueue)
                    {
                        _pendingQueue.Enqueue(q);
                    }

                    if (_pendingQueue.TryDequeue(out item))
                    {
                        item.Status = "processing";
                        item.ProcessingStartedAt = DateTime.Now;

                        if (_activeRequests.TryAdd(item.DeviceId, item))
                        {
                            var waitTime = (item.ProcessingStartedAt.Value - item.EnqueuedAt).TotalSeconds;
                            _logger.LogInformation($"⚡ Processing queued request: {item.DeviceId} (Waited: {waitTime:F2}s, Paid: {item.IsPaidUser})");
                        }
                    }
                }

                await Task.Delay(100); // Check every 100ms
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in queue processor");
                await Task.Delay(1000);
            }
        }
    }

    /// <summary>
    /// Background worker dọn dẹp các request bị timeout
    /// </summary>
    private async Task CleanupStaleRequestsAsync()
    {
        _logger.LogInformation("🧹 Cleanup worker started");

        while (true)
        {
            try
            {
                var now = DateTime.Now;
                var staleRequests = _activeRequests
                    .Where(kvp => (now - (kvp.Value.ProcessingStartedAt ?? kvp.Value.EnqueuedAt)).TotalSeconds > REQUEST_TIMEOUT_SECONDS)
                    .ToList();

                foreach (var stale in staleRequests)
                {
                    if (_activeRequests.TryRemove(stale.Key, out var item))
                    {
                        item.Status = "timeout";
                        _logger.LogWarning($"⏱️ Request timeout: {stale.Key} (Age: {(now - item.EnqueuedAt).TotalSeconds:F2}s)");
                    }
                }

                await Task.Delay(5000); // Check every 5 seconds
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in cleanup worker");
                await Task.Delay(5000);
            }
        }
    }

    /// <summary>
    /// Lấy thông tin hàng đợi hiện tại
    /// </summary>
    public QRQueueStats GetQueueStats()
    {
        var now = DateTime.Now;
        var uptime = (now - _serviceStartTime).TotalSeconds;

        return new QRQueueStats
        {
            ActiveRequests = _activeRequests.Count,
            QueuedRequests = _pendingQueue.Count,
            MaxConcurrentRequests = MAX_CONCURRENT_REQUESTS,
            MaxQueueSize = MAX_QUEUE_SIZE,
            TotalProcessed = _totalProcessed,
            TotalQueued = _totalQueued,
            TotalRejected = _totalRejected,
            AverageProcessingRate = uptime > 0 ? _totalProcessed / uptime : 0,
            ServiceUptimeSeconds = uptime,
            ActiveRequestsList = _activeRequests.Values.Select(r => new QRQueueItemInfo
            {
                Id = r.Id,
                DeviceId = MaskDeviceId(r.DeviceId),
                QRCode = r.QRCode,
                IsPaidUser = r.IsPaidUser,
                Status = r.Status,
                EnqueuedAt = r.EnqueuedAt,
                ProcessingStartedAt = r.ProcessingStartedAt,
                WaitTimeSeconds = r.ProcessingStartedAt.HasValue
                    ? (r.ProcessingStartedAt.Value - r.EnqueuedAt).TotalSeconds
                    : (now - r.EnqueuedAt).TotalSeconds
            }).OrderBy(r => r.EnqueuedAt).ToList(),
            QueuedRequestsList = _pendingQueue.Select((r, index) => new QRQueueItemInfo
            {
                Id = r.Id,
                DeviceId = MaskDeviceId(r.DeviceId),
                QRCode = r.QRCode,
                IsPaidUser = r.IsPaidUser,
                Status = r.Status,
                EnqueuedAt = r.EnqueuedAt,
                QueuePosition = index + 1,
                EstimatedWaitSeconds = (index + 1) * 3,
                WaitTimeSeconds = (now - r.EnqueuedAt).TotalSeconds
            }).OrderByDescending(r => r.IsPaidUser).ThenBy(r => r.EnqueuedAt).ToList()
        };
    }

    /// <summary>
    /// Mask device ID for privacy (show only last 6 chars)
    /// </summary>
    private string MaskDeviceId(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId) || deviceId.Length <= 6)
            return deviceId;

        return "..." + deviceId.Substring(deviceId.Length - 6);
    }

    /// <summary>
    /// Xóa toàn bộ hàng đợi và các request đang xử lý (dùng cho admin reset)
    /// </summary>
    public void ClearQueue()
    {
        // Drain the pending queue
        int drained = 0;
        while (_pendingQueue.TryDequeue(out _)) drained++;

        // Remove all active requests
        foreach (var key in _activeRequests.Keys.ToList())
            _activeRequests.TryRemove(key, out _);

        _logger.LogWarning($"🗑 Queue cleared by admin — drained {drained} pending, removed all active requests.");
    }

    /// <summary>
    /// Kiểm tra trạng thái request của device
    /// </summary>
    public QRQueueItemInfo? GetRequestStatus(string deviceId)
    {
        // Check active requests
        if (_activeRequests.TryGetValue(deviceId, out var activeItem))
        {
            return new QRQueueItemInfo
            {
                Id = activeItem.Id,
                DeviceId = MaskDeviceId(activeItem.DeviceId),
                QRCode = activeItem.QRCode,
                IsPaidUser = activeItem.IsPaidUser,
                Status = activeItem.Status,
                EnqueuedAt = activeItem.EnqueuedAt,
                ProcessingStartedAt = activeItem.ProcessingStartedAt,
                WaitTimeSeconds = activeItem.ProcessingStartedAt.HasValue
                    ? (activeItem.ProcessingStartedAt.Value - activeItem.EnqueuedAt).TotalSeconds
                    : (DateTime.Now - activeItem.EnqueuedAt).TotalSeconds
            };
        }

        // Check queued requests
        var queuedItem = _pendingQueue.FirstOrDefault(q => q.DeviceId == deviceId);
        if (queuedItem != null)
        {
            var position = _pendingQueue.ToList().FindIndex(q => q.DeviceId == deviceId) + 1;
            return new QRQueueItemInfo
            {
                Id = queuedItem.Id,
                DeviceId = MaskDeviceId(queuedItem.DeviceId),
                QRCode = queuedItem.QRCode,
                IsPaidUser = queuedItem.IsPaidUser,
                Status = queuedItem.Status,
                EnqueuedAt = queuedItem.EnqueuedAt,
                QueuePosition = position,
                EstimatedWaitSeconds = position * 3,
                WaitTimeSeconds = (DateTime.Now - queuedItem.EnqueuedAt).TotalSeconds
            };
        }

        return null;
    }
}

// ===== Models =====

public class QRQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public bool IsPaidUser { get; set; }
    public int Priority { get; set; } = 1;
    public DateTime EnqueuedAt { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "queued"; // queued, processing, completed, timeout
}

public class QRQueueResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int QueuePosition { get; set; }
    public string? QueueItemId { get; set; }
    public int EstimatedWaitSeconds { get; set; }
}

public class QRQueueStats
{
    public int ActiveRequests { get; set; }
    public int QueuedRequests { get; set; }
    public int MaxConcurrentRequests { get; set; }
    public int MaxQueueSize { get; set; }
    public long TotalProcessed { get; set; }
    public long TotalQueued { get; set; }
    public long TotalRejected { get; set; }
    public double AverageProcessingRate { get; set; }
    public double ServiceUptimeSeconds { get; set; }
    public List<QRQueueItemInfo> ActiveRequestsList { get; set; } = new();
    public List<QRQueueItemInfo> QueuedRequestsList { get; set; } = new();
}

public class QRQueueItemInfo
{
    public string Id { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string QRCode { get; set; } = string.Empty;
    public bool IsPaidUser { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime EnqueuedAt { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public int QueuePosition { get; set; }
    public int EstimatedWaitSeconds { get; set; }
    public double WaitTimeSeconds { get; set; }
}
