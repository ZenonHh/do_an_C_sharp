using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Media; // Phải có dòng này để gọi Text-To-Speech
using System;
using System.Linq; // Để dùng hàm FirstOrDefault tìm ngôn ngữ

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

    // ==========================================
    // THÊM MỚI Ở ĐÂY: Hàm xử lý Text-to-Speech
    // Không đụng chạm gì tới code cũ phía trên
    // ==========================================
    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Lấy danh sách các giọng đọc có sẵn
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            
            // 2. Cố gắng tìm giọng Tiếng Việt ("vi")
            var viLocale = locales.FirstOrDefault(l => l.Language == "vi");

            // 3. Cài đặt thông số giọng đọc
            SpeechOptions options = new SpeechOptions()
            {
                Volume = 1.0f,  
                Pitch = 1.0f,   
                Locale = viLocale 
            };

            // 4. Đoạn văn bản cần đọc (Sau này bạn có thể truyền biến Binding vào đây)
            string textToRead = "Chào mừng bạn đến với khu phố ẩm thực Vĩnh Khánh! Quán ốc Oanh nổi tiếng với món ốc hương xào bắp bơ siêu ngon.";
            
            // 5. Đọc thôi!
            await TextToSpeech.Default.SpeakAsync(textToRead, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi Text-to-Speech", ex.Message, "OK");
        }
    }
}