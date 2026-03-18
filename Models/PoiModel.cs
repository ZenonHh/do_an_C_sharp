namespace DoAnCSharp.Models;

public enum NarrationType
{
    Tts, 
    Audio
}

public class POI
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    // Đối tượng Location chứa cả Lat và Long
    public Location Location { get; set; } = new Location(); 
    
    public double Radius { get; set; }
    public int Priority { get; set; } = 0;
    
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    
    public string? ImageAsset { get; set; }
    public string? MapLink { get; set; }
    
    public NarrationType NarrationType { get; set; } = NarrationType.Tts;
    
    public string? Content { get; set; }

    // SỬA LỖI CS0200: Thêm 'set' để PoiRepository có thể gán giá trị
    public double Latitude 
    { 
        get => Location.Latitude; 
        set => Location.Latitude = value; 
    }

    public double Longitude 
    { 
        get => Location.Longitude; 
        set => Location.Longitude = value; 
    }
}
