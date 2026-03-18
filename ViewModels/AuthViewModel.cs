using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using DoAnCSharp;

namespace DoAnCSharp.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    // SỬA: Dùng field private. 
    // Bộ Toolkit sẽ tự tạo ra Property "Email" và "Password" (viết hoa) cho bạn.
    [ObservableProperty]
#pragma warning disable MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT
    private string _email = string.Empty;
#pragma warning restore MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT

    [ObservableProperty]
#pragma warning disable MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT
    private string _password = string.Empty;
#pragma warning restore MVVMTK0045 // Using [ObservableProperty] on fields is not AOT compatible for WinRT

    public AuthViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login()
    {
        // Trong logic, bạn dùng tên VIẾT HOA (do bộ Toolkit tạo ra)
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert("Thông báo", "Vui lòng nhập email và mật khẩu!", "OK");
            return;
        }

        await _authService.SetLoggedInAsync(true);
        
        // Chuyển sang AppShell (Trang chính có TabBar)
        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();
        }
    }
}
