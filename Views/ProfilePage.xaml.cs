using DoAnCSharp.Services;
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class ProfilePage : ContentPage
{
    public ILanguageService Lang { get; }
    private readonly ScanQuotaService _quotaService;

    public ProfilePage(ProfileViewModel viewModel, ILanguageService langService, ScanQuotaService quotaService)
    {
        InitializeComponent();
        Lang = langService;
        _quotaService = quotaService;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            OnPropertyChanged(nameof(Lang));
            if (BindingContext is ProfileViewModel viewModel)
                await viewModel.LoadUserProfileAsync();

            int remaining = _quotaService.GetRemaining();
            QuotaLabel.Text = remaining > 900
                ? "Không giới hạn lượt nghe"
                : $"Còn {remaining} lượt nghe";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in ProfilePage.OnAppearing: {ex}");
        }
    }

    private async void OnPaymentTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("PaymentPage");
    }

    private async void OnHistoryTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("HistoryPage");
    }

    private async void OnEditProfileTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("EditProfilePage");
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert(Lang["logout"] ?? "Đăng xuất", "Bạn có chắc chắn muốn đăng xuất?", "Đồng ý", "Hủy");
            if (confirm)
            {
                // Xóa phiên đăng nhập
                Preferences.Default.Remove("CurrentUserEmail");
                
                // Cập nhật lại Heartbeat thành guest
                var syncService = ServiceHelper.GetService<AdminSyncService>();
                syncService?.StartHeartbeat("guest");

                // Load lại thông tin -> ViewModel sẽ chuyển sang trạng thái "Chưa đăng nhập" ngay lập tức
                if (BindingContext is ProfileViewModel viewModel)
                {
                    await viewModel.LoadUserProfileAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in OnLogoutTapped: {ex}");
        }
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
{
    bool confirm = await DisplayAlert("Đăng xuất", "Bạn có chắc muốn đăng xuất?", "Có", "Không");
    if (confirm)
    {
        Preferences.Default.Remove("CurrentUserEmail");
        
        // Cập nhật lại Heartbeat thành guest
        var syncService = ServiceHelper.GetService<AdminSyncService>();
        syncService?.StartHeartbeat("guest");

        if (BindingContext is ProfileViewModel viewModel)
        {
            await viewModel.LoadUserProfileAsync(); // Lệnh này sẽ ẩn profile và hiện nút Login
        }
    }
}
}