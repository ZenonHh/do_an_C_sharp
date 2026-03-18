using Microsoft.Maui.Controls;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Projections;
using System;

namespace DoAnCSharp.Views;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();

        // Khởi tạo bản đồ
        foodMapView.Map = CreateMap();
    }

    private Mapsui.Map CreateMap()
    {
        var map = new Mapsui.Map();

        // 1. Thêm bản đồ nền OpenStreetMap
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // 2. Lấy tọa độ dạng cặp số (Tuple)
        var lonLat = SphericalMercator.FromLonLat(106.6953, 10.7769);
        
        // 3. Gói nó vào MPoint để truyền cho bản đồ (Sửa lỗi CS1503 ở đây)
        var hoChiMinhLocation = new MPoint(lonLat.x, lonLat.y);
        
        // 4. Đặt vị trí trung tâm
        map.Home = n => 
        {
            n.CenterOn(hoChiMinhLocation);
        }; 

        return map;
    }
}