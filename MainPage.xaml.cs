using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System.Collections.ObjectModel;

namespace DoAnCSharp;

public partial class MainPage : ContentPage
{
    private readonly ILanguageService _languageService;
    private readonly IPoiRepository _poiRepository;

    public ObservableCollection<POI> RecommendedPois { get; set; } = new();
    public ObservableCollection<POI> AllPois { get; set; } = new();

    // SỬA LỖI CS0103 & CS0120: Truyền service vào đây
    public MainPage(ILanguageService languageService, IPoiRepository poiRepository)
    {
        InitializeComponent();
        _languageService = languageService;
        _poiRepository = poiRepository;
        BindingContext = this;
        LoadData();
    }

    private void LoadData()
    {
        // Gọi qua _poiRepository thay vì gọi tên Class trực tiếp
        var data = _poiRepository.GetTourPoints();
        foreach (var item in data)
        {
            AllPois.Add(item);
            if (RecommendedPois.Count < 2) RecommendedPois.Add(item);
        }
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Ngôn ngữ", "Hủy", null, "Tiếng Việt", "English");
        if (action == "Tiếng Việt") _languageService.ChangeLanguage("vi");
        else if (action == "English") _languageService.ChangeLanguage("en");
    }
}