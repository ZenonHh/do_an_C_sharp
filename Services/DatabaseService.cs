using System.Net.Http.Json;
using DoAnCSharp.Models;

namespace DoAnCSharp.Services;

public class DatabaseService
{
    private readonly HttpClient _httpClient;
    
    // VỊ TRÍ CẦN SỬA: Thay 192.168.1.X bằng địa chỉ IPv4 máy tính của bạn!
    private const string BaseUrl = "http://10.0.2.2:5225/api/";
    private static User? _currentUser;

    public DatabaseService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    // --- 1. QUẢN LÝ QUÁN ĂN ---
    public async Task<List<AudioPOI>> GetPOIsAsync()
    {
        try {
            return await _httpClient.GetFromJsonAsync<List<AudioPOI>>("pois") ?? new List<AudioPOI>();
        } catch { return new List<AudioPOI>(); }
    }

    // --- 2. QUẢN LÝ USER & AUTH ---
    
    // Bỏ chữ 'async' ở đây vì dùng Task.FromResult trực tiếp
    public Task<User?> GetCurrentUserAsync() => Task.FromResult(_currentUser);

    public async Task<User?> LoginUserAsync(string email, string password)
    {
        try {
            var response = await _httpClient.PostAsJsonAsync("users/login", new { Email = email, Password = password });
            if (response.IsSuccessStatusCode) {
                _currentUser = await response.Content.ReadFromJsonAsync<User>();
                return _currentUser;
            }
            return null;
        } catch { return null; }
    }

    public async Task<bool> RegisterUserAsync(User user)
    {
        try {
            var response = await _httpClient.PostAsJsonAsync("users/register", user);
            return response.IsSuccessStatusCode;
        } catch { return false; }
    }

    public async Task<bool> UpdateUserAsync(string email, string fullName, string password, string avatar)
    {
        try {
            var updateData = new { Email = email, FullName = fullName, Password = password, Avatar = avatar };
            var response = await _httpClient.PutAsJsonAsync("users/update", updateData);
            
            if (response.IsSuccessStatusCode) {
                if (_currentUser != null) {
                    _currentUser.FullName = fullName;
                    _currentUser.Password = password;
                    _currentUser.Avatar = avatar;
                }
                return true;
            }
            return false;
        } catch { return false; }
    }

    public async Task<User?> GetOrCreateUserAsync(string email)
    {
        try {
            return await _httpClient.GetFromJsonAsync<User>($"users/email/{email}");
        } catch { return null; }
    }

    // --- 3. QUẢN LÝ LỊCH SỬ (PLAY HISTORY) ---
    
    public async Task SavePlayHistoryAsync(AudioPOI poi)
    {
        if (_currentUser == null) return;

        try {
            var history = new PlayHistory {
                UserId = _currentUser.Id, 
                PoiId = poi.Id,           
                PlayedAt = DateTime.Now
            };
            
            await _httpClient.PostAsJsonAsync("history", history);
        } catch { /* Bỏ qua lỗi kết nối khi lưu lịch sử */ }
    }

    public async Task<List<PlayHistory>> GetAllPlayHistoryAsync()
    {
        try {
            return await _httpClient.GetFromJsonAsync<List<PlayHistory>>("history") ?? new List<PlayHistory>();
        } catch { return new List<PlayHistory>(); }
    }

    public async Task<List<PlayHistory>> GetRecentPlayHistoryAsync(int count = 5)
    {
        var all = await GetAllPlayHistoryAsync();
        return all.OrderByDescending(h => h.PlayedAt).Take(count).ToList();
    }

    public Task SeedDataAsync() => Task.CompletedTask;
}