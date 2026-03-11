import 'package:flutter_tts/flutter_tts.dart';
import '../models/poi_model.dart';
import 'language_service.dart';

class AudioService {
  final FlutterTts _tts = FlutterTts();
  bool _isPlaying = false; 
  bool _isPaused = false;
  POI? _lastPlayedPOI;
  String? _lastLang; 
  
  List<String> _sentences = [];
  int _currentIndex = 0;

  bool get isPlaying => _isPlaying;
  bool get isPaused => _isPaused;

  AudioService() {
    _tts.setCompletionHandler(() {
      if (!_isPaused && _currentIndex < _sentences.length - 1) {
        _currentIndex++;
        _speakCurrent();
      } else {
        _isPlaying = false;
        _currentIndex = 0;
      }
    });
  }

  Future<void> playPOI(POI poi) async {
    String currentLang = LanguageService.currentLocale.value;

    // SỬA LỖI UNUSED_FIELD & NHẦM TIẾNG:
    // Chỉ thoát nếu CÙNG quán VÀ CÙNG ngôn ngữ. 
    // Nếu đổi từ vi sang en, biến _lastLang sẽ khác currentLang -> máy sẽ phát lại tiếng mới ngay.
    if (_lastPlayedPOI?.id == poi.id && 
        _lastLang == currentLang && 
        (_isPlaying || _isPaused)) {
      return; 
    }

    // Xóa sạch danh sách câu cũ để tránh đọc nhầm câu cũ bằng giọng mới
    _sentences = []; 
    await _tts.stop(); 
    
    // Đã sử dụng biến -> Hết lỗi unused_field
    _lastPlayedPOI = poi; 
    _lastLang = currentLang; 
    
    String ttsLang = (currentLang == 'vi') ? "vi-VN" : "en-US";
    await _tts.setLanguage(ttsLang);
    await _tts.setSpeechRate(0.5);

    _isPlaying = true;
    _isPaused = false;

    // Lấy đúng ngăn dữ liệu từ POI model của bạn
    String textToSpeak = (currentLang == 'vi') ? poi.description : poi.descriptionEn;

    _sentences = textToSpeak.split(RegExp(r'(?<=[.!?])\s+'));
    _currentIndex = 0; 

    if (_sentences.isNotEmpty) {
      await _speakCurrent();
    }
  }

  Future<void> _speakCurrent() async {
    if (_currentIndex < _sentences.length && _isPlaying) {
      await _tts.speak(_sentences[_currentIndex]);
    }
  }

  Future<void> pause() async { _isPaused = true; _isPlaying = false; await _tts.stop(); }
  Future<void> resume() async { if (_isPaused) { _isPaused = false; _isPlaying = true; await _speakCurrent(); } }
  
  Future<void> stop() async {
    await _tts.stop();
    _isPlaying = false; _isPaused = false; _currentIndex = 0;
    _lastPlayedPOI = null;
    _lastLang = null; 
  }

  void dispose() => _tts.stop();
}