using SQLite;
using System;

namespace DoAnCSharp.Models;

public class PlayHistory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Đã thêm đầy đủ 3 thuộc tính bị thiếu
    public string PoiName { get; set; } = string.Empty;
    public string ImageAsset { get; set; } = string.Empty;
    public DateTime PlayedAt { get; set; }
}