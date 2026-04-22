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
    private int _remainingScans;

    [ObservableProperty]
    private bool _isProcessing = false;

    public PaymentViewModel(ScanQuotaService quotaService, ILanguageService langService)
    {
        _quotaService = quotaService;
        Lang = langService;
    }

    public void Refresh()
        => RemainingScans = _quotaService.GetRemaining();

    [RelayCommand]
    private async Task BuyPackage(string packageId)
    {
        // 1. KIỂM TRA ĐĂNG NHẬP
        string currentEmail = Microsoft.Maui.Storage.Preferences.Default.Get("CurrentUserEmail", "");
        if (string.IsNullOrEmpty(currentEmail) || currentEmail == "guest")
        {
            await Application.Current!.MainPage!.DisplayAlert("Yêu cầu đăng nhập", "Bạn cần đăng nhập tài khoản để có thể mua thêm lượt quét!", "OK");
            var authPage = ServiceHelper.GetService<Views.AuthPage>();
            if (authPage != null)
            {
                Application.Current.MainPage = authPage;
            }
            return;
        }

        (string name, int scans, string price) = packageId switch
        {
            "basic"   => ("Gói Cơ Bản",   5,    "5.000đ"),
            "premium" => ("Gói Cao Cấp",  20,   "15.000đ"),
            "vip"     => ("Gói VIP",      999,  "50.000đ"),
            _         => ("", 0, "")
        };

        if (scans == 0) return;

        bool confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Xác nhận thanh toán",
            $"{name}\n+{(scans == 999 ? "Không giới hạn" : scans.ToString())} lượt quét\nGiá: {price}",
            "Thanh toán", "Hủy");

        if (!confirm) return;

        IsProcessing = true;

        // Mô phỏng xử lý cổng thanh toán
        await Task.Delay(2000);

        _quotaService.AddScans(scans);
        RemainingScans = _quotaService.GetRemaining();

        // 2. ĐỒNG BỘ DỮ LIỆU THANH TOÁN LÊN WEB ADMIN
        try 
        {
            var dbService = ServiceHelper.GetService<DatabaseService>();
            var syncService = ServiceHelper.GetService<AdminSyncService>();

            if (dbService != null && syncService != null)
            {
                var user = await dbService.GetUserByEmailAsync(currentEmail);
                string fullName = user?.FullName ?? "Người dùng App";
                
                decimal amount = packageId == "vip" ? 50000 : (packageId == "premium" ? 15000 : 5000);
                
                // Gửi thông tin lên Web Admin
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
            $"Thanh toán {price} thành công!\nBạn có {RemainingScans} lượt quét.",
            "OK");
    }

#if DEBUG
    [RelayCommand]
    private void DevReset()
    {
        _quotaService.ResetToFree();
        RemainingScans = _quotaService.GetRemaining();
    }
#endif
}
