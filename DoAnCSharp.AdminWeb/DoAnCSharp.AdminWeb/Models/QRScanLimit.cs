using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class QRScanLimit
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ScanCount { get; set; } = 0;

    public int MaxScans { get; set; } = 5; // Free users: 5 scans/day, Paid users: unlimited

    public DateTime LastResetDate { get; set; } = DateTime.Now;

    public bool IsPaidUser { get; set; } = false;
}
