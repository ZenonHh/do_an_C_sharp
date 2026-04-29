using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DoAnCSharp.ViewModels;

public partial class PaymentViewModel : ObservableObject
{
    private readonly ScanQuotaService _quotaService;
    public ILanguageService Lang { get; }

    [ObservableProperty]
    private int _remainingListens;

    [ObservableProperty]
    private bool _isProcessing = false;

    public PaymentViewModel(ScanQuotaService quotaService, ILanguageService langService)
    {
        _quotaService = quotaService;
        Lang = langService;
    }

    public void Refresh()
        => RemainingListens = _quotaService.GetRemaining();

    [RelayCommand]
    private async Task BuyPackage(string packageId)
    {
        string currentEmail = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "");
        if (string.IsNullOrEmpty(currentEmail) || currentEmail == "guest")
        {
            await Application.Current!.MainPage!.DisplayAlert("Yêu cầu đăng nhập", "Bạn cần đăng nhập tài khoản để có thể mua thêm lượt nghe!", "OK");
            var authPage = ServiceHelper.GetService<Views.AuthPage>();
            if (authPage != null)
                Application.Current.MainPage = authPage;
            return;
        }

        (string name, int listens, string price) = packageId switch
        {
            "basic"   => ("Gói Cơ Bản",   5,    "5.000đ"),
            "premium" => ("Gói Cao Cấp",  20,   "15.000đ"),
            "vip"     => ("Gói VIP",      999,  "50.000đ"),
            _         => ("", 0, "")
        };

        if (listens == 0) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Xác nhận thanh toán",
            $"{name}\n+{(listens == 999 ? "Không giới hạn" : listens.ToString())} lượt nghe\nGiá: {price}",
            "Thanh toán", "Hủy");

        if (!confirm) return;

        IsProcessing = true;

        await Task.Delay(2000);

        _quotaService.AddListens(listens);
        RemainingListens = _quotaService.GetRemaining();

        try
        {
            var dbService = ServiceHelper.GetService<DatabaseService>();
            var syncService = ServiceHelper.GetService<AdminSyncService>();

            if (dbService != null && syncService != null)
            {
                var user = await dbService.GetUserByEmailAsync(currentEmail);
                string fullName = user?.FullName ?? "Người dùng App";
                decimal amount = packageId == "vip" ? 50000 : (packageId == "premium" ? 15000 : 5000);
                await syncService.SyncPaymentToServerAsync(currentEmail, fullName, packageId, amount);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Lỗi đồng bộ thanh toán: {ex.Message}");
        }

        IsProcessing = false;

        await Application.Current!.MainPage!.DisplayAlert(
            "✅ Thành công",
            $"Thanh toán {price} thành công!\nBạn có {RemainingListens} lượt nghe.",
            "OK");
    }

#if DEBUG
    [RelayCommand]
    private void DevReset()
    {
        _quotaService.ResetToFree();
        RemainingListens = _quotaService.GetRemaining();
    }
#endif
}
