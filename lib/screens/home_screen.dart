import 'package:flutter/material.dart';
import '../services/language_service.dart';
import '../services/poi_repository.dart';
import '../models/poi_model.dart';
import 'map_screen.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});

  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _currentIndex = 0;

  // Giữ trạng thái bản đồ để không phải load lại tốn RAM 8GB
  late final List<Widget> _pages;

  @override
  void initState() {
    super.initState();
    _pages = [
      const HomeScreen(),
      const MapScreen(),
      const Center(child: Text("Profile Page")), 
    ];
  }

  @override
  Widget build(BuildContext context) {
    return ValueListenableBuilder<String>(
      valueListenable: LanguageService.currentLocale,
      builder: (context, locale, child) {
        return Scaffold(
          body: IndexedStack(
            index: _currentIndex,
            children: _pages,
          ),
          bottomNavigationBar: BottomNavigationBar(
            currentIndex: _currentIndex,
            onTap: (index) => setState(() => _currentIndex = index),
            selectedItemColor: Colors.orange,
            unselectedItemColor: Colors.grey,
            type: BottomNavigationBarType.fixed,
            items: [
              BottomNavigationBarItem(
                icon: const Icon(Icons.home_rounded), 
                label: LanguageService.t('home')
              ),
              BottomNavigationBarItem(
                icon: const Icon(Icons.map_rounded), 
                label: LanguageService.t('map')
              ),
              BottomNavigationBarItem(
                icon: const Icon(Icons.person_rounded), 
                label: LanguageService.t('profile')
              ),
            ],
          ),
        );
      },
    );
  }
}

class HomeScreen extends StatelessWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final List<POI> pois = POIRepository.getTourPoints();

    return Scaffold(
      backgroundColor: Colors.white,
      body: CustomScrollView(
        slivers: [
          _buildSliverAppBar(context),
          
          SliverToBoxAdapter(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildSectionTitle(LanguageService.t('recommend')),
                _buildHorizontalList(pois),
                _buildSectionTitle(LanguageService.t('categories')),
                _buildCategoryGrid(),
                _buildSectionTitle(LanguageService.t('popular')),
              ],
            ),
          ),

          SliverPadding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            sliver: SliverList(
              delegate: SliverChildBuilderDelegate(
                (context, index) => _buildVerticalItem(pois[index]),
                childCount: pois.length,
              ),
            ),
          ),
          
          const SliverToBoxAdapter(child: SizedBox(height: 100)),
        ],
      ),
    );
  }

  Widget _buildSliverAppBar(BuildContext context) {
    return SliverAppBar(
      expandedHeight: 120,
      floating: true,
      pinned: true,
      backgroundColor: Colors.white,
      elevation: 0,
      flexibleSpace: FlexibleSpaceBar(
        titlePadding: const EdgeInsets.only(left: 20, bottom: 15),
        title: Text(
          "${LanguageService.t('welcome')} Gastronome! 👋",
          style: const TextStyle(
            color: Colors.black, 
            fontSize: 18, 
            fontWeight: FontWeight.bold
          ),
        ),
      ),
      actions: [
        Padding(
          padding: const EdgeInsets.only(right: 10),
          child: IconButton(
            icon: const Icon(Icons.translate, color: Colors.orange),
            onPressed: () => _showLanguageDialog(context),
          ),
        ),
      ],
    );
  }

  Widget _buildSectionTitle(String title) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 20, 20, 10),
      child: Text(
        title, 
        style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)
      ),
    );
  }

  Widget _buildHorizontalList(List<POI> pois) {
    return SizedBox(
      height: 200,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.only(left: 20),
        itemCount: pois.length,
        itemBuilder: (context, index) {
          final poi = pois[index];
          final String name = LanguageService.currentLocale.value == 'vi' 
              ? poi.name 
              : poi.nameEn;

          return Container(
            width: 160,
            margin: const EdgeInsets.only(right: 15),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(15),
                  child: Image.asset(
                    'assets/images/${poi.imageAsset}', 
                    height: 120, width: 160, fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                      height: 120, color: Colors.grey[200], 
                      child: const Icon(Icons.restaurant, color: Colors.grey)
                    ),
                  ),
                ),
                const SizedBox(height: 10),
                Text(name, maxLines: 2, overflow: TextOverflow.ellipsis, 
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
              ],
            ),
          );
        },
      ),
    );
  }

  Widget _buildCategoryGrid() {
    final categories = [
      {'name': 'all', 'icon': Icons.restaurant_menu},
      {'name': 'snails', 'icon': Icons.waves},
      {'name': 'grill', 'icon': Icons.local_fire_department},
      {'name': 'drinks', 'icon': Icons.local_bar},
    ];
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Wrap(
        spacing: 12,
        runSpacing: 10,
        children: categories.map((cat) => ActionChip(
          elevation: 0,
          pressElevation: 4,
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
          avatar: Icon(cat['icon'] as IconData, size: 18, color: Colors.orange),
          label: Text(LanguageService.t(cat['name'] as String)),
          // SỬA LỖI DEPRECATED TẠI ĐÂY:
          backgroundColor: Colors.orange.withValues(alpha: 0.05),
          side: BorderSide(color: Colors.orange.withValues(alpha: 0.2)),
          onPressed: () {},
        )).toList(),
      ),
    );
  }

  Widget _buildVerticalItem(POI poi) {
    final String name = LanguageService.currentLocale.value == 'vi' ? poi.name : poi.nameEn;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(15),
        side: BorderSide(color: Colors.grey.withValues(alpha: 0.1))
      ),
      child: ListTile(
        contentPadding: const EdgeInsets.all(10),
        leading: ClipRRect(
          borderRadius: BorderRadius.circular(10), 
          child: Image.asset(
            'assets/images/${poi.imageAsset}', 
            width: 60, height: 60, fit: BoxFit.cover,
            errorBuilder: (_,__,___) => const Icon(Icons.image, size: 50),
          )
        ),
        title: Text(name, style: const TextStyle(fontWeight: FontWeight.bold)),
        subtitle: Padding(
          padding: const EdgeInsets.only(top: 5),
          child: Row(
            children: [
              const Icon(Icons.star, color: Colors.amber, size: 16),
              const Text(" 4.8", style: TextStyle(fontWeight: FontWeight.w500)),
              const SizedBox(width: 10),
              Text("• District 4", style: TextStyle(color: Colors.grey[600], fontSize: 13)),
            ],
          ),
        ),
        trailing: const Icon(Icons.arrow_forward_ios_rounded, size: 16, color: Colors.grey),
        onTap: () {},
      ),
    );
  }

  void _showLanguageDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: Text(LanguageService.t('language_choice'), style: const TextStyle(fontWeight: FontWeight.bold)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildLangOption(context, "Tiếng Việt", 'vi', Colors.blue),
            const Divider(),
            _buildLangOption(context, "English", 'en', Colors.red),
          ],
        ),
      ),
    );
  }

  Widget _buildLangOption(BuildContext context, String title, String code, Color color) {
    return ListTile(
      leading: Icon(Icons.language, color: color),
      title: Text(title, style: const TextStyle(fontWeight: FontWeight.w500)),
      onTap: () { 
        LanguageService.changeLanguage(code); 
        Navigator.pop(context); 
      },
    );
  }
}