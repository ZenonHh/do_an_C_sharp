namespace DoAnCSharp.AdminWeb.Models;

public class DashboardStatistics
{
    // User Statistics
    public int TotalUsers { get; set; } = 0;
    public int TotalPaidUsers { get; set; } = 0;
    public int TotalFreeUsers { get; set; } = 0;
    public int NewUsersThisMonth { get; set; } = 0;
    public int UsersOnlineToday { get; set; } = 0;

    // Restaurant Statistics
    public int TotalRestaurants { get; set; } = 0;
    public int RestaurantsWithImages { get; set; } = 0;
    public int RestaurantsWithQRCode { get; set; } = 0;
    public int NewRestaurantsThisMonth { get; set; } = 0;

    // Device Statistics
    public int TotalDevicesRegistered { get; set; } = 0;
    public int DevicesOnlineNow { get; set; } = 0;
    public int AndroidDevices { get; set; } = 0;
    public int iOSDevices { get; set; } = 0;

    // Payment Statistics
    public decimal TotalRevenueVND { get; set; } = 0;
    public decimal TotalRevenueUSD { get; set; } = 0;
    public int PaymentTransactionsThisMonth { get; set; } = 0;

    // QR Scanning Statistics
    public int TotalQRScans { get; set; } = 0;
    public int QRScansToday { get; set; } = 0;
    public int QRScansThisMonth { get; set; } = 0;
    public string MostScannedRestaurant { get; set; } = string.Empty;
    public int MostScannedRestaurantScans { get; set; } = 0;

    // System Statistics
    public int TotalAdminUsers { get; set; } = 0;
    public int ActiveAdminSessions { get; set; } = 0;
    public long DatabaseSizeBytes { get; set; } = 0;
    public int TotalAuditLogs { get; set; } = 0;

    // Top Performers
    public List<TopRestaurant> TopRestaurants { get; set; } = new();
    public List<TopUser> TopUsers { get; set; } = new();
}

public class TopRestaurant
{
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int ScanCount { get; set; }
    public double AverageRating { get; set; }
    public int UniqueUsers { get; set; }
}

public class TopUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ScanCount { get; set; }
    public int DeviceCount { get; set; }
}
