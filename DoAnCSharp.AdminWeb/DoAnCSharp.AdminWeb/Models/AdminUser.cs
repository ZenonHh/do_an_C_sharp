using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class AdminUser
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty; // TODO: Hash in production

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "admin"; // admin, manager, viewer

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? LastLoginAt { get; set; }

    public string LastLoginIP { get; set; } = string.Empty;

    public int LoginCount { get; set; } = 0;
}
