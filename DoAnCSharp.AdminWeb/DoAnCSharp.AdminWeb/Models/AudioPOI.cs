using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class AudioPOI
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int Radius { get; set; } = 50;
    public int Priority { get; set; } = 1;
    public string ImageAsset { get; set; } = "dotnet_bot.png";

    public string? QRCode { get; set; }
    public string? AudioUrl { get; set; }
    public int? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
