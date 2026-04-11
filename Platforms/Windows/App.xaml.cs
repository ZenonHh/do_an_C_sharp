using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace DoAnCSharp.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
