#nullable disable
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using Microsoft.Maui.Devices.Sensors;

namespace DoAnCSharp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private List<AudioPOI> _originalPois = new();
    private string _lastQuery = ""; 

    public ILanguageService Lang { get; }

    [ObservableProperty] private string _userName = "Khách";
    [ObservableProperty] private string _userImage = "dotnet_bot.png";
    [ObservableProperty] private string _welcomeMessage = "";
    [ObservableProperty] private bool _isRecommendedVisible = true;
    [ObservableProperty] private string _searchResultTitle = "";
    [ObservableProperty] private bool _isSearchHistoryVisible = false;

    public ObservableCollection<AudioPOI> RecommendedPois { get; set; } = new();
    public ObservableCollection<AudioPOI> AllPois { get; set; } = new();
    public ObservableCollection<string> SearchHistory { get; set; } = new();

    public HomeViewModel(DatabaseService dbService, ILanguageService languageService)
    {
        _dbService = dbService;
        Lang = languageService;

        Lang.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "Item" || e.PropertyName == nameof(Lang.CurrentLocale))
            {
                try
                {
                    WelcomeMessage = $"{Lang["welcome"]} {UserName}!";
                    if (_originalPois != null && _originalPois.Count > 0)
                    {
                        TranslatePois();
                        FilterList(_lastQuery ?? "");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR in HomeViewModel language change: {ex}");
                }
            }
        };
    }

    public async Task LoadDataAsync()
    {
        try
        {
            var currentUser = await _dbService.GetCurrentUserAsync();
            UserName = currentUser != null ? (string.IsNullOrWhiteSpace(currentUser.FullName) ? currentUser.Email : currentUser.FullName) : "Khách";
            UserImage = currentUser?.Avatar ?? "dotnet_bot.png";
            WelcomeMessage = $"{Lang["welcome"]} {UserName}!";

            var data = await _dbService.GetPOIsAsync();
            if (data != null && data.Count > 0)
            {
                _originalPois = data;
                TranslatePois();
                
                try
                {
                    // Yêu cầu cấp quyền GPS nếu chưa có
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        await CalculateDistancesAsync();
                    }
                    else
                    {
                        foreach (var poi in _originalPois) poi.DistanceInfo = "📍 Chưa cấp quyền vị trí";
                    }
                }
                catch
                {
                    foreach (var poi in _originalPois) poi.DistanceInfo = "📍 Lỗi định vị";
                }
                
                FilterList("");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in HomeViewModel.LoadDataAsync: {ex}");
        }
    }

    // ĐÂY LÀ HÀM BẠN BỊ XÓA NHẦM - TÔI ĐÃ THÊM LẠI VÀO ĐÂY
    private void TranslatePois()
    {
        if (_originalPois == null) return;
        foreach (var poi in _originalPois)
        {
            poi.DisplayName = Lang[poi.Name];
            poi.DisplayDescription = Lang[poi.Description];
        }
    }

    private async Task CalculateDistancesAsync()
    {
        try
        {
            // Tăng thời gian chờ lên 5 giây để máy ảo kịp bắt GPS
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
            var userLocation = await Geolocation.Default.GetLastKnownLocationAsync() ?? 
                               await Geolocation.Default.GetLocationAsync(request);
            
            if (_originalPois != null)
            {
                foreach (var poi in _originalPois)
                {
                    if (userLocation != null)
                    {
                        double dist = Location.CalculateDistance(userLocation, poi.Lat, poi.Lng, DistanceUnits.Kilometers);
                        string distStr = dist < 1 ? $"{(int)(dist * 1000)}m" : $"{Math.Round(dist, 1)}km";
                        poi.DistanceInfo = $"📍 {distStr}  •  🚶 {(int)(dist * 12 + 1)} {Lang["mins"]}";
                    }
                    else
                    {
                        poi.DistanceInfo = "📍 Đang tìm GPS...";
                    }
                }
            }
        }
        catch (Exception)
        {
            if (_originalPois != null)
            {
                foreach (var poi in _originalPois) poi.DistanceInfo = "📍 Tắt GPS";
            }
        }
    }

    public void FilterList(string query)
    {
        _lastQuery = query;
        query = query?.ToLower() ?? "";
        RecommendedPois.Clear(); AllPois.Clear();

        if (string.IsNullOrWhiteSpace(query) || query == Lang["popular_btn"].ToLower() || query.Contains("phổ biến"))
        {
            IsRecommendedVisible = true; SearchResultTitle = Lang["all"];
            var top = _originalPois.OrderByDescending(p => p.Priority).Take(2).ToList();
            foreach (var item in top) RecommendedPois.Add(item);
            foreach (var item in _originalPois.Where(p => !top.Contains(p))) AllPois.Add(item);
        }
        else
        {
            IsRecommendedVisible = false; SearchResultTitle = $"{Lang["search"]}: '{query}'";
            var filtered = _originalPois.Where(p => 
                (p.DisplayName?.ToLower().Contains(query) ?? false) || 
                (p.DisplayDescription?.ToLower().Contains(query) ?? false)).ToList();
            foreach (var item in filtered) AllPois.Add(item);
        }
    }

    public void LoadSearchHistory() { /* Logic cũ của bạn */ }
    public void AddSearchHistory(string q) { /* Logic cũ của bạn */ }
}