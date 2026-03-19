using Microsoft.Maui.Controls;
using Mapsui;
using Mapsui.Tiling;
using Mapsui.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
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

    // THÊM DÒNG NÀY: Khai báo tên hình ảnh
    public string ImageAsset { get; set; } = string.Empty;
}

public partial class MapPage : ContentPage
{
    private List<AudioPOI> _pois = new();
    private IDispatcherTimer? _radarTimer;
    private CancellationTokenSource? _ttsCancellationTokenSource;
    private AudioPOI? _currentPoi;
    private bool _isPlaying = false;

    public MapPage()
    {
        InitializeComponent();

        // Gọi thẳng SetupMap thay vì CreateMap
        SetupMap();
        SetupPOIs();
        LoadPinsToMap();

        StartRadar();
    }

    private void SetupMap()
    {
        // Kiểm tra an toàn trước khi nạp bản đồ
        if (foodMapView.Map != null)
        {
            foodMapView.Map.Layers.Add(OpenStreetMap.CreateTileLayer("VinhKhanhFoodTourApp"));
            var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
            foodMapView.Map.Home = n => n.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
        }

        foodMapView.MyLocationEnabled = true;
        if (foodMapView.MyLocationLayer != null)
        {
            foodMapView.MyLocationLayer.IsMoving = false;
        }
    }

    private void SetupPOIs()
    {
        _pois = new List<AudioPOI>
        {
            new AudioPOI {
                Name = "Ốc Oanh",
                Description = "Bạn đã bước vào khu vực Quán Ốc Oanh. Nổi tiếng nhất ở đây là món ốc hương xào bắp bơ siêu to khổng lồ.",
                Lat = 10.7600,
                Lng = 106.7000,
                Radius = 50,
                Priority = 1,
                ImageAsset = "oc_oanh,jpg" // Gán tên ảnh vào đây
            },
            new AudioPOI {
                Name = "Ốc Đào 2",
                Description = "Chi nhánh nổi tiếng của Ốc Đào, hương vị đậm đà đặc trưng Sài Gòn.",
                Lat = 10.7581,
                Lng = 106.7061,
                Radius = 50,
                Priority = 1,
                ImageAsset = "oc_dao2.webp" // Gán tên ảnh vào đây
            },
            new AudioPOI {
                Name = "Ốc Vũ",
                Description = "Quán ăn yêu thích của giới trẻ, nổi bật với món ốc mỡ xào bơ.",
                Lat = 10.7578,
                Lng = 106.7058,
                Radius = 50,
                Priority = 2,
                ImageAsset = "dotnet_bot.png" // Gán tên ảnh vào đây
            }
        };
    }
    private void StartRadar()
    {
        _radarTimer = Dispatcher.CreateTimer();
        _radarTimer.Interval = TimeSpan.FromSeconds(5);
        _radarTimer.Tick += async (s, e) => await CheckGeofenceAndPlayAudio();
        _radarTimer.Start();
    }

    private async Task CheckGeofenceAndPlayAudio()
    {
        try
        {
            var userLoc = new Location(10.7600, 106.7000);
            if (userLoc == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foodMapView.MyLocationLayer?.UpdateMyLocation(new Mapsui.UI.Maui.Position(userLoc.Latitude, userLoc.Longitude));
            });

            var poi = _pois.OrderByDescending(p => p.Priority)
                           .FirstOrDefault(p => Location.CalculateDistance(userLoc, new Location(p.Lat, p.Lng), DistanceUnits.Kilometers) * 1000 <= p.Radius);

            if (poi != null && _currentPoi != poi)
            {
                PlayAudioAlert(poi);
            }
        }
        catch (Exception) { }
    }

    private void CustomMyLocationClicked(object sender, EventArgs e)
    {
        var center = SphericalMercator.FromLonLat(106.7000, 10.7600);
        if (foodMapView.Map != null)
        {
            foodMapView.Map.Navigator.CenterOnAndZoomTo(new MPoint(center.x, center.y), 2);
        }
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        string action = await DisplayActionSheet("Chọn ngôn ngữ thuyết minh", "Hủy", null, "Tiếng Việt", "English");
    }

    private async void PlayAudioAlert(AudioPOI poi)
    {
        _currentPoi = poi;
        _isPlaying = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            AudioText.Text = poi.Name;
            AudioStatusLabel.Text = "Đang phát review:";
            AudioPlayerUI.IsVisible = true;
            PlayStopButton.Text = "⏹";
        });

        _ttsCancellationTokenSource?.Cancel();
        _ttsCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await TextToSpeech.Default.SpeakAsync(poi.Description, cancelToken: _ttsCancellationTokenSource.Token);
        }
        catch { }
        finally
        {
            _isPlaying = false;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PlayStopButton.Text = "▶️";
                AudioStatusLabel.Text = "";
            });
        }
    }

    private void ToggleAudioClicked(object sender, EventArgs e)
    {
        if (_isPlaying)
        {
            _ttsCancellationTokenSource?.Cancel();
        }
        else if (_currentPoi != null)
        {
            PlayAudioAlert(_currentPoi);
        }
    }

    private void LoadPinsToMap()
    {
        if (foodMapView == null) return;
        foodMapView.Pins.Clear();

        foreach (var poi in _pois)
        {
            var pin = new Mapsui.UI.Maui.Pin(foodMapView)
            {
                Label = poi.Name,
                Position = new Mapsui.UI.Maui.Position(poi.Lat, poi.Lng),
                Type = Mapsui.UI.Maui.PinType.Pin,
                Color = Microsoft.Maui.Graphics.Colors.Red,
                Scale = 0.8f,
                Tag = poi // Lưu toàn bộ thông tin POI vào Tag để dùng sau
            };

            foodMapView.Pins.Add(pin);
        }

        // Bắt sự kiện khi người dùng click vào bất kỳ Pin nào trên bản đồ
        foodMapView.PinClicked -= OnMapPinClicked; // Xóa event cũ tránh bị lặp
        foodMapView.PinClicked += OnMapPinClicked;
    }
    // Xử lý khi nhấn vào Pin
    private void OnMapPinClicked(object? sender, Mapsui.UI.Maui.PinClickedEventArgs e)
    {
        if (e.Pin?.Tag is AudioPOI clickedPoi)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Cập nhật chữ
                DetailName.Text = clickedPoi.Name;
                DetailDescription.Text = clickedPoi.Description;

                // THÊM DÒNG NÀY: Cập nhật hình ảnh
                DetailImage.Source = clickedPoi.ImageAsset;

                _currentPoi = clickedPoi;
                PoiDetailCard.IsVisible = true;
                AudioPlayerUI.IsVisible = false;
            });
        }
        e.Handled = true;
    }

    // Xử lý khi bấm nút "X" trên Card
    private void CloseDetailClicked(object sender, EventArgs e)
    {
        PoiDetailCard.IsVisible = false;
    }

    // Xử lý khi bấm nút "Nghe Review" bên trong Card
    private void PlayReviewFromDetailClicked(object sender, EventArgs e)
    {
        if (_currentPoi != null)
        {
            PoiDetailCard.IsVisible = false; // Tắt Card chi tiết
            PlayAudioAlert(_currentPoi);     // Gọi hàm phát âm thanh có sẵn của bạn
        }
    }
}