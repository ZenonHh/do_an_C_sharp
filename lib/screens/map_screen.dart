import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:url_launcher/url_launcher.dart';
import '../models/poi_model.dart';
import '../services/poi_repository.dart';
import '../services/geofence_service.dart';
import '../services/audio_service.dart';
import '../services/language_service.dart';

class MapScreen extends StatefulWidget { 
  const MapScreen({super.key});

  @override
  State<MapScreen> createState() => _MapScreenState();
}

class _MapScreenState extends State<MapScreen> {
  static const _eventChannel = EventChannel('com.example.do_an/location_stream');

  LatLng _userPos = const LatLng(10.7583, 106.7065);
  POI? _activePOI;
  final Set<String> _playedHistory = {};
  final MapController _mapController = MapController();
  StreamSubscription? _locationSubscription;
  bool _isPlaying = false;
  late GeofenceService _geofenceService;
  final AudioService _audioService = AudioService();
  late final List<POI> _poiList;

  @override
  void initState() {
    super.initState();
    _poiList = POIRepository.getTourPoints();
    _geofenceService = GeofenceService(_poiList, cooldown: const Duration(minutes: 5));
    _startListeningLocation();
  }

  void _startListeningLocation() {
    _locationSubscription = _eventChannel.receiveBroadcastStream().listen((data) {
      if (data is Map) {
        _processLocationUpdate(LatLng(data['lat'], data['lng']));
      }
    }, onError: (err) => debugPrint("Lỗi nhận GPS: $err"));
  }

  // HÀM PHÁT ÂM THANH: Tự chọn file theo ngôn ngữ
  void _playNarrative(POI poi) {
  _audioService.playPOI(poi); // Gọi hàm playPOI để dùng máy tự đọc
  setState(() {
    _isPlaying = true;
    _activePOI = poi;
  });
}

  void _showLanguageDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(LanguageService.t('language_choice')),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            ListTile(
              leading: const Icon(Icons.language, color: Colors.blue),
              title: const Text("Tiếng Việt"),
              onTap: () {
                LanguageService.changeLanguage('vi');
                setState(() {}); // Cập nhật tên quán trên bảng sang Tiếng Việt
  
                if (_activePOI != null) {
                  // Ép AudioService dừng hẳn và xóa bộ nhớ đệm ngôn ngữ cũ
                  _audioService.stop().then((_) {
                    _audioService.playPOI(_activePOI!);
                  });
                }
                Navigator.pop(context);
              },
            ),
            ListTile(
              leading: const Icon(Icons.language, color: Colors.red),
              title: const Text("English"),
              onTap: () {
                LanguageService.changeLanguage('en'); // Hoặc 'en' tùy nút
                setState(() {}); // Để cập nhật tên quán trên bảng ngay lập tức
  
                if (_activePOI != null) {
                  // Dừng sạch âm thanh cũ rồi mới phát âm thanh mới
                  _audioService.stop().then((_) {
                  _audioService.playPOI(_activePOI!);
                });
              }
              Navigator.pop(context);
              },
            ),
          ],
        ),
      ),
    );
  }

  void _processLocationUpdate(LatLng newLoc) {
    if (!mounted) return;
    double distanceMoved = const Distance().as(LengthUnit.Meter, _userPos, newLoc);

    if (distanceMoved > 2 || _activePOI == null) {
      setState(() => _userPos = newLoc);
      POI? nearbyPOI = _geofenceService.checkPOIs(newLoc);

      if (nearbyPOI != null) {
        if (nearbyPOI.id != _activePOI?.id && !_playedHistory.contains(nearbyPOI.id) && !_isPlaying) {
          _playNarrative(nearbyPOI); // Sử dụng hàm phát theo ngôn ngữ mới
          _playedHistory.add(nearbyPOI.id);
        }
      } else if (_activePOI != null) {
        setState(() { _activePOI = null; _isPlaying = false; });
        _audioService.stop();
      }
    }
  }

  @override
  void dispose() {
    _locationSubscription?.cancel();
    _audioService.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // Lắng nghe thay đổi ngôn ngữ để UI cập nhật tức thì
    return ValueListenableBuilder<String>(
      valueListenable: LanguageService.currentLocale,
      builder: (context, locale, child) {
        return Scaffold(
          appBar: AppBar(
            title: Text(LanguageService.t('app_title'), style: const TextStyle(fontWeight: FontWeight.bold)),
            backgroundColor: Colors.white,
            foregroundColor: Colors.black,
            elevation: 0,
          ),
          body: Stack(
            children: [
              FlutterMap(
                mapController: _mapController,
                options: MapOptions(initialCenter: _userPos, initialZoom: 17.0),
                children: [
                  TileLayer(urlTemplate: 'https://tile.openstreetmap.org/{z}/{x}/{y}.png', userAgentPackageName: 'com.example.do_an'),
                  CircleLayer(
                    circles: _poiList.map((poi) => CircleMarker(
                      point: poi.location,
                      radius: poi.radius.toDouble(),
                      useRadiusInMeter: true,
                      color: _activePOI?.id == poi.id ? Colors.orange.withAlpha(77) : Colors.blue.withAlpha(25),
                      borderColor: _activePOI?.id == poi.id ? Colors.orange : Colors.blue,
                      borderStrokeWidth: 2,
                    )).toList(),
                  ),
                  MarkerLayer(
                    markers: [
                      Marker(point: _userPos, width: 60, height: 60, child: _buildUserMarker()),
                      ..._poiList.map((poi) {
                        final bool isActive = _activePOI?.id == poi.id;
                        return Marker(
                          point: poi.location,
                          width: 50, height: 50,
                          child: GestureDetector(
                            onTap: () => _showPOIDetail(poi),
                            child: Icon(Icons.restaurant, color: isActive ? Colors.orange : Colors.grey, size: isActive ? 40 : 30),
                          ),
                        );
                      }),
                    ],
                  ),
                ],
              ),

              // CỤM NÚT GÓC TRÊN BÊN PHẢI (Đã khôi phục nút vị trí)
              Positioned(
                top: 10,
                right: 10,
                child: Column(
                  children: [
                    FloatingActionButton(
                      heroTag: "btn_lang", mini: true, backgroundColor: Colors.white,
                      onPressed: () => _showLanguageDialog(context),
                      child: const Icon(Icons.translate, color: Colors.black),
                    ),
                    const SizedBox(height: 10),
                    FloatingActionButton(
                      heroTag: "btn_pos", mini: true, backgroundColor: Colors.white,
                      onPressed: () => _mapController.move(_userPos, 17.0),
                      child: const Icon(Icons.my_location, color: Colors.blue),
                    ),
                  ],
                ),
              ),

              if (_activePOI != null)
                Positioned(bottom: 30, left: 20, right: 20, child: _buildActivePOICard()),
            ],
          ),
        );
      },
    );
  }

  Widget _buildUserMarker() {
    return Container(
      decoration: BoxDecoration(
        color: Colors.blue.withAlpha(51),
        shape: BoxShape.circle,
        border: Border.all(color: Colors.white, width: 2),
      ),
      child: const Center(child: Icon(Icons.person_pin_circle, color: Colors.blue, size: 30)),
    );
  }

  Widget _buildActivePOICard() {
    return Card(
      elevation: 10,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(15)),
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(10),
              child: _activePOI!.imageAsset != null 
                ? Image.asset('assets/images/${_activePOI!.imageAsset}', width: 50, height: 50, fit: BoxFit.cover, errorBuilder: (_, __, ___) => const Icon(Icons.image_not_supported))
                : const Icon(Icons.restaurant, size: 50),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  // DÒNG 1: Hiển thị tên quán (Tự đổi Việt/Anh)
                  Text(
                    LanguageService.currentLocale.value == 'vi' ? _activePOI!.name : _activePOI!.nameEn, 
                    style: const TextStyle(fontWeight: FontWeight.bold),
                  ),
      
                  // DÒNG 2: Chỉ hiện chữ "Đang thuyết minh..." khi đang phát
                  if (_isPlaying) 
                    Text(
                      LanguageService.t('narrating'), // Phải là 'narrating' để lấy từ LanguageService
                        style: const TextStyle(color: Colors.orange, fontSize: 12),
                    ),
            ],
        ),
      ),
            IconButton(
              icon: Icon(_isPlaying ? Icons.pause_circle_filled : Icons.play_circle_filled),
              iconSize: 45, color: Colors.orange,
              onPressed: () {
                _isPlaying ? _audioService.pause() : _audioService.resume();
                setState(() => _isPlaying = !_isPlaying);
              },
            ),
          ],
        ),
      ),
    );
  }

  void _showPOIDetail(POI poi) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (context) => DraggableScrollableSheet(
        expand: false,
        initialChildSize: 0.6,
        builder: (context, scrollController) => SingleChildScrollView(
          controller: scrollController,
          child: Column(
            children: [
              Image.asset('assets/images/${poi.imageAsset}', errorBuilder: (_, __, ___) => const Icon(Icons.image_not_supported)),
              Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(poi.name, style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 10),
                    Text(poi.description),
                    const SizedBox(height: 20),
                    ElevatedButton.icon(
                      onPressed: () => _launchURL('https://www.google.com/maps/search/?api=1&query=${poi.location.latitude},${poi.location.longitude}'),
                      icon: const Icon(Icons.directions),
                      label: Text(LanguageService.t('direction')),
                      style: ElevatedButton.styleFrom(minimumSize: const Size(double.infinity, 50)),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Future<void> _launchURL(String urlString) async {
    final Uri url = Uri.parse(urlString);
    if (!await launchUrl(url, mode: LaunchMode.externalApplication)) debugPrint('Lỗi mở link: $url');
  }
}