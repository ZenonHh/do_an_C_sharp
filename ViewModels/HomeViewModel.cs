using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAnCSharp.Models;
using DoAnCSharp.Services;

namespace DoAnCSharp.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private List<AudioPOI> _originalPois = new();

    // Biến quản lý Ngôn Ngữ để XAML lấy chữ ra dùng
    public ILanguageService Lang { get; }

    public ObservableCollection<AudioPOI> RecommendedPois { get; set; } = new();
    public ObservableCollection<AudioPOI> AllPois { get; set; } = new();

    [ObservableProperty]
    private bool _isRecommendedVisible = true;

    [ObservableProperty]
    private string _searchResultTitle = "Tất cả quán ăn";

    // Tiêm cả 2 dịch vụ vào đây
    public HomeViewModel(DatabaseService dbService, ILanguageService languageService)
    {
        _dbService = dbService;
        Lang = languageService; // Khởi tạo biến Lang
    }

    public async Task LoadDataAsync()
    {
        var data = await _dbService.GetPOIsAsync();
        if (data != null) _originalPois = data;
        FilterList("");
    }

    public void FilterList(string query)
    {
        query = query?.ToLower() ?? "";
        RecommendedPois.Clear();
        AllPois.Clear();

        if (string.IsNullOrWhiteSpace(query) || query == "phổ biến")
        {
            IsRecommendedVisible = true;
            // Thay đổi chữ linh hoạt theo ngôn ngữ
            SearchResultTitle = Lang.CurrentLocale == "vi" ? "Tất cả quán ăn" : "All restaurants";

            var topPois = _originalPois.OrderByDescending(p => p.Priority).Take(2).ToList();
            foreach (var item in topPois) RecommendedPois.Add(item);

            var otherPois = _originalPois.Take(3).ToList();
            foreach (var item in otherPois) AllPois.Add(item);
        }
        else
        {
            IsRecommendedVisible = false;
            // Thay đổi chữ linh hoạt theo ngôn ngữ
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