using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class UserStatus
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }
    
    public bool IsOnline { get; set; }
    
    public DateTime LastActiveAt { get; set; } = DateTime.Now;
    
    public string DeviceInfo { get; set; } = string.Empty;
    
    public string IpAddress { get; set; } = string.Empty;
}
