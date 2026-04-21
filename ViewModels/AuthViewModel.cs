using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using DoAnCSharp;
using System; 
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

    // Phải nhận cả 3 công cụ vào Constructor
    public AuthViewModel(IAuthService authService, DatabaseService dbService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _dbService = dbService; 
        _serviceProvider = serviceProvider;
    }
    [RelayCommand]
    private async Task Login()
    {
        try
        {
            // 1. Kiểm tra dữ liệu rỗng
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng nhập email và mật khẩu!", "OK");
                return;
            }

            // ĐÃ SỬA: Chuẩn hóa email (xóa khoảng trắng thừa và đưa về chữ thường)
            string normalizedEmail = Email.Trim().ToLower();

            // 2. GỌI HÀM KIỂM TRA ĐĂNG NHẬP bằng email đã chuẩn hóa
            var user = await _dbService.LoginUserAsync(normalizedEmail, Password);

            if (user != null) // Nếu đúng tài khoản & Mật khẩu
            {
                // Lưu Email đã chuẩn hóa vào bộ nhớ để trang Cá nhân lấy được
                Microsoft.Maui.Storage.Preferences.Default.Set("CurrentUserEmail", normalizedEmail);

                // Lưu phiên đăng nhập
                if (_authService != null)
                {
                    await _authService.SetLoggedInAsync(true);
                }

                // 3. CHUYỂN GIAO DIỆN: Vào App chính
                if (Application.Current != null)
                {
                    // BẮT BUỘC: Ép hệ thống quay về Luồng chính (UI Thread) để vẽ giao diện
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var appShell = _serviceProvider.GetService(typeof(AppShell)) as AppShell;

                        if (appShell != null)
                        {
                            Application.Current.MainPage = appShell;
                        }
                    });
                }
            }
            else // Nếu sai Email hoặc Mật khẩu
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Lỗi", "Sai Email hoặc Mật khẩu!\nVui lòng kiểm tra lại.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in AuthViewModel.Login: {ex}");
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Lỗi", $"Đăng nhập thất bại: {ex.Message}", "OK");
        }
    }
    // Hàm để bấm nút "Đăng ký ngay" chuyển sang trang RegisterPage
    [RelayCommand]
    private void GoToRegister()
    {
        try
        {
            if (Application.Current != null)
            {
                // Lấy trang RegisterPage ra và chuyển qua
                var registerPage = _serviceProvider.GetService(typeof(Views.RegisterPage)) as Views.RegisterPage;
                if (registerPage != null)
                {
                    Application.Current.MainPage = registerPage;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: RegisterPage not found in service provider");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in GoToRegister: {ex}");
        }
    }
    // Lệnh để quay lại giao diện chính (Bản đồ)
    [RelayCommand]
private void GoBackToApp()
{
    if (Application.Current != null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Lấy lại bộ khung AppShell đã đăng ký trong hệ thống
            var appShell = _serviceProvider.GetService(typeof(AppShell)) as AppShell;
            if (appShell != null)
            {
                Application.Current.MainPage = appShell;
            }
        });
    }
}
}