using System.Collections.Generic;

namespace DoAnCSharp.Services
{
    public class LanguageService
    {
        public static string CurrentLocale = "vi";

        private static readonly Dictionary<string, Dictionary<string, string>> LocalizedValues = new Dictionary<string, Dictionary<string, string>>
        {
            ["vi"] = new Dictionary<string, string>
            {
                ["app_title"] = "Vĩnh Khánh Food Tour",
                ["home"] = "Trang chủ",
                ["search"] = "Tìm kiếm",
                ["map"] = "Bản đồ",
                ["profile"] = "Tôi",
                ["logout"] = "ĐĂNG XUẤT",
                ["login"] = "ĐĂNG NHẬP",
                ["hint_email"] = "Nhập số điện thoại/email",
                ["hint_pass"] = "Nhập mật khẩu",
                ["narrating"] = "Đang thuyết minh...",
                ["direction"] = "Chỉ đường đến quán",
                ["language_choice"] = "Chọn ngôn ngữ",
                ["desc_quan_1"] = "Chào mừng bạn đến với Quán Ốc Đào, nơi nổi tiếng với món ốc tỏi nướng mỡ hành thơm nức mũi...",
                ["desc_quan_2"] = "Quán Ốc Oanh là điểm dừng chân lý tưởng cho những tín đồ mê hải sản tươi sống tại Vĩnh Khánh...",
            },
            ["en"] = new Dictionary<string, string>
            {
                ["app_title"] = "Vinh Khanh Food Tour",
                ["home"] = "Home",
                ["search"] = "Search",
                ["map"] = "Map",
                ["profile"] = "Me",
                ["logout"] = "LOGOUT",
                ["login"] = "LOGIN",
                ["hint_email"] = "Enter phone/email",
                ["hint_pass"] = "Enter password",
                ["narrating"] = "Narrating...",
                ["direction"] = "Get directions",
                ["language_choice"] = "Select Language",
                ["desc_quan_1"] = "Welcome to Dao Snail Restaurant, famous for its mouth-watering grilled garlic snails with scallion oil...",
                ["desc_quan_2"] = "Oanh Snail Restaurant is an ideal stop for fresh seafood lovers at Vinh Khanh street...",
            },
        };

        public static string Translate(string key)
        {
            if (LocalizedValues.ContainsKey(CurrentLocale) && LocalizedValues[CurrentLocale].ContainsKey(key))
            {
                return LocalizedValues[CurrentLocale][key];
            }
            return key;
        }

        public static void ChangeLanguage(string langCode)
        {
            CurrentLocale = langCode;
        }
    }
}
