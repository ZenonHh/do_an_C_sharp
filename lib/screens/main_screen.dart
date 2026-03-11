import 'package:flutter/material.dart';
import 'map_screen.dart'; 
import '../services/language_service.dart';

class MainScreen extends StatefulWidget {
  final VoidCallback onLogout;
  const MainScreen({super.key, required this.onLogout});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _selectedIndex = 2; // Mặc định mở bản đồ

  @override
  Widget build(BuildContext context) {
    // LẮNG NGHE thay đổi ngôn ngữ để cập nhật toàn bộ MainScreen
    return ValueListenableBuilder<String>(
      valueListenable: LanguageService.currentLocale,
      builder: (context, locale, child) {
        // Danh sách trang phải nằm trong build để cập nhật ngôn ngữ mới
        final List<Widget> pages = [
          Center(child: Text(LanguageService.t('home'))),
          Center(child: Text(LanguageService.t('search'))),
          const MapScreen(), 
          ProfilePage(onLogout: widget.onLogout), // Gọi đúng logic logout
        ];

        return Scaffold(
          body: pages[_selectedIndex],
          bottomNavigationBar: BottomNavigationBar(
            currentIndex: _selectedIndex,
            onTap: (index) => setState(() => _selectedIndex = index),
            type: BottomNavigationBarType.fixed,
            selectedItemColor: Colors.orange,
            unselectedItemColor: Colors.grey,
            items: [
              BottomNavigationBarItem(icon: const Icon(Icons.home), label: LanguageService.t('home')),
              BottomNavigationBarItem(icon: const Icon(Icons.search), label: LanguageService.t('search')),
              BottomNavigationBarItem(icon: const Icon(Icons.map), label: LanguageService.t('map')),
              BottomNavigationBarItem(icon: const Icon(Icons.person), label: LanguageService.t('profile')),
            ],
          ),
        );
      },
    );
  }
}

class ProfilePage extends StatelessWidget {
  final VoidCallback onLogout;
  // Quay lại dùng required onLogout để MainScreen truyền vào được
  const ProfilePage({super.key, required this.onLogout});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(LanguageService.t('profile')),
        centerTitle: true,
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.account_circle, size: 80, color: Colors.blueGrey),
            const SizedBox(height: 20),
            Text(
              LanguageService.t('profile'),
              style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 40),

            // Cụm nút chuyển đổi ngôn ngữ
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                _buildLanguageButton("Tiếng Việt", 'vi'),
                const SizedBox(width: 20),
                _buildLanguageButton("English", 'en'),
              ],
            ),
            
            const SizedBox(height: 60),

            ElevatedButton.icon(
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.red, 
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(horizontal: 30, vertical: 12),
              ),
              onPressed: onLogout, // Sử dụng callback từ main.dart
              icon: const Icon(Icons.logout),
              label: Text(LanguageService.t('logout')),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildLanguageButton(String label, String langCode) {
    return ElevatedButton(
      style: ElevatedButton.styleFrom(
        // Đổi màu nút khi được chọn để người dùng dễ nhận biết
        backgroundColor: LanguageService.currentLocale.value == langCode 
            ? Colors.orange.shade100 
            : Colors.white,
      ),
      onPressed: () => LanguageService.changeLanguage(langCode),
      child: Text(label),
    );
  }
}