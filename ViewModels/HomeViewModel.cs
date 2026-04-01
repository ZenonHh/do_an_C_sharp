using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using Microsoft.Maui.Devices.Sensors;
using System;

namespace DoAnCSharp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private List<AudioPOI> _originalPois = new();

    public ILanguageService Lang { get; }

    public ObservableCollection<AudioPOI> RecommendedPois { get; set; } = new();
    public ObservableCollection<AudioPOI> AllPois { get; set; } = new();

    [ObservableProperty]
    private bool _isRecommendedVisible = true;

    [ObservableProperty]
    private string _searchResultTitle = "Tất cả quán ăn";

    public HomeViewModel(DatabaseService dbService, ILanguageService languageService)
    {
        _dbService = dbService;
        Lang = languageService;
    }

    public async Task LoadDataAsync()
    {
        var data = await _dbService.GetPOIsAsync();
        if (data != null)
        {
            _originalPois = data;
            // Gọi hàm tính khoảng cách khi vừa load xong dữ liệu
            await CalculateDistancesAsync();
        }
        FilterList("");
    }

    // HÀM TÍNH TOÁN KHOẢNG CÁCH VÀ THỜI GIAN ĐI BỘ
    private async Task CalculateDistancesAsync()
    {
        try
        {
            // 1. Lấy vị trí hiện tại của người dùng
            var userLocation = await Geolocation.Default.GetLastKnownLocationAsync();
            if (userLocation == null)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(2));
                userLocation = await Geolocation.Default.GetLocationAsync(request);
            }

            // 2. Tính khoảng cách cho từng quán ăn
            if (userLocation != null)
            {
                foreach (var poi in _originalPois)
                {
                    double distanceKm = Location.CalculateDistance(userLocation, poi.Lat, poi.Lng, DistanceUnits.Kilometers);

                    // Hiển thị mét nếu < 1km, ngược lại hiển thị km
                    string distStr = distanceKm < 1 ? $"{(int)(distanceKm * 1000)}m" : $"{Math.Round(distanceKm, 1)}km";

                    // Vận tốc đi bộ trung bình ~5km/h (1km tốn 12 phút)
                    int walkMinutes = (int)(distanceKm * 12);
                    if (walkMinutes < 1) walkMinutes = 1;

                    poi.DistanceInfo = $"📍 {distStr}  •  🚶 {walkMinutes} phút";
                }
            }
        }
        catch
        {
            // Nếu người dùng từ chối cấp quyền GPS
            foreach (var poi in _originalPois)
            {
                poi.DistanceInfo = "📍 Chưa có định vị";
            }
        }
    }

    public void FilterList(string query)
    {
        query = query?.ToLower() ?? "";
        RecommendedPois.Clear();
        AllPois.Clear();

        if (string.IsNullOrWhiteSpace(query) || query == "phổ biến")
        {
            IsRecommendedVisible = true;
            SearchResultTitle = Lang.CurrentLocale == "vi" ? "Tất cả quán ăn" : "All restaurants";

            var topPois = _originalPois.OrderByDescending(p => p.Priority).Take(2).ToList();
            foreach (var item in topPois) RecommendedPois.Add(item);

            var otherPois = _originalPois.Take(3).ToList();
            foreach (var item in otherPois) AllPois.Add(item);
        }
        else
        {
            IsRecommendedVisible = false;
            SearchResultTitle = Lang.CurrentLocale == "vi" ? $"Kết quả tìm kiếm cho '{query}'" : $"Search results for '{query}'";

            var filtered = _originalPois.Where(p =>
                (p.Name != null && p.Name.ToLower().Contains(query)) ||
                (p.Description != null && p.Description.ToLower().Contains(query)) ||
                (query.Contains("ốc") && p.Name != null && p.Name.ToLower().Contains("ốc")) ||
                (query.Contains("lẩu") && p.Name != null && p.Name.ToLower().Contains("lẩu"))
            ).ToList();

            foreach (var item in filtered) AllPois.Add(item);
        }
    }
}