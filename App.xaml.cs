namespace DoAnCSharp;

public partial class App : Application
{
    public App(Views.AuthPage authPage)
    {
        InitializeComponent();

        // Khởi động bằng màn hình Đăng nhập
        MainPage = authPage;
    }
}