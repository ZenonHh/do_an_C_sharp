using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using DoAnCSharp;
using System; // Bắt buộc phải có using System để dùng IServiceProvider
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly DatabaseService _dbService;
    private readonly IServiceProvider _serviceProvider; // Khai báo công cụ chuyển trang

    [ObservableProperty]
#pragma warning disable MVVMTK0045
    private string _email = string.Empty;
#pragma warning restore MVVMTK0045

    [ObservableProperty]
#pragma warning disable MVVMTK0045
    private string _password = string.Empty;
#pragma warning restore MVVMTK0045

    // SỬA: Phải nhận cả 3 công cụ vào Constructor
    public AuthViewModel(IAuthService authService, DatabaseService dbService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _dbService = dbService; 
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task Login()
    {
        // 1. Kiểm tra dữ liệu rỗng
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng nhập email và mật khẩu!", "OK");
            return;
        }

        // 2. GỌI HÀM KIỂM TRA ĐĂNG NHẬP (Kiểm tra khớp cả Email lẫn Password trong SQLite)
        var user = await _dbService.LoginUserAsync(Email, Password);

        if (user != null) // Nếu đúng tài khoản & Mật khẩu
        {
            // Lưu Email vào bộ nhớ để trang Cá nhân lấy được
            Microsoft.Maui.Storage.Preferences.Default.Set("CurrentUserEmail", Email);

            // Lưu phiên đăng nhập
            if (_authService != null)
            {
                await _authService.SetLoggedInAsync(true);
            }

            // CHUYỂN GIAO DIỆN: Vào App chính
            if (Application.Current != null)
            {
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//MapTab");
            }
        }
        else // Nếu sai Email hoặc Mật khẩu
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Lỗi", "Sai Email hoặc Mật khẩu!\nVui lòng kiểm tra lại.", "OK");
        }
    }

    // THÊM: Hàm để bấm nút "Đăng ký ngay" chuyển sang trang RegisterPage
    [RelayCommand]
    private void GoToRegister()
    {
        if (Application.Current != null)
        {
            // Lấy trang RegisterPage ra và chuyển qua
            var registerPage = _serviceProvider.GetService(typeof(Views.RegisterPage)) as Views.RegisterPage;
            if (registerPage != null)
            {
                Application.Current.MainPage = registerPage;
            }
        }
    }
}