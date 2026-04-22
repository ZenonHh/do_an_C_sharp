using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class AuditLog
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int AdminUserId { get; set; } // Who did the action

    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE, LOGIN, EXPORT

    public string EntityType { get; set; } = string.Empty; // User, AudioPOI, Payment, etc.

    public int? EntityId { get; set; } // ID of affected entity

    public string OldValue { get; set; } = string.Empty; // Previous value (JSON)

    public string NewValue { get; set; } = string.Empty; // New value (JSON)

    public string IPAddress { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public bool IsSuccess { get; set; } = true;

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Audit Log Actions
/// </summary>
public static class AuditLogAction
{
    public const string Create = "CREATE";
    public const string Update = "UPDATE";
    public const string Delete = "DELETE";
    public const string Login = "LOGIN";
    public const string Logout = "LOGOUT";
    public const string Export = "EXPORT";
    public const string Import = "IMPORT";
    public const string ViewSensitive = "VIEW_SENSITIVE";
    public const string ChangePassword = "CHANGE_PASSWORD";
    public const string ResetPassword = "RESET_PASSWORD";
}

public static class AuditLogEntityType
{
    public const string User = "User";
    public const string AdminUser = "AdminUser";
    public const string AudioPOI = "AudioPOI";
    public const string UserPayment = "UserPayment";
    public const string UserDevice = "UserDevice";
    public const string SystemSetting = "SystemSetting";
    public const string RestaurantImage = "RestaurantImage";
}
