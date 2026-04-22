using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    [Unique]
    public string Email { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    public string Avatar { get; set; } = "dotnet_bot.png";
    public string Phone { get; set; } = string.Empty;

    public string Language { get; set; } = "vi";

    public bool IsPaid { get; set; } = false;
    public DateTime? PaidAt { get; set; }
}
