using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class QRScanRequest
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int UserId { get; set; }

    [Indexed]
    public int RestaurantId { get; set; }

    public string RestaurantName { get; set; } = string.Empty;

    [Indexed]
    public string QRSessionToken { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public DateTime ScanTime { get; set; }

    public DateTime? AccessTime { get; set; }

    public string Status { get; set; } = "pending"; // pending, accessed, expired, success, blocked

    public string? DeviceInfo { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string Message { get; set; } = string.Empty;

    public int ListeningDurationSeconds { get; set; }
}
