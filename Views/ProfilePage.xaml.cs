using DoAnCSharp.ViewModels;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUserProfileAsync();
    }
    private async void OnHistoryTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("HistoryPage");
    }
    private async void OnEditProfileTapped(object sender, TappedEventArgs e)
    {
    // Mở trang chỉnh sửa thông tin
        await Navigation.PushAsync(new EditProfilePage());
    }
}