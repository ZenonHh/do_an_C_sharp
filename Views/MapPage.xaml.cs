using Microsoft.Maui.Controls;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Projections;

namespace DoAnCSharp.Views;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();

        // Gọi hàm setup bản đồ ngay tại đây
        SetupMap();
    }

    private void SetupMap()
    {
        // 1. TUYỆT ĐỐI KHÔNG TẠO "new Mapsui.Map()" NỮA.
        // Dùng thẳng thuộc tính Map có sẵn của foodMapView và thêm lớp ảnh OpenStreetMap vào
        foodMapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer("VinhKhanhFoodTourApp"));

        // 2. Tính toán tọa độ TP.HCM
        var lonLat = SphericalMercator.FromLonLat(106.6953, 10.7769);
        var hoChiMinhLocation = new MPoint(lonLat.x, lonLat.y);

        // 3. Cài đặt góc nhìn trung tâm
        foodMapView.Map.Home = n =>
        {
            // Mức thu phóng (Resolution) = 5 để thấy rõ từng con đường
            n.CenterOnAndZoomTo(hoChiMinhLocation, 5);
        };

        // 4. Đảm bảo bật dấu chấm định vị máy ảo
        foodMapView.MyLocationEnabled = true;
    }
}