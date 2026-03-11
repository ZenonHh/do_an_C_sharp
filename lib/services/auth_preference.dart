import 'package:shared_preferences/shared_preferences.dart';

class AuthPreference {
  static const String _loginKey = "isLoggedIn";

  static Future<void> setLoggedIn(bool status) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_loginKey, status);
  }

  static Future<bool> isLoggedIn() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool(_loginKey) ?? false;
  }

  // --- THÊM HÀM NÀY ĐỂ ĐĂNG XUẤT ---
  static Future<void> logout() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_loginKey); // Xóa hẳn khóa đăng nhập
  }
}