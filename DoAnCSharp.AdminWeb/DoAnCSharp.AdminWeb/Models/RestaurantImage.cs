using SQLite;

namespace DoAnCSharp.AdminWeb.Models;

public class RestaurantImage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int RestaurantId { get; set; } // Foreign key to AudioPOI

    public string ImagePath { get; set; } = string.Empty; // File path or URL

    public string ImageName { get; set; } = string.Empty; // Original file name

    public long FileSizeBytes { get; set; } = 0; // File size in bytes

    public string MimeType { get; set; } = "image/jpeg"; // MIME type

    public bool IsMainImage { get; set; } = false; // Primary image

    public int DisplayOrder { get; set; } = 0; // Order in gallery

    public DateTime UploadedAt { get; set; } = DateTime.Now;

    public string UploadedBy { get; set; } = string.Empty; // Admin who uploaded
}
