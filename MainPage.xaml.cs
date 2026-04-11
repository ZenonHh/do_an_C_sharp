using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System.Collections.ObjectModel;
using Microsoft.Maui.Media;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnCSharp;

public partial class MainPage : ContentPage
{
    private readonly ILanguageService _languageService;
    private readonly DatabaseService _dbService;

    // Đổi toàn bộ POI thành AudioPOI
    public ObservableCollection<AudioPOI> RecommendedPois { get; set; } = new();
    public ObservableCollection<AudioPOI> AllPois { get; set; } = new();

    public MainPage(ILanguageService languageService, DatabaseService dbService)
    {
        InitializeComponent();
        _languageService = languageService;
        _dbService = dbService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var data = await _dbService.GetPOIsAsync();
        RecommendedPois.Clear();
        AllPois.Clear();
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

    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var viLocale = locales.FirstOrDefault(l => l.Language == "vi");
            SpeechOptions options = new SpeechOptions()
            {
                Volume = 1.0f,
                Pitch = 1.0f,
                Locale = viLocale
            };
            string textToRead = "Chào mừng bạn đến với khu phố ẩm thực Vĩnh Khánh!";
            await TextToSpeech.Default.SpeakAsync(textToRead, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi Text-to-Speech", ex.Message, "OK");
        }
    }

   
    
}