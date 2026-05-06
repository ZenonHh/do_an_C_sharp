using DoAnCSharp.ViewModels;
using DoAnCSharp.Services;

namespace DoAnCSharp.Views;

public partial class AuthPage : ContentPage
{
    private readonly DatabaseService _dbService;

    public AuthPage(AuthViewModel viewModel, DatabaseService dbService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _dbService = dbService;
    }

    private async void OnForgotPasswordTapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Navigation.PushModalAsync(new NavigationPage(new ForgotPasswordPage(_dbService)));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnForgotPasswordTapped: {ex}");
            await DisplayAlert("Lỗi", "Không thể mở trang quên mật khẩu", "OK");
        }
    }
}
