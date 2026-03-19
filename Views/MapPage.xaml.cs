using Microsoft.Maui.Controls;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;

namespace DoAnCSharp.Views;

public class AudioPOI
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Radius { get; set; }
    public int Priority { get; set; }
    public string ImageAsset { get; set; } = string.Empty;
}

public partial class MapPage : ContentPage
{
    private List<AudioPOI> _pois = new();
    private IDispatcherTimer? _radarTimer;
    private CancellationTokenSource? _ttsCancellationTokenSource;
    private AudioPOI? _currentPoi;
    private bool _isPlaying = false;
    
    // BIẾN QUAN TRỌNG: Khóa Radar khi người dùng đang tự chọn quán
    private bool _isManualSelection = false; 
    private string _targetLang = "vi"; 

    public MapPage()
    {
        try 
        {
            InitializeComponent();
            SetupMap();
            SetupPOIs();
            LoadPinsToMap();
            StartRadar();
        }
        catch (Exception ex)
        {
            Dispatcher.Dispatch(async () => await DisplayAlert("LỖI", ex.Message, "OK"));
        }
    }

    private async Task<string> TranslateTextAsync(string text, string toLang)
    {
        if (string.IsNullOrEmpty(text) || toLang == "vi") return text;
        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";
            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            return json.RootElement[0][0][0].GetString() ?? text;
        }
        catch { return text; }
    }

    private void SetupMap()
    {
        if (foodMapView.Map != null)
        {
            foodMapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer("VinhKhanhFoodTourApp"));
            var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
            foodMapView.Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
        }
        foodMapView.MyLocationEnabled = true;
    }

    private void SetupPOIs()
    {
        _pois = new List<AudioPOI>
        {
            new AudioPOI { Name = "Ốc Oanh", Description = "Bạn đã đến Ốc Oanh. Món ăn biểu tượng ở đây là ốc hương xào bắp bơ siêu ngon.", Lat = 10.7600, Lng = 106.7000, Radius = 50, Priority = 1, ImageAsset = "oc_oanh.jpg" },
            new AudioPOI { Name = "Ốc Đào 2", Description = "Chi nhánh Ốc Đào nổi tiếng, nổi bật với nước chấm đậm đà.", Lat = 10.7581, Lng = 106.7061, Radius = 50, Priority = 1, ImageAsset = "oc_dao2.webp" },
            new AudioPOI { Name = "Ốc Vũ", Description = "Quán ăn dành cho tín đồ mê ốc mỡ và bơ tỏi.", Lat = 10.7578, Lng = 106.7058, Radius = 50, Priority = 2, ImageAsset = "dotnet_bot.png" }
        };
    }

    private void StartRadar()
    {
        _radarTimer = Dispatcher.CreateTimer();
        _radarTimer.Interval = TimeSpan.FromSeconds(3); 
        _radarTimer.Tick += async (s, e) => await CheckGeofenceAndPlayAudio();
        _radarTimer.Start();
    }

    // --- LOGIC RADAR THÔNG MINH ---
    private async Task CheckGeofenceAndPlayAudio()
    {
        try
        {
            // NẾU ĐANG CHỦ ĐỘNG NGHE QUÁN KHÁC -> TẠM NGỦ RADAR ĐỂ KHÔNG BỊ CHÈN ĐÈ
            if (_isManualSelection) return;

            var userLoc = new Location(10.7600, 106.7000); // Đang giả lập đứng tại Ốc Oanh
            
            MainThread.BeginInvokeOnMainThread(() => {
                foodMapView.MyLocationLayer?.UpdateMyLocation(new Mapsui.UI.Maui.Position(userLoc.Latitude, userLoc.Longitude));
            });

            var poi = _pois.FirstOrDefault(p => Location.CalculateDistance(userLoc, new Location(p.Lat, p.Lng), DistanceUnits.Kilometers) * 1000 <= p.Radius);

            if (poi != null)
            {
                if (_currentPoi != poi) 
                {
                    PlayAudioAlert(poi); // Bước vào vùng -> Tự động phát
                }
            }
            else
            {
                // Ra khỏi vùng -> Dừng phát và ẩn thanh Player
                if (_currentPoi != null)
                {
                    StopAudio();
                    _currentPoi = null;
                }
            }
        } catch { }
    }

    private void StopAudio()
    {
        _ttsCancellationTokenSource?.Cancel();
        _isPlaying = false;
        
        // Khi bấm Dừng (Stop), mở khóa lại cho Radar hoạt động
        _isManualSelection = false; 

        MainThread.BeginInvokeOnMainThread(() => {
            AudioPlayerUI.IsVisible = false;
            PlayStopButton.Text = "▶️";
        });
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null, "Tiếng Việt", "English", "日本語 (Japanese)", "한국어 (Korean)");
        if (string.IsNullOrEmpty(action) || action == "Hủy") return;

        if (action == "Tiếng Việt") _targetLang = "vi";
        else if (action == "English") _targetLang = "en";
        else if (action == "日本語 (Japanese)") _targetLang = "ja";
        else if (action == "한국어 (Korean)") _targetLang = "ko";

        // Cập nhật giao diện nếu đang mở
        if (_currentPoi != null)
        {
            if (PoiDetailCard.IsVisible) await UpdateDetailCardAsync(_currentPoi);
            else if (_isPlaying) PlayAudioAlert(_currentPoi);
        }
    }

    private async void PlayAudioAlert(AudioPOI poi)
    {
        _currentPoi = poi;
        _isPlaying = true;

        MainThread.BeginInvokeOnMainThread(() => {
            TranslationLoader.IsVisible = true;
            TranslationLoader.IsRunning = true;
            AudioText.Text = _targetLang == "vi" ? poi.Name : "Translating...";
            AudioPlayerUI.IsVisible = true;
            PoiDetailCard.IsVisible = false; // Đảm bảo ẩn Card đi để không bị chồng chéo
            PlayStopButton.Text = "⏹";
        });

        string tName = await TranslateTextAsync(poi.Name, _targetLang);
        string tDesc = await TranslateTextAsync(poi.Description, _targetLang);

        MainThread.BeginInvokeOnMainThread(() => { 
            TranslationLoader.IsRunning = false;
            TranslationLoader.IsVisible = false;
            AudioText.Text = tName; 
            AudioStatusLabel.Text = _targetLang == "vi" ? "Đang phát review:" : "Playing review:"; 
        });

        _ttsCancellationTokenSource?.Cancel();
        _ttsCancellationTokenSource = new CancellationTokenSource();

        try 
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            Locale? locale = null;

            // --- ÉP BUỘC TÌM ĐÚNG GIỌNG BẰNG MÃ CHUẨN ANDROID ---
            if (_targetLang == "en") 
            {
                // Ưu tiên tìm giọng Mỹ (en-US), nếu không có thì tìm giọng Anh bất kỳ
                locale = locales.FirstOrDefault(l => l.Language.Equals("en-US", StringComparison.OrdinalIgnoreCase)) 
                      ?? locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            }
            else if (_targetLang == "ja") 
            {
                locale = locales.FirstOrDefault(l => l.Language.Equals("ja-JP", StringComparison.OrdinalIgnoreCase))
                      ?? locales.FirstOrDefault(l => l.Language.StartsWith("ja", StringComparison.OrdinalIgnoreCase));
            }
            else if (_targetLang == "ko") 
            {
                locale = locales.FirstOrDefault(l => l.Language.Equals("ko-KR", StringComparison.OrdinalIgnoreCase))
                      ?? locales.FirstOrDefault(l => l.Language.StartsWith("ko", StringComparison.OrdinalIgnoreCase));
            }
            else 
            {
                // Tiếng Việt
                locale = locales.FirstOrDefault(l => l.Language.Equals("vi-VN", StringComparison.OrdinalIgnoreCase))
                      ?? locales.FirstOrDefault(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase));
            }

            // Gọi hàm đọc với giọng đã được chỉ định rõ ràng
            await TextToSpeech.Default.SpeakAsync(tDesc, new SpeechOptions { 
                Locale = locale 
            }, _ttsCancellationTokenSource.Token);
        } 
        catch { }
        finally 
        {
            _isPlaying = false;
            MainThread.BeginInvokeOnMainThread(() => { 
                PlayStopButton.Text = "▶️"; 
            });
        }
    }

    private void OnMapPinClicked(object? sender, Mapsui.UI.Maui.PinClickedEventArgs e)
    {
        if (e.Pin?.Tag is AudioPOI clickedPoi)
        {
            // Bấm vào Pin -> CHỈ hiện Card chi tiết, KHÔNG tự động phát nhạc để tránh đè giao diện
            _isManualSelection = true; // Khóa Radar
            _ = UpdateDetailCardAsync(clickedPoi);
        }
        e.Handled = true;
    }

    private async Task UpdateDetailCardAsync(AudioPOI poi)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            DetailName.Text = "Translating...";
            PoiDetailCard.IsVisible = true;
            AudioPlayerUI.IsVisible = false; // Ẩn thanh Player đi
        });

        string tName = await TranslateTextAsync(poi.Name, _targetLang);
        string tDesc = await TranslateTextAsync(poi.Description, _targetLang);

        MainThread.BeginInvokeOnMainThread(() => {
            DetailName.Text = tName;
            DetailDescription.Text = tDesc;
            DetailImage.Source = poi.ImageAsset;
        });
    }

    private void LoadPinsToMap()
    {
        foodMapView.Pins.Clear();
        foreach (var poi in _pois)
        {
            foodMapView.Pins.Add(new Mapsui.UI.Maui.Pin(foodMapView) { Label = poi.Name, Position = new Mapsui.UI.Maui.Position(poi.Lat, poi.Lng), Tag = poi, Color = Microsoft.Maui.Graphics.Colors.Red });
        }
        foodMapView.PinClicked -= OnMapPinClicked;
        foodMapView.PinClicked += OnMapPinClicked;
    }

    private void CustomMyLocationClicked(object sender, EventArgs e)
    {
        var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
        foodMapView.Map?.Navigator.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
    }

    private void ToggleAudioClicked(object sender, EventArgs e) {
        if (_isPlaying) StopAudio(); // Tắt nhạc và mở khóa Radar
        else if (_currentPoi != null) PlayAudioAlert(_currentPoi);
    }

    private void CloseDetailClicked(object sender, EventArgs e) {
        PoiDetailCard.IsVisible = false;
        _isManualSelection = false; // Mở khóa cho Radar chạy lại
    }

    private void PlayReviewFromDetailClicked(object sender, EventArgs e) {
        if (_currentPoi != null) { 
            PoiDetailCard.IsVisible = false; 
            PlayAudioAlert(_currentPoi); // Chuyển từ Card sang chế độ Nghe
        }
    }
}