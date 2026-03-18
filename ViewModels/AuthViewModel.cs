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
        // 1. Kiểm tra dữ liệu rỗng
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            // Chú ý: Đang ở AuthPage (không có Shell) nên phải dùng Application.Current.MainPage để hiện thông báo
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Thông báo", "Vui lòng nhập email và mật khẩu!", "OK");
            }
            return;
        }

        // 2. Lưu trạng thái đăng nhập (nếu có dùng AuthService)
        if (_authService != null)
        {
            await _authService.SetLoggedInAsync(true);
        }

        // 3. CHUYỂN GIAO DIỆN: Nạp khung AppShell (có chứa TabBar)
        if (Application.Current != null)
        {
            Application.Current.MainPage = new AppShell();

            // 4. Ép thanh TabBar tự động nhảy sang Tab Bản đồ ngay lập tức
            // Lưu ý phải có "//" ở trước tên Route để nó nhảy đúng cấp độ Tab
            await Shell.Current.GoToAsync("//MapTab");
        }
    }
}
