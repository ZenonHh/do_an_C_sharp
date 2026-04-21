using SQLite;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DoAnCSharp.Models;

public partial class AudioPOI : ObservableObject
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

    [ObservableProperty]
    [property: Ignore]
    // ĐÃ SỬA: Đặt giá trị mặc định để không bao giờ bị tàng hình
    private string _distanceInfo = "📍 Chưa xác định"; 

    [ObservableProperty]
    [property: Ignore]
    private string _displayName = "";

    [ObservableProperty]
    [property: Ignore]
    private string _displayDescription = "";

    // Cache weight cho thuật toán ưu tiên: priority base + số lượt nghe
    [Ignore]
    public int HeatWeight { get; set; } = 1;
}