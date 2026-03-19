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
                Priority = 1
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
}