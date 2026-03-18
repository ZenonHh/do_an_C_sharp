namespace DoAnCSharp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // PHẢI GỌI APPSHELL Ở ĐÂY ĐỂ HIỆN THANH MENU
        MainPage = new AppShell(); 
    }
}