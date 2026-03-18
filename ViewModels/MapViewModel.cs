using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DoAnCSharp.Services;
using DoAnCSharp.Models;
using System.Collections.ObjectModel;

namespace DoAnCSharp.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly ILocationService _locationService;
    private readonly IGeofenceService _geofenceService;
    private readonly IPoiRepository _poiRepo;

    // SỬA: Thêm dấu ? để hết cảnh báo CS8618 (Biến có thể rỗng lúc mới mở App)
    [ObservableProperty] private Location? _userPos;
    [ObservableProperty] private POI? _activePoi;
    
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isActiveCardVisible;

    public ObservableCollection<POI> Pins { get; } = new();

    public MapViewModel(ILocationService loc, IGeofenceService geo, IPoiRepository repo)
    {
        _locationService = loc;
        _geofenceService = geo;
        _poiRepo = repo;

        LoadData();
        StartTracking();
    }

    private void LoadData()
    {
        var points = _poiRepo.GetTourPoints();
        if (points != null)
        {
            foreach (var p in points) Pins.Add(p);
        }
    }

    private async void StartTracking()
    {
        bool hasPermission = await _locationService.HandleLocationPermission();
        if (hasPermission)
        {
            await _locationService.StartListening(newLocation => 
            {
                UserPos = newLocation;
            });
        }
    }

    [RelayCommand]
    private void ToggleAudio()
    {
        IsPlaying = !IsPlaying;
    }
}
