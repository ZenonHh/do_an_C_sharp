#nullable disable
using DoAnCSharp.Services;
using DoAnCSharp.Models;
using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;
using System;

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

    // Thêm Microsoft.Maui.Controls. vào trước TextChangedEventArgs
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

    // ĐÃ SỬA CHUẨN CHỮ KÝ SỰ KIỆN TAPPED
    private async void OnPoiTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//MapTab");
    }

    // ĐÃ SỬA CHUẨN CHỮ KÝ SỰ KIỆN TAPPED
    private void OnChangeLangClicked(object sender, TappedEventArgs e)
    {
        string newLang = _lang.CurrentLocale == "vi" ? "en" : "vi";
        _lang.ChangeLanguage(newLang);

        _viewModel.FilterList(SearchEntry.Text);
    }
}