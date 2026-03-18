using System.Text.RegularExpressions;
using DoAnCSharp.Models;

namespace DoAnCSharp.Services
{
    public interface IAudioService
    {
        bool IsPlaying { get; }
        bool IsPaused { get; }
        Task PlayPoiAsync(POI poi);
        void Pause();
        void Resume();
        void Stop();
    }

    public class AudioService : IAudioService
    {
        private readonly ITextToSpeech _tts;
        private readonly ILanguageService _languageService;
        
        private CancellationTokenSource? _cts;
        private List<string> _sentences = new();
        private int _currentIndex = 0;
        private POI? _lastPlayedPoi;
        private string? _lastLang;

        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        public AudioService(ITextToSpeech tts, ILanguageService languageService)
        {
            _tts = tts;
            _languageService = languageService;
        }

        public async Task PlayPoiAsync(POI poi)
        {
            string currentLang = _languageService.CurrentLocale;

            // Kiểm tra xem có đang phát đúng quán và đúng ngôn ngữ không
            if (_lastPlayedPoi?.Id == poi.Id && _lastLang == currentLang && (IsPlaying || IsPaused))
            {
                return;
            }

            Stop(); // Dừng mọi thứ cũ trước khi phát mới

            _lastPlayedPoi = poi;
            _lastLang = currentLang;

            string textToSpeak = (currentLang == "vi") ? poi.Description : poi.DescriptionEn;
            
            // Chia câu bằng Regex (giống bên Flutter)
            _sentences = Regex.Split(textToSpeak, @"(?<=[.!?])\s+").ToList();
            _currentIndex = 0;

            if (_sentences.Any())
            {
                await StartSpeakingLoop();
            }
        }

        private async Task StartSpeakingLoop()
        {
            IsPlaying = true;
            IsPaused = false;
            _cts = new CancellationTokenSource();

            try
            {
                while (_currentIndex < _sentences.Count && !_cts.Token.IsCancellationRequested)
                {
                    var options = new SpeechOptions
                    {
                        // vi -> vi-VN, en -> en-US
                        Locale = await GetLocale(_lastLang == "vi" ? "vi-VN" : "en-US"),
                        Pitch = 1.0f,
                        Volume = 1.0f
                    };

                    await _tts.SpeakAsync(_sentences[_currentIndex], options, _cts.Token);
                    
                    if (!_cts.Token.IsCancellationRequested)
                    {
                        _currentIndex++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Xử lý khi bị dừng hoặc tạm dừng
            }
            finally
            {
                if (_currentIndex >= _sentences.Count)
                {
                    IsPlaying = false;
                    _currentIndex = 0;
                }
            }
        }

        private async Task<Locale?> GetLocale(string languageCode)
        {
            var locales = await _tts.GetLocalesAsync();
            return locales.FirstOrDefault(l => l.Language.Contains(languageCode, StringComparison.OrdinalIgnoreCase));
        }

        public void Pause()
        {
            _cts?.Cancel(); // Ngắt luồng phát hiện tại
            IsPaused = true;
            IsPlaying = false;
        }

        public async void Resume()
        {
            if (IsPaused && _sentences.Any())
            {
                await StartSpeakingLoop();
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            IsPlaying = false;
            IsPaused = false;
            _currentIndex = 0;
            _lastPlayedPoi = null;
            _lastLang = null;
        }
    }
}
