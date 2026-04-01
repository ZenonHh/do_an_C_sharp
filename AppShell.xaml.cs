using DoAnCSharp.Services;

namespace DoAnCSharp;

public partial class AppShell : Shell
{
    // ĐÃ SỬA: Thêm tham số ILanguageService
    public AppShell(ILanguageService langService)
    {
        InitializeComponent();
        
        // Gán LanguageService làm nguồn dữ liệu cho giao diện
        BindingContext = langService;
        Routing.RegisterRoute("HistoryPage", typeof(Views.HistoryPage));
    }
}