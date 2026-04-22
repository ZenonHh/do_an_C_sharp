namespace DoAnCSharp.AdminWeb.Models;

public class OnlineUserSummary
{
    public int TotalOnlineUsers { get; set; }
    public int OnlineDevices { get; set; }
    public int ActiveListeningUsers { get; set; }
    public int TotalRegisteredUsers { get; set; }
    public int TotalPaidUsers { get; set; }
    public int TodayQRScans { get; set; }
    public List<UserDevice> OnlineDeviceDetails { get; set; } = new();
}
