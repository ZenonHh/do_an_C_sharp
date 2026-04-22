using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class UserDevice
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Unique]
    public string DeviceId { get; set; } = string.Empty; // Unique device identifier

    public string DeviceName { get; set; } = string.Empty; // e.g., "Nguyễn Văn A - Samsung Galaxy A12"

    public string DeviceModel { get; set; } = string.Empty; // e.g., "Samsung Galaxy A12"

    public string DeviceOS { get; set; } = string.Empty; // Android, iOS

    public string AppVersion { get; set; } = string.Empty; // App version number

    public bool IsOnline { get; set; } = false;

    public DateTime LastOnlineAt { get; set; } = DateTime.Now;

    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    public string IpAddress { get; set; } = string.Empty; // IP address when online

    public string UserAgent { get; set; } = string.Empty; // Browser/Device user agent

    public string LocationInfo { get; set; } = string.Empty; // City, Country

    public double Latitude { get; set; } = 0;

    public double Longitude { get; set; } = 0;

    public bool IsActive { get; set; } = true; // Can be disabled by admin
}
