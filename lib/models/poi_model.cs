namespace DoAnCSharp.Models
{
    public enum NarrationType
    {
        Tts,
        Audio
    }

    public class POI
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;           // Tiếng Việt
        public string NameEn { get; set; } = string.Empty;         // Tiếng Anh
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public int Priority { get; set; } = 0;
        public string Description { get; set; } = string.Empty;    // Tiếng Việt
        public string DescriptionEn { get; set; } = string.Empty;  // Tiếng Anh
        public string? ImageAsset { get; set; }
        public string? MapLink { get; set; }
        public NarrationType NarrationType { get; set; } = NarrationType.Tts;
        public string? Content { get; set; }

        // Constructor mặc định cho C#
        public POI() { }

        // Constructor đầy đủ
        public POI(string id, string name, string nameEn, double latitude, double longitude,
                   double radius, int priority, string description, string descriptionEn,
                   string? imageAsset = null, string? mapLink = null,
                   NarrationType narrationType = NarrationType.Tts, string? content = null)
        {
            Id = id;
            Name = name;
            NameEn = nameEn;
            Latitude = latitude;
            Longitude = longitude;
            Radius = radius;
            Priority = priority;
            Description = description;
            DescriptionEn = descriptionEn;
            ImageAsset = imageAsset;
            MapLink = mapLink;
            NarrationType = narrationType;
            Content = content;
        }
    }
}
