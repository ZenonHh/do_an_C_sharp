using SQLite;
using System;

namespace DoAnCSharp.Models;

public class PlayHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string PoiName { get; set; } = string.Empty;
    public string ImageAsset { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }

    // Chuẩn bị cho backend: ai nghe, đã sync lên server chưa
    public string UserId { get; set; } = "guest";
    public bool IsSynced { get; set; } = false;
}