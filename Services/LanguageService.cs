using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DoAnCSharp.Services;

public interface ILanguageService : INotifyPropertyChanged
{
    string CurrentLocale { get; }
    string T(string key);
    void ChangeLanguage(string langCode);
    Task Initialize();
    string this[string key] { get; }
}

public class LanguageService : ILanguageService
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private string _currentLocale = "vi";
    public string CurrentLocale => _currentLocale;

    public string this[string key] => T(key);
    
    private readonly Dictionary<string, Dictionary<string, string>> _localizedValues = new()
    {
        {
            "vi", new Dictionary<string, string>
            {
                { "vk_street", "📍 Phố Ẩm Thực Vĩnh Khánh" },
                { "welcome", "Xin chào," },
                { "recommend", "Gợi ý cho bạn" },
                { "popular_btn", "🔥 Phổ biến" },
                { "snail_btn", "🐌 Quán Ốc" },
                { "hotpot_btn", "🍲 Lẩu & Nướng" },
                { "all", "Tất cả" },
                { "search_home", "Tìm tên quán, lẩu, ốc..." },
                { "search", "Tìm kiếm" },
                { "history", "Lịch sử đã nghe" },
                { "edit_profile", "Chỉnh sửa thông tin" },
                { "language_choice", "Chọn ngôn ngữ" },
                { "logout", "ĐĂNG XUẤT" },
                { "profile", "Tôi" },
                { "mins", "phút" },
                { "direction", "Chỉ đường" },
                { "listen_review", "Nghe thuyết minh" },
                { "undetermined", "Chưa xác định" },
                { "tab_home", "Trang Chủ" },
                { "tab_map", "Bản Đồ" },
                { "tab_profile", "Tôi" },
                { "login_register", "Đăng nhập / Đăng ký" },
                { "payment_title", "Thanh Toán & Lượt Quét" },
                { "scans_remaining", "Lượt quét QR còn lại" },
                { "scans_unit", "lượt" },
                { "choose_package", "Chọn gói nạp lượt:" },
                { "buy_btn", "Mua" },
                { "processing_payment", "Đang xử lý thanh toán..." },
                { "pkg_basic_name", "Gói Cơ Bản" },
                { "pkg_basic_desc", "+5 lượt quét" },
                { "pkg_premium_name", "Gói Cao Cấp" },
                { "pkg_premium_desc", "+20 lượt quét" },
                { "pkg_vip_name", "Gói VIP" },
                { "pkg_vip_desc", "Không giới hạn lượt quét" },
                { "popular_badge", "🔥 PHỔ BIẾN" },
                { "payment_menu", "Thanh toán & Lượt quét" }
            }
        },
        {
            "en", new Dictionary<string, string>
            {
                { "vk_street", "📍 Vinh Khanh Food Street" },
                { "welcome", "Welcome," },
                { "recommend", "Recommended for you" },
                { "popular_btn", "🔥 Popular" },
                { "snail_btn", "🐌 Snails" },
                { "hotpot_btn", "🍲 Hotpot" },
                { "all", "All" },
                { "search_home", "Search for restaurants..." },
                { "search", "Search" },
                { "history", "Listening History" },
                { "edit_profile", "Edit Profile" },
                { "language_choice", "Select Language" },
                { "logout", "LOGOUT" },
                { "profile", "Profile" },
                { "mins", "mins" },
                { "direction", "Direction" },
                { "listen_review", "Listen Review" },
                { "undetermined", "Undetermined" },
                { "tab_home", "Home" },
                { "tab_map", "Map" },
                { "tab_profile", "Me" },
                { "login_register", "Sign In / Register" },
                { "payment_title", "Payment & Scans" },
                { "scans_remaining", "QR scans remaining" },
                { "scans_unit", "scans" },
                { "choose_package", "Select a package:" },
                { "buy_btn", "Buy" },
                { "processing_payment", "Processing payment..." },
                { "pkg_basic_name", "Basic Package" },
                { "pkg_basic_desc", "+5 scans" },
                { "pkg_premium_name", "Premium Package" },
                { "pkg_premium_desc", "+20 scans" },
                { "pkg_vip_name", "VIP Package" },
                { "pkg_vip_desc", "Unlimited scans" },
                { "popular_badge", "🔥 POPULAR" },
                { "payment_menu", "Payment & Scans" },

                // --- NAMES ---
                { "Phá Lấu Cô Oanh", "Oanh's Offal Stew" },
                { "Sushi Viên Vĩnh Khánh", "Vinh Khanh Sushi" },
                { "Ốc Oanh", "Oanh's Snails" },
                { "Ốc Vũ", "Vu's Snails" },
                { "Ốc Nho", "Nho's Snails" },
                { "Ốc Thảo", "Thao's Snails" },
                { "Ốc Sóc", "Soc's Snails" },
                { "Ốc Tuyết", "Tuyet's Snails" },
                { "Ốc Đào 2", "Dao 2 Snails" },
                { "Quán Nướng Chilli", "Chilli BBQ" },
                { "Lẩu Bò Khu Nhà Cháy", "Khu Nha Chay Beef Hotpot" },
                { "Sườn Nướng Muối Ớt", "Chili Salt Grilled Ribs" },
                { "Khèn BBQ - Nướng Ngói", "Khen BBQ - Tile Grill" },
                { "Lẩu Dê Dũng Mập", "Dung Map Goat Hotpot" },
                { "Trái Cây Tô & Chè", "Fruit Bowls & Sweet Soup" },

                // --- DESCRIPTIONS ---
                { "Phá lấu bò nấu nước cốt dừa béo ngậy, ăn kèm bánh mì n...", "Creamy beef offal stew with coconut milk, served with bread..." },
                { "Sushi lề đường giá học sinh sinh viên nhưng cá hồi, trứ...", "Street-side sushi with student prices but premium salmon..." },
                { "Quán ốc 'huyền thoại' đông nhất Vĩnh Khánh. Nổi tiếng", "Legendary snail place, the most crowded in Vinh Khanh." },
                { "Không gian siêu rộng, menu đa dạng và giá cả bình dân.", "Spacious area, diverse menu and affordable prices." },
                { "Chân á của giới trẻ với các món ốc sốt phô mai kéo sợi,", "A favorite for youth with cheesy snail dishes." },
                { "Quán lâu năm, giữ nguyên hương vị ốc truyền thống Sài", "Long-standing place, keeping traditional Saigon snail flavors." },
                { "Nổi bật với món nghêu hấp sả ớt cay nồng và ốc móng", "Famous for spicy steamed clams with lemongrass and razor clams." },
                { "Quán bình dân nhưng chất lượng tuyệt vời, phục vụ", "Affordable restaurant but excellent quality and service." },
                { "Thương hiệu ốc lâu đời, nêm nếm theo khẩu vị đậm đà", "Long-standing brand with rich and flavorful seasoning." },
                { "Thiên đường hàu nướng với hơn 20 loại sốt khác nhau,", "Grilled oyster paradise with over 20 different sauces." },
                { "Lẩu bò gia truyền nước dùng ngọt thanh từ xương, bò viên", "Traditional beef hotpot with sweet broth from bones..." },
                { "Sườn heo nướng tảng tẩm ớt cay nồng, ăn kèm đồ chua", "Grilled pork ribs with spicy chili, served with pickles." },
                { "Thịt được nướng trên ngói đỏ giúp giữ độ ngọt, không bị", "Meat grilled on red tiles keeps its sweetness and juiciness." },
                { "Lẩu dê nấu chao thơm phức, thịt dê núi mềm ngọt, không", "Goat hotpot with fermented tofu, tender mountain goat meat." },
                { "Tráng miệng mát lạnh giải nhiệt sau khi ăn đồ nướng", "Cooling desserts to refresh after having BBQ." }
            }
        },
        {
            "ja", new Dictionary<string, string>
            {
                { "vk_street", "📍 ビンカイン・フードストリート" },
                { "welcome", "ようこそ、" }, 
                { "tab_home", "ホーム" },
                { "tab_map", "マップ" },
                { "tab_profile", "私" },
                { "mins", "分" },
                { "language_choice", "言語を選択" },
                { "logout", "ログアウト" },
                { "login_register", "ログイン／登録" },
                { "payment_title", "支払い & スキャン" },
                { "scans_remaining", "残りQRスキャン数" },
                { "scans_unit", "回" },
                { "choose_package", "パッケージを選択:" },
                { "buy_btn", "購入" },
                { "processing_payment", "支払い処理中..." },
                { "pkg_basic_name", "ベーシック" },
                { "pkg_basic_desc", "+5 回スキャン" },
                { "pkg_premium_name", "プレミアム" },
                { "pkg_premium_desc", "+20 回スキャン" },
                { "pkg_vip_name", "VIP" },
                { "pkg_vip_desc", "無制限スキャン" },
                { "popular_badge", "🔥 人気" },
                { "payment_menu", "支払い & スキャン" },
                { "all", "すべて" },
                { "direction", "道順" },
                { "listen_review", "レビューを聞く" },
                { "undetermined", "未定" },
                { "history", "視聴履歴" }, 
                { "edit_profile", "プロフィール編集" }, 
                { "recommend", "おすすめ" },
                
                // NAMES
                { "Phá Lấu Cô Oanh", "オアンのモツ煮込み" },
                { "Sushi Viên Vĩnh Khánh", "ビンカインの寿司" },
                { "Ốc Oanh", "オアンのカタツムリ" },
                { "Ốc Vũ", "ヴーのカタツムリ" },
                { "Ốc Nho", "ニョのカタツムリ" },
                { "Ốc Thảo", "タオのカタツムリ" },
                { "Ốc Sóc", "ソックのカタツムリ" },
                { "Ốc Tuyết", "トゥエットのカタツムリ" },
                { "Ốc Đào 2", "ダオ2のカタツムリ" },
                { "Quán Nướng Chilli", "Chilli 焼肉" },
                { "Lẩu Bò Khu Nhà Cháy", "クニャチャイの牛鍋" },
                { "Sườn Nướng Muối Ớt", "激辛塩焼きスペアリブ" },
                { "Khèn BBQ - Nướng Ngói", "Khen BBQ - 瓦焼き" },
                { "Lẩu Dê Dũng Mập", "Dung Map 山羊鍋" },
                { "Trái Cây Tô & Chè", "フルーツボウルとチェー" },

                // DESCRIPTIONS (BỔ SUNG ĐẦY ĐỦ 15 CÂU MÔ TẢ TIẾNG NHẬT)
                { "Phá lấu bò nấu nước cốt dừa béo ngậy, ăn kèm bánh mì n...", "ココナッツミルクで煮込んだ牛モツ煮込み、パン付き..." },
                { "Sushi lề đường giá học sinh sinh viên nhưng cá hồi, trứ...", "手頃な価格の路上寿司、プレミアムサーモン..." },
                { "Quán ốc 'huyền thoại' đông nhất Vĩnh Khánh. Nổi tiếng", "ビンカインで最も混雑する伝説のカタツムリ店。" },
                { "Không gian siêu rộng, menu đa dạng và giá cả bình dân.", "広々とした空間、多彩なメニュー、手頃な価格。" },
                { "Chân á của giới trẻ với các món ốc sốt phô mai kéo sợi,", "チーズたっぷりのカタツムリ料理で若者に人気。" },
                { "Quán lâu năm, giữ nguyên hương vị ốc truyền thống Sài", "伝統的なサイゴンのカタツムリの味を保つ老舗。" },
                { "Nổi bật với món nghêu hấp sả ớt cay nồng và ốc móng", "レモングラスと唐辛子のピリ辛ハマグリ蒸しが有名。" },
                { "Quán bình dân nhưng chất lượng tuyệt vời, phục vụ", "手頃な価格のレストランですが、質とサービスは最高です。" },
                { "Thương hiệu ốc lâu đời, nêm nếm theo khẩu vị đậm đà", "豊かな風味の味付けが自慢の老舗カタツムリブランド。" },
                { "Thiên đường hàu nướng với hơn 20 loại sốt khác nhau,", "20種類以上のソースが楽しめる焼き牡蠣のパラダイス。" },
                { "Lẩu bò gia truyền nước dùng ngọt thanh từ xương, bò viên", "骨からとった甘いスープの伝統的な牛鍋..." },
                { "Sườn heo nướng tảng tẩm ớt cay nồng, ăn kèm đồ chua", "スパイシーなチリで味付けされた豚スペアリブのグリル。" },
                { "Thịt được nướng trên ngói đỏ giúp giữ độ ngọt, không bị", "赤い瓦の上で焼かれる肉は、甘みとジューシーさを保ちます。" },
                { "Lẩu dê nấu chao thơm phức, thịt dê núi mềm ngọt, không", "発酵豆腐を使ったヤギ鍋、柔らかい山のヤギ肉。" },
                { "Tráng miệng mát lạnh giải nhiệt sau khi ăn đồ nướng", "焼肉の後の冷たいデザート" }
            }
        },
        {
            "ko", new Dictionary<string, string>
            {
                { "vk_street", "📍 빈칸 푸드 스트리트" },
                { "welcome", "환영합니다," }, 
                { "tab_home", "홈" },
                { "tab_map", "지도" },
                { "tab_profile", "내 정보" },
                { "mins", "분" },
                { "language_choice", "언어 선택" },
                { "logout", "로그아웃" },
                { "login_register", "로그인 / 회원가입" },
                { "payment_title", "결제 & 스캔" },
                { "scans_remaining", "남은 QR 스캔 횟수" },
                { "scans_unit", "회" },
                { "choose_package", "패키지 선택:" },
                { "buy_btn", "구매" },
                { "processing_payment", "결제 처리 중..." },
                { "pkg_basic_name", "기본 패키지" },
                { "pkg_basic_desc", "+5 스캔" },
                { "pkg_premium_name", "프리미엄 패키지" },
                { "pkg_premium_desc", "+20 스캔" },
                { "pkg_vip_name", "VIP 패키지" },
                { "pkg_vip_desc", "무제한 스캔" },
                { "popular_badge", "🔥 인기" },
                { "payment_menu", "결제 & 스캔" },
                { "all", "전체" },
                { "direction", "길 찾기" },
                { "listen_review", "리뷰 듣기" },
                { "undetermined", "미정" },
                { "history", "청취 기록" }, 
                { "edit_profile", "프로필 편집" }, 

                // NAMES (Giữ nguyên của bạn)
                { "Phá Lấu Cô Oanh", "오안 아줌마의 내장 전골" },
                { "Sushi Viên Vĩnh Khánh", "빈칸 스시" },
                { "Ốc Oanh", "오안 아줌마의 달팽이 요리" },
                { "Ốc Vũ", "부의 달팽이 요리" },
                { "Ốc Nho", "뇨의 달팽이 요리" },
                { "Ốc Thảo", "타오의 달팽이 요리" },
                { "Ốc Sóc", "속의 달팽이 요리" },
                { "Ốc Tuyết", "뚜엣의 달팽이 요리" },
                { "Ốc Đào 2", "다오 2 달팽이 요리" },
                { "Quán Nướng Chilli", "Chilli 바비큐" },
                { "Lẩu Bò Khu Nhà Cháy", "쿠냐차이 소고기 전골" },
                { "Sườn Nướng Muối Ớt", "칠리 소금 갈비 구이" },
                { "Khèn BBQ - Nướng 기와 구이", "Khen BBQ - 기와 구이" },
                { "Khèn BBQ - Nướng Ngói", "Khen BBQ - 기와 구이" },
                { "Lẩu Dê Dũng Mập", "중맙 염소 전골" },
                { "Trái Cây Tô & Chè", "과일 보울 & 체" },

                // DESCRIPTIONS (BỔ SUNG ĐẦY ĐỦ 15 CÂU MÔ TẢ TIẾNG HÀN)
                { "Phá lấu bò nấu nước cốt dừa béo ngậy, ăn kèm bánh mì n...", "코코넛 밀크로 요리한 고소한 소내장 전골, 빵과 함께..." },
                { "Sushi lề đường giá học sinh sinh viên nhưng cá hồi, trứ...", "저렴한 가격의 길거리 스시, 연어와 계란말이..." },
                { "Quán ốc 'huyền thoại' đông nhất Vĩnh Khánh. Nổi tiếng", "빈칸에서 가장 붐비는 전설적인 달팽이 식당." },
                { "Không gian siêu rộng, menu đa dạng và giá cả bình dân.", "넓은 공간, 다양한 메뉴, 저렴한 가격." },
                { "Chân á của giới trẻ với các món ốc sốt phô mai kéo sợi,", "치즈가 듬뿍 들어간 달팽이 요리로 젊은이들에게 인기." },
                { "Quán lâu năm, giữ nguyên hương vị ốc truyền thống Sài", "전통적인 사이공 달팽이 맛을 유지하는 오래된 식당." },
                { "Nổi bật với món nghêu hấp sả ớt cay nồng và ốc móng", "레몬그라스와 칠리를 곁들인 매콤한 조개찜이 유명합니다." },
                { "Quán bình dân nhưng chất lượng tuyệt vời, phục vụ", "저렴하지만 훌륭한 품질과 서비스를 제공하는 식당." },
                { "Thương hiệu ốc lâu đời, nêm nếm theo khẩu vị đậm đà", "풍부하고 진한 양념을 자랑하는 오래된 달팽이 브랜드." },
                { "Thiên đường hàu nướng với hơn 20 loại sốt khác nhau,", "20가지 이상의 소스가 있는 구운 굴 천국." },
                { "Lẩu bò gia truyền nước dùng ngọt thanh từ xương, bò viên", "뼈를 우려낸 달콤한 국물의 전통 소고기 전골..." },
                { "Sườn heo nướng tảng tẩm ớt cay nồng, ăn kèm đồ chua", "매콤한 칠리로 양념한 돼지 갈비 구이." },
                { "Thịt được nướng trên ngói đỏ giúp giữ độ ngọt, không bị", "붉은 기와 위에서 구운 고기는 단맛과 육즙을 유지합니다." },
                { "Lẩu dê nấu chao thơm phức, thịt dê núi mềm ngọt, không", "발효 두부를 넣은 염소 전골, 부드러운 산염소 고기." },
                { "Tráng miệng mát lạnh giải nhiệt sau khi ăn đồ nướng", "바비큐를 먹은 후 시원하고 상쾌한 디저트" }
            }
        }
    };

    public Task Initialize()
    {
        _currentLocale = Preferences.Default.Get("selected_language", "vi");
        OnPropertyChanged(nameof(CurrentLocale));
        OnPropertyChanged("Item");
        return Task.CompletedTask;
    }

    public string T(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;

        if (_localizedValues.ContainsKey(_currentLocale))
        {
            var currentDict = _localizedValues[_currentLocale];
            
            // 1. Khớp chính xác 100% (Cho các từ ngắn như Tên quán, Nút bấm...)
            if (currentDict.ContainsKey(key))
                return currentDict[key];

            // 2. Khớp thông minh (Chữa lỗi các đoạn mô tả bị cắt bớt bằng dấu ...)
            foreach (var kvp in currentDict)
            {
                string cleanDictKey = kvp.Key.Replace("...", "").Trim();
                
                // Nếu mô tả trong Database chứa nội dung của từ điển (hoặc ngược lại) thì vẫn dịch!
                if (cleanDictKey.Length > 10 && (key.StartsWith(cleanDictKey) || key.Contains(cleanDictKey)))
                {
                    return kvp.Value;
                }
            }
        }
        return key; // Nếu không tìm ra, giữ nguyên tiếng Việt gốc
    }

    public void ChangeLanguage(string langCode)
    {
        if (_currentLocale == langCode) return;
        _currentLocale = langCode;
        Preferences.Default.Set("selected_language", langCode);
        OnPropertyChanged(nameof(CurrentLocale));
        OnPropertyChanged("Item"); 
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}