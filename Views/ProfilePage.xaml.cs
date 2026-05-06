using DoAnCSharp.Services;
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class ProfilePage : ContentPage
{
    public ILanguageService Lang { get; }
    private readonly ScanQuotaService _quotaService;
    private readonly AdminSyncService _syncService;

    public ProfilePage(ProfileViewModel viewModel, ILanguageService langService, ScanQuotaService quotaService, AdminSyncService syncService)
    {
        InitializeComponent();
        Lang = langService;
        _quotaService = quotaService;
        _syncService = syncService;
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

            var currentUrl = _syncService.ServerUrl;
            ServerUrlEntry.Text = currentUrl;
            ServerUrlStatus.Text = $"Hiện tại: {currentUrl}";
            ServerUrlEntryGuest.Text = currentUrl;
            ServerUrlStatusGuest.Text = $"Hiện tại: {currentUrl}";
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
            var syncService = ServiceHelper.GetService<AdminSyncService>();
            syncService?.StartHeartbeat("guest");
            if (BindingContext is ProfileViewModel viewModel)
            {
                await viewModel.LoadUserProfileAsync();
            }
        }
    }

    private void OnSaveServerUrlClicked(object sender, EventArgs e)
    {
        SaveServerUrl(ServerUrlEntry.Text, ServerUrlStatus);
    }

    private void OnSaveServerUrlGuestClicked(object sender, EventArgs e)
    {
        SaveServerUrl(ServerUrlEntryGuest.Text, ServerUrlStatusGuest);
    }

    private void SaveServerUrl(string? url, Label statusLabel)
    {
        url = url?.Trim();
        if (string.IsNullOrEmpty(url))
        {
            statusLabel.Text = "URL không được để trống!";
            statusLabel.TextColor = Colors.Red;
            return;
        }
        _syncService.ServerUrl = url;
        ServerUrlEntry.Text = url;
        ServerUrlEntryGuest.Text = url;
        ServerUrlStatus.Text = $"Đã lưu: {url}";
        ServerUrlStatus.TextColor = Color.FromArgb("#27AE60");
        ServerUrlStatusGuest.Text = $"Đã lưu: {url}";
        ServerUrlStatusGuest.TextColor = Color.FromArgb("#27AE60");
    }
}