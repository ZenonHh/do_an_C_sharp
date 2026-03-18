using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DoAnCSharp.Services;

public interface ILanguageService : INotifyPropertyChanged
{
    string CurrentLocale { get; }
    string T(string key);
    void ChangeLanguage(string langCode);
    // SỬA LỖI CS1061: Thêm khai báo hàm khởi tạo
    Task Initialize(); 
}

public class LanguageService : ILanguageService
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _currentLocale = "vi";
    public string CurrentLocale => _currentLocale;

    private readonly Dictionary<string, Dictionary<string, string>> _localizedValues = new()
    {
        {
            "vi", new Dictionary<string, string>
            {
                { "app_title", "Vĩnh Khánh Food Tour" },
                { "home", "Trang chủ" },
                { "search", "Tìm kiếm" },
                { "map", "Bản đồ" },
                { "profile", "Tôi" },
                { "logout", "ĐĂNG XUẤT" },
                { "login", "ĐĂNG NHẬP" },
                { "hint_email", "Nhập số điện thoại/email" },
                { "hint_pass", "Nhập mật khẩu" },
                { "narrating", "Đang thuyết minh..." },
                { "direction", "Chỉ đường đến quán" },
                { "language_choice", "Chọn ngôn ngữ" },
                { "welcome", "Xin chào," },
                { "recommend", "Gợi ý cho bạn" },
                { "categories", "Danh mục món ăn" },
                { "popular", "Yêu thích nhất" },
                { "start_tour", "Bắt đầu Tour" },
                { "snails", "Ốc" },
                { "grill", "Đồ nướng" },
                { "drinks", "Đồ uống" },
                { "all", "Tất cả" },
                { "desc_quan_1", "Chào mừng bạn đến với Quán Ốc Đào..." },
                { "desc_quan_2", "Quán Ốc Oanh là điểm dừng chân lý tưởng..." }
            }
        },
        {
            "en", new Dictionary<string, string>
            {
                { "app_title", "Vinh Khanh Food Tour" },
                { "home", "Home" },
                { "search", "Search" },
                { "map", "Map" },
                { "profile", "Me" },
                { "logout", "LOGOUT" },
                { "login", "LOGIN" },
                { "hint_email", "Enter phone/email" },
                { "hint_pass", "Enter password" },
                { "narrating", "Narrating..." },
                { "direction", "Get directions" },
                { "language_choice", "Select Language" },
                { "welcome", "Welcome," },
                { "recommend", "Recommended for you" },
                { "categories", "Categories" },
                { "popular", "Most Popular" },
                { "start_tour", "Start Tour" },
                { "snails", "Snails" },
                { "grill", "Grill" },
                { "drinks", "Drinks" },
                { "all", "All" },
                { "desc_quan_1", "Welcome to Dao Snail Restaurant..." },
                { "desc_quan_2", "Oanh Snail Restaurant is an ideal stop..." }
            }
        }
    };

    // SỬA LỖI CS1061: Triển khai hàm Initialize
    public Task Initialize()
    {
        // Mẹo 8GB RAM: Dùng Preferences để lưu lại lựa chọn của người dùng
        // Nếu lần đầu mở App, mặc định sẽ là Tiếng Việt ("vi")
        var savedLanguage = Preferences.Default.Get("selected_language", "vi");
        _currentLocale = savedLanguage;
        
        OnPropertyChanged(nameof(CurrentLocale));
        return Task.CompletedTask;
    }

    public string T(string key)
    {
        if (_localizedValues.ContainsKey(_currentLocale) && _localizedValues[_currentLocale].ContainsKey(key))
        {
            return _localizedValues[_currentLocale][key];
        }
        return key;
    }

    public void ChangeLanguage(string langCode)
    {
        if (_currentLocale != langCode)
        {
            _currentLocale = langCode;
            
            // Lưu lại lựa chọn vào bộ nhớ máy
            Preferences.Default.Set("selected_language", langCode);
            
            OnPropertyChanged(nameof(CurrentLocale));
            OnPropertyChanged("Item"); 
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
