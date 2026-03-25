using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly DatabaseService _dbService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    public RegisterViewModel(DatabaseService dbService, IServiceProvider serviceProvider)
    {
        _dbService = dbService;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task Register()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Vui lòng nhập đầy đủ thông tin!", "OK");
            return;
        }

        // Gọi hàm Đăng ký ở DB
        bool isSuccess = await _dbService.RegisterUserAsync(FullName, Email, Password);

        if (isSuccess)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Thành công", "Đăng ký thành công! Mời bạn đăng nhập.", "OK");
                // Đăng ký xong tự động quay về trang Login
                Application.Current.MainPage = _serviceProvider.GetService(typeof(Views.AuthPage)) as Views.AuthPage;
            }
        }
        else
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Email này đã được sử dụng!", "OK");
        }
    }

    [RelayCommand]
    private void GoToLogin()
    {
        // Quay về trang Đăng nhập nếu bấm nút "Đã có tài khoản"
        if (Application.Current != null)
        {
            Application.Current.MainPage = _serviceProvider.GetService(typeof(Views.AuthPage)) as Views.AuthPage;
        }
    }
}