namespace DoAnCSharp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Thiết lập trang chính là MainPage
            MainPage = new AppShell();
        }
    }
}
