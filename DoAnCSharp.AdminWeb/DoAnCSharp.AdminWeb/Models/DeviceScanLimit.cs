using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class DeviceScanLimit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string DeviceId { get; set; } = string.Empty;

    public int ScanCount { get; set; } = 0;

    public int MaxScans { get; set; } = 5;

    public DateTime LastResetDate { get; set; } = DateTime.Now;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
