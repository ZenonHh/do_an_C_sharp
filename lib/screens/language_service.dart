import 'package:flutter/material.dart';

class LanguageService {
  static ValueNotifier<String> currentLocale = ValueNotifier('vi');

  static final Map<String, Map<String, String>> _localizedValues = {
    'vi': {
      'app_title': 'Vĩnh Khánh Food Tour',
      'home': 'Trang chủ',
      'search': 'Tìm kiếm',
      'map': 'Bản đồ',
      'profile': 'Tôi',
      'logout': 'ĐĂNG XUẤT',
      'login': 'ĐĂNG NHẬP',
      'hint_email': 'Nhập số điện thoại/email',
      'hint_pass': 'Nhập mật khẩu',
      'narrating': 'Đang thuyết minh...',
      'direction': 'Chỉ đường đến quán',
      'language_choice': 'Chọn ngôn ngữ',
      'welcome': 'Xin chào,',
      'recommend': 'Gợi ý cho bạn',
      'categories': 'Danh mục món ăn',
      'popular': 'Yêu thích nhất',
      'start_tour': 'Bắt đầu Tour',
      'snails': 'Ốc',
      'grill': 'Đồ nướng',
      'drinks': 'Đồ uống',
      'all': 'Tất cả',
      
      // --- THÊM CÁC MÔ TẢ QUÁN Ở ĐÂY ---
      'desc_quan_1': 'Chào mừng bạn đến với Quán Ốc Đào, nơi nổi tiếng với món ốc tỏi nướng mỡ hành thơm nức mũi...',
      'desc_quan_2': 'Quán Ốc Oanh là điểm dừng chân lý tưởng cho những tín đồ mê hải sản tươi sống tại Vĩnh Khánh...',
    },
    'en': {
      'app_title': 'Vinh Khanh Food Tour',
      'home': 'Home',
      'search': 'Search',
      'map': 'Map',
      'profile': 'Me',
      'logout': 'LOGOUT',
      'login': 'LOGIN',
      'hint_email': 'Enter phone/email',
      'hint_pass': 'Enter password',
      'narrating': 'Narrating...',
      'direction': 'Get directions',
      'language_choice': 'Select Language',
      'welcome': 'Welcome,',
      'recommend': 'Recommended for you',
      'categories': 'Categories',
      'popular': 'Most Popular',
      'start_tour': 'Start Tour',
      'snails': 'Snails',
      'grill': 'Grill',
      'drinks': 'Drinks',
      'all': 'All',

      // --- BẢN DỊCH TIẾNG ANH TƯƠNG ỨNG ---
      'desc_quan_1': 'Welcome to Dao Snail Restaurant, famous for its mouth-watering grilled garlic snails with scallion oil...',
      'desc_quan_2': 'Oanh Snail Restaurant is an ideal stop for fresh seafood lovers at Vinh Khanh street...',
    },
  };

  static String t(String key) {
    return _localizedValues[currentLocale.value]?[key] ?? key;
  }

  static void changeLanguage(String langCode) {
    currentLocale.value = langCode;
  }
}