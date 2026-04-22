using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class QRCodeSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int RestaurantId { get; set; }

    [Indexed]
    public string QRCode { get; set; } = string.Empty;

    [Indexed]
    public string SessionToken { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public int ScanCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastScannedAt { get; set; }

    public int? LastScannedByUserId { get; set; }
}
