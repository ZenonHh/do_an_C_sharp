using BruTile.Predefined;
using BruTile.Web;
using DoAnCSharp.Models;
using DoAnCSharp.Services;
using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace DoAnCSharp.Views;

public partial class MapPage : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _dbService;
    private readonly ILanguageService _langService;
    private List<AudioPOI> _pois = new();
    private IDispatcherTimer? _radarTimer;
    private CancellationTokenSource? _ttsCancellationTokenSource;
    private AudioPOI? _currentPoi;
    private bool _isPlaying = false;
    private bool _isManualSelection = false;
    private string _targetLang = "vi";

    // --- CONSTRUCTOR ---
    public MapPage(DatabaseService dbService, ILanguageService langService)
    {
        _dbService = dbService;
        _langService = langService; 

        try
        {
            InitializeComponent();
            SetupMap();
            StartRadar();

            // Đăng ký nhận dữ liệu từ QR
            WeakReferenceMessenger.Default.Register<QrScannedMessage>(this, (r, m) =>
            {
                string qrValue = m.Value;
                var foundPoi = _pois.FirstOrDefault(p => p.Name != null && p.Name.ToLower() == qrValue.ToLower());

                if (foundPoi != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _isManualSelection = true; // Chặn Radar
                        StopAudio(); 
                        PlayAudioAlert(foundPoi); // Chỉ gọi Audio để tránh xung đột UI
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Thông báo", $"Không tìm thấy thông tin cho mã: {qrValue}", "OK");
                    });
                }
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Dispatch(async () => await DisplayAlert("Lỗi khởi tạo", ex.Message, "OK"));
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataFromDatabaseAsync();
    }

    private async Task LoadDataFromDatabaseAsync()
    {
        try
        {
            _pois = await _dbService.GetPOIsAsync();

            try
            {
                var userLoc = await Geolocation.Default.GetLastKnownLocationAsync() ??
                              await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(2)));

                if (userLoc != null)
                {
                    foreach (var poi in _pois)
                    {
                        double distanceKm = Location.CalculateDistance(userLoc, poi.Lat, poi.Lng, DistanceUnits.Kilometers);
                        string distStr = distanceKm < 1 ? $"{(int)(distanceKm * 1000)}m" : $"{Math.Round(distanceKm, 1)}km";
                        int walkMinutes = Math.Max(1, (int)(distanceKm * 12));
                        poi.DistanceInfo = $"📍 {distStr}  •  🚶 {walkMinutes} phút";
                    }
                }
            }
            catch
            {
                foreach (var poi in _pois) poi.DistanceInfo = "📍 Chưa có định vị";
            }

            LoadPinsToMap();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể tải dữ liệu: " + ex.Message, "OK");
        }
    }

    // --- FIX LỖI ĐỌC 1 CÂU ---
    private async Task<string> TranslateTextAsync(string text, string toLang)
{
    // Cắt dấu nháy để tránh lỗi ngắt âm TTS
    if (string.IsNullOrEmpty(text) || toLang == "vi") 
        return text.Replace("'", "").Replace("\"", ""); 

    try
    {
        string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";
        using var client = new HttpClient();
        
        // ĐÃ KHÔI PHỤC: Khai báo đầy đủ User-Agent để Google không chặn
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        
        var response = await client.GetStringAsync(url);
        var json = JsonDocument.Parse(response);

        string fullText = "";
        foreach (var element in json.RootElement[0].EnumerateArray())
        {
            // Kiểm tra an toàn trước khi lấy chuỗi
            if (element[0].ValueKind == JsonValueKind.String)
            {
                fullText += element[0].GetString();
            }
        }

        return fullText.Replace("'", "").Replace("\"", "");
    }
    catch 
    { 
        // Nếu kẹt mạng, trả về tiếng Việt gốc
        return text.Replace("'", "").Replace("\"", ""); 
    }
}

    private void SetupMap()
    {
        if (foodMapView.Map == null) foodMapView.Map = new Mapsui.Map();
        foodMapView.Map.Layers.Clear();
        
        var tileSource = new HttpTileSource(
            new GlobalSphericalMercator(), 
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", 
            new[] { "a", "b", "c" }, 
            name: "OpenStreetMap");
            
        foodMapView.Map.Layers.Add(new TileLayer(tileSource));
        var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
        foodMapView.Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
        foodMapView.MyLocationEnabled = true;
    }

    private void StartRadar()
    {
        _radarTimer = Dispatcher.CreateTimer();
        _radarTimer.Interval = TimeSpan.FromSeconds(3);
        _radarTimer.Tick += async (s, e) => await CheckGeofenceAndPlayAudio();
        _radarTimer.Start();
    }

    private async Task CheckGeofenceAndPlayAudio()
    {
        try
        {
            if (_isManualSelection) return;

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(2));
            var userLoc = await Geolocation.Default.GetLocationAsync(request);

            if (userLoc == null) return;

            MainThread.BeginInvokeOnMainThread(() => {
                foodMapView.MyLocationLayer?.UpdateMyLocation(new Mapsui.UI.Maui.Position(userLoc.Latitude, userLoc.Longitude));
            });

            var poi = _pois.FirstOrDefault(p => Location.CalculateDistance(userLoc, new Location(p.Lat, p.Lng), DistanceUnits.Kilometers) * 1000 <= p.Radius);

            if (poi != null)
            {
                if (_currentPoi != poi) PlayAudioAlert(poi);
            }
            else
            {
                if (_currentPoi != null && !_isManualSelection)
                {
                    StopAudio();
                    _currentPoi = null;
                }
            }
        }
        catch { }
    }

    private void StopAudio()
    {
        _ttsCancellationTokenSource?.Cancel();
        _isPlaying = false;
        // Đã xóa dòng _isManualSelection = false; để tránh xung đột Radar
        MainThread.BeginInvokeOnMainThread(() => {
            AudioPlayerUI.IsVisible = false;
            PlayStopButton.Text = "▶️";
        });
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null, "Tiếng Việt", "English", "日本語", "한국어");
        if (string.IsNullOrEmpty(action) || action == "Hủy") return;

        if (action == "Tiếng Việt") _targetLang = "vi";
        else if (action == "English") _targetLang = "en";
        else if (action == "日本語") _targetLang = "ja";
        else if (action == "한국어") _targetLang = "ko";

        if (_currentPoi != null)
        {
            if (PoiDetailCard.IsVisible) _ = UpdateDetailCardAsync(_currentPoi);
            else PlayAudioAlert(_currentPoi);
        }
    }

    private async void PlayAudioAlert(AudioPOI poi)
    {
        _currentPoi = poi;
        _isPlaying = true;

        await _dbService.SavePlayHistoryAsync(poi);

        MainThread.BeginInvokeOnMainThread(() => {
            TranslationLoader.IsVisible = true;
            TranslationLoader.IsRunning = true;
            AudioText.Text = _targetLang == "vi" ? poi.Name : "Đang dịch...";
            AudioPlayerUI.IsVisible = true;
            PoiDetailCard.IsVisible = false;
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

            if (_targetLang == "en") locale = locales.FirstOrDefault(l => l.Language.Equals("en-US", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
            else if (_targetLang == "ja") locale = locales.FirstOrDefault(l => l.Language.Equals("ja-JP", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("ja", StringComparison.OrdinalIgnoreCase));
            else if (_targetLang == "ko") locale = locales.FirstOrDefault(l => l.Language.Equals("ko-KR", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("ko", StringComparison.OrdinalIgnoreCase));
            else locale = locales.FirstOrDefault(l => l.Language.Equals("vi-VN", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase));

            // --- CHIA ĐỂ TRỊ (ĐỌC TỪNG CÂU) ---
            string cleanText = tDesc.Replace("'", "").Replace("\"", "").Replace("\n", " ");
            var sentences = cleanText.Split(new[] { '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sentence in sentences)
            {
                if (_ttsCancellationTokenSource.Token.IsCancellationRequested) break;
                if (string.IsNullOrWhiteSpace(sentence)) continue;
                await TextToSpeech.Default.SpeakAsync(sentence.Trim(), new SpeechOptions { Locale = locale }, _ttsCancellationTokenSource.Token);
            }
        }
        catch { }
        finally
        {
            _isPlaying = false;
            MainThread.BeginInvokeOnMainThread(() => { PlayStopButton.Text = "▶️"; });
        }
    }

    private void OnMapPinClicked(object? sender, Mapsui.UI.Maui.PinClickedEventArgs e)
    {
        if (e.Pin?.Tag is AudioPOI clickedPoi)
        {
            _isManualSelection = true;
            StopAudio();
            _ = UpdateDetailCardAsync(clickedPoi);
        }
        e.Handled = true;
    }

    private async Task UpdateDetailCardAsync(AudioPOI poi)
    {
        _currentPoi = poi;

        MainThread.BeginInvokeOnMainThread(() => {
            DetailName.Text = "Đang tải...";
            PoiDetailCard.IsVisible = true;
            AudioPlayerUI.IsVisible = false;
        });

        string tName = await TranslateTextAsync(poi.Name, _targetLang);
        string tDesc = await TranslateTextAsync(poi.Description, _targetLang);

        string currentDistance = poi.DistanceInfo;
        try
        {
            var userLoc = await Geolocation.Default.GetLastKnownLocationAsync();
            if (userLoc != null)
            {
                double distanceKm = Location.CalculateDistance(userLoc, poi.Lat, poi.Lng, DistanceUnits.Kilometers);
                string distStr = distanceKm < 1 ? $"{(int)(distanceKm * 1000)}m" : $"{Math.Round(distanceKm, 1)}km";
                int walkMinutes = Math.Max(1, (int)(distanceKm * 12));
                currentDistance = $"📍 {distStr}  •  🚶 {walkMinutes} phút";
                poi.DistanceInfo = currentDistance; 
            }
        }
        catch { }

        MainThread.BeginInvokeOnMainThread(() => {
            DetailName.Text = tName;
            // Gộp Distance vào Description, KHÔNG dùng DetailDistance để tránh lỗi CS0103
            DetailDescription.Text = string.IsNullOrEmpty(currentDistance) ? tDesc : $"{currentDistance}\n\n{tDesc}";
            DetailImage.Source = poi.ImageAsset;

            if (PlayReviewButton != null)
                PlayReviewButton.Text = _targetLang == "vi" ? "🔊 Nghe Review" : "🔊 Listen Review";
        });
    }

    private void LoadPinsToMap()
    {
        if (foodMapView == null || _pois == null) return;

        Dispatcher.Dispatch(() =>
        {
            foodMapView.Pins.Clear();

            foreach (var poi in _pois)
            {
                foodMapView.Pins.Add(new Mapsui.UI.Maui.Pin(foodMapView) { 
                    Label = poi.Name, 
                    Position = new Mapsui.UI.Maui.Position(poi.Lat, poi.Lng), 
                    Tag = poi, 
                    Color = Microsoft.Maui.Graphics.Colors.Red 
                });
            }

            foodMapView.PinClicked -= OnMapPinClicked;
            foodMapView.PinClicked += OnMapPinClicked;
            foodMapView.Refresh();
        });
    }

    private void CustomMyLocationClicked(object? sender, EventArgs e)
    {
        var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
        foodMapView.Map?.Navigator.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
    }

    private void ToggleAudioClicked(object? sender, EventArgs e)
    {
        if (_isPlaying) StopAudio();
        else if (_currentPoi != null) PlayAudioAlert(_currentPoi);
    }

    private void CloseDetailClicked(object? sender, EventArgs e)
    {
        PoiDetailCard.IsVisible = false;
        _isManualSelection = false;
    }

    private void PlayReviewFromDetailClicked(object? sender, EventArgs e)
    {
        if (_currentPoi != null)
        {
            _isManualSelection = true;
            PoiDetailCard.IsVisible = false;
            PlayAudioAlert(_currentPoi);
        }
    }

    private void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        string searchText = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(searchText))
        {
            LoadPinsToMap();
            SearchSuggestionsContainer.IsVisible = false;
            return;
        }

        var matchingPois = _pois.Where(p => p.Name != null && p.Name.ToLower().Contains(searchText)).Take(3).ToList();

        if (matchingPois.Any())
        {
            SuggestionsList.ItemsSource = matchingPois;
            SearchSuggestionsContainer.IsVisible = true;
        }
        else
        {
            SearchSuggestionsContainer.IsVisible = false;
        }
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is AudioPOI selectedPoi)
        {
            SearchSuggestionsContainer.IsVisible = false;
            SearchEntry.Unfocus();
            LoadPinsToMap();

            if (foodMapView.Map != null)
            {
                var point = SphericalMercator.FromLonLat(selectedPoi.Lng, selectedPoi.Lat);
                foodMapView.Map.Navigator.CenterOnAndZoomTo(new MPoint(point.x, point.y), 2);
            }

            _isManualSelection = true;
            StopAudio();
            _ = UpdateDetailCardAsync(selectedPoi);
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("SelectedPOI") && query["SelectedPOI"] is AudioPOI poi)
        {
            query.Remove("SelectedPOI");
            await Task.Delay(500);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (foodMapView.Map != null)
                {
                    var point = SphericalMercator.FromLonLat(poi.Lng, poi.Lat);
                    foodMapView.Map.Navigator.CenterOnAndZoomTo(new MPoint(point.x, point.y), 2);
                }

                _isManualSelection = true;
                StopAudio();
                _ = UpdateDetailCardAsync(poi);
                PlayAudioAlert(poi);
            });
        }
    }

    private async void OnScanQRClicked(object sender, EventArgs e) 
    {
        try 
        {
            await Navigation.PushAsync(new ScanQRPage()); 
        }
        catch (Exception ex) 
        {
            await DisplayAlert("Lỗi", "Không thể mở camera: " + ex.Message, "OK"); 
        }
    }
}