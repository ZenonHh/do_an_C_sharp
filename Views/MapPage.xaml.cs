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
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class MapPage : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _dbService;
    
    // Thuộc tính để XAML Binding
    public ILanguageService Lang { get; } 
    
    private List<AudioPOI> _pois = new();
    private IDispatcherTimer? _radarTimer;
    private CancellationTokenSource? _ttsCancellationTokenSource;
    private AudioPOI? _currentPoi;
    private bool _isPlaying = false;
    private bool _isManualSelection = false;

    // --- CONSTRUCTOR ---
    public MapPage(MapViewModel viewModel, DatabaseService dbService, ILanguageService langService)
    {
        _dbService = dbService;
        Lang = langService; 
        BindingContext = viewModel;
        InitializeComponent();
        
        try
        {
            SetupMap();
            // Load dữ liệu NGAY TẠI ĐÂY để Pin hiện lên cùng lúc với Map
            _ = LoadDataFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in MapPage Constructor: {ex}");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            StartRadar();
            
            // CÁCH SỬA LỖI ĐĂNG KÝ NHIỀU LẦN: Hủy cái cũ trước khi đăng ký cái mới
            WeakReferenceMessenger.Default.Unregister<QrScannedMessage>(this);
            WeakReferenceMessenger.Default.Register<QrScannedMessage>(this, (r, m) =>
            {
                string qrValue = m.Value;
                var foundPoi = _pois.FirstOrDefault(p => p.Name != null && p.Name.ToLower() == qrValue.ToLower());

                if (foundPoi != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _isManualSelection = true;
                        StopAudio();
                        PlayAudioAlert(foundPoi);
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Thông báo", $"Không tìm thấy thông tin cho mã: {qrValue}", "OK");
                    });
                }
            });
            
            await UpdateUserAvatarAsync();
            UpdateUI_Language();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in MapPage.OnAppearing: {ex}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAudio();
        _radarTimer?.Stop();
        
        // HỦY ĐĂNG KÝ để không bị lỗi "Target recipient has already subscribed" khi quay lại
        WeakReferenceMessenger.Default.Unregister<QrScannedMessage>(this);
    }

    private async Task UpdateUserAvatarAsync()
    {
        try
        {
            var currentUser = await _dbService.GetCurrentUserAsync();
            if (currentUser != null && !string.IsNullOrEmpty(currentUser.Avatar))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MapUserAvatar.Source = currentUser.Avatar;
                });
            }
        }
        catch { }
    }

    private async Task LoadDataFromDatabaseAsync()
    {
        try
        {
            _pois = await _dbService.GetPOIsAsync();

            try
            {
                var locationPermissionGranted = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted;
                if (locationPermissionGranted)
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
                else
                {
                    foreach (var poi in _pois) poi.DistanceInfo = "📍 Cần cấp quyền vị trí";
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
            System.Diagnostics.Debug.WriteLine($"LoadDataFromDatabaseAsync Error: {ex.Message}");
        }
    }

    private async Task<string> TranslateTextAsync(string text, string toLang)
    {
        if (string.IsNullOrEmpty(text) || toLang == "vi") 
            return text.Replace("'", "").Replace("\"", ""); 

        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var response = await client.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            string fullText = "";
            foreach (var element in json.RootElement[0].EnumerateArray())
            {
                if (element[0].ValueKind == JsonValueKind.String)
                {
                    fullText += element[0].GetString();
                }
            }

            return fullText.Replace("'", "").Replace("\"", "");
        }
        catch 
        { 
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
        if (_radarTimer != null) _radarTimer.Stop();
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
        MainThread.BeginInvokeOnMainThread(() => {
            AudioPlayerUI.IsVisible = false;
            PlayStopButton.Text = "▶️";
        });
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        try
        {
            string action = await DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null, "Tiếng Việt", "English", "日本語", "한국어");
            if (string.IsNullOrEmpty(action) || action == "Hủy") return;

            string langCode = "vi";
            if (action == "English") langCode = "en";
            else if (action == "日本語") langCode = "ja";
            else if (action == "한국어") langCode = "ko";

            Lang.ChangeLanguage(langCode);

            if (_currentPoi != null)
            {
                if (PoiDetailCard.IsVisible) _ = UpdateDetailCardAsync(_currentPoi);
                else PlayAudioAlert(_currentPoi);
            }
        }
        catch { }
    }

    private async void PlayAudioAlert(AudioPOI poi)
    {
        try
        {
            _currentPoi = poi;
            _isPlaying = true;

            await _dbService.SavePlayHistoryAsync(poi);

            string currentLang = Lang.CurrentLocale;

            MainThread.BeginInvokeOnMainThread(() => {
                TranslationLoader.IsVisible = true;
                TranslationLoader.IsRunning = true;
                AudioText.Text = currentLang == "vi" ? poi.Name : "Đang dịch...";
                AudioPlayerUI.IsVisible = true;
                PoiDetailCard.IsVisible = false;
                PlayStopButton.Text = "⏹";
            });

            string tName = await TranslateTextAsync(poi.Name, currentLang);
            string tDesc = await TranslateTextAsync(poi.Description, currentLang);

            MainThread.BeginInvokeOnMainThread(() => {
                TranslationLoader.IsRunning = false;
                TranslationLoader.IsVisible = false;
                AudioText.Text = tName;
                AudioStatusLabel.Text = currentLang == "vi" ? "Đang phát review:" : "Playing review:";
            });

            _ttsCancellationTokenSource?.Cancel();
            _ttsCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var locales = await TextToSpeech.Default.GetLocalesAsync();
                Locale? locale = null;

                if (currentLang == "en") locale = locales.FirstOrDefault(l => l.Language.Equals("en-US", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
                else if (currentLang == "ja") locale = locales.FirstOrDefault(l => l.Language.Equals("ja-JP", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("ja", StringComparison.OrdinalIgnoreCase));
                else if (currentLang == "ko") locale = locales.FirstOrDefault(l => l.Language.Equals("ko-KR", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("ko", StringComparison.OrdinalIgnoreCase));
                else locale = locales.FirstOrDefault(l => l.Language.Equals("vi-VN", StringComparison.OrdinalIgnoreCase)) ?? locales.FirstOrDefault(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase));

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
        catch
        {
            _isPlaying = false;
            MainThread.BeginInvokeOnMainThread(() => { 
                AudioPlayerUI.IsVisible = false;
                PlayStopButton.Text = "▶️";
            });
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
        string currentLang = Lang.CurrentLocale;

        MainThread.BeginInvokeOnMainThread(() => {
            DetailName.Text = "Đang tải...";
            DetailDistance.Text = "📍 Đang tính..."; 
            PoiDetailCard.IsVisible = true;
            AudioPlayerUI.IsVisible = false;
        });

        string tName = await TranslateTextAsync(poi.Name, currentLang);
        string tDesc = await TranslateTextAsync(poi.Description, currentLang);

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
            DetailDescription.Text = tDesc;
            DetailDistance.Text = string.IsNullOrEmpty(currentDistance) ? "📍 Chưa xác định" : currentDistance;
            DetailImage.Source = poi.ImageAsset;

            if (PlayReviewButton != null)
                PlayReviewButton.Text = currentLang == "vi" ? "🔊 Nghe Review" : "🔊 Listen Review";
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

    private async void OnGetDirectionsClicked(object sender, EventArgs e)
    {
        if (_currentPoi == null) return;

        try
        {
            var location = new Location(_currentPoi.Lat, _currentPoi.Lng);
            var options = new MapLaunchOptions
            {
                Name = _currentPoi.Name,
                NavigationMode = NavigationMode.Driving
            };

            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không thể mở ứng dụng bản đồ: " + ex.Message, "OK");
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

    private void UpdateUI_Language()
    {
        string lang = Lang.CurrentLocale;

        if (lang == "en")
        {
            SearchEntry.Placeholder = "Search restaurants, snails...";
            if (PlayReviewButton != null) PlayReviewButton.Text = "🔊 Listen Review";
            AudioStatusLabel.Text = "Playing review:";
        }
        else if (lang == "ja")
        {
            SearchEntry.Placeholder = "レストランや料理を検索...";
            if (PlayReviewButton != null) PlayReviewButton.Text = "🔊 レビューを聞く";
            AudioStatusLabel.Text = "レビューを再生中:";
        }
        else if (lang == "ko")
        {
            SearchEntry.Placeholder = "식당, 음식 검색...";
            if (PlayReviewButton != null) PlayReviewButton.Text = "🔊 리뷰 듣기";
            AudioStatusLabel.Text = "리뷰 재생 중:";
        }
        else 
        {
            SearchEntry.Placeholder = "Tìm quán ăn, món ốc...";
            if (PlayReviewButton != null) PlayReviewButton.Text = "🔊 Nghe Review";
            AudioStatusLabel.Text = "Đang phát review:";
        }
    }
}