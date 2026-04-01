using DoAnCSharp.Services;
using DoAnCSharp.Views; 

namespace DoAnCSharp;

public partial class AppShell : Shell
{
    
    public AppShell(ILanguageService langService)
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ScanQRPage), typeof(ScanQRPage));

        BindingContext = langService;
        Routing.RegisterRoute("HistoryPage", typeof(Views.HistoryPage));
    }
}