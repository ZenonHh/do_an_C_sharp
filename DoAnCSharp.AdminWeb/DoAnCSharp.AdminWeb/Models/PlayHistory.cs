using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class PlayHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int POIId { get; set; } // Restaurant/POI ID
    public string POIName { get; set; } = string.Empty;
    public string ImageAsset { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
}
