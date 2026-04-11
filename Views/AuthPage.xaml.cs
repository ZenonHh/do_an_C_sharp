using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class AuthPage : ContentPage
{
    public AuthPage(AuthViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // Dùng Modal để tránh lỗi Navigation Stack khi chưa vào AppShell
            await Navigation.PushModalAsync(new NavigationPage(new ForgotPasswordPage()));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnForgotPasswordTapped: {ex}");
            await DisplayAlert("Lỗi", "Không thể mở trang quên mật khẩu", "OK");
        }
    }
}