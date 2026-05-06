using SQLite;
using DoAnCSharp.AdminWeb.Models;
using System.Collections.Generic;

namespace DoAnCSharp.AdminWeb.Services;

public class DatabaseService
{
    private const string DbFileName = "VinhKhanhTour_Full.db3";
    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task InitAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_connection != null) return;

            string dbPath;

            // 🔍 Priority order for database path:
            // 1. Environment variable (for custom deployment)
            // 2. Project directory (for easy sharing between machines)
            // 3. AppData folder (fallback for MAUI app)

            var customPath = Environment.GetEnvironmentVariable("VINHKHANH_DB_PATH");
            if (!string.IsNullOrEmpty(customPath) && Directory.Exists(customPath))
            {
                dbPath = Path.Combine(customPath, DbFileName);
            }
            else
            {
                // 🔍 Try to use project directory first (shared across machines)
                var projectDbPath = Path.Combine(AppContext.BaseDirectory, "data", DbFileName);
                var projectDataDir = Path.Combine(AppContext.BaseDirectory, "data");
                Directory.CreateDirectory(projectDataDir);

                // If database exists in project folder, use it
                if (File.Exists(projectDbPath))
                {
                    dbPath = projectDbPath;
                }
                else
                {
                    // Fallback to AppData
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "VinhKhanhTour"
                    );
                    Directory.CreateDirectory(appDataPath);
                    dbPath = Path.Combine(appDataPath, DbFileName);
                }
            }

            _connection = new SQLiteAsyncConnection(dbPath);

            // Create tables
            await _connection.CreateTableAsync<AudioPOI>();
            await _connection.CreateTableAsync<User>();
            await _connection.CreateTableAsync<PlayHistory>();
            await _connection.CreateTableAsync<UserPayment>();
            await _connection.CreateTableAsync<QRScanLimit>();
            await _connection.CreateTableAsync<UserStatus>();

            // New tables for enhanced features
            await _connection.CreateTableAsync<UserDevice>();
            await _connection.CreateTableAsync<RestaurantImage>();
            await _connection.CreateTableAsync<AdminUser>();
            await _connection.CreateTableAsync<SystemSetting>();
            await _connection.CreateTableAsync<AuditLog>();
            await _connection.CreateTableAsync<QRCodeSession>();
            await _connection.CreateTableAsync<QRScanRequest>();
            await _connection.CreateTableAsync<DeviceScanLimit>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // POI Operations
    public async Task<List<AudioPOI>> GetAllPOIsAsync()
    {
        await InitAsync();
        return await _connection!.Table<AudioPOI>().ToListAsync();
    }

    public async Task<AudioPOI?> GetPOIByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<AudioPOI>().Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<AudioPOI?> GetPOIByQRCodeAsync(string qrCode)
    {
        await InitAsync();
        if (string.IsNullOrWhiteSpace(qrCode))
            return null;

        // Try exact match first
        var poi = await _connection!.Table<AudioPOI>()
            .Where(p => p.QRCode == qrCode)
            .FirstOrDefaultAsync();

        // If not found, try to match by code portion (handle case where QRCode field contains full URL)
        if (poi == null && !qrCode.StartsWith("http"))
        {
            poi = await _connection!.Table<AudioPOI>()
                .Where(p => p.QRCode.Contains(qrCode))
                .FirstOrDefaultAsync();
        }

        return poi;
    }

    public async Task<int> InsertPOIAsync(AudioPOI poi)
    {
        await InitAsync();
        return await _connection!.InsertAsync(poi);
    }

    public async Task<int> UpdatePOIAsync(AudioPOI poi)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(poi);
    }

    public async Task<int> DeletePOIAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<AudioPOI>(id);
    }

    // User Operations
    public async Task<List<User>> GetAllUsersAsync()
    {
        await InitAsync();
        return await _connection!.Table<User>().ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserAsync(User user)
    {
        await InitAsync();
        return await _connection!.InsertAsync(user);
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(user);
    }

    public async Task<int> DeleteUserAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<User>(id);
    }

    // Play History Operations
    public async Task<List<PlayHistory>> GetAllHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(h => h.PlayedAt).ToListAsync();
    }

    public async Task<List<PlayHistory>> GetAllPlayHistoryAsync()
    {
        await InitAsync();
        return await _connection!.Table<PlayHistory>().OrderByDescending(h => h.PlayedAt).ToListAsync();
    }

    public async Task<int> DeleteHistoryAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<PlayHistory>(id);
    }

    public async Task<int> InsertPlayHistoryAsync(PlayHistory history)
    {
        await InitAsync();
        return await _connection!.InsertAsync(history);
    }

    // User Payment Operations
    public async Task<UserPayment?> GetUserPaymentByUserIdAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>().Where(p => p.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<int> UpdateUserPaymentAsync(UserPayment payment)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(payment);
    }

    public async Task<int> InsertUserPaymentAsync(UserPayment payment)
    {
        await InitAsync();
        return await _connection!.InsertAsync(payment);
    }

    // QR Scan Limit Operations
    public async Task<QRScanLimit?> GetQRScanLimitByUserIdAsync(int userId)
    {
        await InitAsync();
        var limit = await _connection!.Table<QRScanLimit>().Where(q => q.UserId == userId).FirstOrDefaultAsync();

        if (limit != null)
        {
            // Reset count if it's a new day
            if (limit.LastResetDate.Date < DateTime.Now.Date)
            {
                limit.ScanCount = 0;
                limit.LastResetDate = DateTime.Now;
                await _connection!.UpdateAsync(limit);
            }
        }
        return limit;
    }

    public async Task<bool> CanUserScanQRAsync(int userId, bool isPaidUser)
    {
        await InitAsync();
        var limit = await GetQRScanLimitByUserIdAsync(userId);

        if (limit == null)
        {
            limit = new QRScanLimit
            {
                UserId = userId,
                IsPaidUser = isPaidUser,
                MaxScans = isPaidUser ? int.MaxValue : 5,
                ScanCount = 0,
                LastResetDate = DateTime.Now
            };
            await InsertQRScanLimitAsync(limit);
            return true;
        }

        if (isPaidUser) return true; // Paid users: unlimited
        return limit.ScanCount < limit.MaxScans; // Free users: limited
    }

    public async Task IncrementQRScanCountAsync(int userId)
    {
        await InitAsync();
        var limit = await GetQRScanLimitByUserIdAsync(userId);
        if (limit != null)
        {
            limit.ScanCount++;
            await _connection!.UpdateAsync(limit);
        }
    }

    public async Task<int> InsertQRScanLimitAsync(QRScanLimit limit)
    {
        await InitAsync();
        return await _connection!.InsertAsync(limit);
    }

    public async Task<int> UpdateQRScanLimitAsync(QRScanLimit limit)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(limit);
    }

    // User Status Operations
    public async Task<UserStatus?> GetUserStatusAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserStatus>().Where(s => s.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<List<UserStatus>> GetAllUserStatusAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserStatus>().ToListAsync();
    }

    public async Task<int> UpdateUserStatusAsync(UserStatus status)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(status);
    }

    public async Task<int> InsertUserStatusAsync(UserStatus status)
    {
        await InitAsync();
        return await _connection!.InsertAsync(status);
    }

    public async Task SetUserOnlineAsync(int userId, bool isOnline, string deviceInfo = "", string ipAddress = "")
    {
        await InitAsync();
        var status = await GetUserStatusAsync(userId);

        if (status == null)
        {
            status = new UserStatus
            {
                UserId = userId,
                IsOnline = isOnline,
                LastActiveAt = DateTime.Now,
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress
            };
            await InsertUserStatusAsync(status);
        }
        else
        {
            status.IsOnline = isOnline;
            status.LastActiveAt = DateTime.Now;
            if (!string.IsNullOrEmpty(deviceInfo)) status.DeviceInfo = deviceInfo;
            if (!string.IsNullOrEmpty(ipAddress)) status.IpAddress = ipAddress;
            await UpdateUserStatusAsync(status);
        }
    }

    // Dashboard Operations
    public async Task<OnlineUserSummary> GetDashboardSummaryAsync()
    {
        await InitAsync();

        var allUsers = await GetAllUsersAsync();
        var onlineStatuses = await GetAllUserStatusAsync();
        var onlineUsers = onlineStatuses.Where(s => s.IsOnline).ToList();

        var paidUsers = await _connection!.Table<UserPayment>()
            .Where(p => p.IsPaid == true)
            .ToListAsync();

        // Get today's QR scans
        var today = DateTime.Today;
        var todayScans = await _connection!.Table<PlayHistory>()
            .Where(h => h.PlayedAt >= today)
            .ToListAsync();

        return new OnlineUserSummary
        {
            TotalOnlineUsers = onlineUsers.Count,
            OnlineDevices = onlineUsers.Count,
            ActiveListeningUsers = todayScans.Select(h => h.UserId).Distinct().Count(),
            TotalRegisteredUsers = allUsers.Count,
            TotalPaidUsers = paidUsers.Count,
            TodayQRScans = todayScans.Count,
            OnlineDeviceDetails = new List<UserDevice>()
        };
    }

    public async Task<List<UserDevice>> GetOnlineUsersAsync()
    {
        await InitAsync();

        var onlineStatuses = await GetAllUserStatusAsync();
        var onlineUserIds = onlineStatuses.Where(s => s.IsOnline).Select(s => s.UserId).ToList();

        var devices = new List<UserDevice>();

        foreach (var userId in onlineUserIds)
        {
            var userDevices = await GetUserDevicesAsync(userId);
            foreach (var device in userDevices)
            {
                if (device.IsOnline)
                {
                    devices.Add(device);
                }
            }
        }

        return devices;
    }

    public async Task<(int totalScans, int uniqueUsers, List<(string POIName, int count)> topPOIs)> GetQRActivityTodayAsync()
    {
        await InitAsync();

        var today = DateTime.Today;
        var todayScans = await _connection!.Table<PlayHistory>()
            .Where(h => h.PlayedAt >= today)
            .ToListAsync();

        var uniqueUsers = todayScans.Select(h => h.UserId).Distinct().Count();

        // Top POIs
        var topPOIs = todayScans
            .GroupBy(h => h.POIName)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => (g.Key, g.Count()))
            .ToList();

        return (todayScans.Count, uniqueUsers, topPOIs);
    }

    public async Task<UserPayment?> GetPaymentByUserIdAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>()
            .Where(p => p.UserId == userId)
            .FirstOrDefaultAsync();
    }

    // ===== NEW FEATURE OPERATIONS =====

    // UserDevice Operations
    public async Task<List<UserDevice>> GetAllUserDevicesAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().ToListAsync();
    }

    public async Task<List<UserDevice>> GetUserDevicesAsync(int userId)
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().Where(d => d.UserId == userId).ToListAsync();
    }

    public async Task<UserDevice?> GetUserDeviceByIdAsync(int deviceId)
    {
        await InitAsync();
        return await _connection!.Table<UserDevice>().Where(d => d.Id == deviceId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertUserDeviceAsync(UserDevice device)
    {
        await InitAsync();
        return await _connection!.InsertAsync(device);
    }

    public async Task<int> UpdateUserDeviceAsync(UserDevice device)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(device);
    }

    public async Task<int> DeleteUserDeviceAsync(int deviceId)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<UserDevice>(deviceId);
    }

    // RestaurantImage Operations
    public async Task<List<RestaurantImage>> GetPOIImagesAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<RestaurantImage>().Where(i => i.RestaurantId == restaurantId).ToListAsync();
    }

    public async Task<List<RestaurantImage>> GetRestaurantImagesAsync(int restaurantId)
    {
        return await GetPOIImagesAsync(restaurantId);
    }

    // Returns poiId -> main uploaded image path for all POIs in one query
    public async Task<Dictionary<int, string>> GetAllMainImagePathsAsync()
    {
        await InitAsync();
        var all = await _connection!.Table<RestaurantImage>().ToListAsync();
        var result = new Dictionary<int, string>();
        foreach (var img in all)
        {
            // Prefer IsMainImage; if none marked, take any image per POI
            if (!result.ContainsKey(img.RestaurantId) || img.IsMainImage)
                result[img.RestaurantId] = img.ImagePath;
        }
        return result;
    }

    public async Task<RestaurantImage?> GetRestaurantImageByIdAsync(int imageId)
    {
        await InitAsync();
        return await _connection!.Table<RestaurantImage>().Where(i => i.Id == imageId).FirstOrDefaultAsync();
    }

    public async Task<int> InsertRestaurantImageAsync(RestaurantImage image)
    {
        await InitAsync();
        return await _connection!.InsertAsync(image);
    }

    public async Task<int> UpdateRestaurantImageAsync(RestaurantImage image)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(image);
    }

    public async Task<int> DeleteRestaurantImageAsync(int imageId)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<RestaurantImage>(imageId);
    }

    // AdminUser Operations
    public async Task<List<AdminUser>> GetAllAdminUsersAsync()
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().ToListAsync();
    }

    public async Task<AdminUser?> GetAdminUserByIdAsync(int id)
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().Where(a => a.Id == id).FirstOrDefaultAsync();
    }

    public async Task<AdminUser?> GetAdminUserByUsernameAsync(string username)
    {
        await InitAsync();
        return await _connection!.Table<AdminUser>().Where(a => a.Username == username).FirstOrDefaultAsync();
    }

    public async Task<int> InsertAdminUserAsync(AdminUser admin)
    {
        await InitAsync();
        return await _connection!.InsertAsync(admin);
    }

    public async Task<int> UpdateAdminUserAsync(AdminUser admin)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(admin);
    }

    public async Task<int> DeleteAdminUserAsync(int id)
    {
        await InitAsync();
        return await _connection!.DeleteAsync<AdminUser>(id);
    }

    // SystemSetting Operations
    public async Task<string?> GetSettingValueAsync(string key)
    {
        await InitAsync();
        var setting = await _connection!.Table<SystemSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
        return setting?.Value;
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync()
    {
        await InitAsync();
        return await _connection!.Table<SystemSetting>().ToListAsync();
    }

    public async Task<int> UpsertSettingAsync(string key, string value, string description = "", string settingType = "string", string updatedBy = "system")
    {
        await InitAsync();
        var existing = await _connection!.Table<SystemSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.Value = value;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = updatedBy;
            return await _connection!.UpdateAsync(existing);
        }
        else
        {
            var newSetting = new SystemSetting
            {
                Key = key,
                Value = value,
                Description = description,
                SettingType = settingType,
                UpdatedAt = DateTime.Now,
                UpdatedBy = updatedBy
            };
            return await _connection!.InsertAsync(newSetting);
        }
    }

    // AuditLog Operations
    public async Task<List<AuditLog>> GetAllAuditLogsAsync()
    {
        await InitAsync();
        return await _connection!.Table<AuditLog>().OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<int> InsertAuditLogAsync(AuditLog log)
    {
        await InitAsync();
        return await _connection!.InsertAsync(log);
    }

    public async Task LogAdminActionAsync(int adminUserId, string action, string entityType, int? entityId, string? oldValue = null, string? newValue = null, string? ipAddress = null, string? userAgent = null, bool isSuccess = true, string? errorMessage = null)
    {
        var log = new AuditLog
        {
            AdminUserId = adminUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            IPAddress = ipAddress ?? "",
            UserAgent = userAgent ?? "",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage ?? "",
            CreatedAt = DateTime.Now
        };
        await InsertAuditLogAsync(log);
    }

    // ===== QR CODE SESSION OPERATIONS =====

    public async Task<QRCodeSession?> GetQRCodeSessionByTokenAsync(string token)
    {
        await InitAsync();
        return await _connection!.Table<QRCodeSession>()
            .Where(q => q.SessionToken == token && q.IsActive)
            .FirstOrDefaultAsync();
    }

    public async Task<QRCodeSession?> GetCurrentQRCodeAsync(int restaurantId)
    {
        await InitAsync();
        var now = DateTime.Now;
        return await _connection!.Table<QRCodeSession>()
            .Where(q => q.RestaurantId == restaurantId && q.IsActive && q.ExpiresAt > now)
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> CreateQRCodeSessionAsync(int restaurantId, int durationMinutes = 5)
    {
        await InitAsync();

        // Deactivate old sessions
        var oldSessions = await _connection!.Table<QRCodeSession>()
            .Where(q => q.RestaurantId == restaurantId && q.IsActive)
            .ToListAsync();

        foreach (var session in oldSessions)
        {
            session.IsActive = false;
            await _connection!.UpdateAsync(session);
        }

        // Create new session
        var newSession = new QRCodeSession
        {
            RestaurantId = restaurantId,
            QRCode = GenerateQRCode(),
            SessionToken = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMinutes(durationMinutes),
            ScanCount = 0,
            IsActive = true,
            LastScannedAt = null,
            LastScannedByUserId = null
        };

        return await _connection!.InsertAsync(newSession);
    }

    public async Task<int> UpdateQRCodeSessionAsync(QRCodeSession session)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(session);
    }

    // ===== QR SCAN REQUEST OPERATIONS =====

    public async Task<int> InsertQRScanRequestAsync(QRScanRequest request)
    {
        await InitAsync();
        return await _connection!.InsertAsync(request);
    }

    public async Task<List<QRScanRequest>> GetRestaurantScanRequestsAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId)
            .OrderByDescending(r => r.ScanTime)
            .ToListAsync();
    }

    public async Task<List<QRScanRequest>> GetPendingScanRequestsAsync(int restaurantId)
    {
        await InitAsync();
        return await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId && r.Status == "pending")
            .OrderByDescending(r => r.ScanTime)
            .ToListAsync();
    }

    public async Task<int> UpdateQRScanRequestAsync(QRScanRequest request)
    {
        await InitAsync();
        return await _connection!.UpdateAsync(request);
    }

    public async Task<(int totalScansToday, int uniqueUsersToday, Dictionary<string, int> scansPerDay)> GetWeeklyScanStatisticsAsync(int restaurantId)
    {
        await InitAsync();

        var requests = await _connection!.Table<QRScanRequest>()
            .Where(r => r.RestaurantId == restaurantId)
            .ToListAsync();

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        var totalScansToday = requests.Count(r => r.ScanTime.Date == today);
        var uniqueUsersToday = requests
            .Where(r => r.ScanTime.Date == today)
            .Select(r => r.UserId)
            .Distinct()
            .Count();

        var scansPerDay = new Dictionary<string, int>();
        var dayNames = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

        for (int i = 0; i < 7; i++)
        {
            var dayDate = weekStart.AddDays(i);
            var dayScans = requests.Count(r => r.ScanTime.Date == dayDate);
            scansPerDay[dayNames[i]] = dayScans;
        }

        return (totalScansToday, uniqueUsersToday, scansPerDay);
    }

    // DeviceScanLimit Operations (NEW for QR scan limits per device)
    public async Task<DeviceScanLimit?> GetDeviceScanLimitAsync(string deviceId)
    {
        await InitAsync();
        return await _connection!.Table<DeviceScanLimit>().Where(d => d.DeviceId == deviceId).FirstOrDefaultAsync();
    }

    public async Task SaveDeviceScanLimitAsync(DeviceScanLimit limit)
    {
        await InitAsync();
        var existing = await _connection!.Table<DeviceScanLimit>().Where(d => d.DeviceId == limit.DeviceId).FirstOrDefaultAsync();
        if (existing != null)
        {
            // Update existing
            existing.ScanCount = limit.ScanCount;
            existing.MaxScans = limit.MaxScans;
            existing.LastResetDate = limit.LastResetDate;
            await _connection!.UpdateAsync(existing);
        }
        else
        {
            // Insert new
            limit.CreatedAt = DateTime.UtcNow;
            await _connection!.InsertAsync(limit);
        }
    }

    private string GenerateQRCode()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
    }

    // Seed sample data for development
    public async Task SeedSampleDataAsync()
    {
        await InitAsync();

        // Remove previously seeded virtual/test accounts so only real accounts remain
        var virtualEmails = new[] { "user1@example.com", "user2@example.com", "user3@example.com", "user4@example.com", "user5@example.com" };
        foreach (var email in virtualEmails)
        {
            var vUser = await _connection!.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
            if (vUser != null)
            {
                await _connection!.ExecuteAsync("DELETE FROM UserPayment WHERE UserId = ?", vUser.Id);
                await _connection!.ExecuteAsync("DELETE FROM PlayHistory WHERE UserId = ?", vUser.Id);
                await _connection!.ExecuteAsync("DELETE FROM UserDevice WHERE UserId = ?", vUser.Id);
                await _connection!.DeleteAsync(vUser);
            }
        }

        // Remove previously seeded virtual devices by their hardcoded names/models
        var virtualDeviceNames = new[] { "iPhone 12", "Samsung Galaxy A12", "iPad Air 4" };
        foreach (var name in virtualDeviceNames)
        {
            var vDevice = await _connection!.Table<UserDevice>().Where(d => d.DeviceName == name).FirstOrDefaultAsync();
            if (vDevice != null) await _connection!.DeleteAsync(vDevice);
        }

        // Do NOT seed sample users — only real accounts from the app should appear

        var poiCount = await _connection!.Table<AudioPOI>().CountAsync();
        if (poiCount < 15)
        {
            // Xóa dữ liệu cũ (nếu có seed thiếu) rồi seed lại đủ 15 quán
            await _connection!.ExecuteAsync("DELETE FROM AudioPOI");

            static string DeepLink(string name) =>
                $"vinhkhanhtour://play_audio?poi_name={Uri.EscapeDataString(name)}";

            var samplePOIs = new List<AudioPOI>
            {
                // ---- THẾ GIỚI ỐC ----
                new AudioPOI { Name = "Ốc Oanh",              Address = "534 Vĩnh Khánh, Q.4",               Description = "Quán ốc huyền thoại đông nhất Vĩnh Khánh. Nổi tiếng với ốc hương rang muối ớt và càng ghẹ nướng.", DescriptionEn = "The most legendary and busy snail restaurant in Vinh Khanh. Famous for roasted salted snails with chili and grilled crab claws.", DescriptionJa = "ビンカン地区で最も有名で賑わっているカタツムリレストラン。塩辛い揚げカタツムリと唐辛子と蒸しカニで有名です。", DescriptionRu = "Самый легендарный и оживленный ресторан с улитками во Винь Кхане. Известен обжаренными улитками с солью и перцем и жареными крабовыми когтями.", DescriptionZh = "永康最传奇、最繁忙的蜗牛餐厅。以盐烤蜗牛和辣椒烤蟹爪而闻名。", Lat = 10.7595, Lng = 106.7045, Radius = 40, Priority = 1, ImageAsset = "oc_oanh.jpg",   QRCode = DeepLink("Ốc Oanh"),              CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Vũ",                Address = "37 Vĩnh Khánh, Q.4",                Description = "Không gian siêu rộng, menu đa dạng và giá cả bình dân. Món khuyên dùng: Ốc tỏi nướng mỡ hành.", DescriptionEn = "Spacious area, diverse menu and affordable prices. Recommended dish: Garlic snails roasted with lard and scallions.", DescriptionJa = "広々としたスペース、多様なメニューと手頃な価格。推奨料理：ニンニク入りカタツムリの豚脂と小ネギ焙煎。", DescriptionRu = "Просторный зал, разнообразное меню и доступные цены. Рекомендуемое блюдо: Улитки с чесноком, обжаренные со сливочным маслом и зеленью.", DescriptionZh = "宽敞的空间、丰富的菜单和价格便宜。推荐菜肴：蒜蜗牛用猪油和葱烤。", Lat = 10.7578, Lng = 106.7058, Radius = 40, Priority = 1, ImageAsset = "oc_vu.jpg",     QRCode = DeepLink("Ốc Vũ"),                CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Nho",               Address = "178 Vĩnh Khánh, Q.4",               Description = "Chân ái của giới trẻ với các món ốc sốt phô mai kéo sợi, sốt trứng muối béo ngậy cực đỉnh.",               Lat = 10.7582, Lng = 106.7052, Radius = 40, Priority = 1, ImageAsset = "oc_nho.jpg",    QRCode = DeepLink("Ốc Nho"),               CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Thảo",              Address = "383 Vĩnh Khánh, Q.4",               Description = "Quán lâu năm, giữ nguyên hương vị ốc truyền thống Sài Gòn. Nước mắm gừng pha cực ngon.",                   Lat = 10.7590, Lng = 106.7042, Radius = 40, Priority = 1, ImageAsset = "oc_thao.jpg",   QRCode = DeepLink("Ốc Thảo"),              CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Sóc",               Address = "D58 Vĩnh Khánh, Q.4",               Description = "Nổi bật với món nghêu hấp sả ớt cay nồng và ốc móng tay xào rau muống.",                                   Lat = 10.7587, Lng = 106.7048, Radius = 40, Priority = 1, ImageAsset = "oc_soc.jpg",    QRCode = DeepLink("Ốc Sóc"),               CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Tuyết",             Address = "430 Vĩnh Khánh, Q.4",               Description = "Quán bình dân nhưng chất lượng tuyệt vời, phục vụ nhanh nhẹn, các món xào me rất đậm đà.",                  Lat = 10.7585, Lng = 106.7032, Radius = 40, Priority = 1, ImageAsset = "oc_tuyet.jpg",  QRCode = DeepLink("Ốc Tuyết"),             CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Ốc Đào 2",             Address = "Vĩnh Khánh, P.4, Q.4",              Description = "Thương hiệu ốc lâu đời, nêm nếm theo khẩu vị đậm đà đặc trưng, ốc xào sa tế cay xé lưỡi.",                 Lat = 10.7581, Lng = 106.7061, Radius = 40, Priority = 1, ImageAsset = "oc_dao.jpg",    QRCode = DeepLink("Ốc Đào 2"),             CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                // ---- LẨU & NƯỚNG ----
                new AudioPOI { Name = "Quán Nướng Chilli",    Address = "232 Vĩnh Khánh, Q.4",               Description = "Thiên đường hàu nướng với hơn 20 loại sốt khác nhau, hải sản nướng ngói thơm lừng.", DescriptionEn = "Oyster grilling paradise with over 20 different sauce varieties, aromatic ceramic-roasted seafood.", DescriptionJa = "20種類以上のソースを備えたカキ焼きの楽園、香ばしい陶板焼きシーフード。", DescriptionRu = "Рай для жарки устриц с более чем 20 различными видами соусов, ароматные морепродукты, запеченные на керамике.", DescriptionZh = "拥有20多种不同酱汁的牡蛎烧烤天堂，香喷喷的陶板烤海鲜。", Lat = 10.7586, Lng = 106.7055, Radius = 50, Priority = 2, ImageAsset = "nuong_chilli.jpg", QRCode = DeepLink("Quán Nướng Chilli"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Lẩu Bò Khu Nhà Cháy", Address = "Chung cư Đoàn Văn Bơ, gần Vĩnh Khánh", Description = "Lẩu bò gia truyền nước dùng ngọt thanh từ xương, bò viên tự làm dai giòn sừn sựt.",                  Lat = 10.7590, Lng = 106.7025, Radius = 50, Priority = 2, ImageAsset = "lau_bo.jpg",    QRCode = DeepLink("Lẩu Bò Khu Nhà Cháy"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Sườn Nướng Muối Ớt",  Address = "Dọc đường Vĩnh Khánh, Q.4",          Description = "Sườn heo nướng tẩm ớt cay nồng, ăn kèm đồ chua giải ngấy cực kỳ bắt bia.",                                Lat = 10.7588, Lng = 106.7040, Radius = 40, Priority = 2, ImageAsset = "suon_nuong.jpg", QRCode = DeepLink("Sườn Nướng Muối Ớt"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Khèn BBQ - Nướng Ngói", Address = "165 Vĩnh Khánh, Q.4",              Description = "Thịt được nướng trên ngói đỏ giúp giữ độ ngọt, không bị ám khói than, tẩm ướp chuẩn vị Tây Bắc.",          Lat = 10.7592, Lng = 106.7038, Radius = 40, Priority = 2, ImageAsset = "khen_bbq.jpg",  QRCode = DeepLink("Khèn BBQ - Nướng Ngói"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Lẩu Dê Dũng Mập",     Address = "Đầu đường Vĩnh Khánh",               Description = "Lẩu dê nấu chao thơm phức, thịt dê núi mềm ngọt, không bị hôi, ăn kèm rau rừng.",                          Lat = 10.7602, Lng = 106.7049, Radius = 50, Priority = 2, ImageAsset = "lau_de.jpg",    QRCode = DeepLink("Lẩu Dê Dũng Mập"),     CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                // ---- ĂN VẶT & MÓN KHÁC ----
                new AudioPOI { Name = "Phá Lấu Cô Oanh",     Address = "Đoạn giao Tôn Đản - Vĩnh Khánh",     Description = "Phá lấu bò nấu nước cốt dừa béo ngậy, ăn kèm bánh mì nóng giòn chấm mắm me chua ngọt.",                   Lat = 10.7570, Lng = 106.7065, Radius = 30, Priority = 3, ImageAsset = "pha_lau.jpg",   QRCode = DeepLink("Phá Lấu Cô Oanh"),     CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Sushi Viên Vĩnh Khánh", Address = "Dọc vỉa hè Vĩnh Khánh",            Description = "Sushi lề đường giá học sinh sinh viên nhưng cá hồi, trứng cuộn rất tươi và sạch sẽ.",                       Lat = 10.7598, Lng = 106.7042, Radius = 30, Priority = 3, ImageAsset = "sushi_vien.jpg", QRCode = DeepLink("Sushi Viên Vĩnh Khánh"), CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new AudioPOI { Name = "Trái Cây Tô & Chè",   Address = "Giữa phố Vĩnh Khánh",                Description = "Tráng miệng mát lạnh giải nhiệt sau khi ăn đồ nướng cay nóng, trái cây xô ngập tràn sữa chua.",             Lat = 10.7584, Lng = 106.7050, Radius = 30, Priority = 3, ImageAsset = "trai_cay_to.jpg", QRCode = DeepLink("Trái Cây Tô & Chè"),  CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            foreach (var poi in samplePOIs)
            {
                await _connection!.InsertAsync(poi);
            }
        }

        // Seed sample AdminUsers if none exist
        var adminCount = await _connection!.Table<AdminUser>().CountAsync();
        if (adminCount == 0)
        {
            var sampleAdmins = new List<AdminUser>
            {
                new AdminUser { Username = "admin", Password = "Admin@123", FullName = "Quản Trị Viên", Email = "admin@vinhkhanhtour.com", Role = "admin", IsActive = true, CreatedAt = DateTime.Now, LoginCount = 0 },
                new AdminUser { Username = "manager", Password = "Manager@123", FullName = "Quản Lý Hệ Thống", Email = "manager@vinhkhanhtour.com", Role = "manager", IsActive = true, CreatedAt = DateTime.Now, LoginCount = 0 }
            };

            foreach (var admin in sampleAdmins)
            {
                await _connection!.InsertAsync(admin);
            }
        }

        // Seed base settings only when DB is brand new
        var settingCount = await _connection!.Table<SystemSetting>().CountAsync();
        if (settingCount == 0)
        {
            var baseSettings = new List<SystemSetting>
            {
                new SystemSetting { Key = "App.Name", Value = "Vĩnh Khánh Tour", Description = "Tên ứng dụng", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "App.Version", Value = "1.0.0", Description = "Phiên bản ứng dụng", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Payment.Price", Value = "99000", Description = "Giá premium (VND)", SettingType = "decimal", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Payment.Currency", Value = "VND", Description = "Đơn vị tiền tệ", SettingType = "string", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Feature.QRScanning", Value = "true", Description = "Bật/tắt quét QR", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Feature.AudioGuide", Value = "true", Description = "Bật/tắt hướng dẫn âm thanh", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Security.SessionTimeout", Value = "3600", Description = "Thời gian hết phiên (giây)", SettingType = "int", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
                new SystemSetting { Key = "Maintenance.Mode", Value = "false", Description = "Chế độ bảo trì", SettingType = "bool", UpdatedAt = DateTime.Now, UpdatedBy = "system" },
            };
            foreach (var s in baseSettings)
                await _connection!.InsertAsync(s);
        }

        // Always upsert Payment.* settings so they exist even on existing DBs.
        // These are read by the mobile app at startup via GET /api/settings/app-config.
        // Admin can change values at runtime; this block only inserts if the key is missing.
        var paymentDefaults = new (string Key, string Value, string Desc, string Type)[]
        {
            ("Payment.Model",           "listen",  "Mô hình thanh toán: 'listen' hoặc 'scan'",             "string"),
            ("Payment.DailyFreeListens","5",        "Số lượt nghe miễn phí mỗi ngày",                       "int"),
            ("Payment.PkgBasicListens", "5",        "Gói Cơ Bản — số lượt nghe",                            "int"),
            ("Payment.PkgBasicPrice",   "5000",     "Gói Cơ Bản — giá (VND)",                               "int"),
            ("Payment.PkgPremiumListens","20",      "Gói Cao Cấp — số lượt nghe",                           "int"),
            ("Payment.PkgPremiumPrice", "15000",    "Gói Cao Cấp — giá (VND)",                              "int"),
            ("Payment.PkgVipListens",   "999",      "Gói VIP — số lượt nghe (999 = không giới hạn)",        "int"),
            ("Payment.PkgVipPrice",     "50000",    "Gói VIP — giá (VND)",                                  "int"),
        };
        foreach (var (key, value, desc, type) in paymentDefaults)
        {
            var exists = await _connection!.Table<SystemSetting>().Where(s => s.Key == key).FirstOrDefaultAsync();
            if (exists == null)
                await _connection!.InsertAsync(new SystemSetting { Key = key, Value = value, Description = desc, SettingType = type, UpdatedAt = DateTime.Now, UpdatedBy = "system" });
        }
    }

    // ===== ADDITIONAL METHODS FOR ADMIN DASHBOARD =====

    /// <summary>
    /// Get all payments for admin dashboard
    /// </summary>
    public async Task<List<UserPayment>> GetAllPaymentsAsync()
    {
        await InitAsync();
        return await _connection!.Table<UserPayment>().ToListAsync();
    }

    /// <summary>
    /// Get all device scan limits for admin dashboard
    /// </summary>
    public async Task<List<DeviceScanLimit>> GetAllDeviceScanLimitsAsync()
    {
        await InitAsync();
        var limits = await _connection!.Table<DeviceScanLimit>().ToListAsync();

        // Reset counts if new day
        foreach (var limit in limits)
        {
            if (limit.LastResetDate < DateTime.UtcNow.Date)
            {
                limit.ScanCount = 0;
                limit.LastResetDate = DateTime.UtcNow.Date;
                await _connection!.UpdateAsync(limit);
            }
        }

        return limits;
    }
}