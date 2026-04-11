using DoAnCSharp.Services;
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class ProfilePage : ContentPage
{
    public ILanguageService Lang { get; }

    public ProfilePage(ProfileViewModel viewModel, ILanguageService langService)
    {
        InitializeComponent();
        Lang = langService;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            // Cập nhật lại từ điển Lang để đồng bộ ngôn ngữ
            OnPropertyChanged(nameof(Lang));

            // Tải lại dữ liệu người dùng để tránh bị mất thông tin khi chuyển trang/đổi ngôn ngữ
            if (BindingContext is ProfileViewModel viewModel)
            {
                await viewModel.LoadUserProfileAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in ProfilePage.OnAppearing: {ex}");
            await DisplayAlert("Lỗi", "Không thể tải thông tin người dùng", "OK");
        }
    }

    private async void OnHistoryTapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("HistoryPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnHistoryTapped: {ex}");
            await DisplayAlert("Lỗi", "Không thể mở lịch sử", "OK");
        }
    }

    private async void OnEditProfileTapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("EditProfilePage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnEditProfileTapped: {ex}");
            await DisplayAlert("Lỗi", "Không thể mở trang chỉnh sửa", "OK");
        }
    }

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert(Lang["logout"], "Confirm logout?", "Yes", "No");
            if (confirm)
            {
                Preferences.Default.Remove("CurrentUserEmail");
                await Shell.Current.GoToAsync("//AuthPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnLogoutTapped: {ex}");
            await DisplayAlert("Lỗi", "Không thể đăng xuất", "OK");
        }
    }
}