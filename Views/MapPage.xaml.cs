using DoAnCSharp.Models;
using DoAnCSharp.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json; // Still needed for JsonSerializer
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DoAnCSharp.ViewModels;

namespace DoAnCSharp.Views;

public partial class MapPage : ContentPage, IQueryAttributable
{
    private readonly DatabaseService _dbService;
    private readonly ScanQuotaService _quotaService;
    public ILanguageService Lang { get; }

    private List<AudioPOI> _pois = new();
    private IDispatcherTimer? _radarTimer;
    private CancellationTokenSource? _ttsCancellationTokenSource;
    private AudioPOI? _currentPoi;
    private bool _isPlaying = false;
    private bool _isManualSelection = false;
    private bool _isMapSetup = false;
    private bool _isMapLoaded = false;
    private bool _isHeatmapOn = false;

    public MapPage(MapViewModel viewModel, DatabaseService dbService, ILanguageService langService, ScanQuotaService quotaService)
    {
        _dbService = dbService;
        _quotaService = quotaService;
        Lang = langService;
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Đảm bảo không đăng ký trùng khi OnAppearing được gọi nhiều lần
        WeakReferenceMessenger.Default.Unregister<QrScannedMessage>(this);
        WeakReferenceMessenger.Default.Register<QrScannedMessage>(this, async (_, msg) =>
        {
            WeakReferenceMessenger.Default.Unregister<QrScannedMessage>(this);
            string poiName = msg.Value;

            if (_pois.Count == 0)
                await LoadDataFromDatabaseAsync();

            var target = _pois.FirstOrDefault(p =>
                string.Equals(p.Name, poiName, StringComparison.OrdinalIgnoreCase));

            if (target == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                RunScript($"centerOn({target.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {target.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 17)");
                _isManualSelection = true;
                StopAudio();
                _ = UpdateDetailCardAsync(target);
                PlayAudioAlert(target);
            });
        });

        try
        {
            if (!_isMapSetup)
                SetupWebMap();

            if (_pois.Count == 0)
                await LoadDataFromDatabaseAsync();
            else
                LoadPinsToMap();

            StartRadar();

            await UpdateUserAvatarAsync();
            UpdateUI_Language();

            // Refresh popup nếu đang mở (vd. user vừa đổi ngôn ngữ bên Profile rồi quay lại)
            if (_currentPoi != null && PoiDetailCard.IsVisible)
                _ = UpdateDetailCardAsync(_currentPoi);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR in MapPage.OnAppearing: {ex}");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<QrScannedMessage>(this);
        StopAudio();
        _radarTimer?.Stop();
    }

    // ── MAP SETUP ────────────────────────────────────────────────────────────

    private void SetupWebMap()
    {
        System.Diagnostics.Debug.WriteLine("MapPage: Setting up WebView...");
        foodMapView.Navigating += OnWebViewNavigating;
        foodMapView.Navigated += OnWebViewNavigated;
        foodMapView.Source = "map.html";
        _isMapSetup = true;
        System.Diagnostics.Debug.WriteLine($"MapPage: WebView source set to 'map.html'. IsMapSetup: {_isMapSetup}");
    }

    private void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _isMapLoaded = true;
        MainThread.BeginInvokeOnMainThread(LoadPinsToMap);
        System.Diagnostics.Debug.WriteLine($"MapPage: WebView navigated to {e.Url}. IsMapLoaded: {_isMapLoaded}");
    }
    
    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("mapaction://pinclicked")) return;

        e.Cancel = true;
        var raw = e.Url.Replace("mapaction://pinclicked?name=", "");
        var name = Uri.UnescapeDataString(raw);
        var foundPoi = _pois.FirstOrDefault(p => p.Name == name);
        if (foundPoi != null)
        {
            _isManualSelection = true;
            StopAudio();
            _ = UpdateDetailCardAsync(foundPoi);
        }
        System.Diagnostics.Debug.WriteLine($"MapPage: WebView navigating to {e.Url}. Cancelled: {e.Cancel}");
    }

    private void RunScript(string js)
    {
        if (!_isMapLoaded) return;
        System.Diagnostics.Debug.WriteLine($"MapPage: Running JavaScript: {js}");
        MainThread.BeginInvokeOnMainThread(() => _ = foodMapView.EvaluateJavaScriptAsync(js));
        System.Diagnostics.Debug.WriteLine($"MapPage: JavaScript executed: {js}");
    }

    // ── DATA ─────────────────────────────────────────────────────────────────

    private async Task LoadDataFromDatabaseAsync()
    {
        try
        {
            _pois = await _dbService.GetPOIsAsync();
            if (_pois.Count == 0)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                    DisplayAlert("Thông báo", "Không có dữ liệu quán ăn. Vui lòng khởi động lại.", "OK"));
                return;
            }

            // Gán HeatWeight cho từng POI để dùng trong thuật toán ưu tiên
            await RefreshHeatWeightsAsync();

            try
            {
                bool granted = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>() == PermissionStatus.Granted;
                if (granted)
                {
                    var loc = await Geolocation.Default.GetLastKnownLocationAsync()
                           ?? await Geolocation.Default.GetLocationAsync(
                                  new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(2)));
                    if (loc != null)
                    {
                        foreach (var poi in _pois)
                        {
                            double km = Location.CalculateDistance(loc, poi.Lat, poi.Lng, DistanceUnits.Kilometers);
                            string dist = km < 1 ? $"{(int)(km * 1000)}m" : $"{Math.Round(km, 1)}km";
                            poi.DistanceInfo = $"📍 {dist}  •  🚶 {Math.Max(1, (int)(km * 12))} phút";
                        }
                    }
                }
                else
                    foreach (var poi in _pois) poi.DistanceInfo = "📍 Cần cấp quyền vị trí";
            }
            catch
            {
                foreach (var poi in _pois) poi.DistanceInfo = "📍 Chưa có định vị";
            }

            MainThread.BeginInvokeOnMainThread(LoadPinsToMap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadDataFromDatabaseAsync Error: {ex.Message}");
        }
    }

    private void LoadPinsToMap()
    {
        if (!_isMapLoaded || _pois == null || _pois.Count == 0) return;

        var data = _pois.Select(p => new { lat = p.Lat, lng = p.Lng, name = p.Name ?? "" });
        var json = JsonSerializer.Serialize(data);
        RunScript($"addPOIs({json})");
    }

    // Cập nhật HeatWeight cho tất cả POI (priority base + lượt nghe thực tế)
    private async Task RefreshHeatWeightsAsync()
    {
        var counts = await _dbService.GetPOIPlayCountsAsync();
        var weightMap = counts.ToDictionary(
            c => $"{c.Lat:F4}_{c.Lng:F4}",
            c => c.Weight);

        foreach (var poi in _pois)
        {
            string key = $"{poi.Lat:F4}_{poi.Lng:F4}";
            poi.HeatWeight = weightMap.TryGetValue(key, out var w) ? w : 1;
        }
    }

    // ── RADAR / GEOFENCE ─────────────────────────────────────────────────────

    private void StartRadar()
    {
        _radarTimer?.Stop();
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

            var userLoc = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(2)));
            if (userLoc == null) return;

            RunScript($"updateUserLocation({userLoc.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {userLoc.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)})");

            // Tìm tất cả POI mà user đang đứng trong vùng bán kính
            var inRangePois = _pois
                .Select(p => new
                {
                    Poi = p,
                    DistanceM = Location.CalculateDistance(userLoc, new Location(p.Lat, p.Lng), DistanceUnits.Kilometers) * 1000
                })
                .Where(x => x.DistanceM <= x.Poi.Radius)
                .ToList();

            AudioPOI? poi = null;
            if (inRangePois.Count == 1)
            {
                poi = inRangePois[0].Poi;
            }
            else if (inRangePois.Count > 1)
            {
                // Vùng giao nhau: score = weight / distance — vừa hot vừa gần thì thắng
                poi = inRangePois
                    .OrderByDescending(x => x.Poi.HeatWeight / Math.Max(1.0, x.DistanceM))
                    .First().Poi;
            }

            if (poi != null) { if (_currentPoi != poi) PlayAudioAlert(poi); }
            else if (_currentPoi != null && !_isManualSelection) { StopAudio(); _currentPoi = null; }
        }
        catch { }
    }

    // ── AUDIO ─────────────────────────────────────────────────────────────────

    private void StopAudio()
    {
        _ttsCancellationTokenSource?.Cancel();
        _isPlaying = false;
        MainThread.BeginInvokeOnMainThread(() => { AudioPlayerUI.IsVisible = false; PlayStopButton.Text = "▶️"; });
    }

    private async void PlayAudioAlert(AudioPOI poi)
    {
        try
        {
            _currentPoi = poi;
            _isPlaying = true;
            await _dbService.SavePlayHistoryAsync(poi);

            string lang = Lang.CurrentLocale;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TranslationLoader.IsVisible = true;
                TranslationLoader.IsRunning = true;
                AudioText.Text = lang == "vi" ? poi.Name : "Đang dịch...";
                AudioPlayerUI.IsVisible = true;
                PoiDetailCard.IsVisible = false;
                PlayStopButton.Text = "⏹";
            });

            string tName = await TranslateTextAsync(poi.Name, lang);
            string tDesc = await TranslateTextAsync(poi.Description, lang);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TranslationLoader.IsRunning = false;
                TranslationLoader.IsVisible = false;
                AudioText.Text = tName;
                AudioStatusLabel.Text = lang == "vi" ? "Đang phát review:" : "Playing review:";
            });

            _ttsCancellationTokenSource?.Cancel();
            _ttsCancellationTokenSource = new CancellationTokenSource();

            try
            {
                var locales = await TextToSpeech.Default.GetLocalesAsync();
                Locale? locale = lang switch
                {
                    "en" => locales.FirstOrDefault(l => l.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase)),
                    "ja" => locales.FirstOrDefault(l => l.Language.StartsWith("ja", StringComparison.OrdinalIgnoreCase)),
                    "ko" => locales.FirstOrDefault(l => l.Language.StartsWith("ko", StringComparison.OrdinalIgnoreCase)),
                    _ => locales.FirstOrDefault(l => l.Language.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
                };

                string clean = tDesc.Replace("'", "").Replace("\"", "").Replace("\n", " ");
                foreach (var sentence in clean.Split(new[] { '.', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (_ttsCancellationTokenSource.Token.IsCancellationRequested) break;
                    if (string.IsNullOrWhiteSpace(sentence)) continue;
                    await TextToSpeech.Default.SpeakAsync(sentence.Trim(),
                        new SpeechOptions { Locale = locale }, _ttsCancellationTokenSource.Token);
                }
            }
            catch { }
            finally
            {
                _isPlaying = false;
                MainThread.BeginInvokeOnMainThread(() => PlayStopButton.Text = "▶️");
            }
        }
        catch
        {
            _isPlaying = false;
            MainThread.BeginInvokeOnMainThread(() => { AudioPlayerUI.IsVisible = false; PlayStopButton.Text = "▶️"; });
        }
    }

    private static async Task<string> TranslateTextAsync(string text, string toLang)
    {
        if (string.IsNullOrEmpty(text) || toLang == "vi")
            return text.Replace("'", "").Replace("\"", "");
        try
        {
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={toLang}&dt=t&q={Uri.EscapeDataString(text)}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var response = await client.GetStringAsync(url);
            var json = System.Text.Json.JsonDocument.Parse(response);
            string full = "";
            foreach (var el in json.RootElement[0].EnumerateArray())
                if (el[0].ValueKind == System.Text.Json.JsonValueKind.String)
                    full += el[0].GetString();
            return full.Replace("'", "").Replace("\"", "");
        }
        catch { return text.Replace("'", "").Replace("\"", ""); }
    }

    // ── DETAIL CARD ───────────────────────────────────────────────────────────

    private async Task UpdateDetailCardAsync(AudioPOI poi)
    {
        _currentPoi = poi;
        string lang = Lang.CurrentLocale;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            DetailName.Text = "Đang tải...";
            DetailDistance.Text = "📍 Đang tính...";
            PoiDetailCard.IsVisible = true;
            AudioPlayerUI.IsVisible = false;
        });

        // Dùng LanguageService trước (có sẵn dict cho tên + mô tả)
        // Nếu không tìm thấy (trả về cùng chuỗi gốc) và không phải tiếng Việt thì mới gọi Google Translate
        string tName = Lang.T(poi.Name);
        if (tName == poi.Name && lang != "vi")
            tName = await TranslateTextAsync(poi.Name, lang);

        string tDesc = Lang.T(poi.Description);
        if (tDesc == poi.Description && lang != "vi")
            tDesc = await TranslateTextAsync(poi.Description, lang);

        string dist = poi.DistanceInfo;
        try
        {
            var loc = await Geolocation.Default.GetLastKnownLocationAsync();
            if (loc != null)
            {
                double km = Location.CalculateDistance(loc, poi.Lat, poi.Lng, DistanceUnits.Kilometers);
                string ds = km < 1 ? $"{(int)(km * 1000)}m" : $"{Math.Round(km, 1)}km";
                string minsLabel = Lang["mins"] ?? "phút";
                dist = $"📍 {ds}  •  🚶 {Math.Max(1, (int)(km * 12))} {minsLabel}";
                poi.DistanceInfo = dist;
            }
        }
        catch { }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            DetailName.Text = tName;
            DetailDescription.Text = tDesc;
            DetailDistance.Text = string.IsNullOrEmpty(dist) ? $"📍 {Lang["undetermined"] ?? "Chưa xác định"}" : dist;
            DetailImage.Source = poi.ImageAsset;
            if (PlayReviewButton != null)
                PlayReviewButton.Text = $"🔊 {Lang["listen_review"] ?? "Nghe thuyết minh"}";
        });
    }

    // ── UI EVENTS ─────────────────────────────────────────────────────────────

    private async Task UpdateUserAvatarAsync()
    {
        try
        {
            var user = await _dbService.GetCurrentUserAsync();
            if (user != null && !string.IsNullOrEmpty(user.Avatar))
                MainThread.BeginInvokeOnMainThread(() => MapUserAvatar.Source = user.Avatar);
        }
        catch { }
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        try
        {
            string action = await DisplayActionSheet("Ngôn ngữ / Language", "Hủy", null,
                "Tiếng Việt", "English", "日本語", "한국어");
            if (string.IsNullOrEmpty(action) || action == "Hủy") return;

            string code = action switch { "English" => "en", "日本語" => "ja", "한국어" => "ko", _ => "vi" };
            Lang.ChangeLanguage(code);

            if (_currentPoi != null)
            {
                if (PoiDetailCard.IsVisible) _ = UpdateDetailCardAsync(_currentPoi);
                else PlayAudioAlert(_currentPoi);
            }
        }
        catch { }
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
        if (_currentPoi != null) { _isManualSelection = true; PoiDetailCard.IsVisible = false; PlayAudioAlert(_currentPoi); }
    }

    private async void OnGetDirectionsClicked(object sender, EventArgs e)
    {
        if (_currentPoi == null) return;
        try
        {
            await Microsoft.Maui.ApplicationModel.Map.Default.OpenAsync(
                new Location(_currentPoi.Lat, _currentPoi.Lng),
                new MapLaunchOptions { Name = _currentPoi.Name, NavigationMode = NavigationMode.Driving });
        }
        catch (Exception ex) { await DisplayAlert("Lỗi", "Không thể mở bản đồ: " + ex.Message, "OK"); }
    }

    private void CustomMyLocationClicked(object? sender, EventArgs e)
    {
        RunScript("centerOn(10.7585, 106.7042, 15)");
    }

    private async void OnScanQRClicked(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new ScanQRPage(_quotaService)); }
        catch (Exception ex) { await DisplayAlert("Lỗi", "Không thể mở camera: " + ex.Message, "OK"); }
    }

    private async void OnHeatmapToggleClicked(object sender, EventArgs e)
    {
        _isHeatmapOn = !_isHeatmapOn;

        if (_isHeatmapOn)
        {
            // Đổi màu nút -> cam để báo hiệu đang bật
            HeatmapToggleButton.BackgroundColor = Color.FromArgb("#FF6B35");

            var counts = await _dbService.GetPOIPlayCountsAsync();
            if (counts.Count == 0) { _isHeatmapOn = false; HeatmapToggleButton.BackgroundColor = Colors.White; return; }

            var json = System.Text.Json.JsonSerializer.Serialize(
                counts.Select(c => new { lat = c.Lat, lng = c.Lng, weight = c.Weight }));
            RunScript($"showHeatmap({json})");
        }
        else
        {
            // Đổi màu nút -> trắng (tắt)
            HeatmapToggleButton.BackgroundColor = Colors.White;
            RunScript("hideHeatmap()");
        }
    }

    private void OnSearchTextChanged(object sender, Microsoft.Maui.Controls.TextChangedEventArgs e)
    {
        string q = e.NewTextValue?.ToLower() ?? "";
        if (string.IsNullOrWhiteSpace(q)) { LoadPinsToMap(); SearchSuggestionsContainer.IsVisible = false; return; }

        var matches = _pois.Where(p => p.Name != null && p.Name.ToLower().Contains(q)).Take(3).ToList();
        if (matches.Count > 0) { SuggestionsList.ItemsSource = matches; SearchSuggestionsContainer.IsVisible = true; }
        else SearchSuggestionsContainer.IsVisible = false;
    }

    private void OnSuggestionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not AudioPOI poi) return;

        SearchSuggestionsContainer.IsVisible = false;
        SearchEntry.Unfocus();
        LoadPinsToMap();

        RunScript($"centerOn({poi.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {poi.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 17)");

        _isManualSelection = true;
        StopAudio();
        _ = UpdateDetailCardAsync(poi);
        ((CollectionView)sender).SelectedItem = null;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        AudioPOI? targetPoi = null;

        // Handle direct POI object from internal navigation (e.g., from HomePage)
        if (query.TryGetValue("SelectedPOI", out var val) && val is AudioPOI poi)
        {
            targetPoi = poi;
        }
        // Handle deep link with poi_name string (e.g., from App.xaml.cs handling a deep link)
        else if (query.TryGetValue("poi_name", out var poiNameObj) && poiNameObj is string poiName)
        {
            // Ensure _pois is loaded before trying to find the POI
            if (_pois.Count == 0)
            {
                await LoadDataFromDatabaseAsync(); // Load data if not already loaded
            }
            targetPoi = _pois.FirstOrDefault(p => p.Name != null &&
                string.Equals(p.Name, poiName, StringComparison.OrdinalIgnoreCase));
        }

        if (targetPoi == null) return;

        query.Clear(); // Clear processed attributes

        await Task.Delay(500);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            RunScript($"centerOn({targetPoi.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {targetPoi.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture)}, 17)");
            _isManualSelection = true;
            StopAudio();
            _ = UpdateDetailCardAsync(targetPoi);
            PlayAudioAlert(targetPoi);
        });
    }

    private void UpdateUI_Language()
    {
        string lang = Lang.CurrentLocale;
        SearchEntry.Placeholder = lang switch
        {
            "en" => "Search restaurants, snails...",
            "ja" => "レストランや料理を検索...",
            "ko" => "식당, 음식 검색...",
            _ => "Tìm quán ăn, món ốc..."
        };
        if (PlayReviewButton != null)
            PlayReviewButton.Text = lang switch
            {
                "en" => "🔊 Listen Review",
                "ja" => "🔊 レビューを聞く",
                "ko" => "🔊 리뷰 듣기",
                _ => "🔊 Nghe Review"
            };
        AudioStatusLabel.Text = lang switch
        {
            "en" => "Playing review:",
            "ja" => "レビューを再生中:",
            "ko" => "리뷰 재생 중:",
            _ => "Đang phát review:"
        };
    }
}
