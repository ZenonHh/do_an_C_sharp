import 'package:latlong2/latlong.dart';

enum NarrationType { tts, audio }

class POI {
  final String id;
  final String name;           // Tiếng Việt
  final String nameEn;         // Tiếng Anh
  final LatLng location;
  final double radius;
  final int priority;
  final String description;    // Tiếng Việt
  final String descriptionEn;  // Tiếng Anh
  final String? imageAsset;
  final String? mapLink;
  final NarrationType narrationType;
  final String? content;       // Để dấu ? để không bắt buộc phải điền

  POI({
    required this.id,
    required this.name,
    required this.nameEn,
    required this.location,
    required this.radius,
    this.priority = 0,
    required this.description,
    required this.descriptionEn,
    this.imageAsset,
    this.mapLink,
    this.narrationType = NarrationType.tts,
    this.content, // Không có chữ required ở đây
  });
}