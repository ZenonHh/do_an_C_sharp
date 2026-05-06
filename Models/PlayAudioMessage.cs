namespace DoAnCSharp.Models;

/// <summary>
/// Message để báo cho các page biết rằng cần phát audio của một POI
/// </summary>
public class PlayAudioMessage
{
    public string PoiName { get; set; }
    public int PoiId { get; set; }

    public PlayAudioMessage(string poiName, int poiId)
    {
        PoiName = poiName;
        PoiId = poiId;
    }
}
