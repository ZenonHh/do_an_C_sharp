using System.Collections.Generic; // Bắt buộc phải có để dùng List
using DoAnCSharp.Models;

namespace DoAnCSharp.Services;

public interface IPoiRepository
{
    List<POI> GetTourPoints();
}

public class PoiRepository : IPoiRepository
{
    public List<POI> GetTourPoints()
    {
        // Sử dụng Target-typed new để code gọn hơn (Tính năng C# mới)
        return new()
        {
            new POI
            {
                Id = "oc_oanh",
                Name = "Ốc Oanh - 534 Vĩnh Khánh",
                NameEn = "Oc Oanh Snail Restaurant",
                Latitude = 10.7583, // Sẽ hết lỗi sau khi sửa POI
                Longitude = 106.7065,
                Radius = 15, // Tăng nhẹ bán kính để GPS 8GB RAM bắt nhạy hơn
                Priority = 1,
                Description = "Quán ốc nổi tiếng nhất phố Vĩnh Khánh, nổi tiếng với món ốc hương rang muối ớt.",
                DescriptionEn = "The most famous snail restaurant on Vinh Khanh Street.",
                ImageAsset = "dotnet_bot.png",
                MapLink = "https://maps.app.goo.gl/oc_oanh_link",
                NarrationType = NarrationType.Tts
            },
            new POI
            {
                Id = "oc_dao_2",
                Name = "Ốc Đào 2",
                NameEn = "Oc Dao 2 Snail",
                Latitude = 10.7581,
                Longitude = 106.7061,
                Radius = 15,
                Priority = 1,
                Description = " Chi nhánh nổi tiếng của Ốc Đào.",
                DescriptionEn = "A famous branch of Oc Dao with authentic Saigon flavors.",
                ImageAsset = "oc_dao.jpg",
                MapLink = "https://maps.app.goo.gl/oc_dao_link",
                NarrationType = NarrationType.Tts
            },
            new POI
            {
                Id = "oc_vu",
                Name = "Ốc Vũ",
                NameEn = "Oc Vu Restaurant",
                Latitude = 10.7578,
                Longitude = 106.7058,
                Radius = 12,
                Priority = 2,
                Description = "Quán ăn yêu thích của giới trẻ, nổi bật với món ốc mỡ xào bơ.",
                DescriptionEn = "A favorite spot for youngsters, famous for butter-sauteed snails.",
                ImageAsset = "oc_vu.jpg",
                MapLink = "https://maps.app.goo.gl/oc_vu_link",
                NarrationType = NarrationType.Tts
            }
        };
    }
}
