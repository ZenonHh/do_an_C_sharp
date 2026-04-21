using DoAnCSharp.Services;
using DoAnCSharp.Views; 
namespace DoAnCSharp;

public partial class AppShell : Shell
{
    public ILanguageService Lang { get; }

    public AppShell(ILanguageService langService)
    {
        InitializeComponent();
        
        Lang = langService;
        BindingContext = this;
        
        try
        {
            // Đăng ký các đường dẫn trang
            Routing.RegisterRoute(nameof(ScanQRPage), typeof(ScanQRPage));
            Routing.RegisterRoute("HistoryPage", typeof(HistoryPage));
            Routing.RegisterRoute("EditProfilePage", typeof(EditProfilePage));
            Routing.RegisterRoute("PaymentPage", typeof(PaymentPage));

            // Set initial tab titles
            UpdateTabTitles();

            // Cập nhật ngôn ngữ cho thanh TabBar khi ngôn ngữ thay đổi
            Lang.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Item" || e.PropertyName == nameof(Lang.CurrentLocale))
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            UpdateTabTitles();
                            OnPropertyChanged(nameof(Lang));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"ERROR updating tab titles: {ex}");
                        }
                    });
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in AppShell constructor: {ex}");
        }
    }

    private void UpdateTabTitles()
    {
        try
        {
            if (TabHome != null) TabHome.Title = Lang["tab_home"] ?? "Trang Chủ";
            if (TabMap != null) TabMap.Title = Lang["tab_map"] ?? "Bản Đồ";
            if (TabProfile != null) TabProfile.Title = Lang["tab_profile"] ?? "Tôi";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in UpdateTabTitles: {ex}");
        }
    }
}