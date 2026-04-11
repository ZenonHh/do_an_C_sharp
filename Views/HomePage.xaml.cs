#nullable disable
using DoAnCSharp.Services;
using DoAnCSharp.Models;
using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;

namespace DoAnCSharp.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;
    private readonly ILanguageService _lang;

    public HomePage(HomeViewModel viewModel, ILanguageService lang)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _lang = lang;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadDataAsync();
            UpdateUI_Language();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in HomePage.OnAppearing: {ex}");
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu trang chủ: " + ex.Message, "OK");
        }
    }

    private void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        _viewModel.FilterList(e.NewTextValue);
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedBtn)
        {
            BtnPhoBien.BackgroundColor = Colors.White; BtnPhoBien.TextColor = Color.FromArgb("#2C3E50");
            BtnQuanOc.BackgroundColor = Colors.White; BtnQuanOc.TextColor = Color.FromArgb("#2C3E50");
            BtnLauNuong.BackgroundColor = Colors.White; BtnLauNuong.TextColor = Color.FromArgb("#2C3E50");

            clickedBtn.BackgroundColor = Color.FromArgb("#FF6B6B");
            clickedBtn.TextColor = Colors.White;

            _viewModel.FilterList(clickedBtn.Text);
        }
    }

    private async void OnPoiTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Models.AudioPOI selectedPoi)
        {
            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedPOI", selectedPoi }
            };
            
            // Chuyển tab sang trang Map (MapTab là tên Route bạn đặt trong AppShell)
            await Shell.Current.GoToAsync($"//MapTab", navigationParameter);
        }
    }

    private void OnChangeLangClicked(object sender, TappedEventArgs e)
    {
        string newLang = _lang.CurrentLocale == "vi" ? "en" : "vi";
        _lang.ChangeLanguage(newLang);
        _viewModel.FilterList(SearchEntry.Text);
    }

    // ==========================================
    // LOGIC GIAO DIỆN LỊCH SỬ TÌM KIẾM
    // ==========================================
    private void OnSearchFocused(object sender, FocusEventArgs e)
    {
        _viewModel.LoadSearchHistory();
        if (_viewModel.SearchHistory.Count > 0)
            _viewModel.IsSearchHistoryVisible = true;
    }

    private void OnSearchUnfocused(object sender, FocusEventArgs e)
    {
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () => {
            _viewModel.IsSearchHistoryVisible = false;
        });
    }

    private void OnSearchCompleted(object sender, EventArgs e)
    {
        _viewModel.AddSearchHistory(SearchEntry.Text);
        SearchEntry.Unfocus();
    }

    private void OnSearchHistoryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string query)
        {
            SearchEntry.Text = query;
            _viewModel.AddSearchHistory(query);
            SearchEntry.Unfocus();
        }
    }
private void UpdateUI_Language()
{
    // Giả sử HomePage của bạn có inject _langService, nếu chưa có thì lấy qua ServiceHelper hoặc App.Current
    var langService = IPlatformApplication.Current.Services.GetService<ILanguageService>();
    string lang = langService.CurrentLocale;

    if (lang == "en")
    {
        SearchEntry.Placeholder = "Search for restaurants, hotpot...";
        BtnPhoBien.Text = "🔥 Popular";
        BtnQuanOc.Text = "🐌 Snail Places";
        BtnLauNuong.Text = "🍲 Hotpot & Grill";
    }
    else if (lang == "ja")
    {
        SearchEntry.Placeholder = "レストランや鍋を検索...";
        BtnPhoBien.Text = "🔥 人気";
        BtnQuanOc.Text = "🐌 カタツムリ店";
        BtnLauNuong.Text = "🍲 鍋と焼き肉";
    }
    else if (lang == "ko")
    {
        SearchEntry.Placeholder = "식당, 전골 검색...";
        BtnPhoBien.Text = "🔥 인기";
        BtnQuanOc.Text = "🐌 달팽이 식당";
        BtnLauNuong.Text = "🍲 전골 및 구이";
    }
    else // vi
    {
        SearchEntry.Placeholder = "Tìm tên quán, lẩu, ốc...";
        BtnPhoBien.Text = "🔥 Phổ biến";
        BtnQuanOc.Text = "🐌 Quán Ốc";
        BtnLauNuong.Text = "🍲 Lẩu & Nướng";
    }
    
    // NẾU ViewModel của HomePage có chứa dữ liệu cần dịch, hãy gọi nó tải lại:
    // var vm = BindingContext as HomeViewModel;
    // vm?.RefreshLanguage(); 
}
}