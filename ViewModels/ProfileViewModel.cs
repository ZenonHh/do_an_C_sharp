using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly IServiceProvider _serviceProvider;

    // ĐÂY CHÍNH LÀ BIẾN "Lang" ĐỂ GIAO DIỆN LẤY CHỮ DỊCH
    public ILanguageService Lang { get; }

    [ObservableProperty]
    private User? _currentUser;

    // Tiêm các Service vào
    public ProfileViewModel(DatabaseService dbService, IServiceProvider serviceProvider, ILanguageService langService)
    {
        _dbService = dbService;
        _serviceProvider = serviceProvider;
        Lang = langService; // Gán giá trị để XAML có thể dùng
    }

    public async Task LoadUserProfileAsync()
    {
        CurrentUser = await _dbService.GetCurrentUserAsync();
    }

    [RelayCommand]
    private void Logout()
    {
        if (Application.Current != null)
        {
            var authPage = _serviceProvider.GetService(typeof(Views.AuthPage)) as Views.AuthPage;
            Application.Current.MainPage = authPage;
        }
    }

    [RelayCommand]
    private async Task EditProfile()
    {
        if (Application.Current?.MainPage != null)
            await Application.Current.MainPage.DisplayAlert("Thông báo", "Chức năng chỉnh sửa đang phát triển!", "OK");
    }
    [RelayCommand]
    private async Task ChangeLanguage()
    {
        if (Application.Current?.MainPage != null)
        {
            // 1. Hiện bảng chọn 4 ngôn ngữ giống hệt bên trang Bản đồ
            string action = await Application.Current.MainPage.DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null, "Tiếng Việt", "English", "日本語", "한국어");
            
            // Nếu người dùng bấm Hủy hoặc ra ngoài thì bỏ qua
            if (string.IsNullOrEmpty(action) || action == "Hủy") return;

            // 2. Lấy mã ngôn ngữ
            string langCode = "vi";
            if (action == "English") langCode = "en";
            else if (action == "日本語") langCode = "ja";
            else if (action == "한국어") langCode = "ko";

            // 3. Ra lệnh cho LanguageService đổi ngôn ngữ TOÀN APP (Nó sẽ tự lưu vào Preferences)
            Lang.ChangeLanguage(langCode);
        }
    }
    [RelayCommand]
    private async Task OpenSettings()
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Thông báo", "Tính năng Cài đặt chung đang được phát triển!", "OK");
        }
    }

    public System.Collections.ObjectModel.ObservableCollection<PlayHistory> AudioHistoryList { get; set; } = new();

    public async Task LoadHistoryAsync()
    {
        var list = await _dbService.GetRecentPlayHistoryAsync();
        AudioHistoryList.Clear();
        foreach (var item in list)
        {
            AudioHistoryList.Add(item);
        }
    }
}