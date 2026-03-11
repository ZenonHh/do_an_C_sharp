using System.Collections.Generic;
using DoAnCSharp.Models;

namespace DoAnCSharp.Services
{
    public class POIRepository
    {
        public static List<POI> GetTourPoints()
        {
            return new List<POI>
            {
                new POI(
                    id: "oc_oanh",
                    name: "Ốc Oanh - 534 Vĩnh Khánh",
                    nameEn: "Oc Oanh Snail Restaurant",
                    latitude: 10.7583,
                    longitude: 106.7065,
                    radius: 12,
                    priority: 1,
                    description: "Quán ốc nổi tiếng nhất phố Vĩnh Khánh, nổi tiếng với món ốc hương rang muối ớt và các món xào me.",
                    descriptionEn: "The most famous snail restaurant on Vinh Khanh Street, renowned for its chili salt roasted snails and tamarind stir-fry dishes.",
                    imageAsset: "oc_oanh.jpg",
                    mapLink: "https://maps.app.goo.gl/9x8p8p8p8p8p8p8p",
                    narrationType: NarrationType.Tts
                ),
                new POI(
                    id: "oc_dao_2",
                    name: "Ốc Đào 2",
                    nameEn: "Oc Dao 2 Snail",
                    latitude: 10.7581,
                    longitude: 106.7061,
                    radius: 12,
                    priority: 1,
                    description: "Chi nhánh nổi tiếng của Ốc Đào, không gian rộng rãi, sạch sẽ, hương vị đậm đà đặc trưng Sài Gòn.",
                    descriptionEn: "A famous branch of Oc Dao, featuring a spacious and clean environment with authentic Saigon flavors.",
                    imageAsset: "oc_dao.jpg",
                    mapLink: "https://maps.app.goo.gl/axaxaxaxaxaxaxax",
                    narrationType: NarrationType.Tts
                ),
                new POI(
                    id: "oc_vu",
                    name: "Ốc Vũ",
                    nameEn: "Oc Vu Restaurant",
                    latitude: 10.7578,
                    longitude: 106.7058,
                    radius: 10,
                    priority: 2,
                    description: "Quán ăn yêu thích của giới trẻ với thực đơn đa dạng và giá cả phải chăng, nổi bật với món ốc mỡ xào bơ.",
                    descriptionEn: "A favorite spot for youngsters with a diverse menu and affordable prices, highlighted by their butter-sauteed snails.",
                    imageAsset: "oc_vu.jpg",
                    mapLink: "https://maps.app.goo.gl/yzyzyzyzyzyzyzyz",
                    narrationType: NarrationType.Tts
                )
            };
        }
    }
}
