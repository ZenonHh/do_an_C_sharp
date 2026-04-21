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
    public ILanguageService Lang { get; }

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoggedIn))]
    private bool _isLoggedIn;

    public bool IsNotLoggedIn => !IsLoggedIn;

    public ProfileViewModel(DatabaseService dbService, IServiceProvider serviceProvider, ILanguageService langService)
    {
        _dbService = dbService;
        _serviceProvider = serviceProvider;
        Lang = langService; 
    }

    public async Task LoadUserProfileAsync()
    {
        // Kiểm tra Preferences để xem có ai đang đăng nhập không
        var email = Preferences.Default.Get("CurrentUserEmail", string.Empty);
        
        if (string.IsNullOrEmpty(email))
        {
            IsLoggedIn = false;
            CurrentUser = null;
        }
        else
        {
            CurrentUser = await _dbService.GetCurrentUserAsync();
            IsLoggedIn = CurrentUser != null;
        }
    }

    [RelayCommand]
    private void GoToLogin()
    {
        if (Application.Current != null)
        {
            // Chuyển sang trang Đăng nhập
            var authPage = _serviceProvider.GetService(typeof(Views.AuthPage)) as Views.AuthPage;
            Application.Current.MainPage = authPage;
        }
    }

    [RelayCommand]
    private void GoToRegister()
    {
        if (Application.Current != null)
        {
            // Chuyển sang trang Đăng ký
            var registerPage = _serviceProvider.GetService(typeof(Views.RegisterPage)) as Views.RegisterPage;
            Application.Current.MainPage = registerPage;
        }
    }

    // Command này có thể dùng nếu bạn gọi trực tiếp từ Binding
    [RelayCommand]
    private async Task Logout()
    {
        Preferences.Default.Remove("CurrentUserEmail");
        await LoadUserProfileAsync(); // Cập nhật lại giao diện tại chỗ
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
            string action = await Application.Current.MainPage.DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null, "Tiếng Việt", "English", "日本語", "한국어");
            if (string.IsNullOrEmpty(action) || action == "Hủy") return;

            string langCode = "vi";
            if (action == "English") langCode = "en";
            else if (action == "日本語") langCode = "ja";
            else if (action == "한국어") langCode = "ko";

            Lang.ChangeLanguage(langCode);
        }
    }
}