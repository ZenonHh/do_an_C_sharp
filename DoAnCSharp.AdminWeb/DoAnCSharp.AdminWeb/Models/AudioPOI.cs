using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

/// <summary>
/// Audio POI Model - Contains all multilingual descriptions and metadata
/// </summary>
public class AudioPOI
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // Multilingual descriptions
    public string Description { get; set; } = string.Empty;          // Vietnamese
    public string DescriptionEn { get; set; } = string.Empty;        // English
    public string DescriptionJa { get; set; } = string.Empty;        // Japanese
    public string DescriptionRu { get; set; } = string.Empty;        // Russian
    public string DescriptionZh { get; set; } = string.Empty;        // Chinese

    // Location and metadata
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int Radius { get; set; } = 50;
    public int Priority { get; set; } = 1;
    public string ImageAsset { get; set; } = "dotnet_bot.png";

    // QR and Audio
    public string QRCode { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }

    // Ownership and Timestamps
    public int OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
