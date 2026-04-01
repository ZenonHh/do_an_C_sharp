using SQLite;

namespace DoAnCSharp.Models;

public class AudioPOI
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    // ĐÂY CHÍNH LÀ DÒNG BẠN ĐANG THIẾU ĐỂ TRỊ LỖI CS0117
    public string Address { get; set; } = string.Empty; 
    
    public string Description { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public int Radius { get; set; } = 50; 
    public int Priority { get; set; } = 1;
    public string ImageAsset { get; set; } = "dotnet_bot.png";

    [Ignore]
    public string DistanceInfo { get; set; } = "";
}