import 'package:flutter/material.dart';
import 'screens/main_screen.dart';
import 'screens/auth_screen.dart';
import 'services/auth_preference.dart';
import 'services/language_service.dart'; // Import thêm file này

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const MyApp());
}

class MyApp extends StatefulWidget {
  const MyApp({super.key});

  @override
  State<MyApp> createState() => _MyAppState();
}

class _MyAppState extends State<MyApp> {
  bool? _isLoggedIn;

  @override
  void initState() {
    super.initState();
    _checkLoginStatus();
  }

  void _checkLoginStatus() async {
    bool status = await AuthPreference.isLoggedIn();
    if (mounted) {
      setState(() {
        _isLoggedIn = status;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder<String>(
      valueListenable: LanguageService.currentLocale,
      builder: (context, locale, child) {
        return MaterialApp(
          // Key này giúp App xóa bộ nhớ đệm và vẽ lại toàn bộ khi đổi ngôn ngữ
          title: LanguageService.t('app_title'),
          debugShowCheckedModeBanner: false,
          theme: ThemeData(
            colorScheme: ColorScheme.fromSeed(seedColor: Colors.orange),
            useMaterial3: true,
          ),
          // Sửa lỗi điều hướng:
          home: _isLoggedIn == null
              ? Scaffold(
                  body: Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const CircularProgressIndicator(),
                        const SizedBox(height: 10),
                        Text(LanguageService.t('loading')),
                      ],
                    ),
                  ),
                )
              : (_isLoggedIn!
                  ? MainScreen(
                      onLogout: () {
                        setState(() => _isLoggedIn = false);
                      },
                    )
                  : AuthScreen(
                      onLoginSuccess: () {
                        setState(() => _isLoggedIn = true);
                      },
                    )),
        );
      },
    );
  }
}