using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices; // Thư viện để tính khoảng cách Location
using DoAnCSharp.Models;

namespace DoAnCSharp.Services;

public interface IGeofenceService
{
    POI? CheckPois(Location userLocation);
    void ResetHistory();
}

public class GeofenceService : IGeofenceService
{
    private readonly List<POI> _pois;
    private readonly Dictionary<string, DateTime> _history = new();
    private readonly TimeSpan _cooldown;

    // SỬA: Inject IPoiRepository vào để lấy danh sách quán, giúp DI không bị lỗi Code 3
    public GeofenceService(IPoiRepository poiRepo)
    {
        _pois = poiRepo.GetTourPoints() ?? new List<POI>();
        _cooldown = TimeSpan.FromMinutes(5);
    }

    public POI? CheckPois(Location userLocation)
    {
        if (userLocation == null || _pois == null) return null;

        POI? bestPoi = null;
        double minDistance = double.MaxValue;

        foreach (var poi in _pois)
        {
            var poiLocation = new Location(poi.Latitude, poi.Longitude);

            // Tính khoảng cách (C# trả về Kilometers, * 1000 để ra Meters)
            double distance = Location.CalculateDistance(userLocation, poiLocation, DistanceUnits.Kilometers) * 1000;

            if (distance <= poi.Radius)
            {
                // Kiểm tra Cooldown
                if (_history.TryGetValue(poi.Id, out DateTime lastTriggered))
                {
                    if (DateTime.Now - lastTriggered < _cooldown)
                        continue;
                }

                // Ưu tiên Priority cao nhất
                if (bestPoi == null || 
                    poi.Priority > bestPoi.Priority || 
                    (poi.Priority == bestPoi.Priority && distance < minDistance))
                {
                    bestPoi = poi;
                    minDistance = distance;
                }
            }
        }

        if (bestPoi != null)
        {
            _history[bestPoi.Id] = DateTime.Now;
        }

        return bestPoi;
    }

    public void ResetHistory()
    {
        _history.Clear();
    }
}
