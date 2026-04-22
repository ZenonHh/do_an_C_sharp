using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class SystemSetting
{
    [PrimaryKey]
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SettingType { get; set; } = "string"; // string, int, bool, decimal

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Predefined system settings keys
/// </summary>
public static class SystemSettingKeys
{
    // App Settings
    public const string AppName = "app_name"; // Default: Vĩnh Khánh Tour
    public const string AppVersion = "app_version";
    public const string AppDescription = "app_description";

    // Payment Settings
    public const string PremiumPriceVND = "premium_price_vnd"; // Default: 99000
    public const string PremiumPriceUSD = "premium_price_usd"; // Default: 4.99
    public const string DailyFreeQRScans = "daily_free_qr_scans"; // Default: 5
    public const string PaymentCurrency = "payment_currency"; // VND, USD

    // Location Settings
    public const string DefaultLatitude = "default_latitude"; // 10.7600
    public const string DefaultLongitude = "default_longitude"; // 106.7000
    public const string DefaultMapZoom = "default_map_zoom"; // 2
    public const string LocationRadius = "location_radius"; // 50

    // Notification Settings
    public const string EnableNotifications = "enable_notifications"; // true/false
    public const string NotificationFrequency = "notification_frequency"; // daily, weekly, never

    // Security Settings
    public const string SessionTimeoutMinutes = "session_timeout_minutes"; // 30
    public const string PasswordMinLength = "password_min_length"; // 6
    public const string AllowUserRegistration = "allow_user_registration"; // true/false
    public const string RequireEmailVerification = "require_email_verification"; // true/false

    // Feature Flags
    public const string EnablePaymentGateway = "enable_payment_gateway"; // true/false
    public const string EnableAudioGuide = "enable_audio_guide"; // true/false
    public const string EnableQRScanning = "enable_qr_scanning"; // true/false
    public const string EnableSocialSharing = "enable_social_sharing"; // true/false

    // Maintenance
    public const string MaintenanceMode = "maintenance_mode"; // true/false
    public const string MaintenanceMessage = "maintenance_message";
}
