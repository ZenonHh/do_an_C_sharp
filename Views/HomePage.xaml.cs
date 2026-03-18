using DoAnCSharp.Services;
using DoAnCSharp.Models;

namespace DoAnCSharp.Views;

public partial class HomePage : ContentPage
{
    private readonly IPoiRepository _repo;
    private readonly ILanguageService _lang;

    public HomePage(IPoiRepository repo, ILanguageService lang)
    {
        InitializeComponent();
        _repo = repo;
        _lang = lang;

        LoadData();
        UpdateLanguage();
    }

    private void LoadData()
    {
        // Đổ dữ liệu thẳng vào CollectionView
        PoiCollectionView.ItemsSource = _repo.GetTourPoints();
    }

    private void UpdateLanguage()
    {
        // Cập nhật chữ trực tiếp bằng tên (x:Name) của các Label
        WelcomeLabel.Text = $"{_lang.T("welcome")} Gastronome! 👋";
        RecommendTitle.Text = _lang.T("recommend");
        // ... Cập nhật các Label khác ở đây
    }

    private void OnChangeLangClicked(object sender, EventArgs e)
    {
        string newLang = _lang.CurrentLocale == "vi" ? "en" : "vi";
        _lang.ChangeLanguage(newLang);
        UpdateLanguage(); // Đổi chữ ngay lập tức
        LoadData();       // Load lại danh sách nếu cần đổi tên quán
    }
}
