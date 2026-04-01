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
        await _viewModel.LoadDataAsync();
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
        // Nhận dữ liệu quán ăn và gửi sang Bản đồ
        if (e.Parameter is AudioPOI selectedPoi)
        {
            var navParams = new Dictionary<string, object>
            {
                { "SelectedPOI", selectedPoi }
            };
            await Shell.Current.GoToAsync("//MapTab", navParams);
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
}